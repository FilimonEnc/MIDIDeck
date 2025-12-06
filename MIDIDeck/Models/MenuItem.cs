using System.Reactive;
using ReactiveUI;

namespace MIDIDeck.ViewModels;

public class MenuItem(string text, ReactiveCommand<Unit, Unit> command)
{
    public string Text { get; } = text;
    public ReactiveCommand<Unit, Unit> Command { get; } = command;
}