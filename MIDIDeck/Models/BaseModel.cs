using System;
using ReactiveUI;

namespace MIDIDeck.Models;

public class BaseModel : ReactiveObject
{
    private Guid _id = Guid.NewGuid();

    public Guid Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
}