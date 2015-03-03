using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading;

namespace CSharpEssentials.GetterOnlyAutoProperty
{
    /// <summary>
    /// An analyzer that looks for C# auto properties that could be made readonly.
    /// Such properties have a private setter that is never invoked outside a constructor.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class UseGetterOnlyAutoPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseGetterOnlyAutoProperty);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(OnType, SymbolKind.NamedType);
        }

        private static void OnType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;
            var candidates = GetAutoPropsWithPrivateSetters(type, context.CancellationToken);
            if (candidates == null || candidates.Count == 0)
            {
                return;
            }

            // Staying within this type, look for assignments to candidate properties.
            // This outer loop is for partial types.
            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                SemanticModel model = null;

                // Walk the tree looking for identifiers
                foreach (var node in reference.GetSyntax(context.CancellationToken).DescendantNodes())
                {
                    if (node.IsKind(SyntaxKind.IdentifierName))
                    {
                        if (model == null)
                        {
                            model = context.Compilation.GetSemanticModel(reference.SyntaxTree);
                        }

                        var property = model.GetSymbolInfo(node, context.CancellationToken).Symbol;
                        if (property?.Kind == SymbolKind.Property &&
                            candidates.Contains(property) &&
                            IsIdentifierWithinAnAssignmentButNotInAConstructor(node, property, model, context.CancellationToken))
                        {
                            if (candidates.Remove(property) && candidates.Count == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // Anything left in the candidates set should be reported.
            foreach (var candidate in candidates)
            {
                context.ReportDiagnostic(CreateDiagnostic(candidate, context.CancellationToken));
            }
        }

        private static bool IsIdentifierWithinAnAssignmentButNotInAConstructor(
            SyntaxNode identifier,
            ISymbol identifierSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            // Is the property being updated but not within the constructor?
            var updatingExpression = UpdatingExpression(identifier);
            if (updatingExpression == null)
            {
                return false;
            }

            // Is this an assignment to the property that is not within a constructor (for the identifier's containing type)?
            var updatedSymbol = semanticModel.GetSymbolInfo(updatingExpression, cancellationToken).Symbol;
            return updatedSymbol == identifierSymbol && !IsWithinConstructorOf(updatingExpression.Parent, identifierSymbol.ContainingType, semanticModel, cancellationToken);
        }

        private static HashSet<ISymbol> GetAutoPropsWithPrivateSetters(INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            HashSet<ISymbol> candidates = null;

            var allProperties = type.GetMembers().Where(s => s.Kind == SymbolKind.Property);
            foreach (var property in allProperties)
            {
                var propertySymbol = (IPropertySymbol)property;
                if (!propertySymbol.IsReadOnly && propertySymbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
                {
                    var setMethod = propertySymbol.SetMethod;
                    if (setMethod != null && setMethod.DeclaredAccessibility == Accessibility.Private)
                    {
                        // Find the syntax for the setter.
                        var reference = setMethod.DeclaringSyntaxReferences.FirstOrDefault();
                        if (reference != null)
                        {
                            var declaration = reference.GetSyntax(cancellationToken) as AccessorDeclarationSyntax;
                            if (declaration != null && declaration.Body == null)
                            {
                                // An empty body indicates it's an auto-prop
                                (candidates ?? (candidates = new HashSet<ISymbol>())).Add(propertySymbol);
                            }
                        }
                    }
                }
            }

            return candidates;
        }

        private static SyntaxNode UpdatingExpression(SyntaxNode node)
        {
            var identifier = node;
            for (node = node.Parent; node != null; node = node.Parent)
            {
                switch (node.Kind())
                {
                    // Simple or compound assignment
                    case SyntaxKind.SimpleAssignmentExpression:
                    case SyntaxKind.OrAssignmentExpression:
                    case SyntaxKind.AndAssignmentExpression:
                    case SyntaxKind.ExclusiveOrAssignmentExpression:
                    case SyntaxKind.AddAssignmentExpression:
                    case SyntaxKind.SubtractAssignmentExpression:
                    case SyntaxKind.MultiplyAssignmentExpression:
                    case SyntaxKind.DivideAssignmentExpression:
                    case SyntaxKind.ModuloAssignmentExpression:
                    case SyntaxKind.LeftShiftAssignmentExpression:
                    case SyntaxKind.RightShiftAssignmentExpression:
                        var assignment = (AssignmentExpressionSyntax)node;
                        return assignment.Left;

                    // Prefix unary expression
                    case SyntaxKind.PreIncrementExpression:
                    case SyntaxKind.PreDecrementExpression:
                        return ((PrefixUnaryExpressionSyntax)node).Operand;

                    // Postfix unary expression
                    case SyntaxKind.PostIncrementExpression:
                    case SyntaxKind.PostDecrementExpression:
                        return ((PostfixUnaryExpressionSyntax)node).Operand;

                    // Early loop termination
                    case SyntaxKind.Block:
                    case SyntaxKind.ExpressionStatement:
                        return null;
                }
            }

            return null;
        }

        private static bool IsWithinConstructorOf(SyntaxNode node, INamedTypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            // Are we in a constructor?
            for (; node != null; node = node.Parent)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.ConstructorDeclaration:
                        // In a constructor. Is it the constructor for the type that contains the property?
                        var constructorSymbol = semanticModel.GetDeclaredSymbol(node, cancellationToken);
                        return constructorSymbol != null && (object)constructorSymbol.ContainingType == type;

                    // If it's in a lambda expression, even if in a constructor, then it counts as a
                    // non-constructor case.
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                        return false;

                    // Early out cases. There are many others, but these are the common ones.
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.StructDeclaration:
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.PropertyDeclaration:
                        return false;
                }
            }

            return false;
        }

        private static Diagnostic CreateDiagnostic(ISymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                // The span should be on the setter, but the symbol is for the whole property declaration.
                var property = (PropertyDeclarationSyntax)reference.GetSyntax(cancellationToken);
                var setter = property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
                if (setter != null)
                {
                    return Diagnostic.Create(
                            DiagnosticDescriptors.UseGetterOnlyAutoProperty,
                            setter.GetLocation());
                }
            }

            return null;
        }
    }
}
