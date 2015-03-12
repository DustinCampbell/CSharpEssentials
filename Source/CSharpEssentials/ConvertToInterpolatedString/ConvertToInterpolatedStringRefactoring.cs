using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using static System.Diagnostics.Debug;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpEssentials.ConvertToInterpolatedString
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp)]
    public class ConvertToInterpolatedStringRefactoring : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            var stringType = semanticModel.Compilation.GetTypeByMetadataName("System.String");
            if (stringType == null)
            {
                return;
            }

            var formatMethods = stringType
                .GetMembers("Format")
                .RemoveAll(IsValidStringFormatMethod);

            if (formatMethods.Length == 0)
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

            var invocation = root.FindNode(context.Span, getInnermostNodeForTie: true)?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            while (invocation != null)
            {
                if (invocation.ArgumentList != null)
                {
                    var arguments = invocation.ArgumentList.Arguments;
                    if (arguments.Count >= 2)
                    {
                        var firstArgument = arguments[0]?.Expression as LiteralExpressionSyntax;
                        if (firstArgument?.Token.IsKind(SyntaxKind.StringLiteralToken) == true)
                        {
                            var invocationSymbol = semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;
                            if (formatMethods.Contains(invocationSymbol))
                            {
                                break;
                            }
                        }
                    }
                }

                invocation = invocation.Parent?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            }

            if (invocation != null)
            {
                context.RegisterRefactoring(
                    CodeAction.Create("Convert to interpolated string", c => CreateInterpolatedString(invocation, context.Document, c)));
            }
        }

        private async Task<Document> CreateInterpolatedString(InvocationExpressionSyntax invocation, Document document, CancellationToken cancellationToken)
        {
            Assert(invocation.ArgumentList != null);
            Assert(invocation.ArgumentList.Arguments.Count >= 2);
            Assert(invocation.ArgumentList.Arguments[0].Expression.IsKind(SyntaxKind.StringLiteralExpression));

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var arguments = invocation.ArgumentList.Arguments;
            var text = ((LiteralExpressionSyntax)arguments[0].Expression).Token.ToString();

            var builder = ImmutableArray.CreateBuilder<ExpressionSyntax>();
            for (int i = 1; i < arguments.Count; i++)
            {
                builder.Add(CastAndParenthesize(arguments[i].Expression, semanticModel));
            }

            var expandedArguments = builder.ToImmutable();

            var interpolatedString = (InterpolatedStringExpressionSyntax)SyntaxFactory.ParseExpression("$" + text);

            var newInterpolatedString = InterpolatedStringRewriter.Visit(interpolatedString, expandedArguments);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(invocation, newInterpolatedString);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ExpressionSyntax Parenthesize(ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.ParenthesizedExpression)
                ? expression
                : ParenthesizedExpression(
                    openParenToken: Token(SyntaxTriviaList.Empty, SyntaxKind.OpenParenToken, SyntaxTriviaList.Empty),
                    expression: expression,
                    closeParenToken: Token(SyntaxTriviaList.Empty, SyntaxKind.CloseParenToken, SyntaxTriviaList.Empty))
                    .WithAdditionalAnnotations(Simplifier.Annotation);
        }

        private static ExpressionSyntax Cast(ExpressionSyntax expression, ITypeSymbol targetType)
        {
            if (targetType == null)
            {
                return expression;
            }

            var type = ParseTypeName(targetType.ToDisplayString());

            return CastExpression(type, Parenthesize(expression))
                .WithAdditionalAnnotations(Simplifier.Annotation);
        }

        private static ExpressionSyntax CastAndParenthesize(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var targetType = semanticModel.GetTypeInfo(expression).ConvertedType;

            return Parenthesize(Cast(expression, targetType));
        }

        private static bool IsValidStringFormatMethod(ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method || !symbol.IsStatic)
            {
                return true;
            }

            var methodSymbol = (IMethodSymbol)symbol;
            if (methodSymbol.Parameters.Length == 0)
            {
                return true;
            }

            var firstParameter = methodSymbol.Parameters[0];
            if (firstParameter?.Name != "format")
            {
                return true;
            }

            return false;
        }

        private class InterpolatedStringRewriter : CSharpSyntaxRewriter
        {
            private readonly ImmutableArray<ExpressionSyntax> expandedArguments;

            private InterpolatedStringRewriter(ImmutableArray<ExpressionSyntax> expandedArguments)
            {
                this.expandedArguments = expandedArguments;
            }

            public override SyntaxNode VisitInterpolation(InterpolationSyntax node)
            {
                var literalExpression = node.Expression as LiteralExpressionSyntax;
                if (literalExpression != null && literalExpression.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    var index = (int)literalExpression.Token.Value;
                    if (index >= 0 && index < expandedArguments.Length)
                    {
                        return node.WithExpression(expandedArguments[index]);
                    }
                }

                return base.VisitInterpolation(node);
            }

            public static InterpolatedStringExpressionSyntax Visit(InterpolatedStringExpressionSyntax interpolatedString, ImmutableArray<ExpressionSyntax> expandedArguments)
            {
                return (InterpolatedStringExpressionSyntax)new InterpolatedStringRewriter(expandedArguments).Visit(interpolatedString);
            }
        }
    }
}
