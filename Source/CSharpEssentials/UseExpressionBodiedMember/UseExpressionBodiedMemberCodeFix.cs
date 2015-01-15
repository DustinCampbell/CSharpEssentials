using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.UseExpressionBodiedMember
{
    [ExportCodeFixProvider("Use Expression-bodied Member", LanguageNames.CSharp)]
    internal class UseExpressionBodiedMemberCodeFix : CodeFixProvider
    {
        public override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var declaration = root.FindNode(context.Span)?.FirstAncestorOrSelf<MemberDeclarationSyntax>();

            switch (declaration?.CSharpKind())
            {
                case SyntaxKind.MethodDeclaration:
                    context.RegisterFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (MethodDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.OperatorDeclaration:
                    context.RegisterFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (OperatorDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.ConversionOperatorDeclaration:
                    context.RegisterFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (ConversionOperatorDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.PropertyDeclaration:
                    context.RegisterFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (PropertyDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.IndexerDeclaration:
                    context.RegisterFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (IndexerDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;
            }
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.Body)))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, OperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.Body)))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, ConversionOperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.Body)))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.AccessorList)))
                .WithAccessorList(null)
                .WithSemicolon(GetSemicolon(declaration.AccessorList));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, IndexerDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.AccessorList)))
                .WithAccessorList(null)
                .WithSemicolon(GetSemicolon(declaration.AccessorList));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ExpressionSyntax GetExpression(BlockSyntax block)
        {
            return ((ReturnStatementSyntax)block.Statements[0]).Expression;
        }

        private static ExpressionSyntax GetExpression(AccessorListSyntax accessorList)
        {
            return ((ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0]).Expression;
        }

        private static SyntaxToken GetSemicolon(BlockSyntax block)
        {
            return ((ReturnStatementSyntax)block.Statements[0]).SemicolonToken;
        }

        private static SyntaxToken GetSemicolon(AccessorListSyntax accessorList)
        {
            return ((ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0]).SemicolonToken;
        }

        public override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return ImmutableArray.Create(DiagnosticIds.UseExpressionBodiedMember);
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
