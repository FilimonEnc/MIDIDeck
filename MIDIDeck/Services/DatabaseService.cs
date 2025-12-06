using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MIDIDeck.Data;
using MIDIDeck.Models;

namespace MIDIDeck.Services;

/// <summary>
/// Интерфейс для работы с базой данных
/// </summary>
public interface IDatabaseService : IDisposable
{
    Task<List<KeyBinding>> GetAllKeyBindingsAsync();
    Task<KeyBinding?> GetKeyBindingByMidiNoteAsync(int midiNote);
    Task<KeyBinding?> GetKeyBindingByIdAsync(int id);
    Task<KeyBinding> AddKeyBindingAsync(KeyBinding binding);
    Task UpdateKeyBindingAsync(KeyBinding binding);
    Task DeleteKeyBindingAsync(int id);
    
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllSettingsAsync();
}

/// <summary>
/// Сервис для работы с базой данных
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }

    public async Task<List<KeyBinding>> GetAllKeyBindingsAsync()
    {
        return await _context.KeyBindings.ToListAsync();
    }

    public async Task<KeyBinding?> GetKeyBindingByMidiNoteAsync(int midiNote)
    {
        return await _context.KeyBindings
            .FirstOrDefaultAsync(k => k.MidiNoteNumber == midiNote && k.IsEnabled);
    }

    public async Task<KeyBinding?> GetKeyBindingByIdAsync(int id)
    {
        return await _context.KeyBindings.FindAsync(id);
    }

    public async Task<KeyBinding> AddKeyBindingAsync(KeyBinding binding)
    {
        _context.KeyBindings.Add(binding);
        await _context.SaveChangesAsync();
        return binding;
    }

    public async Task UpdateKeyBindingAsync(KeyBinding binding)
    {
        _context.KeyBindings.Update(binding);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteKeyBindingAsync(int id)
    {
        var binding = await _context.KeyBindings.FindAsync(id);
        if (binding != null)
        {
            _context.KeyBindings.Remove(binding);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            setting = new AppSetting { Key = key, Value = value };
            _context.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        var settings = await _context.AppSettings.ToListAsync();
        return settings.ToDictionary(s => s.Key, s => s.Value);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
