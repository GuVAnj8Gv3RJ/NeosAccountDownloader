using CloudX.Shared;
using ReactiveUI;
using System;
using BaseX;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;

namespace AccountDownloader.Services
{
    public class ReactiveStorageRecord : ReactiveObject, IStorageRecord
    {
        [Reactive]
        public long UsedBytes { get; set; }

        [Reactive]
        public long TotalBytes { get; set; }

        // See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [ObservableAsProperty]
        public string FormattedUsed { get;} = string.Empty;
        [ObservableAsProperty]
        public string FormattedTotal { get; } = string.Empty;

        public OwnerType OwnerType { get; set; }
        public string Id { get; set; }

        public void Update(long used, long total)
        {
            UsedBytes = used;
            TotalBytes = total;
        }

        public void Update(User? u)
        {
            if (OwnerType != OwnerType.User)
                return;
            if (u == null)
                return;
            if (u.Id != Id)
                return;
            Update(u.UsedBytes, u.QuotaBytes);
        }

        public void Update(Group g)
        {
            if (OwnerType != OwnerType.Group)
                return;
            if (g.GroupId != Id)
                return;
            Update(g.UsedBytes, g.QuotaBytes);
        }

        public ReactiveStorageRecord(string id, OwnerType type)
        {
            Id = id;
            OwnerType = type;

            // This essentially keeps Formatted* properties up to date when the non-formatted variants change
            // See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
            this.WhenAnyValue(x => x.UsedBytes).Select(x=> UnitFormatting.FormatBytes(x)).ToPropertyEx(this, x => x.FormattedUsed);
            this.WhenAnyValue(x => x.TotalBytes).Select(x => UnitFormatting.FormatBytes(x)).ToPropertyEx(this, x => x.FormattedTotal);
        }
    }
    internal class CloudStorageService : IStorageService
    {
        private CloudXInterface Interface { get; }
        private ILogger Logger { get; }
        public CloudStorageService(CloudXInterface? _interface, ILogger? logger)
        {
            if (_interface == null)
                throw new ArgumentNullException(nameof(_interface));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Logger = logger;
            Interface = _interface;
        }

        public IStorageRecord GetUserStorage()
        {
            var storage = new ReactiveStorageRecord(Interface.CurrentUser.Id, OwnerType.User);
            storage.Update(Interface.CurrentUser.UsedBytes, Interface.CurrentUser.QuotaBytes);
            Interface.UserUpdated += storage.Update;

            return storage;
        }

        public IStorageRecord GetGroupStorage(string groupId)
        {
            ReactiveStorageRecord storage;
            var groupInfo = Interface.TryGetCurrentUserGroupInfo(groupId);
            if (groupInfo == null)
                storage = new ReactiveStorageRecord(groupId, OwnerType.Group);
            else
            {
                storage = new ReactiveStorageRecord(groupId, OwnerType.Group);
                storage.Update(groupInfo.UsedBytes, groupInfo.QuotaBytes);
            }
                
            Interface.GroupUpdated += storage.Update;

            return storage;
        }
    }
}
