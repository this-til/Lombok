﻿# Lombok

## [Get] [Set]

源:

    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo1 {
        [Get] [Set] public int aInt;
        [Get] [Set] public float aFloat;
        [Get] [Set] private IList<int> aIntList = new List<int>();
        [Get] [Set] public IDictionary<string, string> dictionary = new Dictionary<string, string>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo1
    {
        public int getAInt()
        {
            return this.aInt;
        }
        public float getAFloat()
        {
            return this.aFloat;
        }
        public IList<int> getAIntList()
        {
            return this.aIntList;
        }
        public IDictionary<string, string> getDictionary()
        {
            return this.dictionary;
        }
    }
    
    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo1
    {
        public void setAInt(int aInt)
        {
            this.aInt = aInt;
        }
        public void setAFloat(float aFloat)
        {
            this.aFloat = aFloat;
        }
        public void setAIntList(IList<int> aIntList)
        {
            this.aIntList = aIntList;
        }
        public void setDictionary(IDictionary<string, string> dictionary)
        {
            this.dictionary = dictionary;
        }
    }

## link noNull

设置属性 link 可以生成链式方法

设置属性 noNull 可以生成非空判断

源:

    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo2 {
        [Set(link = true)] public float aFloat;
        [Set(noNull = true)] private IList<int> aIntList = new List<int>();
    }

生成:

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo2
    {
        public Demo2 setAFloat(float aFloat)
        {
            this.aFloat = aFloat;
            return this;
        }
        public void setAIntList(IList<int> aIntList)
        {
            Util.noNull(aIntList);
            this.aIntList = aIntList;
        }
    }

## [IFreeze]

源:

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze] // 添加冻结支持代码
    public partial class Demo3 {
        [Set(freezeTag = "a")] public int aInt;
        [Set(freezeTag = "b")] public float aFloat;
        [Set(freezeTag = "c")] private IList<int> aIntList = new List<int>();
        [Set(freezeTag = "d")] public IDictionary<string, string> dictionary = new Dictionary<string, string>();
    }

