using AccountDownloader.Views;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;


namespace AccountDownloader.Utilities
{
    public readonly struct MessageBoxRequest
    {
        public readonly string Message { get; }
        public readonly string? Title { get; } = null;

        public MessageBoxRequest(string message, string? title = null)
        {
            Message = message;
            Title = title;
        }
    }
    // Interactions are an MVVM way to interact between the view layer and the view model layer. They make it easier to swap things around for other platforms too.
    // https://www.reactiveui.net/docs/handbook/interactions/
    // These ones are global because lots of things need to use them.
    public class GlobalInteractions
    {
        public static readonly Interaction<MessageBoxRequest, Unit> ShowError = new Interaction<MessageBoxRequest, Unit>();
        public static readonly Interaction<MessageBoxRequest, Unit> ShowMessageBox = new Interaction<MessageBoxRequest, Unit>();
        public static readonly Interaction<MessageBoxRequest, InteractionResult<YesNo>> ShowYesNoBox = new Interaction<MessageBoxRequest, InteractionResult<YesNo>>();

        public static readonly Interaction<WindowClosingEventArgs, bool> OnMainWindowClose = new Interaction<WindowClosingEventArgs, bool>();

        public static readonly Interaction<string, Unit> OpenFolderLocation = new Interaction<string, Unit>();
        public static readonly Interaction<Unit, Unit> ShowAboutWindow = new Interaction<Unit, Unit>();
    }
}
