using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Til.Lombok {

    public class Context {

        public readonly ClassDeclarationSyntax contextTargetNode;

        public readonly NameSyntax contextNamespaceNameSyntax;

        public readonly SemanticModel semanticModel;

        public readonly GeneratorAttributeSyntaxContext generatorAttributeSyntaxContext;

        public readonly CancellationToken cancellationToken;

        public readonly List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList;

        public readonly List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList;

        public Context? previous;

        public Context
        (
            ClassDeclarationSyntax contextTargetNode,
            NameSyntax contextNamespaceNameSyntax,
            SemanticModel semanticModel,
            GeneratorAttributeSyntaxContext generatorAttributeSyntaxContext,
            CancellationToken cancellationToken,
            List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList,
            List<MemberDeclarationSyntax> namespaceMemberDeclarationSyntaxList,
            List<MemberDeclarationSyntax> compilationMemberDeclarationSyntaxList,
            Context? previous = null
        ) {
            this.contextTargetNode = contextTargetNode;
            this.contextNamespaceNameSyntax = contextNamespaceNameSyntax;
            this.semanticModel = semanticModel;
            this.generatorAttributeSyntaxContext = generatorAttributeSyntaxContext;
            this.cancellationToken = cancellationToken;
            this.partialClassMemberDeclarationSyntaxList = partialClassMemberDeclarationSyntaxList;
            this.namespaceMemberDeclarationSyntaxList = namespaceMemberDeclarationSyntaxList;
            this.compilationMemberDeclarationSyntaxList = compilationMemberDeclarationSyntaxList;
            this.previous = previous;
        }

        public void addInPartialClassMembers(params MemberDeclarationSyntax[] members) {
            if (members is null) {
                return;
            }
            this.partialClassMemberDeclarationSyntaxList.AddRange(members);
        }

        public void addInNamespaceMembers(params MemberDeclarationSyntax[] members) {
            if (members is null) {
                return;
            }
            this.namespaceMemberDeclarationSyntaxList.AddRange(members);
        }

        public void addInCompilationMembers(params MemberDeclarationSyntax[] members) {
            if (members is null) {
                return;
            }
            this.compilationMemberDeclarationSyntaxList.AddRange(members);
        }

    }

    public class FieldOrPropertyPack {

        public readonly FieldDeclarationSyntax? fieldDeclarationSyntax;

        public readonly VariableDeclaratorSyntax? variableDeclaratorSyntax;

        public readonly PropertyDeclarationSyntax? propertyDeclarationSyntax;

        public readonly TypeSyntax typeSyntax;

        public readonly SyntaxToken name;

        public readonly SyntaxList<AttributeListSyntax> attributeSyntaxList;

        public readonly bool field;

        public FieldOrPropertyPack(FieldDeclarationSyntax fieldDeclarationSyntax, VariableDeclaratorSyntax variableDeclaratorSyntax) {
            this.fieldDeclarationSyntax = fieldDeclarationSyntax;
            this.variableDeclaratorSyntax = variableDeclaratorSyntax;
            this.typeSyntax = this.fieldDeclarationSyntax.Declaration.Type;
            this.name = this.variableDeclaratorSyntax.Identifier;
            this.attributeSyntaxList = fieldDeclarationSyntax.AttributeLists;
            this.field = true;
        }

        public FieldOrPropertyPack(PropertyDeclarationSyntax propertyDeclarationSyntax) {
            this.propertyDeclarationSyntax = propertyDeclarationSyntax;
            this.typeSyntax = propertyDeclarationSyntax.Type;
            this.name = this.propertyDeclarationSyntax.Identifier;
            this.field = false;
        }

    }

}