using CSharpEssentials.UseExpressionBodiedMember;
using Microsoft.CodeAnalysis.CodeFixes;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    [TestFixture]
    public class UseExpressionBodiedMemberCodeFixTests : CodeFixTestFixture
    {
        public override CodeFixProvider CreateProvider()
        {
            return new UseExpressionBodiedMemberCodeFix();
        }

        [Test]
        [Ignore("Incorrect behavior")]
        public void TestSimpleProperty()
        {
            const string markupCode = @"
class C
{
    [|int P { get { return 42; } }|]
}
";

            const string expected = @"
class C
{
    int P => 42; }
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }
    }
}
