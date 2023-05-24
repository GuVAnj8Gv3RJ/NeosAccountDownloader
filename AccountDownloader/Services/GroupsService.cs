using CloudX.Shared;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountDownloader.Services
{
    public class GroupRecord : ReactiveObject, IGroup
    {
        public string Name { get; }

        public string Id { get; }

        [Reactive]
        public bool ShouldDownload { get; set; } = false;

        [Reactive]
        public IStorageRecord Storage { get; set; }

        public long RequiredBytes => Storage.UsedBytes; 

        public GroupRecord(string id, string name, IStorageRecord storage)
        {
            Name = name;
            Id = id;
            Storage = storage;
        }
    }
    public class GroupsService: IGroupsService
    {
        private CloudXInterface Interface { get; } 
        private ILogger Log { get; }
        private IStorageService StorageService { get; }
        public GroupsService(CloudXInterface? cloudInterface,IStorageService? storageService, ILogger? logger) {

            Interface = cloudInterface ?? throw new ArgumentNullException(nameof(cloudInterface));
            StorageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            Log = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<IGroup>> GetGroups()
        {
            Log.LogInformation("Getting groups for: {user}", Interface.CurrentUser.Username);
            List<IGroup> groups = new();
            var memberships = await Interface.GetUserGroupMemeberships();

            //TODO: Error handling
            if (!memberships.IsOK)
            {
                Log.LogWarning("Unable to get groups for reason: {reason}", memberships.Content);
                return groups;
            }

            if (memberships.Entity.Count == 0)
            {
                Log.LogInformation("User does not belong to any groups");
                return groups;
            }

            foreach (var membership in memberships.Entity)
            {
                var group = await Interface.GetGroup(membership.GroupId);

                if (group.IsOK)
                {
                    var groupRecord = new GroupRecord(group.Entity.GroupId, group.Entity.Name, StorageService.GetGroupStorage(group.Entity.GroupId));
                    groups.Add(groupRecord);
                }
            }

            if (groups.Count == 0)
                Log.LogInformation("User is not an admin for any groups");
            else
                Log.LogInformation("Got {count} groups for user.", groups.Count);
            return groups;
        }
    }
}
