using CSharpEssentials.GetterOnlyAutoProperty;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace CSharpEssentials.Tests
{
    [TestFixture]
    public class UseGetterOnlyAutoPropertyAnalyzerTests : AnalyzerTestFixture
    {
        public override DiagnosticAnalyzer CreateAnalyzer() => new UseGetterOnlyAutoPropertyAnalyzer();

        [Test]
        public void AutoPropDeclaredAndUsedInConstructor()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; [|private set;|] }
    public C(bool f)
    {
        MyProperty = f;
    }
}";

            HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndUsedInMethodInPartialType()
        {
            const string code = @"
partial class C
{
    public int MyProperty { get; private set; }
}

partial class C
{
    public void M()
    {
        MyProperty = 0;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropAlreadyReadonly()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; }
    public C(bool f)
    {
        MyProperty = f;
    }
}";
            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void NotAnAutoProp()
        {
            const string code = @"
class C
{
    public bool MyProperty {
        get { return false; }
        private set { ; }
    }

    public C(bool f)
    {
        MyProperty = f;
    }
}";
            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndAssignedInMethod()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; private set; }
    public void Method()
    {
        MyProperty = false;
    }
}";
            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndAssignedViaPostIncrement()
        {
            const string code = @"
class C
{
    public int MyProperty { get; private set; }
    public void Method()
    {
        MyProperty++;
    }
}";
            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndAssignedViaOrAssignment()
        {
            const string code = @"
class C
{
    public int MyProperty { get; private set; }
    public void Method()
    {
        MyProperty |= 0x1;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndAssignedViaRightShiftAssignment()
        {
            const string code = @"
class C
{
    public int MyProperty { get; private set; }
    public void Method()
    {
        MyProperty >>= 1;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropDeclaredAndAssignedComplex()
        {
            const string code = @"
class C
{
    public int MyProperty { get; private set; }
    public float Method()
    {
        float y = 0;
        for (int i = 0; i < 10; i++)
        {
            y += (((float)(unchecked((MyProperty) = i))));
        }
        return y;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropUsedInNestedConstructor()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; private set; }
    private class Nested
    {
        public Nested(C c)
        {
            c.MyProperty = false;
        }
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }
    }
}
