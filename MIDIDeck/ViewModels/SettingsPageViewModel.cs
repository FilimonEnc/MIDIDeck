using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using MIDIDeck.Services;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

/// <summary>
/// ViewModel страницы настроек
/// </summary>
public class SettingsPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService _databaseService;
    
    private bool _startWithWindows;
    private bool _minimizeToTray;
    private bool _showNotifications = true;
    private int _lastSelectedDeviceIndex = -1;

    public SettingsPageViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        
        SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);
        ResetSettingsCommand = ReactiveCommand.Create(ResetSettings);

        _ = LoadSettingsAsync();
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => this.RaiseAndSetIfChanged(ref _startWithWindows, value);
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => this.RaiseAndSetIfChanged(ref _minimizeToTray, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => this.RaiseAndSetIfChanged(ref _showNotifications, value);
    }

    public int LastSelectedDeviceIndex
    {
        get => _lastSelectedDeviceIndex;
        set => this.RaiseAndSetIfChanged(ref _lastSelectedDeviceIndex, value);
    }

    public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetSettingsCommand { get; }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var startWithWindows = await _databaseService.GetSettingAsync("StartWithWindows");
            StartWithWindows = startWithWindows == "true";

            var minimizeToTray = await _databaseService.GetSettingAsync("MinimizeToTray");
            MinimizeToTray = minimizeToTray == "true";

            var showNotifications = await _databaseService.GetSettingAsync("ShowNotifications");
            ShowNotifications = showNotifications != "false";

            var deviceIndex = await _databaseService.GetSettingAsync("LastDeviceIndex");
            if (int.TryParse(deviceIndex, out var index))
                LastSelectedDeviceIndex = index;
        }
        catch
        {
            // Ignore errors on load
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            await _databaseService.SetSettingAsync("StartWithWindows", StartWithWindows.ToString().ToLower());
            await _databaseService.SetSettingAsync("MinimizeToTray", MinimizeToTray.ToString().ToLower());
            await _databaseService.SetSettingAsync("ShowNotifications", ShowNotifications.ToString().ToLower());
            await _databaseService.SetSettingAsync("LastDeviceIndex", LastSelectedDeviceIndex.ToString());

            ApplyAutoStart();
            
            MessageBoxShow("Успех", "Настройки сохранены");
        }
        catch (Exception ex)
        {
            MessageBoxShowError("Ошибка", $"Не удалось сохранить: {ex.Message}");
        }
    }

    private void ResetSettings()
    {
        StartWithWindows = false;
        MinimizeToTray = false;
        ShowNotifications = true;
        LastSelectedDeviceIndex = -1;
    }

    private void ApplyAutoStart()
    {
        try
        {
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutPath = Path.Combine(startupPath, "MIDIDeck.lnk");
            var exePath = Environment.ProcessPath;

            if (StartWithWindows && !string.IsNullOrEmpty(exePath))
            {
                if (!File.Exists(shortcutPath))
                {
                    var psCommand = $@"$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('{shortcutPath}'); $Shortcut.TargetPath = '{exePath}'; $Shortcut.Save()";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{psCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            else if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }
        catch
        {
            // Ignore autostart errors
        }
    }
}
