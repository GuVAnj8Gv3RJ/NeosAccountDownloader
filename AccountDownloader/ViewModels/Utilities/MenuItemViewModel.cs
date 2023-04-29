
using System.Collections.Generic;
using System.Windows.Input;

namespace AccountDownloader.ViewModels
{
    //https://docs.avaloniaui.net/docs/controls/menu#dynamically-creating-menus
    public class MenuItemViewModel
    {
        public string? Header { get; set; }
        public ICommand? Command { get; set; }
        public object? CommandParameter { get; set; }
        public IList<MenuItemViewModel>? Items { get; set; }
    }
}