生成：

    using System.Collections.Generic;
    namespace Lombok.Test {
        public partial class Demo3 {
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
    
    
    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo3
    {
        public void setAInt(int aInt)
        {
            this.validateNonFrozen("a");
            this.aInt = aInt;
        }
        public void setAFloat(float aFloat)
        {
            this.validateNonFrozen("b");
            this.aFloat = aFloat;
        }
        public void setAIntList(IList<int> aIntList)
        {
            this.validateNonFrozen("c");
            this.aIntList = aIntList;
        }
        public void setDictionary(IDictionary<string, string> dictionary)
        {
            this.validateNonFrozen("d");
            this.dictionary = dictionary;
        }
    }

## list [add]

源:

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo4 {
        [Add] public IList<int> list = new List<int>();
        [Add(freezeTag = "a", link = true, noNull = true)]
        public IList<IList<string>> twoList = new List<IList<string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo4
    {
        public void addInList(int aInt)
        {
            this.list.Add(aInt);
        }
        public Demo4 addInTwoList(IList<string> aIList)
        {
            this.validateNonFrozen("a");
            Util.noNull(twoList);
            this.twoList.Add(aIList);
            return this;
        }
    }

## list 可自定义类型

源:

    [ILombok] // 引入注释才能被扫描到类
    public partial class Demo9 {
        [Add(type = "int")] public IList<object> list = new List<object>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo9
    {
        public void addInList(int aInt)
        {
            this.list.Add(aInt);
        }
    }

# list [Contain]

源:

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo5 {
        [Contain] public IList<int> list = new List<int>();
        [Contain(freezeTag = "a")]
        public IList<IList<string>> twoList = new List<IList<string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;

    namespace Lombok.Test;
    #nullable enable
    public partial class Demo5
    {
    public bool contaInList(int aInt)
    {
    return this.list.Contains(aInt);
    }
    
        public bool contaInTwoList(IList<string> aIList)
        {
            this.validateNonFrozen("a");
            return this.twoList.Contains(aIList);
        }
    }

# list [Remove]

源:

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo6 {
    [Remove] public IList<int> list = new List<int>();
        [Remove(freezeTag = "a")]
        public IList<IList<string>> twoList = new List<IList<string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo6  
    {
        public void removeInList(int aInt)
        {
            this.list.Remove(aInt);
        }
    
        public void removeInTwoList(IList<string> aIList)
        {
            this.validateNonFrozen("a");
            this.twoList.Remove(aIList);
        }
    }

# list [Index]

源:：

    [ILombok] // 引入注释才能被扫描到类
    [IFreeze]
    public partial class Demo7 {
        [Index] public IList<int> list = new List<int>(); 
        [Index(freezeTag = "a")] public IList<IList<string>> twoList = new List<IList<string>>();
    }

生成:

// <auto-generated/>
using System.Diagnostics.CodeAnalysis;
using Til.Lombok;
using Xunit;

namespace Lombok.Test;
#nullable enable
public partial class Demo7
{
public int indexInList(int i)
{
return list[i];
}

    public IList<string> indexInTwoList(int i)
    {
        this.validateNonFrozen("a");
        return twoList[i];
    }

}

# list [For]

源:

[ILombok] // 引入注释才能被扫描到类
[IFreeze]
public partial class Demo8 {
[For] public IList<int> list = new List<int>();

    [For(freezeTag = "a", useYield = true)] 
    public IList<IList<string>> twoList = new List<IList<string>>();

}

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;

    namespace Lombok.Test;
    #nullable enable
    public partial class Demo8
    {
        public IEnumerable<int> forList()
        {
            return this.list;
        }
    
        public IEnumerable<IList<string>> forTwoList()
        {
            this.validateNonFrozen("a");
            foreach (var i in this.twoList)
            {
                yield return i;
            }
        }
    }

# map [Put]

源:

    [ILombok]
    [IFreeze]
    public partial class Demo10 {
    [Put] private Dictionary<string, string> map = new Dictionary<string, string>();
    
        [Put(freezeTag = "a", noNull = true, link = true)]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();
    }

生成：

    using Til.Lombok;
    using Xunit;
    
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo10
    {
        public void putInMap(string key, string value)
        {
            if (this.map.ContainsKey(key))
            {
                map[key] = value;
            }
            else
            {
                this.map.Add(key, value);
            }
        }
    
        public Demo10 putInTwoMap(Dictionary<string, string> key, Dictionary<string, string> value)
        {
            this.validateNonFrozen("a");
            if (this.twoMap.ContainsKey(key))
            {
                twoMap[key] = value;
            }
            else
            {
                this.twoMap.Add(key, value);
            }
    
            return this;
        }
    }

# map 同理可以指定类型

源:

    [ILombok]
    [IFreeze]
    public partial class Demo11 {
        [Put(keyType = "int", valueType = "string")] private Dictionary<object, object> map = new Dictionary<object, object>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;

    namespace Lombok.Test;
    #nullable enable
    public partial class Demo11
    {
        public void putInMap(int key, string value)
        {
            if (this.map.ContainsKey(key))
            {
                map[key] = value;
            }
            else
            {
                this.map.Add(key, value);
            }
        }
    }

# map [ContainKey] [ContainValue]

源:

    [ILombok]
    [IFreeze]
    public partial class Demo12 {
        [ContainKey] [ContainValue] private Dictionary<string, string> map = new Dictionary<string, string>();
        [ContainKey(freezeTag = "a")] [ContainValue(freezeTag = "a")]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo12
    {
        public bool containKeyInMap(string key)
        {
            return this.map.ContainsKey(key);
        }
        public bool containKeyInTwoMap(Dictionary<string, string> key)
        {
            this.validateNonFrozen("a");
            return this.twoMap.ContainsKey(key);
        }
    }


    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo12
    {
        public bool containKeyInMap(string key)
        {
            return this.map.ContainsKey(key);
        }
        public bool containKeyInTwoMap(Dictionary<string, string> key)
        {
            this.validateNonFrozen("a");
            return this.twoMap.ContainsKey(key);
        }
    }

# map  [RemoveKey] [RemoveValue]

ps： RemoveValue 会生成调用一个 ContainsValue的方法，或许你可以考虑自己实现个 emmm

源：

    [ILombok]
    [IFreeze]
    public partial class Demo13 {
        [ContainKey]  private Dictionary<string, string> map = new Dictionary<string, string>();
        [ContainKey(freezeTag = "a")]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo13
    {
        public bool containKeyInMap(string key)
        {
            return this.map.ContainsKey(key);
        }
        public bool containKeyInTwoMap(Dictionary<string, string> key)
        {
            this.validateNonFrozen("a");
            return this.twoMap.ContainsKey(key);
        }
    }

# map  [ForKey] [ForValue] [ForAll]

    [ILombok]
    [IFreeze]
    public partial class Demo14 {
        [ForKey] [ForValue] [ForAll] private Dictionary<string, string> map = new Dictionary<string, string>();
        [ForKey(freezeTag = "aa", useYield = true)] [ForValue(freezeTag = "aa", useYield = true)] [ForAll(freezeTag = "aa", useYield = true)]
        public Dictionary<Dictionary<string, string>, Dictionary<string, string>> twoMap = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();
    }

生成：

    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo14
    {
        public IEnumerable<string> forMapKey()
        {
            return this.map.Keys;
        }
        public IEnumerable<Dictionary<string, string>> forTwoMapKey()
        {
            this.validateNonFrozen("aa");
            foreach (var i in this.twoMap.Keys)
            {
                yield return i;
            }
        }
    }
    
    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo14
    {
        public IEnumerable<string> forMapValue()
        {
            return this.map.Values;
        }
        public IEnumerable<Dictionary<string, string>> forTwoMapValue()
        {
            this.validateNonFrozen("aa");
            foreach (var i in this.twoMap.Values)
            {
                yield return i;
            }
        }
    }
    
    // <auto-generated/>
    using System.Diagnostics.CodeAnalysis;
    using Til.Lombok;
    using Xunit;
    namespace Lombok.Test;
    #nullable enable
    public partial class Demo14
    {
        public IEnumerable<KeyValuePair<string, string>> forMap()
        {
            return this.map;
        }
        public IEnumerable<KeyValuePair<Dictionary<string, string>, Dictionary<string, string>>> forTwoMap()
        {
            this.validateNonFrozen("aa");
            foreach (var i in this.twoMap)
            {
                yield return i;
            }
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    