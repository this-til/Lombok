using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Til.Lombok.Generator {

    public abstract class GeneratorComponent {

        public abstract void fill
        (
            BasicsContext basicsContext
        );

    }

    public abstract class FieldsGeneratorComponent : GeneratorComponent {

        public sealed override void fill
        (
            BasicsContext basicsContext
        ) {

            foreach (MemberDeclarationSyntax member in basicsContext.contextTargetNode.Members) {
                switch (member) {
                    // 检查成员是否是字段或属性  
                    case FieldDeclarationSyntax fieldDeclaration: {
                        foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {
                            try {
                                fill
                                (
                                    new FieldsContext
                                    (
                                        basicsContext,
                                        variableDeclaratorSyntax.Identifier.ToString(),
                                        new TypeContext
                                        (
                                            fieldDeclaration.Declaration.Type.ToString(),
                                            fieldDeclaration.Declaration.Type
                                        ),
                                        fieldDeclaration.AttributeLists,
                                        fieldDeclaration,
                                        variableDeclaratorSyntax,
                                        null
                                    )
                                );
                            }
                            catch (Exception e) {
                                e.PrintExceptionSummaryAndStackTrace();
                            }

                        }
                        break;
                    }
                    case PropertyDeclarationSyntax propertyDeclaration: {
                        try {
                            fill
                            (
                                new FieldsContext
                                (
                                    basicsContext,
                                    propertyDeclaration.Identifier.ToString(),
                                    new TypeContext
                                    (
                                        propertyDeclaration.Type.ToString(),
                                        propertyDeclaration.Type
                                    ),
                                    propertyDeclaration.AttributeLists,
                                    null,
                                    null,
                                    propertyDeclaration
                                )
                            );
                        }
                        catch (Exception e) {
                            e.PrintExceptionSummaryAndStackTrace();
                        }

                        break;
                    }
                }
            }
        }

        protected abstract void fill(FieldsContext fieldsContext);

    }

    public abstract class FieldsAttributeGeneratorComponent<A> : FieldsGeneratorComponent where A : MetadataAttribute {

        protected virtual A restore(Dictionary<string, string> data) {
            return (A)Activator.CreateInstance(typeof(A), data);
        }

        protected sealed override void fill(FieldsContext fieldsContext) {
            List<AttributeSyntax> attributeSyntaxeList = fieldsContext.attributeListSyntaxes.tryGetSpecifiedAttribute(typeof(A).Name).ToList();

            if (attributeSyntaxeList.Count == 0) {
                return;
            }

            IReadOnlyList<AttributeContext<A>> attributeContexts = attributeSyntaxeList.Select
                (
                    syntax => {
                        A attribute = restore
                        (
                            syntax.getAttributeArgumentsAsDictionary
                            (
                                fieldsContext.basicsContext.semanticModel
                            )
                        );
                        return new AttributeContext<A>
                        (
                            attribute,
                            syntax
                        );
                    }
                )
                .Where(a => a != null)
                .ToList()!;

            try {
                fill
                (
                    new FieldsAttributeContext<A>
                    (
                        fieldsContext.basicsContext,
                        fieldsContext,
                        attributeContexts
                    )
                );
            }
            catch (Exception e) {
                e.PrintExceptionSummaryAndStackTrace();
            }

        }

        protected abstract void fill(FieldsAttributeContext<A> fieldsAttributeContext);

    }

    public abstract class DistributeFieldsAttributeGeneratorComponent<A> : FieldsAttributeGeneratorComponent<A> where A : MetadataAttribute {

        protected sealed override void fill(FieldsAttributeContext<A> fieldsAttributeContext) {
            foreach (AttributeContext<A> attributeContext in fieldsAttributeContext.attributeContext) {
                TypeContext typeContext = new TypeContext(fieldsAttributeContext.fieldsContext.typeContext);
                typeContext.receive(attributeContext.attribute);
                fieldsAttributeContext.fieldsContext.typeContext = typeContext;
                try {
                    fill(fieldsAttributeContext, attributeContext);
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }
            }
        }

        protected abstract void fill(FieldsAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext);

    }

    public abstract class ListDistributeFieldsAttributeGeneratorComponent<A> : DistributeFieldsAttributeGeneratorComponent<A> where A : ListMetadataAttribute {

        protected override void fill(FieldsAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext) {
            string? listCellType = attributeContext.attribute.listCellType ?? fieldsAttributeContext.fieldsContext.typeContext.tryGetGenericTypeContexts(0)?.typeName;
            if (listCellType is null) {
                return;
            }
            fill(fieldsAttributeContext, attributeContext, new ListTypeContext(fieldsAttributeContext.fieldsContext.typeContext, listCellType));
        }

        protected abstract void fill(FieldsAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext, ListTypeContext listTypeContext);

    }

    public abstract class MapDistributeFieldsAttributeGeneratorComponent<A> : DistributeFieldsAttributeGeneratorComponent<A> where A : MapMetadataAttribute {

        protected override void fill(FieldsAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext) {
            string? keyTypeName = attributeContext.attribute.keyType ?? fieldsAttributeContext.fieldsContext.typeContext.tryGetGenericTypeContexts(0)?.typeName;
            string? valueTypeName = attributeContext.attribute.valueType ?? fieldsAttributeContext.fieldsContext.typeContext.tryGetGenericTypeContexts(1)?.typeName;
            if (keyTypeName is null || valueTypeName is null) {
                return;
            }
            fill(fieldsAttributeContext, attributeContext, new MapTypeContext(fieldsAttributeContext.fieldsContext.typeContext, keyTypeName, valueTypeName));
        }

        protected abstract void fill(FieldsAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext, MapTypeContext mapTypeContext);

    }

    public abstract class MethodGeneratorComponent : GeneratorComponent {

        public sealed override void fill(BasicsContext basicsContext) {
            foreach (MemberDeclarationSyntax memberDeclarationSyntax in basicsContext.contextTargetNode.Members) {
                switch (memberDeclarationSyntax) {
                    case MethodDeclarationSyntax methodDeclarationSyntax:
                        try {
                            fill
                            (
                                new MethodContext
                                (
                                    basicsContext,
                                    methodDeclarationSyntax.Identifier.ToString(),
                                    new TypeContext
                                    (
                                        methodDeclarationSyntax.ReturnType.ToString(),
                                        methodDeclarationSyntax.ReturnType
                                    ),
                                    methodDeclarationSyntax,
                                    memberDeclarationSyntax.AttributeLists
                                )
                            );
                        }
                        catch (Exception e) {
                            e.PrintExceptionSummaryAndStackTrace();
                        }
                        break;
                }
            }
        }

        public abstract void fill(MethodContext methodContext);

    }

    public abstract class MethodAttributeGeneratorComponent<A> : MethodGeneratorComponent where A : MethodAttribute {

        protected virtual A restore(Dictionary<string, string> data) {
            return (A)Activator.CreateInstance(typeof(A), data);
        }

        public sealed override void fill(MethodContext methodContext) {
            List<AttributeSyntax> attributeSyntaxeList = methodContext.attributeListSyntaxes.tryGetSpecifiedAttribute(typeof(A).Name).ToList();

            if (attributeSyntaxeList.Count == 0) {
                return;
            }

            IReadOnlyList<AttributeContext<A>> attributeContexts = attributeSyntaxeList.Select
                (
                    syntax => {
                        A attribute = restore
                        (
                            syntax.getAttributeArgumentsAsDictionary
                            (
                                methodContext.basicsContext.semanticModel
                            )
                        );
                        return new AttributeContext<A>
                        (
                            attribute,
                            syntax
                        );
                    }
                )
                .Where(a => a != null)
                .ToList()!;

            try {
                fill
                (
                    new MethodAttributeContext<A>
                    (
                        methodContext.basicsContext,
                        methodContext,
                        attributeContexts
                    )
                );
            }
            catch (Exception e) {
                e.PrintExceptionSummaryAndStackTrace();
            }

        }

        public abstract void fill(MethodAttributeContext<A> methodAttributeContext);

    }

    public abstract class DistributeMethodAttributeGeneratorComponent<A> : MethodAttributeGeneratorComponent<A> where A : MethodAttribute {

        public override void fill(MethodAttributeContext<A> methodAttributeContext) {
            foreach (AttributeContext<A> attributeContext in methodAttributeContext.attributeContext) {
                TypeContext typeContext = new TypeContext(methodAttributeContext.methodContext.returnType);
                typeContext.receive(attributeContext.attribute);
                methodAttributeContext.methodContext.returnType = typeContext;
                try {
                    fill(methodAttributeContext, attributeContext);
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }
            }
        }

        public abstract void fill(MethodAttributeContext<A> methodAttributeContext, AttributeContext<A> attributeContext);

    }

    public abstract class ClassAttributeGeneratorComponent<A> : GeneratorComponent where A : Attribute {

        protected virtual A restore(Dictionary<string, string> data) {
            return (A)Activator.CreateInstance(typeof(A), data);
        }

        public override void fill(BasicsContext basicsContext) {
            List<AttributeSyntax> attributeSyntaxeList = basicsContext.contextTargetNode.AttributeLists.tryGetSpecifiedAttribute(typeof(A).Name).ToList();

            if (attributeSyntaxeList.Count == 0) {
                return;
            }

            IReadOnlyList<AttributeContext<A>> attributeContexts = attributeSyntaxeList.Select
                (
                    syntax => {
                        A attribute = restore
                        (
                            syntax.getAttributeArgumentsAsDictionary
                            (
                                basicsContext.semanticModel
                            )
                        );

                        return new AttributeContext<A>
                        (
                            attribute,
                            syntax
                        );
                    }
                )
                .Where(a => a != null)
                .ToList()!;

            try {
                fill
                (
                    new ClassAttributeContext<A>
                    (
                        basicsContext,
                        attributeContexts
                    )
                );
            }
            catch (Exception e) {
                e.PrintExceptionSummaryAndStackTrace();
            }

        }

        protected abstract void fill(ClassAttributeContext<A> fieldsAttributeContext);

    }

    public abstract class DistributeClassAttributeGeneratorComponent<A> : ClassAttributeGeneratorComponent<A> where A : Attribute {

        protected override void fill(ClassAttributeContext<A> fieldsAttributeContext) {
            foreach (AttributeContext<A> attributeContext in fieldsAttributeContext.attributeContextList) {
                fill(fieldsAttributeContext, attributeContext);
            }
        }

        protected abstract void fill(ClassAttributeContext<A> fieldsAttributeContext, AttributeContext<A> attributeContext);

    }

    [GeneratorComponent]
    public class GetGenerator : DistributeFieldsAttributeGeneratorComponent<GetAttribute> {

        protected sealed override void fill(FieldsAttributeContext<GetAttribute> fieldsAttributeContext, AttributeContext<GetAttribute> attributeContext) {

            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            fieldsAttributeContext.fieldsContext.typeContext.typeName
                        ),
                        "get" + fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            MemberAccessExpression
                            (
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName
                                (
                                    fieldsAttributeContext.fieldsContext.fieldName
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class OpenGenerator : DistributeFieldsAttributeGeneratorComponent<OpenAttribute> {

        protected override void fill(FieldsAttributeContext<OpenAttribute> fieldsAttributeContext, AttributeContext<OpenAttribute> attributeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        "open" + fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "action"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    $"Action<{fieldsAttributeContext.fieldsContext.typeContext.typeName}>"
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                    IdentifierName
                                    (
                                        "action"
                                    ),
                                    IdentifierName
                                    (
                                        "Invoke"
                                    )
                                ),
                                ArgumentList
                                ( // 创建参数列表  
                                    SingletonSeparatedList
                                    ( // 单个参数列表  
                                        Argument
                                        ( // 创建一个参数表达式  
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                                ThisExpression(),
                                                IdentifierName
                                                (
                                                    fieldsAttributeContext.fieldsContext.fieldName
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class SetGenerator : DistributeFieldsAttributeGeneratorComponent<SetAttribute> {

        protected override void fill(FieldsAttributeContext<SetAttribute> fieldsAttributeContext, AttributeContext<SetAttribute> attributeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        "set" + fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    fieldsAttributeContext.fieldsContext.fieldName
                                        .toCamelCaseIdentifier()
                                        .eliminateGeneric()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    fieldsAttributeContext.fieldsContext.typeContext.typeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            AssignmentExpression
                            (
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldsAttributeContext.fieldsContext.fieldName
                                    )
                                ),
                                IdentifierName
                                (
                                    fieldsAttributeContext.fieldsContext.fieldName
                                        .toCamelCaseIdentifier()
                                        .eliminateGeneric()
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class CountGenerator : DistributeFieldsAttributeGeneratorComponent<CountAttribute> {

        protected override void fill(FieldsAttributeContext<CountAttribute> fieldsAttributeContext, AttributeContext<CountAttribute> attributeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "int"
                        ),
                        $"countIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            IdentifierName($"this.{fieldsAttributeContext.fieldsContext.fieldName}.Count")
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class IndexGenerator : ListDistributeFieldsAttributeGeneratorComponent<IndexAttribute> {

        protected override void fill(FieldsAttributeContext<IndexAttribute> fieldsAttributeContext, AttributeContext<IndexAttribute> attributeContext, ListTypeContext listTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            listTypeContext.listTypeName
                        ),
                        $"indexIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "i"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    "int"
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        ( // 创建一个 return 语句  
                            ElementAccessExpression
                            ( // 创建一个数组或列表的索引访问表达式  
                                IdentifierName
                                (
                                    fieldsAttributeContext.fieldsContext.fieldName
                                ),
                                BracketedArgumentList
                                ( // 索引参数列表  
                                    SingletonSeparatedList
                                    ( // 单个参数列表  
                                        Argument
                                        ( // 创建一个参数表达式  
                                            IdentifierName
                                            (
                                                "i"
                                            ) // 引用参数 i  
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class AddGenerator : ListDistributeFieldsAttributeGeneratorComponent<AddAttribute> {

        protected override void fill(FieldsAttributeContext<AddAttribute> fieldsAttributeContext, AttributeContext<AddAttribute> attributeContext, ListTypeContext listTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"addIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier().eliminateGeneric()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + listTypeContext.listTypeName
                                        .toPascalCaseIdentifier()
                                        .eliminateGeneric()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    listTypeContext.listTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            InvocationExpression
                            ( // 创建一个方法调用表达式  
                                MemberAccessExpression
                                ( // 创建一个成员访问表达式（this.list.Add）  
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Add"
                                    ) // 访问名为list的成员  
                                ),
                                ArgumentList
                                ( // 创建参数列表  
                                    SingletonSeparatedList
                                    ( // 单个参数列表  
                                        Argument
                                        ( // 创建一个参数表达式  
                                            IdentifierName
                                            (
                                                "a"
                                                + listTypeContext.listTypeName
                                                    .toPascalCaseIdentifier()
                                                    .eliminateGeneric()
                                            ) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveGenerator : ListDistributeFieldsAttributeGeneratorComponent<RemoveAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveAttribute> fieldsAttributeContext, AttributeContext<RemoveAttribute> attributeContext, ListTypeContext listTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + listTypeContext.listTypeName
                                        .toPascalCaseIdentifier()
                                        .eliminateGeneric()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    listTypeContext.listTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            InvocationExpression
                            ( // 创建一个方法调用表达式  
                                MemberAccessExpression
                                ( // 创建一个成员访问表达式（this.list.Add）  
                                    SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression, // 使用点号访问  ,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Remove"
                                    ) // 访问名为list的成员  
                                ),
                                ArgumentList
                                ( // 创建参数列表  
                                    SingletonSeparatedList
                                    ( // 单个参数列表  
                                        Argument
                                        ( // 创建一个参数表达式  
                                            IdentifierName
                                            (
                                                "a"
                                                + listTypeContext.listTypeName
                                                    .toPascalCaseIdentifier()
                                                    .eliminateGeneric()
                                            ) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainGenerator : ListDistributeFieldsAttributeGeneratorComponent<ContainAttribute> {

        protected override void fill(FieldsAttributeContext<ContainAttribute> fieldsAttributeContext, AttributeContext<ContainAttribute> attributeContext, ListTypeContext listTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + listTypeContext.listTypeName
                                        .toPascalCaseIdentifier()
                                        .eliminateGeneric()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    listTypeContext.listTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Contains"
                                    )
                                ),
                                ArgumentList
                                (
                                    SingletonSeparatedList
                                    (
                                        Argument
                                        (
                                            IdentifierName
                                            (
                                                "a"
                                                + listTypeContext.listTypeName
                                                    .toPascalCaseIdentifier()
                                                    .eliminateGeneric()
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForGenerator : ListDistributeFieldsAttributeGeneratorComponent<ForAttribute> {

        protected override void fill(FieldsAttributeContext<ForAttribute> fieldsAttributeContext, AttributeContext<ForAttribute> attributeContext, ListTypeContext listTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{listTypeContext.listTypeName}>"
                        ),
                        $"for{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        attributeContext.attribute.useYield
                            ? ForEachStatement
                            (
                                ParseTypeName
                                (
                                    "var"
                                ), // 声明变量类型 var  
                                Identifier
                                (
                                    "i"
                                ), // 变量名 i  
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldsAttributeContext.fieldsContext.fieldName
                                    )
                                ), // 假设 list 是一个字段或属性  
                                Block
                                ( // 循环体  
                                    SingletonList<StatementSyntax>
                                    (
                                        YieldStatement
                                        (
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName
                                            (
                                                "i"
                                            )
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldsAttributeContext.fieldsContext.fieldName
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class PutGenerator : MapDistributeFieldsAttributeGeneratorComponent<PutAttribute> {

        protected override void fill(FieldsAttributeContext<PutAttribute> fieldsAttributeContext, AttributeContext<PutAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"putIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "key"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.keyTypeName
                                )
                            ),
                        Parameter
                            (
                                Identifier
                                (
                                    "value"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.valueTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        IfStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "ContainsKey"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "key"
                                                )
                                            ),
                                        }
                                    )
                                )
                            ),
                            Block
                            (
                                ExpressionStatement
                                (
                                    AssignmentExpression
                                    (
                                        SyntaxKind.SimpleAssignmentExpression,
                                        ElementAccessExpression
                                        ( // 创建一个数组或列表的索引访问表达式  
                                            IdentifierName
                                            (
                                                fieldsAttributeContext.fieldsContext.fieldName
                                            ),
                                            BracketedArgumentList
                                            ( // 索引参数列表  
                                                SingletonSeparatedList
                                                ( // 单个参数列表  
                                                    Argument
                                                    ( // 创建一个参数表达式  
                                                        IdentifierName
                                                        (
                                                            "key"
                                                        ) // 引用参数 i  
                                                    )
                                                )
                                            )
                                        ),
                                        IdentifierName
                                        (
                                            "value"
                                        )
                                    )
                                )
                            ),
                            ElseClause
                            (
                                Block
                                (
                                    ExpressionStatement
                                    (
                                        InvocationExpression
                                        (
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression
                                                (
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    ThisExpression(),
                                                    IdentifierName
                                                    (
                                                        fieldsAttributeContext.fieldsContext.fieldName
                                                    )
                                                ),
                                                IdentifierName
                                                (
                                                    "Add"
                                                )
                                            ),
                                            ArgumentList
                                            (
                                                SeparatedList
                                                (
                                                    new[] {
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "key"
                                                            )
                                                        ),
                                                        Argument
                                                        (
                                                            IdentifierName
                                                            (
                                                                "value"
                                                            )
                                                        )
                                                    }
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class MapGetGenerator : MapDistributeFieldsAttributeGeneratorComponent<MapGetAttribute> {

        protected override void fill(FieldsAttributeContext<MapGetAttribute> fieldsAttributeContext, AttributeContext<MapGetAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            mapTypeContext.valueTypeName
                        ),
                        $"getIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "key"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.keyTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        IfStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "ContainsKey"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "key"
                                                )
                                            ),
                                        }
                                    )
                                )
                            ),
                            Block
                            (
                                ReturnStatement
                                (
                                    ElementAccessExpression
                                    (
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        ),
                                        BracketedArgumentList
                                        (
                                            SingletonSeparatedList
                                            (
                                                Argument
                                                (
                                                    IdentifierName
                                                    (
                                                        "key"
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            ),
                            ElseClause
                            (
                                Block
                                (
                                    ReturnStatement
                                    (
                                        IdentifierName
                                        (
                                            "default!"
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveKeyGenerator : MapDistributeFieldsAttributeGeneratorComponent<RemoveKeyAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveKeyAttribute> fieldsAttributeContext, AttributeContext<RemoveKeyAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeKeyIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "key"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.keyTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Remove"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "key"
                                                )
                                            ),
                                        }
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveValueGenerator : MapDistributeFieldsAttributeGeneratorComponent<RemoveValueAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveValueAttribute> fieldsAttributeContext, AttributeContext<RemoveValueAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeValueIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "value"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.keyTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ExpressionStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Remove"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "value"
                                                )
                                            ),
                                        }
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainKeyGenerator : MapDistributeFieldsAttributeGeneratorComponent<ContainKeyAttribute> {

        protected override void fill(FieldsAttributeContext<ContainKeyAttribute> fieldsAttributeContext, AttributeContext<ContainKeyAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containKeyIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "key"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.valueTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "ContainsKey"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "key"
                                                )
                                            ),
                                        }
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainValueGenerator : MapDistributeFieldsAttributeGeneratorComponent<ContainValueAttribute> {

        protected override void fill(FieldsAttributeContext<ContainValueAttribute> fieldsAttributeContext, AttributeContext<ContainValueAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containValueIn{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "value"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    mapTypeContext.valueTypeName
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            InvocationExpression
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "ContainsValue"
                                    )
                                ),
                                ArgumentList
                                (
                                    SeparatedList
                                    (
                                        new[] {
                                            Argument
                                            (
                                                IdentifierName
                                                (
                                                    "value"
                                                )
                                            ),
                                        }
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForKeyGenerator : MapDistributeFieldsAttributeGeneratorComponent<ForKeyAttribute> {

        protected override void fill(FieldsAttributeContext<ForKeyAttribute> fieldsAttributeContext, AttributeContext<ForKeyAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{mapTypeContext.keyTypeName}>"
                        ),
                        $"for{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}Key"
                    )
                    .AddBodyStatements
                    (
                        attributeContext.attribute.useYield
                            ? ForEachStatement
                            (
                                ParseTypeName
                                (
                                    "var"
                                ), // 声明变量类型 var  
                                Identifier
                                (
                                    "i"
                                ), // 变量名 i  
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Keys"
                                    )
                                ), // 假设 list 是一个字段或属性  
                                Block
                                ( // 循环体  
                                    SingletonList<StatementSyntax>
                                    (
                                        YieldStatement
                                        (
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName
                                            (
                                                "i"
                                            )
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Keys"
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForValueGenerator : MapDistributeFieldsAttributeGeneratorComponent<ForValueAttribute> {

        protected override void fill(FieldsAttributeContext<ForValueAttribute> fieldsAttributeContext, AttributeContext<ForValueAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{mapTypeContext.valueTypeName}>"
                        ),
                        $"for{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}Value"
                    )
                    .AddBodyStatements
                    (
                        attributeContext.attribute.useYield
                            ? ForEachStatement
                            (
                                ParseTypeName
                                (
                                    "var"
                                ), // 声明变量类型 var  
                                Identifier
                                (
                                    "i"
                                ), // 变量名 i  
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Values"
                                    )
                                ), // 假设 list 是一个字段或属性  
                                Block
                                ( // 循环体  
                                    SingletonList<StatementSyntax>
                                    (
                                        YieldStatement
                                        (
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName
                                            (
                                                "i"
                                            )
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName
                                        (
                                            fieldsAttributeContext.fieldsContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Values"
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForAllGenerator : MapDistributeFieldsAttributeGeneratorComponent<ForAllAttribute> {

        protected override void fill(FieldsAttributeContext<ForAllAttribute> fieldsAttributeContext, AttributeContext<ForAllAttribute> attributeContext, MapTypeContext mapTypeContext) {
            fieldsAttributeContext.partialClassMemberDeclarationSyntaxList.Add
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<{mapTypeContext.keyTypeName}, {mapTypeContext.valueTypeName}>>"
                        ),
                        $"for{fieldsAttributeContext.fieldsContext.fieldName.toPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        attributeContext.attribute.useYield
                            ? ForEachStatement
                            (
                                ParseTypeName
                                (
                                    "var"
                                ), // 声明变量类型 var  
                                Identifier
                                (
                                    "i"
                                ), // 变量名 i  
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldsAttributeContext.fieldsContext.fieldName
                                    )
                                ),
                                Block
                                ( // 循环体  
                                    SingletonList<StatementSyntax>
                                    (
                                        YieldStatement
                                        (
                                            SyntaxKind.YieldReturnStatement,
                                            IdentifierName
                                            (
                                                "i"
                                            )
                                        )
                                    ) // 包含 yield return 语句的列表  
                                )
                            )
                            : ReturnStatement
                            (
                                MemberAccessExpression
                                (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName
                                    (
                                        fieldsAttributeContext.fieldsContext.fieldName
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext, attributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class PartialFieldsGenerator : DistributeFieldsAttributeGeneratorComponent<IPartialAttribute> {

        protected override void fill(FieldsAttributeContext<IPartialAttribute> fieldsAttributeContext, AttributeContext<IPartialAttribute> attributeContext) {
            BasicsContext basicsContext = fieldsAttributeContext.basicsContext;
            IPartialAttribute partialAttribute = attributeContext.attribute;

            Dictionary<string, string> fillMap = new Dictionary<string, string>();

            if (partialAttribute._customFill is not null) {
                foreach (KeyValuePair<string, string> keyValuePair in partialAttribute._customFill) {
                    fillMap[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            string typeName = basicsContext.contextTargetNode.toClassName();

            List<string> quote = new List<string>();
            BasicsContext _b = basicsContext;
            do {
                quote.Add(_b.contextTargetNode.toClassName());
                _b = _b.nestContext!;
            } while (_b != null);
            
            quote.Reverse();

            fillMap["type"] = typeName;
            fillMap["fullType"] = string.Join(".", quote);
            fillMap["fullType_underline"] = string.Join("_", quote);
            fillMap["field"] = fieldsAttributeContext.fieldsContext.fieldName;
            fillMap["fieldType"] = fieldsAttributeContext.fieldsContext.typeContext.typeName;
            fillMap["namespace"] = basicsContext.contextNamespaceNameSyntax.ToString();

            PartialGenerator.fill(partialAttribute, basicsContext, fillMap);
        }

    }

    [GeneratorComponent]
    public sealed class PartialGenerator : DistributeClassAttributeGeneratorComponent<IPartialAttribute> {

        protected override void fill(ClassAttributeContext<IPartialAttribute> fieldsAttributeContext, AttributeContext<IPartialAttribute> attributeContext) {
            BasicsContext basicsContext = fieldsAttributeContext.basicsContext;
            IPartialAttribute partialAttribute = attributeContext.attribute;

            Dictionary<string, string> fillMap = new Dictionary<string, string>();

            if (partialAttribute._customFill is not null) {
                foreach (KeyValuePair<string, string> keyValuePair in partialAttribute._customFill) {
                    fillMap[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            string typeName = basicsContext.contextTargetNode.toClassName();

            List<string> quote = new List<string>();
            BasicsContext _b = basicsContext;
            do {
                quote.Add(_b.contextTargetNode.toClassName());
                _b = _b.nestContext!;
            } while (_b != null);
            
            quote.Reverse();

            fillMap["type"] = typeName;
            fillMap["fullType"] = string.Join(".", quote);
            fillMap["fullType_underline"] = string.Join("_", quote);
            fillMap["namespace"] = basicsContext.contextNamespaceNameSyntax.ToString();

            fill(partialAttribute, basicsContext, fillMap);
        }

        public static void fill(IPartialAttribute partialAttribute, BasicsContext basicsContext, Dictionary<string, string> fillMap) {

            string? model = partialAttribute.model;

            if (model is null) {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            model.format
            (
                stringBuilder,
                k => {
                    string[] ks = k.Split(':');
                    string f = ks.Length < 2
                        ? String.Empty
                        : ks[1];
                    string value = fillMap[ks[0]];
                    stringBuilder.Append(value.format(f));
                }
            );

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            List<MemberDeclarationSyntax> joinMemberDeclarationSyntaxList;
            BasicsContext? nestContext = null;

            switch (partialAttribute.partialPos) {
                case PartialPos.Interior:
                    joinMemberDeclarationSyntaxList = basicsContext.partialClassMemberDeclarationSyntaxList;
                    nestContext = basicsContext.nestContext;
                    break;
                case PartialPos.UpLevel:
                    if (basicsContext.nestContext is not null) {
                        joinMemberDeclarationSyntaxList = basicsContext.nestContext.partialClassMemberDeclarationSyntaxList;
                        break;
                    }
                    joinMemberDeclarationSyntaxList = basicsContext.namespaceMemberDeclarationSyntaxList;
                    nestContext = basicsContext.nestContext;
                    break;
                case PartialPos.Compilation:
                    joinMemberDeclarationSyntaxList = basicsContext.compilationMemberDeclarationSyntaxList;
                    break;
                case PartialPos.Namespace:
                    joinMemberDeclarationSyntaxList = basicsContext.namespaceMemberDeclarationSyntaxList;
                    break;
                default:
                    return;
            }

            List<ClassDeclarationSyntax> classDeclarationSyntaxList = memberDeclarationSyntaxes.OfType<ClassDeclarationSyntax>().ToList();
            List<MemberDeclarationSyntax> interiorMemberDeclarationSyntax = memberDeclarationSyntaxes.Where(m => m is not ClassDeclarationSyntax).ToList();

            for (int index = 0; index < classDeclarationSyntaxList.Count; index++) {
                ClassDeclarationSyntax classDeclarationSyntax = classDeclarationSyntaxList[index];

                List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();
                BasicsContext context = new BasicsContext
                (
                    classDeclarationSyntax,
                    basicsContext.contextNamespaceNameSyntax,
                    basicsContext.semanticModel,
                    basicsContext.context,
                    basicsContext.cancellationToken,
                    partialClassMemberDeclarationSyntaxList,
                    basicsContext.namespaceMemberDeclarationSyntaxList,
                    basicsContext.compilationMemberDeclarationSyntaxList
                ) {
                    nestContext = nestContext
                };
                UnifiedGenerator.generatedPartialClass
                (
                    context,
                    !context.className.Equals(context.className)
                );
                classDeclarationSyntax = classDeclarationSyntax.AddMembers(partialClassMemberDeclarationSyntaxList.ToArray());
                joinMemberDeclarationSyntaxList.Add(classDeclarationSyntax);
            }

            joinMemberDeclarationSyntaxList.AddRange(interiorMemberDeclarationSyntax);

            if (interiorMemberDeclarationSyntax.Count > 0) {

                ClassDeclarationSyntax declarationSyntax = basicsContext.contextTargetNode.CreateNewPartialClass().AddMembers(memberDeclarationSyntaxes.Where(m => m is not ClassDeclarationSyntax).ToArray());

                ClassDeclarationSyntax? classDeclarationSyntax = CSharpSyntaxTree.ParseText(declarationSyntax.NormalizeWhitespace().ToFullString())
                    .GetRoot()
                    .ChildNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

                if (classDeclarationSyntax is not null) {
                    List<MemberDeclarationSyntax> partialClassMemberDeclarationSyntaxList = new List<MemberDeclarationSyntax>();
                    UnifiedGenerator.generatedPartialClass
                    (
                        new BasicsContext
                        (
                            classDeclarationSyntax,
                            basicsContext.contextNamespaceNameSyntax,
                            basicsContext.semanticModel,
                            basicsContext.context,
                            basicsContext.cancellationToken,
                            partialClassMemberDeclarationSyntaxList,
                            basicsContext.namespaceMemberDeclarationSyntaxList,
                            basicsContext.compilationMemberDeclarationSyntaxList
                        ) {
                            nestContext = nestContext
                        },
                        false
                    );
                    joinMemberDeclarationSyntaxList.AddRange(partialClassMemberDeclarationSyntaxList);
                }

            }
        }

    }

}