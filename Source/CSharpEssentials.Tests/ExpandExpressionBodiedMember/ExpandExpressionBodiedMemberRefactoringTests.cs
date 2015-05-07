using CSharpEssentials.ExpandExpressionBodiedMember;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using NUnit.Framework;
using RoslynNUnitLight;

namespace CSharpEssentials.Tests.ExpandExpressionBodiedMember
{
    public class ExpandExpressionBodiedMemberRefactoringTests : CodeRefactoringTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override CodeRefactoringProvider CreateProvider() => new ExpandExpressionBodiedMemberRefactoring();

        [Test]
        public void TestSimpleProperty()
        {
            const string markupCode = @"
class C
{
    [|int P => 42;|]
}
";

            const string expected = @"
class C
{
    int P
    {
        get
        {
            return 42;
        }
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestPropertyBeforeAnotherProperty()
        {
            const string markupCode = @"
class C
{
    [|int P1 => 42;|]
    int P2 { get { return 42; } }
}
";

            const string expected = @"
class C
{
    int P1
    {
        get
        {
            return 42;
        }
    }

    int P2 { get { return 42; } }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestSimpleIndexer()
        {
            const string markupCode = @"
class C
{
    [|int this[int index] => 42;|]
}
";

            const string expected = @"
class C
{
    int this[int index]
    {
        get
        {
            return 42;
        }
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestSimpleMethod()
        {
            const string markupCode = @"
class C
{
    [|int M() => 42;|]
}
";

            const string expected = @"
class C
{
    int M()
    {
        return 42;
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestSimpleOperator()
        {
            const string markupCode = @"
class C
{
    [|int operator +(C c1, C c2) => 42;|]
}
";

            const string expected = @"
class C
{
    int operator +(C c1, C c2)
    {
        return 42;
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestSimpleConversionOperator()
        {
            const string markupCode = @"
class C
{
    [|public static implicit operator int (C c) => 42;|]
}
";

            const string expected = @"
class C
{
    public static implicit operator int (C c)
    {
        return 42;
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }

        [Test]
        public void TestMethodWithComment()
        {
            const string markupCode = @"
class C
{
    [|int M() => 42;|] // comment
}
";

            const string expected = @"
class C
{
    int M()
    {
        return 42; // comment
    }
}
";

            TestCodeRefactoring(markupCode, expected);
        }
    }
}
