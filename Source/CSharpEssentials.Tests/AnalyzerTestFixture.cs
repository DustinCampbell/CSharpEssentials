using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    public abstract class AnalyzerTestFixture
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

        private bool TryGetDocumentAndSpan(string markupCode, out Document document, out TextSpan span)
        {
            string code;
            if (!TryGetCodeAndSpan(markupCode, out code, out span))
            {
                document = null;
                return false;
            }

            document = GetDocument(code);
            return true;
        }

        private bool TryGetCodeAndSpan(string markupCode, out string code, out TextSpan span)
        {
            code = null;
            span = default(TextSpan);

            var builder = new StringBuilder();

            var start = markupCode.IndexOf("[|");
            if (start < 0)
            {
                return false;
            }

            builder.Append(markupCode.Substring(0, start));

            var end = markupCode.IndexOf("|]");
            if (end < 0)
            {
                return false;
            }

            builder.Append(markupCode.Substring(start + 2, end - start - 2));
            builder.Append(markupCode.Substring(end + 2));

            code = builder.ToString();
            span = TextSpan.FromBounds(start, end - 2);

            return true;
        }

        private Document GetDocument(string code)
        {
            return new CustomWorkspace()
                .AddProject("TestProject", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly))
                .AddDocument("TestDocument", code);
        }
    }
}
