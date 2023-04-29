using System.Threading.Tasks.Dataflow;
using CloudX.Shared;
using AccountDownloaderLibrary.Extensions;

namespace AccountDownloaderLibrary
{
    public class AccountDownloadController
    {
        readonly IAccountDataGatherer source;
        readonly IAccountDataStore target;
        readonly AccountDownloadConfig config;

        #region PROGRESS REPORT

        public string ProgressMessage { get; private set; } = string.Empty;

        public event Action<string> ProgressMessagePosted;

        public AccountDownloadStatus Status { get; private set; } = new AccountDownloadStatus();

        #endregion

        public AccountDownloadController(IAccountDataGatherer source, IAccountDataStore target, AccountDownloadConfig config)
        {
            this.source = source;
            this.target = target;

            this.config = config;

            this.source.ProgressMessage += str => ProgressMessagePosted?.Invoke(str);
            this.target.ProgressMessage += str => ProgressMessagePosted?.Invoke(str);
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
            if (config.GroupsToDownload == null || config.GroupsToDownload.Count == 0)
                return true;

            return config.GroupsToDownload.Contains(group.GroupId);
        }

        private async Task HandleCancel()
        {
            await source.Cancel();
            await target.Cancel();
        }

        public async Task<bool> Download(CancellationToken cancellationToken)
        {
            try
            {
                Status.Phase = "Setup";
                SetProgressMessage("Beginning download");

                Status.StartedOn = DateTimeOffset.UtcNow;

                SetProgressMessage("Preparing data source");
                await source.Prepare(cancellationToken).ConfigureAwait(false);

                SetProgressMessage("Preparing data target");
                await target.Prepare(cancellationToken).ConfigureAwait(false);

                Status.CurrentlyDownloadingName = source.Username;

                // Status.Phase for this step is set in DownloadContacts
                if (config.DownloadContacts)
                    await DownloadContacts(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    await HandleCancel();
                    return false;
                }

                // Status.Phase for this segment is set in the method
                await DownloadOwned(source.UserId,
                    Status.UserRecordsStatus, Status.UserVariablesStatus,
                    config.RecordsToDownload, config.VariablesToDownload,
                    cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    await HandleCancel();
                    return false;
                }

                // download all stuff from the groups the user owns
                if (config.DownloadGroups)
                {
                    await foreach (var groupData in source.GetGroups())
                    {
                        Status.Phase = "Groups";
                        if (cancellationToken.IsCancellationRequested)
                            return false;

                        var group = groupData.group;

                        Status.TotalGroupCount = source.FetchedGroupCount;

                        if (!ProcessGroup(group))
                            continue;

                        var groupStatus = Status.GetGroupStatus(group.GroupId, group.Name);

                        Status.CurrentlyDownloadingName = group.Name;

                        SetProgressMessage($"Downloading group {group.Name} ({group.GroupId})");

                        await target.StoreGroup(group, groupData.storage).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            return false;

                        Status.Phase = "Group Members";
                        await DownloadMembers(group, groupStatus, cancellationToken).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested)
                            return false;

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
                    return false;
                }


                // During this part, the download processor inside the local account store will repeatedly overwrite our progress messages
                // as it progress through the stack of Scheduled Asset downloads.

                // So we basically just say "uh we're waiting for stuff to finish".

                Status.Phase = "Waiting for queued jobs to finish";
                await target.Complete().ConfigureAwait(false);

                SetProgressMessage("Download complete");

                Status.Phase = "Complete";

                return true;
            }
            catch (Exception ex)
            {
                Status.Phase = "Error";
                // Use the message as the full error contains a stack trace.
                Status.Error = ex.Message;

                // Cancel the download on an error
                await HandleCancel();

                return false;
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
            if (config.DownloadCloudVariableDefinitions)
            {
                Status.Phase = "Cloud Variable Definitions";
                await DownloadVariableDefinitions(ownerId, variablesStatus, cancellationToken).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            Status.Phase = "Cloud Variables";
            if (config.DownloadCloudVariables)
            {
                if (variablesToDownload?.Count > 0)
                {
                    var variables = new List<CloudVariable>();

                    foreach (var path in variablesToDownload)
                    {
                        var variable = await source.GetVariable(ownerId, path).ConfigureAwait(false);

                        if (variable != null)
                            variables.Add(variable);
                    }

                    await target.StoreVariables(variables).ConfigureAwait(false);

                    variablesStatus.DownloadedVariableCount = variables.Count;
                }
                else
                    await DownloadVariables(ownerId, variablesStatus, cancellationToken).ConfigureAwait(false);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            Status.Phase = "Queueing Record Downloads";
            if (config.DownloadUserRecords || IdUtil.GetOwnerType(ownerId) != OwnerType.User)
            {
                if (recordsToDownload?.Count > 0)
                {
                    var callbacks = SetupCallbacks(recordsStatus);

                    recordsStatus.TotalRecordCount = recordsToDownload.Count;

                    foreach (var recordId in recordsToDownload)
                    {
                        var record = await source.GetRecord(ownerId, recordId).ConfigureAwait(false);

                        if (record != null)
                        {
                            Status.CurrentlyDownloadingItem = $"Record {record.Name} ({record.CombinedRecordId})";

                            var error = await target.StoreRecord(record, source, callbacks, config.ForceOverwrite).ConfigureAwait(false);

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
                    await DownloadRecords(ownerId, recordsStatus, config.OnlyNewRecords, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DownloadVariables(string ownerId, VariableDownloadStatus status, CancellationToken cancellationToken)
        {
            SetProgressMessage($"Downloading variables for {ownerId}...");

            var variables = await source.GetVariables(ownerId).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                return;

            await target.StoreVariables(variables).ConfigureAwait(false);

            status.DownloadedVariableCount = variables.Count;

            SetProgressMessage($"Downloaded {variables.Count} variables");
        }

        public async Task DownloadVariableDefinitions(string ownerId, VariableDownloadStatus status, CancellationToken _)
        {
            SetProgressMessage($"Downloading variable definitions for {ownerId}...");

            var definitions = await source.GetVariableDefinitions(ownerId).ConfigureAwait(false);
            await target.StoreDefinitions(definitions).ConfigureAwait(false);

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
                AssetUploaded = () =>
                {
                    lock (status)
                        status.AssetsUploaded++;
                },
                AssetMissing = hash => Status.RegisterMissingAsset(hash)
            };
        }

        public async Task DownloadRecords(string ownerId, RecordDownloadStatus status, bool onlyNew, CancellationToken cancellationToken)
        {
            DateTime? latest = null;

            if (onlyNew)
            {
                latest = await target.GetLatestRecordTime(ownerId).ConfigureAwait(false);

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

                    var error = await target.StoreRecord(r, source, SetupCallbacks(status), config.ForceOverwrite).ConfigureAwait(false);

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
                    MaxDegreeOfParallelism = 8,
                    EnsureOrdered = false
                });

            await foreach (var record in source.GetRecords(ownerId, latest).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                status.TotalRecordCount = source.FetchedRecordCount(ownerId);

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

        public async Task DownloadContacts(CancellationToken cancellationToken)
        {
            Status.Phase = "Contacts";
            SetProgressMessage("Downloading contacts");

            var contacts = await source.GetContacts().ConfigureAwait(false);

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

                await target.StoreContact(contact);
                SetProgressMessage($"Downloaded {contact.FriendUsername} ({contact.FriendUserId})");

                contactIdMapping.Add(contact.FriendUserId, originalContactId);
            }


            // Download messages
            if (config.DownloadMessageHistory)
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
            var latest = await target.GetLatestMessageTime(contact.FriendUserId).ConfigureAwait(false);

            string latestText = latest == CloudAccountDataStore.EARLIEST_API_TIME ? "the beginning" : latest.ToString();

            // move it back a bit just to be sure
            latest = latest.AddMinutes(-10);

            SetProgressMessage($"Fetching messages from {contact.FriendUserId} from {latestText}.");

            int count = 0;

            await foreach (var message in source.GetMessages(contactId, latest).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Status.CurrentlyDownloadingItem = $"Message {message.Id} ({message.SendTime})";

                message.SetOtherUserId(contact.FriendUserId);

                await target.StoreMessage(message).ConfigureAwait(false);

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

            var memberData = await source.GetMembers(group.GroupId).ConfigureAwait(false);

            foreach (var data in memberData)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Status.CurrentlyDownloadingItem = $"Member {data.member.UserId} of {group.Name}";
                await target.StoreMember(group, data.member, data.storage).ConfigureAwait(false);

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
