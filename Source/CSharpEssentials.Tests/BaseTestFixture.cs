using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpEssentials.Tests
{
    public abstract class BaseTestFixture
    {
        protected bool TryGetDocumentAndSpan(string markupCode, out Document document, out TextSpan span)
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

        protected Document GetDocument(string code)
        {
            return new AdhocWorkspace()
                .AddProject("TestProject", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly))
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly))
                .AddDocument("TestDocument", code);
        }
    }
}
