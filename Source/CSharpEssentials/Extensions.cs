using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials
{
    internal static class Extensions
    {
        public static ImmutableArray<IParameterSymbol> GetParameters(this ISymbol symbol)
        {
            switch (symbol?.Kind)
            {
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).Parameters;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Parameters;
                default:
                    return ImmutableArray<IParameterSymbol>.Empty;
            }
        }

        public static BaseParameterListSyntax GetParameterList(this SyntaxNode node)
        {
            switch (node?.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)node).ParameterList;
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)node).ParameterList;
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)node).ParameterList;
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((ParenthesizedLambdaExpressionSyntax)node).ParameterList;
                case SyntaxKind.AnonymousMethodExpression:
                    return ((AnonymousMethodExpressionSyntax)node).ParameterList;
                default:
                    return null;
            }
        }

        internal struct ArgumentInfo
        {
            public readonly ISymbol MethodOrProperty;
            public readonly IParameterSymbol Parameter;

            public ArgumentInfo(ISymbol methodOrProperty, IParameterSymbol parameter)
            {
                this.MethodOrProperty = methodOrProperty;
                this.Parameter = parameter;
            }
        }

        public static ArgumentInfo GetArgumentInfo(this SemanticModel semanticModel, ArgumentSyntax argument)
        {
            if (semanticModel == null)
            {
                throw new ArgumentNullException("semanticModel");
            }

            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }

            var argumentList = argument.Parent as ArgumentListSyntax;
            if (argumentList == null)
            {
                return default(ArgumentInfo);
            }

            var expression = argumentList.Parent as ExpressionSyntax;
            if (expression == null)
            {
                return default(ArgumentInfo);
            }

            var methodOrProperty = semanticModel.GetSymbolInfo(expression).Symbol;
            if (methodOrProperty == null)
            {
                return default(ArgumentInfo);
            }

            var parameters = methodOrProperty.GetParameters();
            if (parameters.Length == 0)
            {
                return default(ArgumentInfo);
            }

            if (argument.NameColon != null)
            {
                if (argument.NameColon.Name == null)
                {
                    return default(ArgumentInfo);
                }

                // We've got a named argument...
                var nameText = argument.NameColon.Name.Identifier.ValueText;
                if (nameText == null)
                {
                    return default(ArgumentInfo);
                }

                foreach (var parameter in parameters)
                {
                    if (string.Equals(parameter.Name, nameText, StringComparison.Ordinal))
                    {
                        return new ArgumentInfo(methodOrProperty, parameter);
                    }
                }
            }
            else
            {
                // Positional argument...
                var index = argumentList.Arguments.IndexOf(argument);
                if (index < 0)
                {
                    return default(ArgumentInfo);
                }

                if (index < parameters.Length)
                {
                    return new ArgumentInfo(methodOrProperty, parameters[index]);
                }

                if (index >= parameters.Length &&
                    parameters[parameters.Length - 1].IsParams)
                {
                    return new ArgumentInfo(methodOrProperty, parameters[parameters.Length - 1]);
                }
            }

            return default(ArgumentInfo);
        }

        private static bool IsGeneratedCode(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var fileName = Path.GetFileName(filePath);
            if (fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (fileNameWithoutExtension.EndsWith("AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.EndsWith(".g.i", StringComparison.OrdinalIgnoreCase) ||
                fileNameWithoutExtension.EndsWith(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static bool IsGeneratedCode(this SyntaxTree tree)
        {
            return IsGeneratedCode(tree.FilePath);
        }
    }
}
