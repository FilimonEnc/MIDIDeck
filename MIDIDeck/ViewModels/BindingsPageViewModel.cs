using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MIDIDeck.Models;
using MIDIDeck.Services;
using MIDIDeck.Utils;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

/// <summary>
/// ViewModel страницы привязок клавиш
/// </summary>
public class BindingsPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private KeyBinding? _selectedBinding;
    private bool _isEditing;
    private bool _isListening;

    // Поля редактирования
    private string _editName = string.Empty;
    private int _editMidiNote;
    private BindingActionType _editActionType;
    private string _editActionValue = string.Empty;
    private string _editLaunchArguments = string.Empty;
    private bool _editRunAsAdmin;
    private bool _editIsEnabled = true;

    public BindingsPageViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Bindings = new ObservableCollection<KeyBinding>();
        
        ActionTypes = new ObservableCollection<BindingActionType>(Enum.GetValues<BindingActionType>());
        FunctionKeys = new ObservableCollection<string>(Enumerable.Range(13, 12).Select(i => $"F{i}"));

        AddBindingCommand = ReactiveCommand.Create(AddBinding);
        SaveBindingCommand = ReactiveCommand.CreateFromTask(SaveBindingAsync);
        DeleteBindingCommand = ReactiveCommand.CreateFromTask(DeleteBindingAsync);
        CancelEditCommand = ReactiveCommand.Create(CancelEdit);
        BrowseFileCommand = ReactiveCommand.CreateFromTask(BrowseFileAsync);
        StartListeningCommand = ReactiveCommand.Create(StartListening);

        _ = LoadBindingsAsync();
    }

    public ObservableCollection<KeyBinding> Bindings { get; }
    public ObservableCollection<BindingActionType> ActionTypes { get; }
    public ObservableCollection<string> FunctionKeys { get; }

    public KeyBinding? SelectedBinding
    {
        get => _selectedBinding;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedBinding, value);
            if (value != null)
            {
                LoadBindingForEdit(value);
                IsEditing = true;
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public bool IsListening
    {
        get => _isListening;
        set => this.RaiseAndSetIfChanged(ref _isListening, value);
    }

    #region Edit Properties

    public string EditName
    {
        get => _editName;
        set => this.RaiseAndSetIfChanged(ref _editName, value);
    }

    public int EditMidiNote
    {
        get => _editMidiNote;
        set => this.RaiseAndSetIfChanged(ref _editMidiNote, value);
    }

    public BindingActionType EditActionType
    {
        get => _editActionType;
        set => this.RaiseAndSetIfChanged(ref _editActionType, value);
    }

    public string EditActionValue
    {
        get => _editActionValue;
        set => this.RaiseAndSetIfChanged(ref _editActionValue, value);
    }

    public string EditLaunchArguments
    {
        get => _editLaunchArguments;
        set => this.RaiseAndSetIfChanged(ref _editLaunchArguments, value);
    }

    public bool EditRunAsAdmin
    {
        get => _editRunAsAdmin;
        set => this.RaiseAndSetIfChanged(ref _editRunAsAdmin, value);
    }

    public bool EditIsEnabled
    {
        get => _editIsEnabled;
        set => this.RaiseAndSetIfChanged(ref _editIsEnabled, value);
    }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> AddBindingCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveBindingCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteBindingCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }
    public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; }
    public ReactiveCommand<Unit, Unit> StartListeningCommand { get; }

    #endregion

    private async Task LoadBindingsAsync()
    {
        var bindings = await _databaseService.GetAllKeyBindingsAsync();
        Bindings.Clear();
        foreach (var binding in bindings)
            Bindings.Add(binding);
    }

    private void AddBinding()
    {
        _selectedBinding = null;
        ClearEditFields();
        IsEditing = true;
    }

    private void LoadBindingForEdit(KeyBinding binding)
    {
        EditName = binding.Name;
        EditMidiNote = binding.MidiNoteNumber;
        EditActionType = binding.ActionType;
        EditActionValue = binding.ActionValue;
        EditLaunchArguments = binding.LaunchArguments ?? string.Empty;
        EditRunAsAdmin = binding.RunAsAdmin;
        EditIsEnabled = binding.IsEnabled;
    }

    private async Task SaveBindingAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            MessageBoxShow("Ошибка", "Введите название привязки");
            return;
        }

        var binding = _selectedBinding ?? new KeyBinding();
        binding.Name = EditName;
        binding.MidiNoteNumber = EditMidiNote;
        binding.ActionType = EditActionType;
        binding.ActionValue = EditActionValue;
        binding.LaunchArguments = string.IsNullOrWhiteSpace(EditLaunchArguments) ? null : EditLaunchArguments;
        binding.RunAsAdmin = EditRunAsAdmin;
        binding.IsEnabled = EditIsEnabled;

        if (_selectedBinding == null)
        {
            await _databaseService.AddKeyBindingAsync(binding);
            Bindings.Add(binding);
        }
        else
        {
            await _databaseService.UpdateKeyBindingAsync(binding);
            var index = Bindings.IndexOf(_selectedBinding);
            if (index >= 0)
            {
                Bindings.RemoveAt(index);
                Bindings.Insert(index, binding);
            }
        }

        CancelEdit();
    }

    private async Task DeleteBindingAsync()
    {
        if (_selectedBinding == null) return;

        await _databaseService.DeleteKeyBindingAsync(_selectedBinding.Id);
        Bindings.Remove(_selectedBinding);
        CancelEdit();
    }

    private void CancelEdit()
    {
        _selectedBinding = null;
        IsEditing = false;
        IsListening = false;
        ClearEditFields();
    }

    private void ClearEditFields()
    {
        EditName = string.Empty;
        EditMidiNote = 0;
        EditActionType = BindingActionType.None;
        EditActionValue = string.Empty;
        EditLaunchArguments = string.Empty;
        EditRunAsAdmin = false;
        EditIsEnabled = true;
    }

    private async Task BrowseFileAsync()
    {
        var path = await FileDialogHelper.OpenExecutablePickerAsync();
        if (!string.IsNullOrEmpty(path))
            EditActionValue = path;
    }

    private void StartListening()
    {
        IsListening = true;
    }

    public void OnMidiNoteReceived(int noteNumber)
    {
        if (IsListening)
        {
            EditMidiNote = noteNumber;
            IsListening = false;
        }
    }

    public void SetFunctionKey(string key)
    {
        EditActionValue = key;
        EditActionType = BindingActionType.PressKey;
    }
}
