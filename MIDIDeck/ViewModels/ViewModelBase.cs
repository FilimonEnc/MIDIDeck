using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MIDIDeck.Views;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    // Унифицированный вызов уведомления о свойстве (если требуется)
    protected void OnPropertyChanged(string propertyName)
    {
        this.RaisePropertyChanged(propertyName);
    }

    // Показываем простое модальное окно сообщения (UI-поток)
    public static void MessageBoxShow(string title, string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var dlg = new MessageDialog();
            dlg.SetText(title, message);
            // Если есть активное окно, ставим владельца
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
                dlg.ShowDialog(desktop.MainWindow);
            else
                dlg.Show();
        });
    }

    public static void MessageBoxShowError(string title, string message)
    {
        MessageBoxShow(title, message);
    }
}