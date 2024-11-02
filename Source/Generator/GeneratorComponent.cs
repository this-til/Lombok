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
                                        new TypeContext
                                        (
                                            variableDeclaratorSyntax.Identifier.ToString(),
                                            fieldDeclaration.Declaration.Type.ToString(),
                                            basicsContext.contextTargetNode.getHasGenericName()
                                        ),
                                        fieldDeclaration.Declaration.Type,
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
                                    new TypeContext
                                    (
                                        propertyDeclaration.Identifier.ToString(),
                                        propertyDeclaration.Type.ToString(),
                                        basicsContext.contextTargetNode.getHasGenericName()
                                    ),
                                    propertyDeclaration.Type,
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

            List<A> attributeList = attributeSyntaxeList.Select
                (
                    a => restore
                    (
                        a.getAttributeArgumentsAsDictionary
                        (
                            fieldsContext.basicsContext.semanticModel
                        )
                    )
                )
                .ToList();

            for (var i = 0; i < attributeList.Count; i++) {

                AttributeContext<A> attributeContext = new AttributeContext<A>
                (
                    attributeList[i],
                    attributeSyntaxeList[i],
                    attributeList,
                    attributeSyntaxeList
                );

                switch (attributeContext.firstAttribute) {
                    case ListMetadataAttribute listMetadataAttribute: {
                        fieldsContext.typeContext.listCellType = listMetadataAttribute.listCellType!;

                        if (fieldsContext.typeSyntax is GenericNameSyntax genericNameSyntax) {
                            if (genericNameSyntax.TypeArgumentList.Arguments.Count > 0) {
                                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                                fieldsContext.typeContext.listCellType ??= firstOrDefault?.ToFullString()!;
                            }
                        }
                        if (fieldsContext.typeContext.listCellType is null) {
                            return;
                        }
                        break;
                    }

                    case MapMetadataAttribute mapMetadataAttribute: {
                        fieldsContext.typeContext.keyType = mapMetadataAttribute.keyType!;
                        fieldsContext.typeContext.valueType = mapMetadataAttribute.valueType!;

                        if (fieldsContext.typeSyntax is GenericNameSyntax genericNameSyntax) {
                            if (genericNameSyntax.TypeArgumentList.Arguments.Count > 0) {
                                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments.FirstOrDefault();
                                fieldsContext.typeContext.keyType ??= firstOrDefault?.ToFullString()!;
                            }
                            if (genericNameSyntax.TypeArgumentList.Arguments.Count > 1) {
                                TypeSyntax? firstOrDefault = genericNameSyntax.TypeArgumentList.Arguments[1];
                                fieldsContext.typeContext.valueType ??= firstOrDefault?.ToFullString()!;
                            }
                        }

                        if (fieldsContext.typeContext.keyType is null || fieldsContext.typeContext.valueType is null) {
                            return;
                        }

                        break;
                    }

                    case MetadataAttribute metadataAttribute: {
                        if (metadataAttribute.customType is not null) {
                            fieldsContext.typeContext.typeName = metadataAttribute.customType;
                        }
                        break;
                    }
                }

                try {
                    fill
                    (
                        new FieldsAttributeContext<A>
                        (
                            fieldsContext.basicsContext,
                            fieldsContext,
                            fieldsContext.typeContext,
                            attributeContext
                        )
                    );
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }

            }

        }

        protected abstract void fill(FieldsAttributeContext<A> fieldsAttributeContext);

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

            List<A> attributeList = attributeSyntaxeList.Select
                (
                    a => restore
                    (
                        a.getAttributeArgumentsAsDictionary
                        (
                            basicsContext.semanticModel
                        )
                    )
                )
                .ToList();

            for (var i = 0; i < attributeList.Count; i++) {

                AttributeContext<A> attributeContext = new AttributeContext<A>
                (
                    attributeList[i],
                    attributeSyntaxeList[i],
                    attributeList,
                    attributeSyntaxeList
                );

                try {
                    fill
                    (
                        new ClassAttributeContext<A>
                        (
                            basicsContext,
                            attributeContext
                        )
                    );
                }
                catch (Exception e) {
                    e.PrintExceptionSummaryAndStackTrace();
                }

            }

        }

        protected abstract void fill(ClassAttributeContext<A> fieldsAttributeContext);

    }

    public abstract class ClassFieldAttributeGeneratorComponent<FA, CA> : GeneratorComponent where CA : Attribute where FA : Attribute {

        protected virtual CA restoreCa(Dictionary<string, string> data) {
            return (CA)Activator.CreateInstance(typeof(FA), data);
        }

        protected virtual FA restoreFa(Dictionary<string, string> data) {
            return (FA)Activator.CreateInstance(typeof(FA), data);
        }

        public override void fill(BasicsContext basicsContext) {

            List<AttributeSyntax> caAttributeSyntax = basicsContext.contextTargetNode.AttributeLists.tryGetSpecifiedAttribute(typeof(CA).Name).ToList();
            List<CA> caList = caAttributeSyntax.Select(a => restoreCa(a.getAttributeArgumentsAsDictionary(basicsContext.semanticModel))).ToList();

            AttributeContext<CA>? caAttributeContext = null;

            if (caList.Count != 0) {
                caAttributeContext = new AttributeContext<CA>
                (
                    caList.First(),
                    caAttributeSyntax.First(),
                    caList,
                    caAttributeSyntax
                );
            }

            List<FieldsAttributeContext<FA>> fieldsAttributeContextList = new List<FieldsAttributeContext<FA>>();

            foreach (MemberDeclarationSyntax member in basicsContext.contextTargetNode.Members) {

                switch (member) {
                    // 检查成员是否是字段或属性  
                    case FieldDeclarationSyntax fieldDeclaration: {
                        List<AttributeSyntax> attributeSyntaxeList = member.AttributeLists.tryGetSpecifiedAttribute(typeof(FA).Name).ToList();
                        if (attributeSyntaxeList.Count == 0) {
                            break;
                        }
                        List<FA> attributeList = attributeSyntaxeList.Select(a => restoreFa(a.getAttributeArgumentsAsDictionary(basicsContext.semanticModel))).ToList();
                        foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables) {

                            TypeContext typeContext = new TypeContext
                            (
                                variableDeclaratorSyntax.Identifier.ToString(),
                                fieldDeclaration.Declaration.Type.ToString(),
                                basicsContext.contextTargetNode.getHasGenericName()
                            );
                            fieldsAttributeContextList.Add
                            (
                                new FieldsAttributeContext<FA>
                                (
                                    basicsContext,
                                    new FieldsContext
                                    (
                                        basicsContext,
                                        typeContext,
                                        fieldDeclaration.Declaration.Type,
                                        fieldDeclaration.AttributeLists,
                                        fieldDeclaration,
                                        variableDeclaratorSyntax,
                                        null
                                    ),
                                    typeContext,
                                    new AttributeContext<FA>
                                    (
                                        attributeList.First(),
                                        attributeSyntaxeList.First(),
                                        attributeList,
                                        attributeSyntaxeList
                                    )
                                )
                            );
                        }
                        break;
                    }
                    case PropertyDeclarationSyntax propertyDeclaration: {
                        List<AttributeSyntax> attributeSyntaxeList = member.AttributeLists.tryGetSpecifiedAttribute(typeof(FA).Name).ToList();
                        if (attributeSyntaxeList.Count == 0) {
                            break;
                        }
                        List<FA> attributeList = attributeSyntaxeList.Select(a => restoreFa(a.getAttributeArgumentsAsDictionary(basicsContext.semanticModel))).ToList();

                        TypeContext typeContext = new TypeContext
                        (
                            propertyDeclaration.Identifier.ToString(),
                            propertyDeclaration.Type.ToString(),
                            basicsContext.contextTargetNode.getHasGenericName()
                        );
                        fieldsAttributeContextList.Add
                        (
                            new FieldsAttributeContext<FA>
                            (
                                basicsContext,
                                new FieldsContext
                                (
                                    basicsContext,
                                    typeContext,
                                    propertyDeclaration.Type,
                                    propertyDeclaration.AttributeLists,
                                    null,
                                    null,
                                    propertyDeclaration
                                ),
                                typeContext,
                                new AttributeContext<FA>
                                (
                                    attributeList.First(),
                                    attributeSyntaxeList.First(),
                                    attributeList,
                                    attributeSyntaxeList
                                )
                            )
                        );
                        break;
                    }
                }
            }

            if (fieldsAttributeContextList.Count == 0) {
                return;
            }

            fill(new ClassFieldAttributeContext<FA, CA>(basicsContext, caAttributeContext, fieldsAttributeContextList));

        }

        public abstract void fill(ClassFieldAttributeContext<FA, CA> context);

    }

    [GeneratorComponent]
    public class GetGenerator : FieldsAttributeGeneratorComponent<GetAttribute> {

        protected sealed override void fill(FieldsAttributeContext<GetAttribute> fieldsAttributeContext) {

            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            fieldsAttributeContext.typeContext.typeName
                        ),
                        "get" + fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()
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
                                    fieldsAttributeContext.typeContext.fieldName
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class OpenGenerator : FieldsAttributeGeneratorComponent<OpenAttribute> {

        protected override void fill(FieldsAttributeContext<OpenAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        "open" + fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()
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
                                    $"Action<{fieldsAttributeContext.typeContext.typeName}>"
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
                                                    fieldsAttributeContext.typeContext.fieldName
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class SetGenerator : FieldsAttributeGeneratorComponent<SetAttribute> {

        protected override void fill(FieldsAttributeContext<SetAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        "set" + fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    fieldsAttributeContext.typeContext.fieldName
                                        .ToCamelCaseIdentifier()
                                        .genericEliminate()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    fieldsAttributeContext.typeContext.typeName
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
                                        fieldsAttributeContext.typeContext.fieldName
                                    )
                                ),
                                IdentifierName
                                (
                                    fieldsAttributeContext.typeContext.fieldName
                                        .ToCamelCaseIdentifier()
                                        .genericEliminate()
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class CountGenerator : FieldsAttributeGeneratorComponent<CountAttribute> {

        protected override void fill(FieldsAttributeContext<CountAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "int"
                        ),
                        $"countIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        ReturnStatement
                        (
                            IdentifierName($"this.{fieldsAttributeContext.typeContext.fieldName}.Count")
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class IndexGenerator : FieldsAttributeGeneratorComponent<IndexAttribute> {

        protected override void fill(FieldsAttributeContext<IndexAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            fieldsAttributeContext.typeContext.listCellType
                        ),
                        $"indexIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class AddGenerator : FieldsAttributeGeneratorComponent<AddAttribute> {

        protected override void fill(FieldsAttributeContext<AddAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"addIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier().genericEliminate()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + fieldsAttributeContext.typeContext.listCellType
                                        .ToPascalCaseIdentifier()
                                        .genericEliminate()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    fieldsAttributeContext.typeContext.listCellType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                                + fieldsAttributeContext.typeContext.listCellType
                                                    .ToPascalCaseIdentifier()
                                                    .genericEliminate()
                                            ) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveGenerator : FieldsAttributeGeneratorComponent<RemoveAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + fieldsAttributeContext.typeContext.listCellType
                                        .ToPascalCaseIdentifier()
                                        .genericEliminate()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    fieldsAttributeContext.typeContext.listCellType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                                + fieldsAttributeContext.typeContext.listCellType
                                                    .ToPascalCaseIdentifier()
                                                    .genericEliminate()
                                            ) // 引用参数i  
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainGenerator : FieldsAttributeGeneratorComponent<ContainAttribute> {

        protected override void fill(FieldsAttributeContext<ContainAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "a"
                                    + fieldsAttributeContext.typeContext.listCellType
                                        .ToPascalCaseIdentifier()
                                        .genericEliminate()
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    fieldsAttributeContext.typeContext.listCellType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                                + fieldsAttributeContext.typeContext.listCellType
                                                    .ToPascalCaseIdentifier()
                                                    .genericEliminate()
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForGenerator : FieldsAttributeGeneratorComponent<ForAttribute> {

        protected override void fill(FieldsAttributeContext<ForAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{fieldsAttributeContext.typeContext.listCellType}>"
                        ),
                        $"for{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        fieldsAttributeContext.attributeContext.firstAttribute.useYield
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
                                        fieldsAttributeContext.typeContext.fieldName
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
                                        fieldsAttributeContext.typeContext.fieldName
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class PutGenerator : FieldsAttributeGeneratorComponent<PutAttribute> {

        protected override void fill(FieldsAttributeContext<PutAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"putIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.keyType
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
                                    fieldsAttributeContext.typeContext.valueType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                                fieldsAttributeContext.typeContext.fieldName
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
                                                        fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class MapGetGenerator : FieldsAttributeGeneratorComponent<MapGetAttribute> {

        protected override void fill(FieldsAttributeContext<MapGetAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            fieldsAttributeContext.typeContext.valueType
                        ),
                        $"getIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.keyType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveKeyGenerator : FieldsAttributeGeneratorComponent<RemoveKeyAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveKeyAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeKeyIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.keyType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class RemoveValueGenerator : FieldsAttributeGeneratorComponent<RemoveValueAttribute> {

        protected override void fill(FieldsAttributeContext<RemoveValueAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "void"
                        ),
                        $"removeValueIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.keyType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainKeyGenerator : FieldsAttributeGeneratorComponent<ContainKeyAttribute> {

        protected override void fill(FieldsAttributeContext<ContainKeyAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containKeyIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.valueType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ContainValueGenerator : FieldsAttributeGeneratorComponent<ContainValueAttribute> {

        protected override void fill(FieldsAttributeContext<ContainValueAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        $"containValueIn{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
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
                                    fieldsAttributeContext.typeContext.valueType
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForKeyGenerator : FieldsAttributeGeneratorComponent<ForKeyAttribute> {

        protected override void fill(FieldsAttributeContext<ForKeyAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{fieldsAttributeContext.typeContext.keyType}>"
                        ),
                        $"for{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}Key"
                    )
                    .AddBodyStatements
                    (
                        fieldsAttributeContext.attributeContext.firstAttribute.useYield
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                            fieldsAttributeContext.typeContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Keys"
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForValueGenerator : FieldsAttributeGeneratorComponent<ForValueAttribute> {

        protected override void fill(FieldsAttributeContext<ForValueAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<{fieldsAttributeContext.typeContext.valueType}>"
                        ),
                        $"for{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}Value"
                    )
                    .AddBodyStatements
                    (
                        fieldsAttributeContext.attributeContext.firstAttribute.useYield
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
                                            fieldsAttributeContext.typeContext.fieldName
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
                                            fieldsAttributeContext.typeContext.fieldName
                                        )
                                    ),
                                    IdentifierName
                                    (
                                        "Values"
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class ForAllGenerator : FieldsAttributeGeneratorComponent<ForAllAttribute> {

        protected override void fill(FieldsAttributeContext<ForAllAttribute> fieldsAttributeContext) {
            fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            $"System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<{fieldsAttributeContext.typeContext.keyType}, {fieldsAttributeContext.typeContext.valueType}>>"
                        ),
                        $"for{fieldsAttributeContext.typeContext.fieldName.ToPascalCaseIdentifier()}"
                    )
                    .AddBodyStatements
                    (
                        fieldsAttributeContext.attributeContext.firstAttribute.useYield
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
                                        fieldsAttributeContext.typeContext.fieldName
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
                                        fieldsAttributeContext.typeContext.fieldName
                                    )
                                )
                            )
                    )
                    .applyAll(fieldsAttributeContext)
            );
        }

    }

    [GeneratorComponent]
    public sealed class PartialGenerator : ClassAttributeGeneratorComponent<IPartialAttribute> {

        protected override void fill(ClassAttributeContext<IPartialAttribute> fieldsAttributeContext) {

            AttributeContext<IPartialAttribute> attributeContext = fieldsAttributeContext.attributeContext;

            string? model = attributeContext.firstAttribute.model;

            if (model is null) {
                return;
            }

            Dictionary<string, string> fillMap = new Dictionary<string, string>();

            if (attributeContext.firstAttribute._customFill is not null) {
                foreach (KeyValuePair<string, string> keyValuePair in attributeContext.firstAttribute._customFill) {
                    fillMap[keyValuePair.Key] = keyValuePair.Value;
                }
            }
            fillMap["type"] = fieldsAttributeContext.basicsContext.contextTargetNode.toClassName();
            fillMap["namespace"] = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.Name.ToString();

            StringBuilder stringBuilder = new StringBuilder();
            model.format
            (
                stringBuilder,
                k => stringBuilder.Append
                (
                    fillMap[k]
                )
            );

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            switch (attributeContext.firstAttribute.partialPos) {
                case PartialPos.Interior:
                    fieldsAttributeContext.partialClass = fieldsAttributeContext.partialClass.AddMembers(memberDeclarationSyntaxes);
                    break;
                case PartialPos.UpLevel:
                    if (fieldsAttributeContext.basicsContext.nestContext is not null) {
                        fieldsAttributeContext.basicsContext.nestContext.partialClass.value = fieldsAttributeContext.basicsContext.nestContext.partialClass.value.AddMembers(memberDeclarationSyntaxes);
                        break;
                    }
                    fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.AddMembers(memberDeclarationSyntaxes);
                    break;
                case PartialPos.Namespace:
                    fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.AddMembers(memberDeclarationSyntaxes);
                    break;
                case PartialPos.Compilation:
                    fieldsAttributeContext.basicsContext.compilationUnitSyntax.value = fieldsAttributeContext.basicsContext.compilationUnitSyntax.value.AddMembers(memberDeclarationSyntaxes);
                    break;
            }

            /*
            if (!attributeContext.firstAttribute.allOverAgainGeneration) {
                return;
            }

            List<ClassDeclarationSyntax> classDeclarationSyntaxes = memberDeclarationSyntaxes.OfType<ClassDeclarationSyntax>().ToList();

            MemberDeclarationSyntax[] declarationSyntaxes = memberDeclarationSyntaxes.Where(m => m is not ClassDeclarationSyntax).ToArray();

            if (declarationSyntaxes.Length > 0 && attributeContext.firstAttribute.partialPos == PartialPos.Interior) {

                classDeclarationSyntaxes.Add(fieldsAttributeContext.basicsContext.partialClass.value.CreateNewPartialClass().AddMembers(declarationSyntaxes));

            }
            

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classDeclarationSyntaxes) {

                Ptr<ClassDeclarationSyntax> partialClass = new Ptr<ClassDeclarationSyntax>(classDeclarationSyntax.CreateNewPartialClass());
                
                UnifiedGenerator.generatedPartialClass
                (
                    new BasicsContext
                    (
                        classDeclarationSyntax,
                        fieldsAttributeContext.basicsContext.semanticModel,
                        fieldsAttributeContext.basicsContext.context,
                        fieldsAttributeContext.basicsContext.cancellationToken,
                        partialClass,
                        fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax,
                        fieldsAttributeContext.basicsContext.compilationUnitSyntax
                    ) {
                        nestContext = fieldsAttributeContext.basicsContext
                    }
                );
                
                switch (attributeContext.firstAttribute.partialPos) {
                    case PartialPos.Interior:
                    case PartialPos.UpLevel:
                        if (fieldsAttributeContext.basicsContext.nestContext is not null) {
                            fieldsAttributeContext.basicsContext.nestContext.partialClass.value = fieldsAttributeContext.basicsContext.nestContext.partialClass.value.AddMembers(partialClass.value);
                            break;
                        }
                        fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.AddMembers(partialClass.value);
                        break;
                    case PartialPos.Namespace:
                        fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.AddMembers(partialClass.value);
                        break;
                    case PartialPos.Compilation:
                        fieldsAttributeContext.basicsContext.compilationUnitSyntax.value = fieldsAttributeContext.basicsContext.compilationUnitSyntax.value.AddMembers(partialClass.value);
                        break;
                }
                
            }
            */

        }

    }

    [GeneratorComponent]
    public sealed class FreezeGenerator : ClassAttributeGeneratorComponent<IFreezeAttribute> {

        protected override void fill(ClassAttributeContext<IFreezeAttribute> fieldsAttributeContext) {

            Dictionary<string, string> fillMap = new Dictionary<string, string>();
            fillMap["type"] = fieldsAttributeContext.basicsContext.contextTargetNode.toClassName();
            fillMap["namespace"] = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.Name.ToString();
            string model = @"

public partial class {type} : Til.Lombok.IFreeze {{

    protected System.Collections.Generic.HashSet<string> _frozen = new System.Collections.Generic.HashSet<string>();

    public bool isFrozen(string tag) => _frozen.Contains(tag);

    public void frozen(string tag) => _frozen.Add(tag);

    protected void unFrozen(string tag) => _frozen.Remove(tag);

    public void validateNonFrozen(string tag) {{
        if (isFrozen(tag)) {{
            throw new System.InvalidOperationException(""Cannot modify frozen property"");
        }}
    }}

}}

";

            StringBuilder stringBuilder = new StringBuilder();

            model.format
            (
                stringBuilder,
                k => stringBuilder.Append
                (
                    fillMap[k]
                )
            );

            MemberDeclarationSyntax[] memberDeclarationSyntaxes = CSharpSyntaxTree.ParseText(stringBuilder.ToString())
                .GetRoot()
                .ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .ToArray();

            if (fieldsAttributeContext.basicsContext.nestContext is not null) {
                fieldsAttributeContext.basicsContext.nestContext.partialClass.value = fieldsAttributeContext.basicsContext.nestContext.partialClass.value.AddMembers(memberDeclarationSyntaxes);
            }
            else {
                fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value = fieldsAttributeContext.basicsContext.namespaceDeclarationSyntax.value.AddMembers(memberDeclarationSyntaxes);
            }

        }

    }

    [GeneratorComponent]
    public sealed class ToStringGenerator : ClassFieldAttributeGeneratorComponent<ToStringFieldAttribute, ToStringClassAttribute> {

        public override void fill(ClassFieldAttributeContext<ToStringFieldAttribute, ToStringClassAttribute> context) {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .Append
                (
                    '$'
                )
                .Append
                (
                    '"'
                )
                .Append
                (
                    context.basicsContext.contextTargetNode.toClassName()
                )
                .Append
                (
                    '('
                )
                .Append
                (
                    string.Join
                    (
                        ",",
                        context.fieldsAttributeContextList.Select
                        (
                            s => $"{s.typeContext.fieldName}={{this.{s.typeContext.fieldName}}}"
                        )
                    )
                );
            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {

                stringBuilder.Append
                    (
                        ','
                    )
                    .Append
                    (
                        "base={base.ToString()}"
                    );

            }

            stringBuilder.Append
                (
                    ')'
                )
                .Append
                (
                    '"'
                );

            context.partialClass = context.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "string"
                        ),
                        "ToString"
                    )
                    .AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        ),
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    )
                    .WithBody
                    (
                        Block
                        (
                            ReturnStatement
                            (
                                IdentifierName
                                (
                                    stringBuilder.ToString()
                                )
                            )
                        )
                    )
            );

        }

    }

    [GeneratorComponent]
    public sealed class EqualsGenerator : ClassFieldAttributeGeneratorComponent<EqualsFieldAttribute, EqualsClassAttribute> {

        public override void fill(ClassFieldAttributeContext<EqualsFieldAttribute, EqualsClassAttribute> context) {

            IsPatternExpressionSyntax isPatternExpressionSyntax = IsPatternExpression
            (
                ParseTypeName
                (
                    "obj"
                ),
                DeclarationPattern
                (
                    ParseTypeName
                    (
                        context.basicsContext.contextTargetNode.toClassName()
                    ),
                    SingleVariableDesignation
                    (
                        Identifier
                        (
                            "_obj"
                        )
                    )
                )
            );

            List<InvocationExpressionSyntax> list = new List<InvocationExpressionSyntax>();

            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {
                list.Add
                (
                    InvocationExpression
                    (
                        MemberAccessExpression
                        (
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName
                            (
                                "base"
                            ),
                            IdentifierName
                            (
                                "Equals"
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
                                            "_obj"
                                        )
                                    )
                                }
                            )
                        )
                    )
                );
            }

            foreach (FieldsAttributeContext<EqualsFieldAttribute>? equal in context.fieldsAttributeContextList) {

                string fieldName = equal.typeContext.fieldName;

                list.Add
                (
                    InvocationExpression
                    (
                        MemberAccessExpression
                        (
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName
                            (
                                "object"
                            ),
                            IdentifierName
                            (
                                "Equals"
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
                                            fieldName
                                        )
                                    ),
                                    Argument
                                    (
                                        MemberAccessExpression
                                        (
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName
                                            (
                                                "_obj"
                                            ),
                                            IdentifierName
                                            (
                                                fieldName
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    )
                );
            }

            ExpressionSyntax expression = null!;

            for (var i = 0; i < list.Count; i++) {
                if (i == 0) {
                    expression = list[0];
                    continue;
                }
                expression = BinaryExpression
                (
                    SyntaxKind.LogicalAndExpression,
                    expression,
                    list[i]
                );
            }

            context.partialClass = context.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "bool"
                        ),
                        "Equals"
                    )
                    .AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        ),
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    )
                    .AddParameterListParameters
                    (
                        Parameter
                            (
                                Identifier
                                (
                                    "obj"
                                )
                            )
                            .WithType
                            (
                                ParseTypeName
                                (
                                    "object"
                                )
                            )
                    )
                    .AddBodyStatements
                    (
                        IfStatement
                        (
                            BinaryExpression
                            (
                                SyntaxKind.EqualsExpression,
                                IdentifierName
                                (
                                    "obj"
                                ),
                                ThisExpression()
                            ),
                            Block
                            (
                                ReturnStatement
                                (
                                    LiteralExpression
                                    (
                                        SyntaxKind.TrueLiteralExpression
                                    )
                                )
                            )
                        ),
                        IfStatement
                        (
                            isPatternExpressionSyntax.WithPattern
                            (
                                UnaryPattern
                                (
                                    Token
                                    (
                                        SyntaxKind.NotKeyword
                                    ),
                                    isPatternExpressionSyntax.Pattern
                                )
                            ),
                            Block
                            (
                                ReturnStatement
                                (
                                    LiteralExpression
                                    (
                                        SyntaxKind.FalseLiteralExpression
                                    )
                                )
                            )
                        ),
                        ReturnStatement
                        (
                            expression
                        )
                    )
            );

        }

    }

    [GeneratorComponent]
    public sealed class HashCodeGenerator : ClassFieldAttributeGeneratorComponent<HashCodeFieldAttribute, HashCodeClassAttribute> {

        public override void fill(ClassFieldAttributeContext<HashCodeFieldAttribute, HashCodeClassAttribute> context) {
            List<StatementSyntax> list = new List<StatementSyntax>();

            if (context.caAttributeContext?.firstAttribute.hasBase ?? false) {
                list.Add
                (
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName
                            (
                                "h"
                            ),
                            BinaryExpression
                            (
                                SyntaxKind.AddExpression,
                                BinaryExpression
                                (
                                    SyntaxKind.MultiplyExpression,
                                    IdentifierName
                                    (
                                        "h"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal
                                        (
                                            23
                                        )
                                    )
                                ),
                                InvocationExpression
                                (
                                    MemberAccessExpression
                                    (
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName
                                        (
                                            "base"
                                        ),
                                        IdentifierName
                                        (
                                            nameof(GetHashCode)
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            }

            foreach (FieldsAttributeContext<HashCodeFieldAttribute>? se in context.fieldsAttributeContextList) {
                bool isValueType = (se.basicsContext.semanticModel.GetSymbolInfo(se.fieldsContext.typeSyntax).Symbol as ITypeSymbol)?.IsValueType ?? false;
                string fieldName = se.typeContext.fieldName;

                list.Add
                (
                    ExpressionStatement
                    (
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName
                            (
                                "h"
                            ),
                            BinaryExpression
                            (
                                SyntaxKind.AddExpression,
                                BinaryExpression
                                (
                                    SyntaxKind.MultiplyExpression,
                                    IdentifierName
                                    (
                                        "h"
                                    ),
                                    LiteralExpression
                                    (
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal
                                        (
                                            23
                                        )
                                    )
                                ),
                                isValueType
                                    ? InvocationExpression
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
                                                    fieldName
                                                )
                                            ),
                                            IdentifierName
                                            (
                                                nameof(GetHashCode)
                                            )
                                        )
                                    )
                                    : BinaryExpression
                                    (
                                        SyntaxKind.CoalesceExpression,
                                        ConditionalAccessExpression
                                        (
                                            MemberAccessExpression
                                            (
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName
                                                (
                                                    fieldName
                                                )
                                            ),
                                            InvocationExpression
                                            (
                                                MemberBindingExpression
                                                (
                                                    Token
                                                    (
                                                        SyntaxKind.DotToken
                                                    ),
                                                    IdentifierName
                                                    (
                                                        nameof(GetHashCode)
                                                    )
                                                )
                                            )
                                        ),
                                        LiteralExpression
                                        (
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal
                                            (
                                                0
                                            )
                                        )
                                    )
                            )
                        )
                    )
                );
            }

            list.Add
            (
                ReturnStatement
                (
                    IdentifierName
                    (
                        "h"
                    )
                )
            );

            context.partialClass = context.partialClass.AddMembers
            (
                MethodDeclaration
                    (
                        IdentifierName
                        (
                            "int"
                        ),
                        "GetHashCode"
                    )
                    .AddModifiers
                    (
                        Token
                        (
                            SyntaxKind.PublicKeyword
                        ),
                        Token
                        (
                            SyntaxKind.OverrideKeyword
                        )
                    )
                    .AddBodyStatements
                    (
                        Block
                        (
                            CheckedStatement
                            (
                                SyntaxKind.UncheckedStatement,
                                Block()
                                    .AddStatements
                                    (
                                        LocalDeclarationStatement
                                        (
                                            VariableDeclaration
                                            (
                                                PredefinedType
                                                (
                                                    Token
                                                    (
                                                        SyntaxKind.IntKeyword
                                                    )
                                                ),
                                                SeparatedList
                                                (
                                                    new[] {
                                                        VariableDeclarator
                                                        (
                                                            Identifier
                                                            (
                                                                "h"
                                                            ),
                                                            null,
                                                            EqualsValueClause
                                                            (
                                                                LiteralExpression
                                                                (
                                                                    SyntaxKind.NumericLiteralExpression,
                                                                    Literal
                                                                    (
                                                                        17
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    }
                                                )
                                            )
                                        )
                                    )
                                    .AddStatements
                                    (
                                        list.ToArray()
                                    )
                            )
                        )
                    )
            );
        }

    }

}
