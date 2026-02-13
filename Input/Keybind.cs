using System.Collections.Generic;
using System.Linq;

namespace Cornifer.Input;

public class Keybind {
    public Keybind(string name, params KeybindInput[] defaults) : this(name, new[] { defaults }) {
    }

    public Keybind(string name, IEnumerable<IEnumerable<KeybindInput>> defaults) {
        Name = name;
        foreach (var d in defaults) Inputs.Add(new ComboInput(d.ToList()));
    }

    public string Name { get; }
    public List<ComboInput> Inputs { get; } = new();

    public KeybindState State => Inputs.Select(x => x.State).DefaultIfEmpty(KeybindState.Released).Max();

    public bool Pressed => State == KeybindState.Pressed;
    public bool JustPressed => State == KeybindState.JustPressed;
    public bool JustReleased => State == KeybindState.JustReleased;
    public bool Released => State == KeybindState.Released;
}

public class ComboInput(List<KeybindInput> inputs) : KeybindInput {
    public List<KeybindInput> Inputs { get; } = inputs;

    // 存储包含当前组合键的更长组合键（防止 Ctrl 被 Ctrl+S 触发）
    public List<ComboInput> EncapsulatingCombos { get; } = new();

    public override bool CurrentState =>
        Inputs.All(x => x.CurrentState) && !EncapsulatingCombos.Any(x => x.CurrentState);

    public override bool PrevState => Inputs.All(x => x.PrevState) && !EncapsulatingCombos.Any(x => x.PrevState);
    public override string KeyName => Inputs.Count == 0 ? "None" : string.Join(" + ", Inputs.Select(ki => ki.KeyName));

    public bool ComboEncapsulates(ComboInput other) {
        if (Inputs.Count <= other.Inputs.Count) return false;
        return other.Inputs.All(o => Inputs.Any(i => i.InputEquality(o)));
    }

    public override bool InputEquality(KeybindInput other) {
        if (other is not ComboInput combo || combo.Inputs.Count != Inputs.Count) return false;
        return Inputs.All(i => combo.Inputs.Any(ci => ci.InputEquality(i)));
    }
}