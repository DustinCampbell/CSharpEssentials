using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    public abstract class AnalyzerTestFixture : BaseTestFixture
    {
        public abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void NoDiagnostic(string code, string diagnosticId)
        {
            var document = GetDocument(code);

            var diagnostics = GetDiagnostics(document);

            Assert.That(diagnostics.All(d => d.Id == diagnosticId), Is.True);
        }

        protected void Diagnostic(string markupCode, string diagnosticId)
        {
            Document document;
            TextSpan span;
            Assert.That(TryGetDocumentAndSpan(markupCode, out document, out span), Is.True);

            var diagnostics = GetDiagnostics(document);

            Assert.That(diagnostics.Length, Is.EqualTo(1));

            var diagnostic = diagnostics[0];
            Assert.That(diagnostic.Id, Is.EqualTo(diagnosticId));
            Assert.That(diagnostic.Location.IsInSource, Is.True);
            Assert.That(diagnostic.Location.SourceSpan, Is.EqualTo(span));
        }

        private ImmutableArray<Diagnostic> GetDiagnostics(Document document)
        {
            var analyzers = ImmutableArray.Create(CreateAnalyzer());
            var compilation = document.Project.GetCompilationAsync(CancellationToken.None).Result;
            var driver = AnalyzerDriver.Create(compilation, analyzers, null, out compilation, CancellationToken.None);
            var discarded = compilation.GetDiagnostics(CancellationToken.None);

            var tree = document.GetSyntaxTreeAsync(CancellationToken.None).Result;

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (var diagnostic in driver.GetDiagnosticsAsync().Result)
            {
                var location = diagnostic.Location;
                if (location.IsInSource &&
                    location.SourceTree == tree)
                {
                    builder.Add(diagnostic);
                }
            }

            return builder.ToImmutable();
        }
    }
}
