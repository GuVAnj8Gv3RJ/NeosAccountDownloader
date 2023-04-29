using System;
using AccountDownloader.ViewModels;
using ReactiveUI;
using Avalonia.Controls.Templates;
using Avalonia.Controls;

namespace AccountDownloader;


[StaticViewLocator]
public partial class AppViewLocator : IViewLocator, IDataTemplate
{
    public AppViewLocator() {
    }

    // https://www.reactiveui.net/docs/handbook/view-location/
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel is null) return null;

        var type = viewModel.GetType();
        if (s_views.TryGetValue(type, out var func))
        {
            return (IViewFor?) func.Invoke();
        }
        throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data) => data is ViewModelBase;

    public Control? Build(object? param)
    {
        if (param is null) return null;

        var type = param.GetType();
        if (s_views.TryGetValue(type, out var func))
        {
            return func.Invoke();
        }
        throw new Exception($"Unable to create view for type: {type}");
    }
}
