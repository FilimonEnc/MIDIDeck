using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MIDIDeck.ViewModels;
using MIDIDeck.Views;

namespace MIDIDeck;

public class App : Application
{
    private MainWindowViewModel? _mainViewModel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainViewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            // Подписываемся на событие завершения приложения, чтобы освободить ресурсы
            desktop.Exit += (_, __) => _mainViewModel?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}