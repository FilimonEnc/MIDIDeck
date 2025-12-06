using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace MIDIDeck.Utils;

/// <summary>
/// Утилита для работы с диалогами выбора файлов
/// </summary>
public static class FileDialogHelper
{
    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }

    /// <summary>
    /// Открыть диалог выбора файла
    /// </summary>
    public static async Task<string?> OpenFilePickerAsync(string title = "Выберите файл", FilePickerFileType[]? filters = null)
    {
        var window = GetMainWindow();
        if (window == null) return null;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters ?? new[]
            {
                new FilePickerFileType("Все файлы") { Patterns = new[] { "*.*" } }
            }
        };

        var result = await window.StorageProvider.OpenFilePickerAsync(options);
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    /// <summary>
    /// Открыть диалог выбора исполняемого файла
    /// </summary>
    public static async Task<string?> OpenExecutablePickerAsync()
    {
        var filters = new[]
        {
            new FilePickerFileType("Исполняемые файлы") { Patterns = new[] { "*.exe", "*.bat", "*.cmd", "*.lnk" } },
            new FilePickerFileType("Все файлы") { Patterns = new[] { "*.*" } }
        };

        return await OpenFilePickerAsync("Выберите приложение", filters);
    }

    /// <summary>
    /// Открыть диалог выбора папки
    /// </summary>
    public static async Task<string?> OpenFolderPickerAsync(string title = "Выберите папку")
    {
        var window = GetMainWindow();
        if (window == null) return null;

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        var result = await window.StorageProvider.OpenFolderPickerAsync(options);
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }
}
