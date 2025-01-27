﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok {

    public interface IFreeze {

        /// <summary>
        /// 判断指定标签的元素是否被冻结
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool isFrozen(string tag);

        /// <summary>
        /// 冻结指定标签的元素
        /// </summary>
        /// <param name="tag"></param>
        public void frozen(string tag);

        /// <summary>
        /// 验证指定标签的元素是否没有被冻结
        /// </summary>
        /// <param name="tag"></param>
        public void validateNonFrozen(string tag);

    }

    public interface IChangeIdentifier {

        SyntaxToken changeIdentifier(SyntaxToken tag);

    }

    public interface IChangeType {

        public TypeSyntax changeTypeSyntax(TypeSyntax typeSyntax);
        
    }

}