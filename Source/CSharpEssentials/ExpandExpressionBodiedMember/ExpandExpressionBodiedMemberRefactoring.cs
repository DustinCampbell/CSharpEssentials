using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpEssentials.ExpandExpressionBodiedMember
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp)]
    public class ExpandExpressionBodiedMemberRefactoring : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

            var declaration = root.FindNode(context.Span).FirstAncestorOrSelf<MemberDeclarationSyntax>();

            switch (declaration?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    var methodDeclaration = (MethodDeclarationSyntax)declaration;
                    if (methodDeclaration.ExpressionBody != null)
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Expand expression-bodied member",
                                c => HandleMethodDeclaration(methodDeclaration, context.Document, c)));
                    }

                    break;

                case SyntaxKind.OperatorDeclaration:
                    var operatorDeclaration = (OperatorDeclarationSyntax)declaration;
                    if (operatorDeclaration.ExpressionBody != null)
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Expand expression-bodied member",
                                c => HandleOperatorDeclaration(operatorDeclaration, context.Document, c)));
                    }

                    break;

                case SyntaxKind.ConversionOperatorDeclaration:
                    var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)declaration;
                    if (conversionOperatorDeclaration.ExpressionBody != null)
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Expand expression-bodied member",
                                c => HandleConversionOperatorDeclaration(conversionOperatorDeclaration, context.Document, c)));
                    }

                    break;

                case SyntaxKind.PropertyDeclaration:
                    var propertyDeclaration = (PropertyDeclarationSyntax)declaration;
                    if (propertyDeclaration.ExpressionBody != null)
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Expand expression-bodied member",
                                c => HandlePropertyDeclaration(propertyDeclaration, context.Document, c)));
                    }

                    break;

                case SyntaxKind.IndexerDeclaration:
                    var indexerDeclaration = (IndexerDeclarationSyntax)declaration;
                    if (indexerDeclaration.ExpressionBody != null)
                    {
                        context.RegisterRefactoring(
                            CodeAction.Create(
                                "Expand expression-bodied member",
                                c => HandleIndexerDeclaration(indexerDeclaration, context.Document, c)));
                    }

                    break;
            }
        }

        private async Task<Document> HandleMethodDeclaration(MethodDeclarationSyntax declaration, Document document, CancellationToken cancellationToken)
        {
            var returnStatement = SyntaxFactory.ReturnStatement(
                returnKeyword: SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                expression: declaration.ExpressionBody.Expression,
                semicolonToken: declaration.SemicolonToken);

            var newDeclaration = declaration
                .WithBody(SyntaxFactory.Block(returnStatement))
                .WithExpressionBody(null)
                .WithSemicolonToken(default(SyntaxToken))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> HandleOperatorDeclaration(OperatorDeclarationSyntax declaration, Document document, CancellationToken cancellationToken)
        {
            var returnStatement = SyntaxFactory.ReturnStatement(
                returnKeyword: SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                expression: declaration.ExpressionBody.Expression,
                semicolonToken: declaration.SemicolonToken);

            var newDeclaration = declaration
                .WithBody(SyntaxFactory.Block(returnStatement))
                .WithExpressionBody(null)
                .WithSemicolonToken(default(SyntaxToken))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> HandleConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax declaration, Document document, CancellationToken cancellationToken)
        {
            var returnStatement = SyntaxFactory.ReturnStatement(
                returnKeyword: SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                expression: declaration.ExpressionBody.Expression,
                semicolonToken: declaration.SemicolonToken);

            var newDeclaration = declaration
                .WithBody(SyntaxFactory.Block(returnStatement))
                .WithExpressionBody(null)
                .WithSemicolonToken(default(SyntaxToken))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> HandlePropertyDeclaration(PropertyDeclarationSyntax declaration, Document document, CancellationToken cancellationToken)
        {
            var returnStatement = SyntaxFactory.ReturnStatement(
                returnKeyword: SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                expression: declaration.ExpressionBody.Expression,
                semicolonToken: declaration.SemicolonToken);

            var accessorDeclaration = SyntaxFactory.AccessorDeclaration(
                kind: SyntaxKind.GetAccessorDeclaration,
                body: SyntaxFactory.Block(returnStatement));

            var newDeclaration = declaration
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList(accessorDeclaration)))
                .WithExpressionBody(null)
                .WithSemicolonToken(default(SyntaxToken))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private async Task<Document> HandleIndexerDeclaration(IndexerDeclarationSyntax declaration, Document document, CancellationToken cancellationToken)
        {
            var returnStatement = SyntaxFactory.ReturnStatement(
                returnKeyword: SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                expression: declaration.ExpressionBody.Expression,
                semicolonToken: declaration.SemicolonToken);

            var accessorDeclaration = SyntaxFactory.AccessorDeclaration(
                kind: SyntaxKind.GetAccessorDeclaration,
                body: SyntaxFactory.Block(returnStatement));

            var newDeclaration = declaration
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList(accessorDeclaration)))
                .WithExpressionBody(null)
                .WithSemicolonToken(default(SyntaxToken))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
