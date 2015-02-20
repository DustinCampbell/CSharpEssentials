using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.GetterOnlyAutoProperty
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseGetterOnlyAutoPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseGetterOnlyAutoProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalzyerAccessorDeclaration, SyntaxKind.SetAccessorDeclaration);
        }

        private void AnalzyerAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var accessorDeclaration = (AccessorDeclarationSyntax)context.Node;

            if (accessorDeclaration.Body == null &&
                !accessorDeclaration.SemicolonToken.IsMissing &&
                accessorDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.UseGetterOnlyAutoProperty,
                        accessorDeclaration.GetLocation()));
            }
        }
    }
}
