using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    public abstract class CodeFixTestFixture : BaseTestFixture
    {
        public abstract CodeFixProvider CreateProvider();

        protected void TestCodeFix(string markupCode, string expected, DiagnosticDescriptor descriptor)
        {
            Document document;
            TextSpan span;
            Assert.That(TryGetDocumentAndSpan(markupCode, out document, out span), Is.True);

            var codeFixes = GetCodeFixes(document, span, descriptor);

            Assert.That(codeFixes.Length, Is.EqualTo(1));
        }

        private ImmutableArray<CodeAction> GetCodeFixes(Document document, TextSpan span, DiagnosticDescriptor descriptor)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            Action<CodeAction, ImmutableArray<Diagnostic>> registerFix =
                (a, _) => builder.Add(a);

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).Result;
            var diagnostic = Diagnostic.Create(descriptor, Location.Create(tree, span));

            var context = new CodeFixContext(document, diagnostic, registerFix, CancellationToken.None);
            var provider = CreateProvider();
            provider.ComputeFixesAsync(context).Wait();

            return builder.ToImmutable();
        }
    }
}
