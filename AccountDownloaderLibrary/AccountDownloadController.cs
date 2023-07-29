using System.Threading.Tasks.Dataflow;
using CloudX.Shared;
using AccountDownloaderLibrary.Extensions;
using AccountDownloaderLibrary.Interfaces;

namespace AccountDownloaderLibrary
{
    public class AccountDownloadController
    {
        readonly IAccountDataGatherer Source;
        readonly IAccountDataStore Target;
        readonly AccountDownloadConfig Config;

        #region PROGRESS REPORT

        public string ProgressMessage { get; private set; } = string.Empty;

        public event Action<string> ProgressMessagePosted;

        public AccountDownloadStatus Status { get; private set; } = new AccountDownloadStatus();

        #endregion

        public AccountDownloadController(IAccountDataGatherer source, IAccountDataStore target, AccountDownloadConfig config)
        {
            this.Source = source;
            this.Target = target;

            this.Config = config;

            this.Source.ProgressMessage += str => ProgressMessagePosted?.Invoke(str);
            this.Target.ProgressMessage += str => ProgressMessagePosted?.Invoke(str);
        }

        void SetProgressMessage(string message)
        {
            ProgressMessage = message;

            ProgressMessagePosted?.Invoke(message);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        static bool ProcessRecord(Record record) => true;
        static bool ProcessContact(Friend contact) => true;
#pragma warning restore IDE0060 // Remove unused parameter
        bool ProcessGroup(Group group)
        {
            if (Config.GroupsToDownload == null || Config.GroupsToDownload.Count == 0)
                return true;

            return Config.GroupsToDownload.Contains(group.GroupId);
        }

        private async Task HandleCancel()
        {
            await Source.Cancel();
            await Target.Cancel();
        }

        public async Task<IDownloadResult> Download(CancellationToken cancellationToken)
        {
            try
            {
                Status.Phase = "Setup";
                SetProgressMessage("Beginning download");

                Status.StartedOn = DateTimeOffset.UtcNow;

                SetProgressMessage("Preparing data source");
                await Source.Prepare(cancellationToken).ConfigureAwait(false);

                SetProgressMessage("Preparing data target");
                await Target.Prepare(cancellationToken).ConfigureAwait(false);

                Status.CurrentlyDownloadingName = Source.Username;

                // Status.Phase for this step is set in the method
                if (Config.DownloadUserMetadata)
                    await DownloadUserMetadata(cancellationToken).ConfigureAwait(false);

                // Status.Phase for this step is set in DownloadContacts
                if (Config.DownloadContacts)
                    await DownloadContacts(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    await HandleCancel();
                    return DownloadResult.Cancelled;
                }

                // Status.Phase for this segment is set in the method
                await DownloadOwned(Source.UserId,
                    Status.UserRecordsStatus, Status.UserVariablesStatus,
                    Config.RecordsToDownload, Config.VariablesToDownload,
                    cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    await HandleCancel();
                    return DownloadResult.Cancelled;
                }

                // download all stuff from the groups the user owns
                if (Config.DownloadGroups)
                {
                    await foreach (var groupData in Source.GetGroups())
                    {
                        Status.Phase = "Groups";
                        if (cancellationToken.IsCancellationRequested)
                            return DownloadResult.Cancelled;

                        var group = groupData.group;

                        Status.TotalGroupCount = Source.FetchedGroupCount;

                        if (!ProcessGroup(group))
                            continue;

                        var groupStatus = Status.GetGroupStatus(group.GroupId, group.Name);

                        Status.CurrentlyDownloadingName = group.Name;

                        SetProgressMessage($"Downloading group {group.Name} ({group.GroupId})");

                        await Target.StoreGroup(group, groupData.storage).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            return DownloadResult.Cancelled;

                        Status.Phase = "Group Members";
                        await DownloadMembers(group, groupStatus, cancellationToken).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            return DownloadResult.Cancelled;

                        Status.Phase = "Group Records";
                        await DownloadOwned(group.GroupId,
                            groupStatus.RecordsStatus, groupStatus.VariablesStatus,
                            null, null,
                            cancellationToken).ConfigureAwait(false);

                        Status.DownloadedGroupCount++;
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    await HandleCancel();
                    return DownloadResult.Cancelled;
                }


                // During this part, the download processor inside the local account store will repeatedly overwrite our progress messages
                // as it progress through the stack of Scheduled Asset downloads.

                // So we basically just say "uh we're waiting for stuff to finish".

                Status.Phase = "Waiting for queued jobs to finish";
                await Target.Complete().ConfigureAwait(false);

                SetProgressMessage("Download complete");

                Status.Phase = "Complete";

                return DownloadResult.Successful;
            }
            catch (Exception ex)
            {
                Status.Phase = "Error";
                // Use the message as the full error contains a stack trace.
                Status.Error = ex.Message;

                // Cancel the download on an error
                await HandleCancel();

                // Pass both the string message and the whole exception up
                // This lets us deal with things nicely.
                return new DownloadResult(DownloadResultType.Error, ex.Message, ex);
            }
            finally
            {
                Status.CompletedOn = DateTimeOffset.UtcNow;
                Status.CurrentlyDownloadingName = null;
                Status.CurrentlyDownloadingItem = null;
            }
        }

        public async Task DownloadOwned(string ownerId,
            RecordDownloadStatus recordsStatus, VariableDownloadStatus variablesStatus,
            List<string> recordsToDownload, List<string> variablesToDownload,
            CancellationToken cancellationToken)
        {
            if (Config.DownloadCloudVariableDefinitions)
            {
                Status.Phase = "Cloud Variable Definitions";
                await DownloadVariableDefinitions(ownerId, variablesStatus, cancellationToken).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            Status.Phase = "Cloud Variables";
            if (Config.DownloadCloudVariables)
            {
                if (variablesToDownload?.Count > 0)
                {
                    var variables = new List<CloudVariable>();

                    foreach (var path in variablesToDownload)
                    {
                        var variable = await Source.GetVariable(ownerId, path).ConfigureAwait(false);

                        if (variable != null)
                            variables.Add(variable);
                    }

                    await Target.StoreVariables(variables).ConfigureAwait(false);

                    variablesStatus.DownloadedVariableCount = variables.Count;
                }
                else
                    await DownloadVariables(ownerId, variablesStatus, cancellationToken).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            Status.Phase = "Queueing Record Downloads";
            if (Config.DownloadUserRecords || IdUtil.GetOwnerType(ownerId) != OwnerType.User)
            {
                // If specific records are specied
                if (recordsToDownload?.Count > 0)
                {
                    var callbacks = SetupCallbacks(recordsStatus);

                    recordsStatus.TotalRecordCount = recordsToDownload.Count;

                    foreach (var recordId in recordsToDownload)
                    {
                        SetProgressMessage($"Queueing: {recordId}");
                        var record = await Source.GetRecord(ownerId, recordId).ConfigureAwait(false);

                        if (record != null)
                        {
                            Status.CurrentlyDownloadingItem = $"Record {record.Name} ({record.CombinedRecordId})";

                            var error = await Target.StoreRecord(record, Source, callbacks, Config.ForceOverwrite).ConfigureAwait(false);

                            if (error != null)
                                HandleRecordError(recordsStatus, record, error);
                            else
                            {
                                lock (recordsStatus)
                                    recordsStatus.DownloadedRecordCount++;
                            }
                        }
                    }
                }
                else
                    await DownloadRecords(ownerId, recordsStatus, Config.OnlyNewRecords, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DownloadVariables(string ownerId, VariableDownloadStatus status, CancellationToken cancellationToken)
        {
            SetProgressMessage($"Downloading variables for {ownerId}...");

            var variables = await Source.GetVariables(ownerId).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                return;

            await Target.StoreVariables(variables).ConfigureAwait(false);

            status.DownloadedVariableCount = variables.Count;

            SetProgressMessage($"Downloaded {variables.Count} variables");
        }

        public async Task DownloadVariableDefinitions(string ownerId, VariableDownloadStatus status, CancellationToken _)
        {
            SetProgressMessage($"Downloading variable definitions for {ownerId}...");

            var definitions = await Source.GetVariableDefinitions(ownerId).ConfigureAwait(false);
            await Target.StoreDefinitions(definitions).ConfigureAwait(false);

            status.DownloadedVariableDefinitionCount = definitions.Count;

            SetProgressMessage($"Downloaded {definitions.Count} variable definitions");
        }

        RecordStatusCallbacks SetupCallbacks(RecordDownloadStatus status)
        {
            return new RecordStatusCallbacks()
            {
                AssetToUploadAdded = diff =>
                {
                    lock (status)
                    {
                        status.AssetsToUpload++;
                        status.BytesToUpload += diff.Bytes;
                    }
                },
                BytesUploaded = bytes =>
                {
                    lock (status)
                        status.BytesUploaded += bytes;
                },
                AssetUploaded = hash =>
                {
                    lock (status)
                        status.AssetsUploaded++;
                },
                AssetFailure =  failure => Status.RegisterAssetFailure(failure),
                AssetSkipped = hash =>
                {
                    lock (status)
                        Status.AssetsSkipped++;
                }
            };
        }

        public async Task DownloadRecords(string ownerId, RecordDownloadStatus status, bool onlyNew, CancellationToken cancellationToken)
        {
            DateTime? latest = null;

            if (onlyNew)
            {
                latest = await Target.GetLatestRecordTime(ownerId).ConfigureAwait(false);

                if (latest != null)
                {
                    try
                    {
                        latest = latest.Value.AddDays(-1);
                    }
                    catch
                    {
                        latest = null;
                    }
                }
            }

            string latestText = onlyNew ? $" from {latest}" : "";

            SetProgressMessage($"Downloading records for {ownerId}" + latestText);

            int count = 0;

            ActionBlock<Record> recordProcessing = new(
                async r =>
                {
                    Status.CurrentlyDownloadingItem = $"Record {r.Name} ({r.CombinedRecordId})";

                    var error = await Target.StoreRecord(r, Source, SetupCallbacks(status), Config.ForceOverwrite).ConfigureAwait(false);

                    if (error != null)
                    {
                        HandleRecordError(status, r, error);
                        return;
                    };

                    var newCount = Interlocked.Increment(ref count);

                    lock (status)
                        status.DownloadedRecordCount++;

                    if (newCount % 100 == 0)
                        SetProgressMessage($"Downloaded {count} records...");
                },
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = Config.MaxDegreeOfParallelism,
                    EnsureOrdered = false
                });

            await foreach (var record in Source.GetRecords(ownerId, latest).ConfigureAwait(false))
            {
                SetProgressMessage($"Queueing: {record.CombinedRecordId}");
                if (cancellationToken.IsCancellationRequested)
                    return;

                status.TotalRecordCount = Source.FetchedRecordCount(ownerId);

                if (!ProcessRecord(record))
                    continue;

                recordProcessing.Post(record);

                // wait a bit if there's a lot of tasks
                while (recordProcessing.InputCount > 16)
                    await Task.Delay(100, cancellationToken);
            }

            recordProcessing.Complete();
            await recordProcessing.Completion.ConfigureAwait(false);

            SetProgressMessage($"Downloaded {count} records.");
        }

        public async Task DownloadUserMetadata(CancellationToken cancellationToken)
        {
            Status.Phase = "User Metadata";
            SetProgressMessage("Downloading user metadata");

            var userMetadata = Source.GetUserMetadata();
            await Target.StoreUserMetadata(userMetadata);
        }

        public async Task DownloadContacts(CancellationToken cancellationToken)
        {
            Status.Phase = "Contacts";
            SetProgressMessage("Downloading contacts");

            var contacts = await Source.GetContacts().ConfigureAwait(false);

            Status.TotalContactCount = contacts.Count;

            Dictionary<string, string> contactIdMapping = new();

            foreach (var contact in contacts)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!ProcessContact(contact))
                    continue;

                Status.CurrentlyDownloadingItem = "Contact: " + contact.FriendUsername;

                var originalContactId = contact.FriendUserId;

                await Target.StoreContact(contact);
                SetProgressMessage($"Downloaded {contact.FriendUsername} ({contact.FriendUserId})");

                contactIdMapping.Add(contact.FriendUserId, originalContactId);
            }


            // Download messages
            if (Config.DownloadMessageHistory)
            {
                Status.Phase = "Message History";
                foreach (var contact in contacts)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (!ProcessContact(contact))
                        continue;

                    await DownloadMessages(contact, contactIdMapping[contact.FriendUserId], cancellationToken).ConfigureAwait(false);

                    Status.DownloadedContactCount++;
                }
            }
            Status.DownloadedContactCount = Status.TotalContactCount;
        }

        public async Task DownloadMessages(Friend contact, string contactId, CancellationToken cancellationToken)
        {
            var latest = await Target.GetLatestMessageTime(contact.FriendUserId).ConfigureAwait(false);

            string latestText = latest == CloudAccountDataStore.EARLIEST_API_TIME ? "the beginning" : latest.ToString();

            // move it back a bit just to be sure
            latest = latest.AddMinutes(-10);

            SetProgressMessage($"Fetching messages from {contact.FriendUserId} from {latestText}.");

            int count = 0;

            await foreach (var message in Source.GetMessages(contactId, latest).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Status.CurrentlyDownloadingItem = $"Message {message.Id} ({message.SendTime})";

                message.SetOtherUserId(contact.FriendUserId);

                await Target.StoreMessage(message).ConfigureAwait(false);

                Status.DownloadedMessageCount++;
                count++;

                if (count % 100 == 0)
                    SetProgressMessage($"Downloaded {count} messages...");
            }

            SetProgressMessage($"Downloaded {count} messages.");
        }

        public async Task DownloadMembers(Group group, GroupDownloadStatus groupStatus, CancellationToken cancellationToken)
        {
            SetProgressMessage($"Downloading members for {group.GroupId}");

            var memberData = await Source.GetMembers(group.GroupId).ConfigureAwait(false);

            foreach (var data in memberData)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Status.CurrentlyDownloadingItem = $"Member {data.member.UserId} of {group.Name}";
                await Target.StoreMember(group, data.member, data.storage).ConfigureAwait(false);

                groupStatus.DownloadedMemberCount++;
            }

            SetProgressMessage($"Downloaded {memberData.Count} members.");
        }

        static void HandleRecordError(RecordDownloadStatus status, Record record, string error)
        {
            lock (status)
            {
                status.FailedRecords ??= new List<RecordDownloadFailure>();

                status.FailedRecords.Add(new RecordDownloadFailure()
                {
                    RecordId = record.RecordId,
                    OwnerId = record.OwnerId,
                    RecordName = record.Name,
                    RecordPath = record.Path,
                    FailureReason = error
                });
            }
        }
    }
}
