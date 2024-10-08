﻿using System;
using System.Collections.Generic;

namespace Til.Lombok.Test {

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

        [Is]
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
    */

    [ILombok]
    [ISelf(instantiation = "Demo20.of()")]
    public partial class Demo20 {

        public static Demo20 of() => new Demo20();

    }

    [ILombok]
    [IPack]
    public partial class Demo21 {

    }

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
        public List<string> bb;

        [ToStringField]
        [EqualsField]
        [HashCodeField]
        public KeyValuePair<object, object> kv;

    }

    [ILombok]
    public abstract partial class Demo24 {

        [Get(accessLevel = AccessLevel.Private)]
        public int a;

        [Get(accessLevel = AccessLevel.ProtectedInternal)]
        public int b;

        [Get(methodType = MethodType.Partial)]
        public int c;

        [Get(methodType = MethodType.Abstract)]
        public int d;

        public partial int getC() {
            return d;
        }

    }

    /*
    [ILombok]
    [IFreeze]
    public partial class Demo16 {
        [Get(freezeTag = CS.aabb)] public int a;
    }
    */

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
        #1#

        [Get] [Set] [Put] [MapGet] [RemoveKey] [ContainKey] [ContainValue] [ForKey] [ForValue] [ForAll]
        public Dictionary<string, string> map = new Dictionary<string, string>();
    }
    */
    /*
    [ILombok]
    [IFreeze]
    public partial class GT<T> {
        [Get] [Set(link = true)] private int a;
    }
    */

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

}
