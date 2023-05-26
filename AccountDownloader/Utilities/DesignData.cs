using AccountDownloader.Services;
using AccountDownloader.Utilities;
using AccountDownloader.ViewModels;
using AccountDownloaderLibrary;
using CloudX.Shared;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

//TODO: strip this file from the final build somehow
namespace AccountDownloader
{
    // Design based view models that are intended just for the view designer to work, I'm not sure how to handle these so I'll leave them here and deal with them later.
    public class DesignUserProfile : ReactiveObject, IUserProfile
    {
        [Reactive]
        public string UserName { get; set; }

        [Reactive]
        public Uri? PictureURI { get; set; }

        public void UpdateUser(User obj)
        {
            // No-op
        }

        public DesignUserProfile()
        {
            UserName = "User";
            PictureURI = AssetHelper.GetUri("AnonymousHeadset.png");
        }
    }
    public class DesignStorageRecord : IStorageRecord
    {
        public long UsedBytes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long TotalBytes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string FormattedUsed => "20 GB";

        public string FormattedTotal => throw new NotImplementedException();

        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public OwnerType OwnerType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changing => throw new NotImplementedException();

        public IObservable<IReactivePropertyChangedEventArgs<IReactiveObject>> Changed => throw new NotImplementedException();

        public IDisposable SuppressChangeNotifications()
        {
            throw new NotImplementedException();
        }

        public void Update(long used, long total)
        {
            throw new NotImplementedException();
        }
    }
    public class DesignData
    {
        public static List<RecordDownloadFailure> GenerateFailList()
        {
            var list = new List<RecordDownloadFailure>();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new RecordDownloadFailure
                {
                    FailureReason = "Download Failed",
                    OwnerId = "U-User",
                    RecordName = "HomeWorld",
                    RecordPath = "U-User/Worlds/HomeWorld",
                    RecordId = "R-1234"
                });
            }
            return list;
        }
        public static FailedRecordsViewModel DesignFailedRecordsViewModel = new FailedRecordsViewModel(GenerateFailList());
        public static UserProfileViewModel DesignProfileViewModel = new(new DesignUserProfile());
        private static readonly List<GroupRecord> _groups = new() {
            new GroupRecord("G-Group", "Group FOUR", false, new DesignStorageRecord()),
            new GroupRecord("G-Group1", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group2", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group3", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group4", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group5", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group6", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group7", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group8", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group9", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group10", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group2", false, new DesignStorageRecord())
        };
        public static GroupsListViewModel DesignGroupsListViewModel = new(_groups);
    }
}
