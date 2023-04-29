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
        public ObservableCollection<IGroup> Groups { get;}

        [Reactive]
        public bool HasGroups { get; private set; }

        [ObservableAsProperty]
        public long RequiredBytes { get;} = 0;

        public GroupsListViewModel(IEnumerable<IGroup>? groups = null)
        {
            Groups = new ObservableCollection<IGroup>();

            // If the user is any groups
            this.WhenAnyValue(x => x.Groups.Count).Subscribe(x => HasGroups = x > 0);

            if (groups != null)
                AddGroups(groups);

            // When any group updates its "ShouldDownload" property, re-calculate the required bytes for the group list
            Groups
                .ToObservableChangeSet(x => x.Id)
                .AutoRefresh(x => x.ShouldDownload)
                .Filter(x => x.ShouldDownload)
                .Sum(x => x.RequiredBytes)
                .ToPropertyEx(this, x => x.RequiredBytes);
        }

        public void AddGroups(IEnumerable<IGroup> groups)
        {
            foreach (IGroup group in groups)
            {
                Groups.Add(group);
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
