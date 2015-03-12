using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace CSharpEssentials.Tests
{
    public abstract class CodeRefactoringTestFixture : BaseTestFixture
    {
        protected abstract CodeRefactoringProvider CreateProvider();

        protected void TestCodeRefactoring(string markupCode, string expected)
        {
            Document document;
            TextSpan span;
            Assert.That(TryGetDocumentAndSpan(markupCode, out document, out span), Is.True);

            var codeRefactorings = GetCodeRefactorings(document, span);

            Assert.That(codeRefactorings.Length, Is.EqualTo(1));

            var codeRefactoring = codeRefactorings[0];
            var operations = codeRefactoring.GetOperationsAsync(CancellationToken.None).Result;

            Assert.That(operations.Count(), Is.EqualTo(1));

            var operation = operations.Single();
            var workspace = document.Project.Solution.Workspace;
            operation.Apply(workspace, CancellationToken.None);

            var newDocument = workspace.CurrentSolution.GetDocument(document.Id);

            var sourceText = newDocument.GetTextAsync(CancellationToken.None).Result;
            var text = sourceText.ToString();

            Assert.That(text, Is.EqualTo(expected));
        }

        private ImmutableArray<CodeAction> GetCodeRefactorings(Document document, TextSpan span)
        {
            var builder = ImmutableArray.CreateBuilder<CodeAction>();
            Action<CodeAction> registerRefactoring = a => builder.Add(a);

            var context = new CodeRefactoringContext(document, span, registerRefactoring, CancellationToken.None);
            var provider = CreateProvider();
            provider.ComputeRefactoringsAsync(context).Wait();

            return builder.ToImmutable();
        }
    }
}
