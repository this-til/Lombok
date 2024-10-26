using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok.Generator {

    public class BasicsContext {

        public readonly ClassDeclarationSyntax contextTargetNode;

        public readonly SemanticModel semanticModel;

        public readonly GeneratorAttributeSyntaxContext context;

        public readonly CancellationToken cancellationToken;

        public readonly Ptr<ClassDeclarationSyntax> partialClass;

        public readonly Ptr<NamespaceDeclarationSyntax> namespaceDeclarationSyntax;

        public readonly Ptr<CompilationUnitSyntax> compilationUnitSyntax;

        public BasicsContext? nestContext;

        public BasicsContext
        (
            ClassDeclarationSyntax contextTargetNode,
            SemanticModel semanticModel,
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken,
            Ptr<ClassDeclarationSyntax> partialClass,
            Ptr<NamespaceDeclarationSyntax> namespaceDeclarationSyntax,
            Ptr<CompilationUnitSyntax> compilationUnitSyntax
        ) {
            this.contextTargetNode = contextTargetNode;
            this.semanticModel = semanticModel;
            this.context = context;
            this.cancellationToken = cancellationToken;
            this.partialClass = partialClass;
            this.namespaceDeclarationSyntax = namespaceDeclarationSyntax;
            this.compilationUnitSyntax = compilationUnitSyntax;
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

        public ClassDeclarationSyntax partialClass {
            get => basicsContext.partialClass;
            set => basicsContext.partialClass.value = value;
        }

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

        public ClassDeclarationSyntax partialClass {
            get => basicsContext.partialClass;
            set => basicsContext.partialClass.value = value;
        }

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

        public ClassDeclarationSyntax partialClass {
            get => basicsContext.partialClass;
            set => basicsContext.partialClass.value = value;
        }

        public ClassAttributeContext(BasicsContext basicsContext, AttributeContext<A> attributeContext) {
            this.basicsContext = basicsContext;
            this.attributeContext = attributeContext;
        }

    }

    public class ClassFieldAttributeContext<FA, CA> where CA : Attribute where FA : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly AttributeContext<CA>? caAttributeContext;

        public readonly List<FieldsAttributeContext<FA>> fieldsAttributeContextList;

        public ClassDeclarationSyntax partialClass {
            get => basicsContext.partialClass;
            set => basicsContext.partialClass.value = value;
        }

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
