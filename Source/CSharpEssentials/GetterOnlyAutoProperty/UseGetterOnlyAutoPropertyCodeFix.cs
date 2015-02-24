using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpEssentials.GetterOnlyAutoProperty
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Use Getter-only Auto-property")]
    internal class UseGetterOnlyAutoPropertyCodeFix : CodeFixProvider
    {
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create("Use getter-only auto property", c => RemoveAccessor(context)),
                context.Diagnostics);

            return Task.FromResult(true);
        }

        private static async Task<Document> RemoveAccessor(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var accessorDeclaration = root.FindNode(context.Span)?.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            var accessorList = accessorDeclaration?.FirstAncestorOrSelf<AccessorListSyntax>();

            if (accessorList == null)
            {
                return context.Document;
            }

            var newAccessorList = accessorList
                .RemoveNode(accessorDeclaration, SyntaxRemoveOptions.KeepExteriorTrivia)
                .WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(accessorList, newAccessorList);

            return context.Document.WithSyntaxRoot(newRoot);
        }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.UseGetterOnlyAutoProperty);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}
