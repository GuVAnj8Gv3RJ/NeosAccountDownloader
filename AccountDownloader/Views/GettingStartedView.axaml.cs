using Avalonia.Controls;
using Avalonia.ReactiveUI;
using AccountDownloader.ViewModels;

namespace AccountDownloader.Views
{
    public partial class GettingStartedView : ReactiveUserControl<GettingStartedViewModel>
    {
        public GettingStartedView()
        {
            InitializeComponent();
        }
    }
}
