using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MIDIDeck.Services;
using NAudio.Midi;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly MidiService _midiService = new MidiService();

    public MainWindowViewModel()
    {
        Devices = new ObservableCollection<string>();
        ReloadDevices();

        SelectedDeviceIndex = Devices.Any() ? 0 : -1;

        if (SelectedDeviceIndex >= 0)
        {
            var ok = _midiService.OpenDevice(SelectedDeviceIndex);
            if (ok)
            {
                _midiService.MessageReceived += OnMidiMessage;
            }
        }

        ReloadDevicesCommand = ReactiveCommand.Create(ReloadDevices);
    }

    public void ReloadDevices()
    {
        Devices.Clear();
        // Формируем список в виде "index: name" для более однозначного отображения
        var names = MidiService.GetInputDevices();
        for (int i = 0; i < names.Count; i++)
        {
            Devices.Add($"{i}: {names[i]}");
        }

        // Если есть устройства и текущий SelectedDeviceIndex вне диапазона — выставим 0
        if (Devices.Any() && (SelectedDeviceIndex < 0 || SelectedDeviceIndex >= Devices.Count))
        {
            SelectedDeviceIndex = 0;
        }
        else if (!Devices.Any())
        {
            SelectedDeviceIndex = -1;
            _midiService.CloseDevice();
        }
    }

    private void OnMidiMessage(object? sender, MidiInMessageEventArgs e)
    {
        if (e.MidiEvent is NoteEvent noteEvent)
        {
            if (noteEvent.CommandCode == MidiCommandCode.NoteOn && noteEvent is NoteOnEvent noteOn && noteOn.Velocity > 0)
            {
                NoteNumber = noteOn.NoteNumber.ToString();
            }
        }
    }

    private int _selectedDeviceIndex;

    public int SelectedDeviceIndex
    {
        get => _selectedDeviceIndex;
        set
        {
            if (value == _selectedDeviceIndex) return;
            this.RaiseAndSetIfChanged(ref _selectedDeviceIndex, value);

            // При изменении индекса пытаемся открыть новое устройство
            if (value >= 0 && value < Devices.Count)
            {
                var ok = _midiService.OpenDevice(value);
                if (ok)
                {
                    // гарантируем одну подписку
                    _midiService.MessageReceived -= OnMidiMessage;
                    _midiService.MessageReceived += OnMidiMessage;
                }
                else
                {
                    // если не удалось открыть — закроем сервис
                    _midiService.CloseDevice();
                }
            }
            else
            {
                _midiService.CloseDevice();
            }
        }
    }

    public ObservableCollection<string> Devices { get; }

    private string _noteNumber = string.Empty;

    public string NoteNumber
    {
        get => _noteNumber;
        set => this.RaiseAndSetIfChanged(ref _noteNumber, value);
    }

    public ReactiveCommand<Unit, Unit> ReloadDevicesCommand { get; }

    public void Dispose()
    {
        _midiService.MessageReceived -= OnMidiMessage;
        _midiService.Dispose();
    }
}