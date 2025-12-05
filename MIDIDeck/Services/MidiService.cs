using System;
using System.Collections.Generic;
using NAudio;
using NAudio.Midi;

namespace MIDIDeck.Services;

public class MidiService : IDisposable
{
    private MidiIn? _midiIn;

    public MidiService()
    {
        IsAvailable = false;
    }

    public bool IsAvailable { get; private set; }

    public void Dispose()
    {
        CloseDevice();
    }

    public event EventHandler<MidiInMessageEventArgs>? MessageReceived;

    public static IReadOnlyList<string> GetInputDevices()
    {
        var list = new List<string>();
        try
        {
            var count = MidiIn.NumberOfDevices;
            for (var i = 0; i < count; i++)
            {
                var caps = MidiIn.DeviceInfo(i);
                list.Add(caps.ProductName);
            }
        }
        catch
        {
            // если что-то пошло не так при опросе устройств — вернуть пустой список
        }

        return list;
    }

    public static int GetDeviceCount()
    {
        try
        {
            return MidiIn.NumberOfDevices;
        }
        catch
        {
            return 0;
        }
    }

    public bool OpenDevice(int index)
    {
        // Закрываем предыдущее (если было)
        CloseDevice();
        try
        {
            var count = GetDeviceCount();
            if (index < 0 || index >= count) return false;

            _midiIn = new MidiIn(index);
            _midiIn.MessageReceived += MidiIn_MessageReceived;
            _midiIn.ErrorReceived += MidiIn_ErrorReceived;
            _midiIn.Start();
            IsAvailable = true;
            return true;
        }
        catch (MmException)
        {
            // Неудача при открытии нативного устройства
            IsAvailable = false;
            _midiIn?.Dispose();
            _midiIn = null;
            return false;
        }
        catch
        {
            IsAvailable = false;
            _midiIn?.Dispose();
            _midiIn = null;
            return false;
        }
    }

    private void MidiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
    {
        MessageReceived?.Invoke(this, e);
    }

    private void MidiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        // Пробрасываем ошибки как сообщения, чтобы UI мог отреагировать
        MessageReceived?.Invoke(this, e);
    }

    public void CloseDevice()
    {
        if (_midiIn != null)
        {
            try
            {
                _midiIn.Stop();
            }
            catch
            {
            }

            try
            {
                _midiIn.MessageReceived -= MidiIn_MessageReceived;
            }
            catch
            {
            }

            try
            {
                _midiIn.ErrorReceived -= MidiIn_ErrorReceived;
            }
            catch
            {
            }

            try
            {
                _midiIn.Dispose();
            }
            catch
            {
            }

            _midiIn = null;
            IsAvailable = false;
        }
    }
}