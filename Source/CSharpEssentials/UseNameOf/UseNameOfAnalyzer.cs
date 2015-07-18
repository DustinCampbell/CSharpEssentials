using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.UseNameOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseNameOfAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseNameOf);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.Argument);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode())
            {
                return;
            }

            var argument = (ArgumentSyntax)context.Node;
            var parameters = GetParametersInScope(argument);
            if (!parameters.Any())
            {
                return;
            }

            var expression = argument.Expression;

            if (expression?.IsKind(SyntaxKind.StringLiteralExpression) == true)
            {
                var stringText = ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;

                // Are there are any parameters in scope with the same name as the text of the string
                // literal passed to this argument?
                if (parameters.Any(p => string.Equals(p.Identifier.ValueText, stringText, StringComparison.Ordinal)))
                {
                    var argumentInfo = context.SemanticModel.GetArgumentInfo(argument);

                    // We could do better here. Skeet checked for an InvokeParameterNameAttribute.
                    // Is that the right approach? For now, we'll just foolishly check for a particular
                    // parameter name.
                    if (argumentInfo.Parameter?.Name == "paramName")
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseNameOf, expression.GetLocation(), stringText));
                    }
                }
            }
        }

        private static IEnumerable<ParameterSyntax> GetParametersInScope(SyntaxNode node)
        {
            foreach (var ancestor in node.AncestorsAndSelf())
            {
                if (ancestor.IsKind(SyntaxKind.SimpleLambdaExpression))
                {
                    yield return ((SimpleLambdaExpressionSyntax)ancestor).Parameter;
                }
                else
                {
                    var parameterList = ancestor.GetParameterList();
                    if (parameterList != null)
                    {
                        foreach (var parameter in parameterList.Parameters)
                        {
                            yield return parameter;
                        }
                    }
                }
            }
        }
    }
}
