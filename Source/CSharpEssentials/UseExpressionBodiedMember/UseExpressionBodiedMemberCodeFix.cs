using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

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
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, OperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.Body)))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, ConversionOperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.Body)))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.AccessorList)))
                .WithAccessorList(null)
                .WithSemicolon(GetSemicolon(declaration.AccessorList))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, IndexerDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var newDeclaration = declaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression(declaration.AccessorList)))
                .WithAccessorList(null)
                .WithSemicolon(GetSemicolon(declaration.AccessorList))
                .WithAdditionalAnnotations(Formatter.Annotation);

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
            var semicolon = ((ReturnStatementSyntax)block.Statements[0]).SemicolonToken;

            var trivia = semicolon.TrailingTrivia.AsEnumerable();
            trivia = trivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));

            // Append trailing trivia from the closing brace.
            var closeBraceTrivia = block.CloseBraceToken.TrailingTrivia.AsEnumerable();
            trivia = trivia.Concat(closeBraceTrivia);

            return semicolon.WithTrailingTrivia(trivia);
        }

        private static SyntaxToken GetSemicolon(AccessorListSyntax accessorList)
        {
            var semicolon = ((ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0]).SemicolonToken;

            var trivia = semicolon.TrailingTrivia.AsEnumerable();
            trivia = trivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));

            // Append trailing trivia from the closing brace.
            var closeBraceTrivia = accessorList.CloseBraceToken.TrailingTrivia.AsEnumerable();
            trivia = trivia.Concat(closeBraceTrivia);

            return semicolon.WithTrailingTrivia(trivia);
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
