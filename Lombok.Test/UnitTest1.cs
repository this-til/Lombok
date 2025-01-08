
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

namespace Til.Lombok.Test {

    [ILombok]
    public partial class CDemo {

        [Get]
        [Set]
        public int a;

        [Get]
        [Set]
        public int b;

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
