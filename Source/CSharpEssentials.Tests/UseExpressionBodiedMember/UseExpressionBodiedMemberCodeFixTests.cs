using CSharpEssentials.UseExpressionBodiedMember;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using NUnit.Framework;
using RoslynNUnitLight;

namespace CSharpEssentials.Tests.UseExpressionBodiedMember
{
    [TestFixture]
    public class UseExpressionBodiedMemberCodeFixTests : CodeFixTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override CodeFixProvider CreateProvider() => new UseExpressionBodiedMemberCodeFix();

        [Test]
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
    int P => 42;
}
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }

        [Test]
        public void TestPropertyBeforeAnotherProperty()
        {
            const string markupCode = @"
class C
{
    [|int P1 { get { return 42; } }|]
    int P2 { get { return 42; } }
}
";

            const string expected = @"
class C
{
    int P1 => 42;
    int P2 { get { return 42; } }
}
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }

        [Test]
        public void TestSimpleIndexer()
        {
            const string markupCode = @"
class C
{
    [|int this[int index] { get { return 42; } }|]
}
";

            const string expected = @"
class C
{
    int this[int index] => 42;
}
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }

        [Test]
        public void TestSimpleMethod()
        {
            const string markupCode = @"
class C
{
    [|int M()
    {
        return 42;
    }|]
}
";

            const string expected = @"
class C
{
    int M() => 42;
}
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }

        [Test]
        public void TestMethodWithComment()
        {
            const string markupCode = @"
class C
{
    [|int M()
    {
        return 42;
    }|] // comment
}
";

            const string expected = @"
class C
{
    int M() => 42; // comment
}
";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseExpressionBodiedMember);
        }
    }
}
