using Til.Lombok.Unity;
/*
namespace Til.Lombok.Test {

    [ILombok]
    [NetworkSerializationClass]
    public partial class V3 {

        [NetworkSerializationField]
        protected double x;

        [NetworkSerializationField]
        protected double y;

        [NetworkSerializationField]
        protected double z;

    }

}
*/
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Til.Lombok.Test {

    [ILombok]
    public partial class V3 {

        [ToStringField]
        [HashCodeField]
        [EqualsField]
        [Get]
        [Set]
        public double x;

        [ToStringField]
        [HashCodeField]
        [EqualsField]
        [Get]
        [Set]
        public double y;

        [ToStringField]
        [HashCodeField]
        [EqualsField]
        [Get]
        [Set]
        public double z;

    }

    /*
    [ILombok]
    [NetworkSerializationClass(hasBase = true)]
    public partial class V6 : V3 {

        [Get]
        [Set]
        [NetworkSerializationField]
        public double a;

        [Get]
        [Set]
        [NetworkSerializationField]
        public double b;

        [Get]
        [Set]
        [NetworkSerializationField]
        public double c;

    }
    */

    [ILombok]
    public partial class A {

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int a;

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int b;

    }

    [ILombok]
    [ToStringClass(hasBase = true)]
    [HashCodeClass(hasBase = true)]
    [EqualsClass(hasBase = true)]
    public partial class B : A {

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int c;

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public AA a1 = new AA();

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public AA a2 = new AA();

    }

    [ILombok]
    public partial class AA {

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int x;

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int y;

        [Get]
        [Set]
        [ToStringField]
        [HashCodeField]
        [EqualsField]
        public int z;

    }

    public class Tast {

        private readonly ITestOutputHelper _testOutputHelper;

        public Tast(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void tast() {
            B b = new B();
            _testOutputHelper.WriteLine(b.ToString());
        }

    }

    [ILombok]
    [IFreeze]
    public partial class Demo {

        [Get]
        [Get(customName = "getIndex")]
        [Get(customPrefix = "getter")]
        [Get(customType = "object", customSuffix = "AsObject")]
        [Set]
        [Set(accessLevel = AccessLevel.Protected, customSuffix = "Internal", noNull = true)]
        [Open]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected int id;

        [Get(link = true)]
        [Set(link = true, freezeTag = nameof(Demo.dictionary))]
        [Open(link = true, noNull = true)]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected int age;

        [Get]
        [Set]
        [Open]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected double points;

        [Get]
        [Set]
        [Open]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected double balance;

        [Get]
        [Set]
        [Add]
        [Index]
        [Contain]
        [Count]
        [Remove]
        [For]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected List<int> aIntList = new List<int>();

        [Get]
        [Set]
        [MapGet]
        [ContainKey]
        [ContainValue]
        [RemoveKey]
        [RemoveValue]
        [ForAll]
        [ForKey]
        [ForValue]
        [ToStringField]
        [EqualsField]
        [HashCodeField]
        protected Dictionary<string, string> dictionary = new Dictionary<string, string>();

        [ILombok]
        [IPartial
        (
            model = @"
protected static {type} instance;

public static {type} getInstance() {{
    instance ??= new {type}();
    return instance;
}}
"
        )]
        [IPartial
        (
            model = @"
namespace {namespace}{{
    public static class {type}EE {{
        private static Demo.{type} instance;

        public static Demo.{type} getInstance() {{
            instance ??= new Demo.{type}();
            return instance;
        }}
    }}
}}
",
            partialPos = PartialPos.Compilation
        )]
        [IPartial
        (
            model = @"
public static class {type}EEE {{
    private static Demo.{type} instance;

    public static Demo.{type} getInstance() {{
        instance ??= new Demo.{type}();
        return instance;
    }}
}}
",
            partialPos = PartialPos.Namespace
        )]
        public partial class SonDemo {

            [Get]
            [Get(customName = "getIndex")]
            [Get(customPrefix = "getter")]
            [Get(customType = "object", customSuffix = "AsObject")]
            [Set]
            [Set(accessLevel = AccessLevel.Protected, customSuffix = "Internal", noNull = true)]
            [Open]
            [ToStringField]
            [EqualsField]
            [HashCodeField]
            protected int id;

        }

    }

}