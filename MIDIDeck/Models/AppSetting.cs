using System.ComponentModel.DataAnnotations;

namespace MIDIDeck.Models;

/// <summary>
/// Настройки приложения
/// </summary>
public class AppSetting
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Ключ настройки
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Значение настройки
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

