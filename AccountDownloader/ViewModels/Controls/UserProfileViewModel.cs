using AccountDownloader.Services;
using CloudX.Shared;
using ReactiveUI;
using System;

namespace AccountDownloader.ViewModels
{
    public class UserProfileViewModel : ReactiveObject
    {
        public IUserProfile Profile { get; set; }

        public UserProfileViewModel(IUserProfile profile)
        {
            Profile = profile;
        }
    }
}
