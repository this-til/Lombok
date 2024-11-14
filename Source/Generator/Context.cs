using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok.Generator {

    public class BasicsContext {

        public readonly ClassDeclarationSyntax contextTargetNode;

        public readonly NameSyntax contextNamespaceNameSyntax;

        public readonly SemanticModel semanticModel;

        public readonly GeneratorAttributeSyntaxContext context;

        public readonly CancellationToken cancellationToken;

        public readonly List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList;

        public BasicsContext? nestContext;

        public BasicsContext
        (
            ClassDeclarationSyntax contextTargetNode,
            NameSyntax contextNamespaceNameSyntax,
            SemanticModel semanticModel,
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken,
            List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList,
            List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList,
            List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList
        ) {
            this.contextTargetNode = contextTargetNode;
            this.contextNamespaceNameSyntax = contextNamespaceNameSyntax;
            this.semanticModel = semanticModel;
            this.context = context;
            this.cancellationToken = cancellationToken;
            this.partialClassMemberDeclarationSyntaxList = partialClassMemberDeclarationSyntaxList;
            this.namespaceMemberDeclarationSyntaxList = namespaceMemberDeclarationSyntaxList;
            this.compilationMemberDeclarationSyntaxList = compilationMemberDeclarationSyntaxList;
        }

    }

    public class FieldsContext {

        public readonly BasicsContext basicsContext;

        public readonly TypeContext typeContext;

        public readonly TypeSyntax typeSyntax;

        public readonly SyntaxList<AttributeListSyntax> attributeListSyntaxes;

        public readonly FieldDeclarationSyntax? fieldDeclarationSyntax;

        public readonly VariableDeclaratorSyntax? variableDeclaratorSyntax;

        public readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public FieldsContext
        (
            BasicsContext basicsContext,
            TypeContext typeContext,
            TypeSyntax typeSyntax,
            SyntaxList<AttributeListSyntax> attributeListSyntaxes,
            FieldDeclarationSyntax? fieldDeclarationSyntax,
            VariableDeclaratorSyntax? variableDeclaratorSyntax,
            PropertyDeclarationSyntax? propertyDeclarationSyntax
        ) {
            this.basicsContext = basicsContext;
            this.typeContext = typeContext;
            this.typeSyntax = typeSyntax;
            this.attributeListSyntaxes = attributeListSyntaxes;
            this.fieldDeclarationSyntax = fieldDeclarationSyntax;
            this.variableDeclaratorSyntax = variableDeclaratorSyntax;
            this.propertyDeclarationSyntax = propertyDeclarationSyntax;
        }

    }

    public class AttributeContext<A> where A : Attribute {

        public readonly A firstAttribute;

        public readonly AttributeSyntax firstAttributeSyntax;

        public readonly List<A> attribute;

        public readonly List<AttributeSyntax> attributeSyntax;

        public AttributeContext
        (
            A firstAttribute,
            AttributeSyntax firstAttributeSyntax,
            List<A> attribute,
            List<AttributeSyntax> attributeSyntax
        ) {
            this.firstAttribute = firstAttribute;
            this.firstAttributeSyntax = firstAttributeSyntax;
            this.attribute = attribute;
            this.attributeSyntax = attributeSyntax;
        }

    }

    public class TypeContext {

        public string fieldName;

        public string typeName;

        public string className;

        public string listCellType = null!;

        public string keyType = null!;

        public string valueType = null!;

        public TypeContext(string fieldName, string typeName, string className) {
            this.fieldName = fieldName;
            this.typeName = typeName;
            this.className = className;
        }

    }

    public class FieldsAttributeContext<A> where A : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly FieldsContext fieldsContext;

        public readonly TypeContext typeContext;

        public readonly AttributeContext<A> attributeContext;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public FieldsAttributeContext
        (
            BasicsContext basicsContext,
            FieldsContext fieldsContext,
            TypeContext typeContext,
            AttributeContext<A> attributeContext
        ) {
            this.basicsContext = basicsContext;
            this.fieldsContext = fieldsContext;
            this.typeContext = typeContext;
            this.attributeContext = attributeContext;
        }

    }

    public class ClassAttributeContext<A> where A : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly AttributeContext<A> attributeContext;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public ClassAttributeContext(BasicsContext basicsContext, AttributeContext<A> attributeContext) {
            this.basicsContext = basicsContext;
            this.attributeContext = attributeContext;
        }

    }

    public class ClassFieldAttributeContext<FA, CA> where CA : Attribute where FA : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly AttributeContext<CA>? caAttributeContext;

        public readonly List<FieldsAttributeContext<FA>> fieldsAttributeContextList;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public ClassFieldAttributeContext
        (
            BasicsContext basicsContext,
            AttributeContext<CA>? caAttributeContext,
            List<FieldsAttributeContext<FA>> fieldsAttributeContextList
        ) {
            this.basicsContext = basicsContext;
            this.caAttributeContext = caAttributeContext;
            this.fieldsAttributeContextList = fieldsAttributeContextList;
        }

    }

    public class Ptr<K> {

        public K value = default!;

        public Ptr() {
        }

        public Ptr(K value) {
            this.value = value;
        }

        public static implicit operator K(Ptr<K> v) => v.value;

    }

}
