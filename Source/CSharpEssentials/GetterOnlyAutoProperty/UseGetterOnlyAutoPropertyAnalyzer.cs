using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
            analysisContext.RegisterCompilationStartAction(context => { new Tracker(context); });
        }

        private class Tracker
        {
            private readonly HashSet<ISymbol> _declared = new HashSet<ISymbol>();
            private readonly HashSet<ISymbol> _referenced = new HashSet<ISymbol>();

            private static readonly ImmutableArray<SyntaxKind> s_PropertyDeclarationKind = ImmutableArray.Create(SyntaxKind.PropertyDeclaration);
            private static readonly ImmutableArray<SyntaxKind> s_IdentifierKind = ImmutableArray.Create(SyntaxKind.IdentifierName);

            public Tracker(CompilationStartAnalysisContext context)
            {
                context.RegisterSyntaxNodeAction(OnPropertyDeclaration, s_PropertyDeclarationKind);
                context.RegisterSyntaxNodeAction(OnIdentifier, s_IdentifierKind);
                context.RegisterCompilationEndAction(OnCompilationEnd);
            }

            /// <summary>
            /// Look for an auto property declaration with a getter and a private setter.
            /// </summary>
            /// <param name="context">The analyzer context.</param>
            private void OnPropertyDeclaration(SyntaxNodeAnalysisContext context)
            {
                var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
                var accessorList = propertyDeclaration.AccessorList;
                if (accessorList == null)
                {
                    return;
                }

                // Expect exactly 2 accessors
                var accessors = accessorList.Accessors;
                if (accessors.Count != 2)
                {
                    return;
                }

                var firstAccessor = accessors[0];
                var secondAccessor = accessors[1];
                if (firstAccessor.Body != null || secondAccessor.Body != null)
                {
                    // Not an auto-prop
                    return;
                }

                var setter = firstAccessor.IsKind(SyntaxKind.SetAccessorDeclaration) ? firstAccessor :
                             secondAccessor.IsKind(SyntaxKind.SetAccessorDeclaration) ? secondAccessor : null;

                if (setter == null)
                {
                    // Couldn't find a setter
                    return;
                }

                if (setter.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    // Found a private setter. Add the property (not just the setter) to the list of candidates.
                    var symbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
                    if (symbol != null)
                    {
                        _declared.Add(symbol);
                    }
                }
            }

            /// <summary>
            /// Look for identifiers that are properties. If the identifier is the target
            /// of an assignment, but does not appear in a constructor, then then add the
            /// property to the set of 'used' properties.
            /// </summary>
            /// <param name="context">The analyzer context for the syntax node.</param>
            private void OnIdentifier(SyntaxNodeAnalysisContext context)
            {
                // Consider: Should this be a block analyzer that looks "down" the tree instead?

                // Is it a property?
                var property = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol;
                if (property == null || property.Kind != SymbolKind.Property)
                {
                    return;
                }

                // Already seen?
                if (_referenced.Contains(property))
                {
                    return;
                }

                // Is the property being updated?
                var node = context.Node;
                if (!IsUpdatingExpression(ref node))
                {
                    return;
                }

                // Are we in a constructor?
                if (IsWithinConstructorOf(node.Parent, property.ContainingType, context))
                {
                    return;
                }

                _referenced.Add(property);
            }

            private static bool IsDescendant(SyntaxNode root, SyntaxNode node)
            {
                while (node != null)
                {
                    if (node == root)
                    {
                        return true;
                    }

                    node = node.Parent;
                }

                return false;
            }

            private static bool IsUpdatingExpression(ref SyntaxNode node)
            {
                var identifier = node;
                for (node = node.Parent; node != null; node = node.Parent)
                {
                    switch (node.Kind())
                    {
                        // Simple assignment
                        case SyntaxKind.SimpleAssignmentExpression:
                            var assignment = (AssignmentExpressionSyntax)node;
                            return IsDescendant(assignment.Left, identifier);

                        // Implicit assignment
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

                        // Prefix unary expression
                        case SyntaxKind.PreIncrementExpression:
                        case SyntaxKind.PreDecrementExpression:

                        // Postfix unary expression
                        case SyntaxKind.PostIncrementExpression:
                        case SyntaxKind.PostDecrementExpression:
                            return true;

                        // Early loop termination
                        case SyntaxKind.Block:
                        case SyntaxKind.ExpressionStatement:
                            return false;
                    }
                }

                return false;
            }

            private static bool IsWithinConstructorOf(SyntaxNode node, INamedTypeSymbol type, SyntaxNodeAnalysisContext context)
            {
                // Are we in a constructor?
                for (; node != null; node = node.Parent)
                {
                    switch (node.Kind())
                    {
                        case SyntaxKind.ConstructorDeclaration:
                            // In a constructor. Is it the constructor for the type that contains the property?
                            var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
                            return constructorSymbol != null && (object)constructorSymbol.ContainingType == type;

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

            private void OnCompilationEnd(CompilationEndAnalysisContext context)
            {
                // Output the the list of remaining candidates
                _declared.ExceptWith(_referenced);
                foreach (var symbol in _declared)
                {
                    // Report
                    foreach (var reference in symbol.DeclaringSyntaxReferences)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        // The span should be on the setter, but the symbol is for the whole property declaration.
                        var declarationNode = reference.GetSyntax(context.CancellationToken) as PropertyDeclarationSyntax;
                        foreach (var accessor in declarationNode.AccessorList.Accessors)
                        {
                            if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        DiagnosticDescriptors.UseGetterOnlyAutoProperty,
                                        accessor.GetLocation()));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
