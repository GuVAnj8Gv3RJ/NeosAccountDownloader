using AccountDownloader.Services;
using AccountDownloader.Utilities;
using AccountDownloader.ViewModels;
using AccountDownloaderLibrary;
using CloudX.Shared;
using DynamicData;
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
        public static List<RecordDownloadFailure> GenerateRecordFailList()
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
        public static readonly FailedRecordsViewModel DesignFailedRecordsViewModel = new(GenerateRecordFailList(), GenerateAssetFailList());

        private static List<AssetFailure> GenerateAssetFailList()
        {
            var list = new List<AssetFailure>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(new AssetFailure("1234566sodijosdijfoisdjofijsdf9ij", "Asset failed to download", null));
            }
            return list;
        }

        public static readonly UserProfileViewModel DesignProfileViewModel = new(new DesignUserProfile());
        private static readonly List<GroupRecord> _groups = new() {
            new GroupRecord("G-Group0", "Group 1 with Long Name", false, new DesignStorageRecord()),
            new GroupRecord("G-Group1", "Group 2", false, new DesignStorageRecord()),
            new GroupRecord("G-Group2", "Group 3", true, new DesignStorageRecord()),
            new GroupRecord("G-Group3", "Group 4", false, new DesignStorageRecord()),
            new GroupRecord("G-Group4", "Group 5", false, new DesignStorageRecord()),
            new GroupRecord("G-Group5", "Group 6", false, new DesignStorageRecord()),
            new GroupRecord("G-Group6", "Group 7", true, new DesignStorageRecord()),
            new GroupRecord("G-Group7", "Group 8", false, new DesignStorageRecord()),
            new GroupRecord("G-Group8", "Group 9", false, new DesignStorageRecord()),
            new GroupRecord("G-Group9", "Group 10", false, new DesignStorageRecord()),
            new GroupRecord("G-Group10", "Group 11", false, new DesignStorageRecord()),
            new GroupRecord("G-Group11", "Group 12", true, new DesignStorageRecord()),
            new GroupRecord("G-Group12", "Group 13", false, new DesignStorageRecord()),
            new GroupRecord("G-Group13", "Group 14", false, new DesignStorageRecord()),
            new GroupRecord("G-Group14", "Group 15", false, new DesignStorageRecord()),
            new GroupRecord("G-Group15", "Group 16", false, new DesignStorageRecord()),
            new GroupRecord("G-Group16", "Group 17", false, new DesignStorageRecord()),
            new GroupRecord("G-Group17", "Group 18 with longer name that's just silly.", false, new DesignStorageRecord()),
            new GroupRecord("G-Group18", "Group 19", false, new DesignStorageRecord()),
            new GroupRecord("G-Group19", "Group 20", true, new DesignStorageRecord()),
            new GroupRecord("G-Group20", "Group 21", false, new DesignStorageRecord())
        };
        public static readonly GroupsListViewModel DesignGroupsListViewModel = new(_groups);
    }
}
