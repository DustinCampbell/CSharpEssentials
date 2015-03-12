using CSharpEssentials.ConvertToInterpolatedString;
using Microsoft.CodeAnalysis.CodeRefactorings;
using NUnit.Framework;

namespace CSharpEssentials.Tests.ConvertToInterpolatedString
{
    public class ConvertToInterpolatedStringRefactoringTests : CodeRefactoringTestFixture
    {
        protected override CodeRefactoringProvider CreateProvider()
        {
            return new ConvertToInterpolatedStringRefactoring();
        }

        [Test]
        public void SimpleTest()
        {
            const string markupCode = @"
class C
{
    void M()
    {
        var s = [|string.Format(""{0}"", 42)|];
    }
}";

            const string expected = @"
class C
{
    void M()
    {
        var s = $""{42}"";
    }
}";

            TestCodeRefactoring(markupCode, expected);
        }
    }
}
