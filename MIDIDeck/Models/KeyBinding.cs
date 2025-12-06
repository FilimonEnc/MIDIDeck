using System.ComponentModel.DataAnnotations;

namespace MIDIDeck.Models;

/// <summary>
/// Тип действия при нажатии MIDI-клавиши
/// </summary>
public enum BindingActionType
{
    None = 0,
    LaunchApplication = 1,
    PressKey = 2,
    OpenUrl = 3,
    RunCommand = 4
}

/// <summary>
/// Привязка MIDI-клавиши к действию
/// </summary>
public class KeyBinding
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Номер ноты от MIDI-устройства (0-127)
    /// </summary>
    public int MidiNoteNumber { get; set; }
    
    /// <summary>
    /// Отображаемое имя привязки
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Тип действия
    /// </summary>
    public BindingActionType ActionType { get; set; } = BindingActionType.None;
    
    /// <summary>
    /// Значение действия (путь к приложению, URL, код клавиши F13-F24, команда)
    /// </summary>
    public string ActionValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Аргументы запуска (для приложений)
    /// </summary>
    public string? LaunchArguments { get; set; }
    
    /// <summary>
    /// Запускать от имени администратора
    /// </summary>
    public bool RunAsAdmin { get; set; }
    
    /// <summary>
    /// Привязка активна
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

