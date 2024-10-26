using System;
using System.Collections.Generic;

namespace Til.Lombok.Test {

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
        protected int id;

        [Get(link = true)]
        [Set(link = true)]
        [Open(link = true, noNull = true)]
        protected int age;

        [Get]
        [Set]
        [Open]
        protected double points;

        [Get]
        [Set]
        [Open]
        protected double balance;

        [Get]
        [Set]
        [Add]
        [Index]
        [Contain]
        [Count]
        [Remove]
        [For]
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
            protected int id;

        }

    }

    /*
    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo1 {

        [Get]
        [Set]
        public int aInt;

        [Get]
        [Set]
        public float aFloat;

        [Get]
        [Set]
        private IList<int> aIntList = new List<int>();

        [Get]
        [Set]
        public IDictionary<string, string> dictionary = new Dictionary<string, string>();

    }

    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo2 {

        [Set(link = true)]
        public float aFloat;

        [Set(noNull = true)]
        private IList<int> aIntList = new List<int>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze] // 添加冻结支持代码
    public partial class Demo3 {

        [Set(freezeTag = "a")]
        public int aInt;

        [Set(freezeTag = "b")]
        public float aFloat;

        [Set(freezeTag = "c")]
        private IList<int> aIntList = new List<int>();

        [Set(freezeTag = "d")]
        public IDictionary<string, string> dictionary = new Dictionary<string, string>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo4 {

        [Add]
        public IList<int> list = new List<int>();

        [Add(freezeTag = "a", link = true, noNull = true)]
        public IList<IList<string>> twoList = new List<IList<string>>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo5 {

        [Contain]
        public IList<int> list = new List<int>();

        [Contain(freezeTag = "a")]
        public IList<IList<string>> twoList = new List<IList<string>>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo6 {

        [Remove]
        public IList<int> list = new List<int>();

        [Remove(freezeTag = "a")]
        public IList<IList<string>> twoList = new List<IList<string>>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo7 {

        [Index]
        public IList<int> list = new List<int>();

        [Index(freezeTag = "a")]
        public IList<IList<string>> twoList = new List<IList<string>>();

    }

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo8 {

        [For]
        public IList<int> list = new List<int>();

        [For(freezeTag = "a", useYield = true)]
        public IList<IList<string>> twoList = new List<IList<string>>();

    }

    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo9 {

        [Add(type = "int")]
        public IList<object> list = new List<object>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo10 {

        [Put]
        private Dictionary<string, string> map = new Dictionary<string, string>();

        [Put(freezeTag = "a", noNull = true, link = true)]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo11 {

        [Put(keyType = "int", valueType = "string")]
        private Dictionary<object, object> map = new Dictionary<object, object>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo12 {

        [ContainKey]
        [ContainValue]
        private Dictionary<string, string> map = new Dictionary<string, string>();

        [ContainKey(freezeTag = "a")]
        [ContainValue(freezeTag = "a")]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo13 {

        [ContainKey]
        private Dictionary<string, string> map = new Dictionary<string, string>();

        [ContainKey(freezeTag = "a")]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo14 {

        [ForKey]
        [ForValue]
        [ForAll]
        private Dictionary<string, string> map = new Dictionary<string, string>();

        [ForKey(freezeTag = "aa", useYield = true)]
        [ForValue(freezeTag = "aa", useYield = true)]
        [ForAll(freezeTag = "aa", useYield = true)]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();

    }

    [ILombok]
    [IFreeze]
    public partial class Demo15 {

        [Get(freezeTag = CS.aabb)]
        public int a;

    }

    [ILombok]
    [IFreeze]
    public partial class Demo16<T> {

        [Get]
        [Set]
        public T t;

    }

    [ILombok]
    [IFreeze]
    public partial class Demo16<T, V> {

        [Get]
        [Set]
        public T t;

        [Get]
        [Set]
        public V v;

    }

    [ILombok]
    public partial class Demo17 {

        protected bool a;

    }

    [ILombok]
    public partial class Demo18 {

        [Open]
        public object a;

    }

    [ILombok]
    public partial class demo19 {

        [Set(updateField = true)]
        public object upTest;

        protected void updateUpTest(object old, object @new) {
        }

    }

    [ILombok]
    public partial class demo20 {

        [Set(updateField = true)]
        [Add(updateField = true)]
        public List<object> upTest;

        protected void updateUpTest(object old, object @new) {
        }

    }

    /*
    [ILombok]
    [ISelf]
    public partial class Demo19 {
    }

    [ILombok]
    [ISelf]
    public partial class Demo19_2 {
    }
    #1#




    [IPartial(
        model = @"
namespace {namespace}{{
    using System;
    using Til.Lombok;
    public partial class {type} {{
        protected static {type} instance;
        public static {type} getInstance() {{
            instance ??= new {type}();
            return instance;
        }}
    }}
}}"
    )]
    [IPartial(
        model = @"
namespace {namespace}{{
    using System;
    using Til.Lombok;
    public partial class {type} {{
    }}
}}"
    )]
    public partial class Demo22 {

    }

    [ILombok]
    public partial class Demo23 {

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public int a, _a, _aa = 0;

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public int b { get; set; }

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public int c;

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public object aa;

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public List<string> bb = new List<string>();

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public KeyValuePair<object, object> kv = new KeyValuePair<object, object>();

    }


    /*
    [ILombok]
    [IFreeze]
    public partial class Demo16 {
        [Get(freezeTag = CS.aabb)] public int a;
    }
    #1#

    public class CS {

        public const string aabb = "aabb";

    }

    /*
    [ILombok]
    [IFreeze]
    public partial class Data {
        /*
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

        [Get] [Index] [Set] [Add] [Remove] [Contain] [For]
        public List<int> list;

        [Get] [Index] [Set] [Add] [Remove] [Contain] [For(useYield = true)]
        public List<List<int>> twoList;
        #2#

        [Get] [Set] [Put] [MapGet] [RemoveKey] [ContainKey] [ContainValue] [ForKey] [ForValue] [ForAll]
        public Dictionary<string, string> map = new Dictionary<string, string>();
    }
    #1#
    /*
    [ILombok]
    [IFreeze]
    public partial class GT<T> {
        [Get] [Set(link = true)] private int a;
    }
    #1#

    public partial class Model {

        protected HashSet<string> _frozen = new HashSet<string>();

        public bool isFrozen(string tag) => _frozen.Contains(tag);

        public void frozen(string tag) => _frozen.Add(tag);

        protected void unFrozen(string tag) => _frozen.Remove(tag);

        public void validateNonFrozen(string tag) {
            if (isFrozen(tag)) {
                throw new InvalidOperationException("Cannot modify frozen property");
            }
        }

    }*/

}
