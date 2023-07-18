using AccountDownloader.Properties;
using AccountDownloader.Services;
using ReactiveUI;

namespace AccountDownloader.ViewModels;

public class GroupsListItemViewModel: ReactiveObject
{
    private const byte MAX_TITLE_LENGTH = 60;

    public IGroup Group { get;}

    public string GroupTitleLabel
    {
        get
        {
            var title  = $"{(Group.IsAdmin ? $"{Resources.GroupsAdminIndicator} " : "")}{Group.Name}";

            if (title.Length > MAX_TITLE_LENGTH)
            {
                title = $"{title.Substring(0, MAX_TITLE_LENGTH)}...";
            }

            return title ;
        }
    }

    public string GroupByteSizeLabel => $"({Group.Storage.FormattedUsed})";

    public GroupsListItemViewModel(IGroup group)
    {
        Group = group;
    }

    public override string ToString()
    {
        return $"{GroupTitleLabel} {GroupByteSizeLabel}";
    }
}
