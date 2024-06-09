using Til.Lombok;
using Xunit;

namespace Lombok.Test;

[ILombok]
public partial class Data {
    [Get] private int a, b, c, d;

    [Get] private static int staticA;

    [Get] public List<int> list;
}

public class Test {
    [Fact]
    public static void a() {
    }
}