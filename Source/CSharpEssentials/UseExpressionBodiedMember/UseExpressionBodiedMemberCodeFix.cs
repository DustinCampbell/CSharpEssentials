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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Use Expression-bodied Member")]
    internal class UseExpressionBodiedMemberCodeFix : CodeFixProvider
    {
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var declaration = root.FindNode(context.Span)?.FirstAncestorOrSelf<MemberDeclarationSyntax>();

            switch (declaration?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    context.RegisterCodeFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (MethodDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.OperatorDeclaration:
                    context.RegisterCodeFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (OperatorDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.ConversionOperatorDeclaration:
                    context.RegisterCodeFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (ConversionOperatorDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.PropertyDeclaration:
                    context.RegisterCodeFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (PropertyDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;

                case SyntaxKind.IndexerDeclaration:
                    context.RegisterCodeFix(
                        CodeAction.Create("Use expression-bodied member", c => ReplaceWithExpressionBodiedMember(context.Document, (IndexerDeclarationSyntax)declaration, c)),
                        context.Diagnostics);
                    break;
            }
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia;
            var expression = GetExpressionAndLeadingTrivia(declaration.Body, out leadingTrivia);

            var declarationTrivia = declaration.GetLeadingTrivia();
            declarationTrivia = declarationTrivia.AddRange(leadingTrivia);

            var newDeclaration = declaration
                .WithLeadingTrivia(declarationTrivia)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, OperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia;
            var expression = GetExpressionAndLeadingTrivia(declaration.Body, out leadingTrivia);

            var declarationTrivia = declaration.GetLeadingTrivia();
            declarationTrivia = declarationTrivia.AddRange(leadingTrivia);

            var newDeclaration = declaration
                .WithLeadingTrivia(declarationTrivia)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, ConversionOperatorDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia;
            var expression = GetExpressionAndLeadingTrivia(declaration.Body, out leadingTrivia);

            var declarationTrivia = declaration.GetLeadingTrivia();
            declarationTrivia = declarationTrivia.AddRange(leadingTrivia);

            var newDeclaration = declaration
                .WithLeadingTrivia(declarationTrivia)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithBody(null)
                .WithSemicolonToken(GetSemicolon(declaration.Body))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, PropertyDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia;
            var expression = GetExpression(declaration.AccessorList, out leadingTrivia);

            var declarationTrivia = declaration.GetLeadingTrivia();
            declarationTrivia = declarationTrivia.AddRange(leadingTrivia);

            var newDeclaration = declaration
                .WithLeadingTrivia(declarationTrivia)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithAccessorList(null)
                .WithSemicolonToken(GetSemicolon(declaration.AccessorList))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ReplaceWithExpressionBodiedMember(Document document, IndexerDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia;
            var expression = GetExpression(declaration.AccessorList, out leadingTrivia);

            var declarationTrivia = declaration.GetLeadingTrivia();
            declarationTrivia = declarationTrivia.AddRange(leadingTrivia);

            var newDeclaration = declaration
                .WithLeadingTrivia(declarationTrivia)
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithAccessorList(null)
                .WithSemicolonToken(GetSemicolon(declaration.AccessorList))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(declaration, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ExpressionSyntax GetExpressionAndLeadingTrivia(BlockSyntax block, out SyntaxTriviaList leadingTrivia)
        {
            var returnStatement = (ReturnStatementSyntax)block.Statements[0];
            leadingTrivia = returnStatement.GetLeadingTrivia();

            // TODO: Concatenate any trivia between the return keyword and the expression?

            return returnStatement.Expression;
        }

        private static ExpressionSyntax GetExpression(AccessorListSyntax accessorList, out SyntaxTriviaList leadingTrivia)
        {
            var returnStatement = (ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0];
            leadingTrivia = returnStatement.GetLeadingTrivia();

            // TODO: Concatenate any trivia between the return keyword and the expression?

            return returnStatement.Expression;
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

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.UseExpressionBodiedMember);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}
