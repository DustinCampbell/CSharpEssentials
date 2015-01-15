using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpEssentials.UseNameOf
{
    [ExportCodeFixProvider("Use NameOf", LanguageNames.CSharp)]
    internal class UseNameOfCodeFix : CodeFixProvider
    {
        public override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(CancellationToken.None);

            var literalExpression = root.FindNode(context.Span, getInnermostNodeForTie: true) as LiteralExpressionSyntax;
            if (literalExpression != null)
            {
                context.RegisterFix(
                    CodeAction.Create("Use NameOf", c => ReplaceWithNameOf(context.Document, literalExpression, c)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> ReplaceWithNameOf(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
        {
            var stringText = literalExpression.Token.ValueText;
            var nameOfExpression = InvocationExpression(
                expression: IdentifierName("nameof"),
                argumentList: ArgumentList(
                    arguments: SingletonSeparatedList(Argument(IdentifierName(stringText)))));

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode<SyntaxNode, ExpressionSyntax>(literalExpression, nameOfExpression);

            return document.WithSyntaxRoot(newRoot);
        }

        public override ImmutableArray<string> GetFixableDiagnosticIds() => ImmutableArray.Create(DiagnosticIds.UseNameOf);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}
