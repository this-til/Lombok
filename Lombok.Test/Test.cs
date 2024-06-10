using System.Diagnostics.CodeAnalysis;
using Til.Lombok;
using Xunit;

namespace Lombok.Test;

[ILombok]
[IFreeze]
public partial class Data {
    [Get] private int a;
    [Set] private int b;
    [Get] [Set(link = true)] private int c;
    [Get] [Set(freezeTag = "aabb")] private int d;

    [Get] [Set(link = true, freezeTag = "aabb")]
    private int e;

    [Get] [Set] private int x { get; set; }

    [Get]
    [Set(link = true, freezeTag = "some time")]
    private int y { get; set; }

    [Get] [Index] [Set] [Add] [Remove] [Contain]
    public List<int> list;

    [Get] [Index] [Set] [Add] [Remove] [Contain]
    public List<List<int>> twoList;

    public int aa(int i) {
        return list[i];
    }
}

[ILombok]
[IFreeze]
public partial class GT<T> {
    [Get] [Set(link = true)] private int a;
}

public class Test {
    [Fact]
    public static void a() {
        Data data = new Data();
        data.addIntInList(5641230);
    }
}

public partial class Model {
    protected Dictionary<string, bool> _frozen = new Dictionary<string, bool>();

    public bool isFrozen(string tag) {
        if (_frozen.ContainsKey(tag)) {
            return _frozen[tag];
        }
        _frozen.Add(tag, false);
        return false;
    }

    public void frozen(string tag) {
        if (_frozen.ContainsKey(tag)) {
            _frozen[tag] = true;
            return;
        }
        _frozen.Add(tag, true);
    }

    public void validateNonFrozen(string tag) {
        if (_frozen.ContainsKey(tag) && _frozen[tag]) {
            throw new InvalidOperationException("Cannot modify frozen property");
        }
    }
}