using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok.Generator {

    public class BasicsContext {

        public readonly string className;

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
            this.className = contextTargetNode.getHasGenericName();
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

        public readonly string fieldName;

        public TypeContext typeContext;

        public readonly SyntaxList<AttributeListSyntax> attributeListSyntaxes;

        public readonly FieldDeclarationSyntax? fieldDeclarationSyntax;

        public readonly VariableDeclaratorSyntax? variableDeclaratorSyntax;

        public readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public FieldsContext
        (
            BasicsContext basicsContext,
            string fieldName,
            TypeContext typeContext,
            SyntaxList<AttributeListSyntax> attributeListSyntaxes,
            FieldDeclarationSyntax? fieldDeclarationSyntax,
            VariableDeclaratorSyntax? variableDeclaratorSyntax,
            PropertyDeclarationSyntax? propertyDeclarationSyntax
        ) {
            this.basicsContext = basicsContext;
            this.typeContext = typeContext;
            this.fieldName = fieldName;
            this.attributeListSyntaxes = attributeListSyntaxes;
            this.fieldDeclarationSyntax = fieldDeclarationSyntax;
            this.variableDeclaratorSyntax = variableDeclaratorSyntax;
            this.propertyDeclarationSyntax = propertyDeclarationSyntax;
        }

    }

    public class FieldsAttributeContext<A> where A : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly FieldsContext fieldsContext;

        public readonly IReadOnlyList<AttributeContext<A>> attributeContext;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public FieldsAttributeContext
        (
            BasicsContext basicsContext,
            FieldsContext fieldsContext,
            IReadOnlyList<AttributeContext<A>> attributeContext
        ) {
            this.basicsContext = basicsContext;
            this.fieldsContext = fieldsContext;
            this.attributeContext = attributeContext;
        }

    }

    public class AttributeContext<A> where A : Attribute {

        public readonly A attribute;

        public readonly AttributeSyntax attributeSyntax;

        public AttributeContext
        (
            A attribute,
            AttributeSyntax attributeSyntax
        ) {
            this.attributeSyntax = attributeSyntax;
            this.attribute = attribute;
        }

    }

    public class TypeContext {

        public string typeName;

        public readonly TypeSyntax typeSyntax;

        public IReadOnlyList<TypeContext> genericTypeContexts;

        public TypeContext
        (
            string typeName,
            TypeSyntax typeSyntax,
            IReadOnlyList<TypeContext> genericTypeContexts
        ) {
            this.typeName = typeName;
            this.typeSyntax = typeSyntax;
            this.genericTypeContexts = genericTypeContexts;
        }

        public TypeContext(TypeContext typeContext) {
            this.typeName = typeContext.typeName;
            this.typeSyntax = typeContext.typeSyntax;
            this.genericTypeContexts = typeContext.genericTypeContexts;
        }

        public TypeContext(string typeName, TypeSyntax typeSyntax) {
            this.typeName = typeName;
            this.typeSyntax = typeSyntax;

            this.genericTypeContexts = this.typeSyntax is GenericNameSyntax typeDeclarationSyntax
                ? typeDeclarationSyntax.TypeArgumentList.Arguments.Select(syntax => new TypeContext(syntax.ToString(), syntax)).ToList()
                : new List<TypeContext>();
        }

        public TypeContext? tryGetGenericTypeContexts(int i) {
            if (i > 0 && i <= genericTypeContexts.Count) {
                return genericTypeContexts[i];
            }
            return null;
        }

        public void receive(MetadataAttribute metadataAttribute) {
            if (metadataAttribute.customType != null) {
                typeName = metadataAttribute.customType;
            }
        }

    }

    public class ListTypeContext : TypeContext {

        public string listTypeName;

        public ListTypeContext(TypeContext typeContext, string listTypeName) : base(typeContext.typeName, typeContext.typeSyntax, typeContext.genericTypeContexts) {
            this.listTypeName = listTypeName;
        }

    }

    public class MapTypeContext : TypeContext {

        public string keyTypeName;

        public string valueTypeName;

        public MapTypeContext(TypeContext typeContext, string KeyTypeName, string valueTypeName) : base(typeContext.typeName, typeContext.typeSyntax, typeContext.genericTypeContexts) {
            this.keyTypeName = KeyTypeName;
            this.valueTypeName = valueTypeName;
        }

    }

    public class MethodContext {

        public readonly BasicsContext basicsContext;

        public readonly string methodName;

        public TypeContext returnType;

        public readonly MethodDeclarationSyntax? methodDeclarationSyntax;

        public SyntaxList<AttributeListSyntax> attributeListSyntaxes;

        public MethodContext
        (
            BasicsContext basicsContext,
            string methodName,
            TypeContext returnType,
            MethodDeclarationSyntax? methodDeclarationSyntax,
            SyntaxList<AttributeListSyntax> attributeListSyntaxes
        ) {
            this.basicsContext = basicsContext;
            this.methodName = methodName;
            this.returnType = returnType;
            this.methodDeclarationSyntax = methodDeclarationSyntax;
            this.attributeListSyntaxes = attributeListSyntaxes;
        }

    }

    public class MethodAttributeContext<A> where A : MethodAttribute {

        public readonly BasicsContext basicsContext;

        public readonly MethodContext methodContext;

        public readonly IReadOnlyList<AttributeContext<A>> attributeContext;

        public MethodAttributeContext(BasicsContext basicsContext, MethodContext methodContext, IReadOnlyList<AttributeContext<A>> attributeContext) {
            this.basicsContext = basicsContext;
            this.methodContext = methodContext;
            this.attributeContext = attributeContext;
        }

    }

    public class ClassAttributeContext<A> where A : Attribute {

        public readonly BasicsContext basicsContext;

        public readonly IReadOnlyList<AttributeContext<A>> attributeContextList;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public ClassAttributeContext(BasicsContext basicsContext, IReadOnlyList<AttributeContext<A>> attributeContextList) {
            this.basicsContext = basicsContext;
            this.attributeContextList = attributeContextList;
        }

    }

    public class IncrementContext {

        public readonly BasicsContext basicsContext;

        public readonly List<MemberDeclarationSyntax> source;

        public readonly List<ClassDeclarationSyntax> partialClass;

        public readonly List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList;

        public IncrementContext(BasicsContext basicsContext, List<ClassDeclarationSyntax> partialClass, List<MemberDeclarationSyntax> source) {
            this.basicsContext = basicsContext;
            this.source = source;
            this.partialClass = partialClass;
            partialClassMemberDeclarationSyntaxList = basicsContext.partialClassMemberDeclarationSyntaxList;
            namespaceMemberDeclarationSyntaxList = basicsContext.namespaceMemberDeclarationSyntaxList;
            compilationMemberDeclarationSyntaxList = basicsContext.compilationMemberDeclarationSyntaxList;
        }

    }

    public class ClassAttributeIncrementContext<CA> where CA : Attribute {

        public readonly IncrementContext incrementContext;

        public readonly BasicsContext basicsContext;

        public readonly IReadOnlyList<AttributeContext<CA>> attributeContextList;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public ClassAttributeIncrementContext(IncrementContext incrementContext, IReadOnlyList<AttributeContext<CA>> attributeContextList) {
            this.incrementContext = incrementContext;
            this.basicsContext = this.incrementContext.basicsContext;
            this.attributeContextList = attributeContextList;

        }

    }

    public class FieldAttributeIncrementContext<FA> where FA : IncrementFieldAttribute {


        public readonly string getInvoke;

        public readonly Func<string, string> setInvoke;

        public readonly TypeContext typeContext;

        public readonly AttributeContext<FA> attributeContext;

        public readonly FieldsAttributeContext<FA> fieldsAttributeContext;

        public FieldAttributeIncrementContext
        (
            string getInvoke,
            Func<string, string> setInvoke,
            TypeContext typeContext,
            AttributeContext<FA> attributeContext,
            FieldsAttributeContext<FA> fieldsAttributeContext
 
        ) {
            this.getInvoke = getInvoke;
            this.setInvoke = setInvoke;
            this.typeContext = typeContext;
            this.attributeContext = attributeContext;
            this.fieldsAttributeContext = fieldsAttributeContext;
        }

    }

    public class ClassFieldAttributeIncrementContext<FA, CA> where CA : IncrementClassAttribute where FA : IncrementFieldAttribute {

        public readonly IncrementContext incrementContext;

        public readonly BasicsContext basicsContext;

        public readonly AttributeContext<CA>? caAttributeContext;

        public readonly List<FieldsAttributeContext<FA>> fieldsAttributeContextList;

        public List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList => basicsContext.partialClassMemberDeclarationSyntaxList;

        public ClassFieldAttributeIncrementContext
        (
            IncrementContext incrementContext,
            AttributeContext<CA>? caAttributeContext,
            List<FieldsAttributeContext<FA>> fieldsAttributeContextList
        ) {
            this.incrementContext = incrementContext;
            this.basicsContext = incrementContext.basicsContext;
            this.caAttributeContext = caAttributeContext;
            this.fieldsAttributeContextList = fieldsAttributeContextList;
        }

    }

    public class TranslationClassFieldAttributeIncrementContext<FA, CA> where CA : IncrementClassAttribute where FA : IncrementFieldAttribute {

        public readonly IncrementContext incrementContext;

        public readonly BasicsContext basicsContext;

        public readonly ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext;

        public readonly List<FieldAttributeIncrementContext<FA>> fieldsAttributeContextList;

        public TranslationClassFieldAttributeIncrementContext(ClassFieldAttributeIncrementContext<FA, CA> classFieldAttributeIncrementContext, List<FieldAttributeIncrementContext<FA>> fieldsAttributeContextList) {
            this.incrementContext = classFieldAttributeIncrementContext.incrementContext;
            this.basicsContext = classFieldAttributeIncrementContext.basicsContext;
            this.fieldsAttributeContextList = fieldsAttributeContextList;
            this.classFieldAttributeIncrementContext = classFieldAttributeIncrementContext;
        }

    }

}
