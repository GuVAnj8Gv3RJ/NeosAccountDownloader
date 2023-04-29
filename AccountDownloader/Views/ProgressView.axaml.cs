using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AccountDownloader.ViewModels;

namespace AccountDownloader.Views;

public partial class ProgressView : ReactiveUserControl<ProgressViewModel>
{
    public ProgressView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

