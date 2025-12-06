using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MIDIDeck.Services;
using NAudio.Midi;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly MidiService _midiService;
    private readonly IDatabaseService _databaseService;
    private readonly ILauncherService _launcherService;
    
    private PageViewModelBase _currentPage = null!;
    private readonly HomePageViewModel _homePage;
    private readonly BindingsPageViewModel _bindingsPage;
    private readonly SettingsPageViewModel _settingsPage;

    public MainWindowViewModel()
    {
        // Инициализация сервисов
        _midiService = new MidiService();
        _databaseService = new DatabaseService();
        _launcherService = new LauncherService();

        // Инициализация страниц
        _homePage = new HomePageViewModel(_midiService);
        _bindingsPage = new BindingsPageViewModel(_databaseService);
        _settingsPage = new SettingsPageViewModel(_databaseService);

        // Начальная страница
        CurrentPage = _homePage;

        // Создание пунктов меню с навигацией
        MenuItems =
        [
            new("🏠 Главная", ReactiveCommand.Create(() => NavigateTo(_homePage))),
            new("🎹 Привязки", ReactiveCommand.Create(() => NavigateTo(_bindingsPage))),
            new("⚙️ Настройки", ReactiveCommand.Create(() => NavigateTo(_settingsPage)))
        ];

        // Подписка на MIDI-события
        _midiService.MessageReceived += OnMidiMessage;

        // Загрузка последнего выбранного устройства
        _ = LoadLastDeviceAsync();
    }

    public ObservableCollection<MenuItem> MenuItems { get; }

    public PageViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    private void NavigateTo(PageViewModelBase page)
    {
        CurrentPage = page;
    }

    private async Task LoadLastDeviceAsync()
    {
        var deviceIndex = await _databaseService.GetSettingAsync("LastDeviceIndex");
        if (int.TryParse(deviceIndex, out var index) && index >= 0)
        {
            _homePage.SelectedDeviceIndex = index;
        }
    }

    private async void OnMidiMessage(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            if (e.MidiEvent is not NoteOnEvent { Velocity: > 0 } noteOn) return;
            var noteNumber = noteOn.NoteNumber;
            
            // Обновляем UI на главной странице
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _homePage.UpdateNote(noteNumber);
            });

            // Если на странице привязок слушаем ноту
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _bindingsPage.OnMidiNoteReceived(noteNumber);
            });

            // Ищем и выполняем привязку
            var binding = await _databaseService.GetKeyBindingByMidiNoteAsync(noteNumber);
            if (binding != null)
            {
                var success = _launcherService.ExecuteBinding(binding);
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _homePage.UpdateAction(success ? binding.Name : $"Ошибка: {binding.Name}");
                });
            }
        }
        catch (Exception exception)
        {
            throw new Exception("Ошибка обработки MIDI-сообщения", exception);
        }
    }

    public void Dispose()
    {
        _midiService.MessageReceived -= OnMidiMessage;
        _midiService.Dispose();
        _databaseService.Dispose();
    }
}