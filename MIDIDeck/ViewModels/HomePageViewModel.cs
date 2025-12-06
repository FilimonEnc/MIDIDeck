using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MIDIDeck.Services;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

/// <summary>
/// ViewModel главной страницы
/// </summary>
public class HomePageViewModel : PageViewModelBase
{
    private readonly MidiService _midiService;
    private string _lastNote = "-";
    private string _lastAction = "Нет";
    private string _deviceStatus = "Не подключено";
    private int _selectedDeviceIndex = -1;

    public HomePageViewModel(MidiService midiService)
    {
        _midiService = midiService;
        Devices = new ObservableCollection<string>();
        
        ReloadDevicesCommand = ReactiveCommand.Create(ReloadDevices);
        ReloadDevices();
    }

    public ObservableCollection<string> Devices { get; }

    public int SelectedDeviceIndex
    {
        get => _selectedDeviceIndex;
        set
        {
            if (value == _selectedDeviceIndex) return;
            this.RaiseAndSetIfChanged(ref _selectedDeviceIndex, value);
            ConnectToDevice(value);
        }
    }

    public string LastNote
    {
        get => _lastNote;
        set => this.RaiseAndSetIfChanged(ref _lastNote, value);
    }

    public string LastAction
    {
        get => _lastAction;
        set => this.RaiseAndSetIfChanged(ref _lastAction, value);
    }

    public string DeviceStatus
    {
        get => _deviceStatus;
        set => this.RaiseAndSetIfChanged(ref _deviceStatus, value);
    }

    public ReactiveCommand<Unit, Unit> ReloadDevicesCommand { get; }

    public void ReloadDevices()
    {
        Devices.Clear();
        var names = MidiService.GetInputDevices();
        for (var i = 0; i < names.Count; i++)
            Devices.Add($"{i}: {names[i]}");

        if (Devices.Any() && (SelectedDeviceIndex < 0 || SelectedDeviceIndex >= Devices.Count))
            SelectedDeviceIndex = 0;
        else if (!Devices.Any())
        {
            SelectedDeviceIndex = -1;
            DeviceStatus = "Устройства не найдены";
        }
    }

    private void ConnectToDevice(int index)
    {
        if (index >= 0 && index < Devices.Count)
        {
            var ok = _midiService.OpenDevice(index);
            DeviceStatus = ok ? "Подключено" : "Ошибка подключения";
        }
        else
        {
            _midiService.CloseDevice();
            DeviceStatus = "Не подключено";
        }
    }

    public void UpdateNote(int noteNumber)
    {
        LastNote = noteNumber.ToString();
    }

    public void UpdateAction(string action)
    {
        LastAction = action;
    }
}
