using CSharpEssentials.GetterOnlyAutoProperty;
using Microsoft.CodeAnalysis.CodeFixes;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    [TestFixture]
    public class UseGetterOnlyAutoPropertyCodeFixTests : CodeFixTestFixture
    {
        protected override CodeFixProvider CreateProvider() => new UseGetterOnlyAutoPropertyCodeFix();

        [Test]
        public void TestSimpleProperty()
        {
            const string markupCode = @"
class C
{
    public bool P1 { get; [|private set;|] }
}";

            const string expected = @"
class C
{
    public bool P1 { get; }
}";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void TestSetterBeforeGetter()
        {
            const string markupCode = @"
class C
{
    public bool P1 { [|private set;|] get; }
}";

            const string expected = @"
class C
{
    public bool P1 { get; }
}";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void TestMultilineAutoProp()
        {
            const string markupCode = @"
class C
{
    public bool P1
    {
        get;
        [|private set;|]
    }
}";
            // TODO: Note the extra blank line. Should this be eliminated?
            const string expected = @"
class C
{
    public bool P1
    {
        get;

    }
}";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void TestCommentsPreserved()
        {
            const string markupCode = @"
class C
{
    public bool P1
    {
        get; // Getter comment
        [|private set;|] // Setter comment
    }
}";

            const string expected = @"
class C
{
    public bool P1
    {
        get; // Getter comment
             // Setter comment
    }
}";

            TestCodeFix(markupCode, expected, DiagnosticDescriptors.UseGetterOnlyAutoProperty);
        }
    }
}
