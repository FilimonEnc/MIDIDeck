using System.Diagnostics;
using System.IO;
using MIDIDeck.Models;

namespace MIDIDeck.Services;

/// <summary>
/// Интерфейс сервиса запуска приложений
/// </summary>
public interface ILauncherService
{
    bool LaunchApplication(string path, string? arguments = null, bool runAsAdmin = false);
    bool OpenUrl(string url);
    bool RunCommand(string command);
    bool SimulateKeyPress(string keyCode);
    bool ExecuteBinding(KeyBinding binding);
}

/// <summary>
/// Сервис для запуска приложений и выполнения действий
/// </summary>
public class LauncherService : ILauncherService
{
    public bool LaunchApplication(string path, string? arguments = null, bool runAsAdmin = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            if (!string.IsNullOrWhiteSpace(arguments))
                startInfo.Arguments = arguments;

            if (runAsAdmin)
                startInfo.Verb = "runas";

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool OpenUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RunCommand(string command)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SimulateKeyPress(string keyCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyCode))
                return false;

            // Используем PowerShell для симуляции F13-F24
            var psCommand = $"Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.SendKeys]::SendWait('{{{keyCode}}}')";
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{psCommand}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ExecuteBinding(KeyBinding binding)
    {
        if (!binding.IsEnabled)
            return false;

        return binding.ActionType switch
        {
            BindingActionType.LaunchApplication => LaunchApplication(binding.ActionValue, binding.LaunchArguments, binding.RunAsAdmin),
            BindingActionType.OpenUrl => OpenUrl(binding.ActionValue),
            BindingActionType.RunCommand => RunCommand(binding.ActionValue),
            BindingActionType.PressKey => SimulateKeyPress(binding.ActionValue),
            _ => false
        };
    }
}

