using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.UseExpressionBodiedMember
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseExpressionBodiedMemberAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.FadedToken, DiagnosticDescriptors.UseExpressionBodiedMember);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(HandleMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleOperatorDeclaration, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(HandleConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(HandlePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(HandleIndexerDeclaration, SyntaxKind.IndexerDeclaration);
        }

        private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode(context.CancellationToken))
            {
                return;
            }

            var methodDecl = (MethodDeclarationSyntax)context.Node;
            if (methodDecl.ExpressionBody != null)
            {
                return;
            }

            if ((methodDecl.ReturnType as PredefinedTypeSyntax)?.Keyword.IsKind(SyntaxKind.VoidKeyword) == true)
            {
                return;
            }

            if (!TryHandleBlock(context, methodDecl.Body))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseExpressionBodiedMember, methodDecl.GetLocation()));
        }

        private void HandleOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode(context.CancellationToken))
            {
                return;
            }

            var operatorDecl = (OperatorDeclarationSyntax)context.Node;
            if (operatorDecl.ExpressionBody != null)
            {
                return;
            }

            if (!TryHandleBlock(context, operatorDecl.Body))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseExpressionBodiedMember, operatorDecl.GetLocation()));
        }

        private void HandleConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode(context.CancellationToken))
            {
                return;
            }

            var conversionOperatorDecl = (ConversionOperatorDeclarationSyntax)context.Node;
            if (conversionOperatorDecl.ExpressionBody != null)
            {
                return;
            }

            if (!TryHandleBlock(context, conversionOperatorDecl.Body))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseExpressionBodiedMember, conversionOperatorDecl.GetLocation()));
        }

        private static void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode(context.CancellationToken))
            {
                return;
            }

            var propertyDecl = (PropertyDeclarationSyntax)context.Node;
            if (propertyDecl.ExpressionBody != null)
            {
                return;
            }

            if (!TryHandleAccessorList(context, propertyDecl.AccessorList))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseExpressionBodiedMember, propertyDecl.GetLocation()));
        }

        private void HandleIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode(context.CancellationToken))
            {
                return;
            }

            var indexerDecl = (IndexerDeclarationSyntax)context.Node;
            if (indexerDecl.ExpressionBody != null)
            {
                return;
            }

            if (!TryHandleAccessorList(context, indexerDecl.AccessorList))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseExpressionBodiedMember, indexerDecl.GetLocation()));
        }

        private static bool TryHandleBlock(SyntaxNodeAnalysisContext context, BlockSyntax block)
        {
            if (block == null)
            {
                return false;
            }

            var statements = block.Statements;
            if (statements.Count == 0)
            {
                return false;
            }

            var returnStatement = statements[0] as ReturnStatementSyntax;
            if (returnStatement == null ||
                returnStatement.Expression == null)
            {
                return false;
            }

            FadeOutToken(context, block.OpenBraceToken);
            FadeOutToken(context, returnStatement.ReturnKeyword);
            FadeOutToken(context, block.CloseBraceToken);

            return true;
        }

        private static bool TryHandleAccessorList(SyntaxNodeAnalysisContext context, AccessorListSyntax accessorList)
        {
            if (accessorList == null)
            {
                return false;
            }

            var accessors = accessorList.Accessors;
            if (accessors.Count != 1)
            {
                return false;
            }

            var accessor = accessors[0];
            if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
            {
                return false;
            }

            if (accessor.AttributeLists.Count != 0)
            {
                return false;
            }

            var accessorBody = accessor.Body;
            if (accessorBody == null)
            {
                return false;
            }

            var statements = accessorBody.Statements;
            if (statements.Count == 0)
            {
                return false;
            }

            var returnStatement = statements[0] as ReturnStatementSyntax;
            if (returnStatement == null ||
                returnStatement.Expression == null)
            {
                return false;
            }

            FadeOutToken(context, accessorList.OpenBraceToken);
            FadeOutToken(context, accessor.Keyword);
            FadeOutToken(context, accessorBody.OpenBraceToken);
            FadeOutToken(context, returnStatement.ReturnKeyword);
            FadeOutToken(context, accessorBody.CloseBraceToken);
            FadeOutToken(context, accessorList.CloseBraceToken);

            return true;
        }

        private static void FadeOutToken(SyntaxNodeAnalysisContext context, SyntaxToken token)
        {
            if (!token.IsMissing)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FadedToken, token.GetLocation()));
            }
        }
    }
}
