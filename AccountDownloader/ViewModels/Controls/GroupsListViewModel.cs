using AccountDownloader.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System;
using System.Linq;
using DynamicData.Binding;
using DynamicData;
using DynamicData.Aggregation;

namespace AccountDownloader.ViewModels
{
    public class GroupsListViewModel: ReactiveObject
    {
        public ObservableCollection<GroupsListItemViewModel> GroupViewModels { get;}

        public IEnumerable<IGroup> Groups => GroupViewModels.Select(v => v.Group);

        [Reactive]
        public bool HasGroups { get; private set; }

        [ObservableAsProperty]
        public long RequiredBytes { get;} = 0;

        public GroupsListViewModel(IEnumerable<IGroup>? groups = null)
        {
            GroupViewModels = new ObservableCollection<GroupsListItemViewModel>();

            // If the user is any groups
            this.WhenAnyValue(x => x.GroupViewModels.Count).Subscribe(x => HasGroups = x > 0);

            if (groups != null)
                AddGroups(groups);

            // When any group updates its "ShouldDownload" property, re-calculate the required bytes for the group list
            GroupViewModels
                .ToObservableChangeSet(x => x.Group.Id)
                .AutoRefresh(x => x.Group.ShouldDownload)
                .Filter(x => x.Group.ShouldDownload)
                .Sum(x => x.Group.RequiredBytes)
                .ToPropertyEx(this, x => x.RequiredBytes);
        }

        public void AddGroups(IEnumerable<IGroup> groups)
        {
            foreach (IGroup group in groups)
            {
                GroupViewModels.Add(new GroupsListItemViewModel(group));
            }
        }

        public IEnumerable<IGroup> GetSelectedGroups() {
            return Groups.Where(x => x.ShouldDownload);
        }

        public IEnumerable<string> GetSelectedGroupIds()
        {
            return GetSelectedGroups().Select(x => x.Id);
        }

        public override string ToString()
        {
            return Groups.Select(g => $"{g.Name}: {g.ShouldDownload}").Aggregate(string.Empty,(last, next) => last + " " + next);
        }
    }
}
