using CSharpEssentials.GetterOnlyAutoProperty;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RoslynNUnitLight;

namespace CSharpEssentials.Tests.GetterOnlyAutoProperty
{
    [TestFixture]
    public class UseGetterOnlyAutoPropertyAnalyzerTests : AnalyzerTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override DiagnosticAnalyzer CreateAnalyzer() => new UseGetterOnlyAutoPropertyAnalyzer();

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
        public void AutoPropWrittenInConstructorInPartialClassCanBeReadonly()
        {
            const string code = @"
partial class C
{
    public int MyProperty { get; [|private set;|] }
}

partial class C
{
    public C()
    {
        MyProperty = 0;
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
        public void AutoPropInInterfaceUsedExplicitly()
        {
            const string code = @"
interface I
{
    int Prop { get; set; }
}

class C : I
{
    int I.Prop { get; set; }
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
        public void AutoPropReferencedButNotAssignedInCompoundAssignment()
        {
            const string code = @"
class C
{
    public int MyProperty { get; [|private set;|] }
    public void Method()
    {
        int x = 0;
        x += MyProperty;
    }
}";

            HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
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

        [Test]
        public void AutoPropUsedInMultipleConstructorsCanBeReadonly()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; [|private set;|] }
    public C()
    {
        MyProperty = false;
    }

    public C(bool b)
    {
        MyProperty = b;
    }
}";

            HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropWrittenInConstructorReadInAnotherMethodCanBeReadonly()
        {
            const string code = @"
class C
{
    public bool MyProperty { get; [|private set;|] }
    public C()
    {
        MyProperty = false;
    }

    bool NotProp()
    {
        return !MyProperty;
    }
}";

            HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropUsedInLambdaInConstructorCannotBeReadonly()
        {
            const string code = @"
class C
{
    public int P { get; private set; }

    public C()
    {
        var f = new Action(() => P = 2);
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropCannotBeStaticIfAssignedInInstanceConstructor()
        {
            const string code = @"
class C
{
    public static int P { get; private set; }

    public C()
    {
        P = 2;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void AutoPropCanBeStaticIfAssignedInStaticConstructor()
        {
            const string code = @"
class C
{
    public static int P { get; [|private set;|] }

    static C()
    {
        P = 2;
    }
}";

            HasDiagnostic(code, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        private void VerifyNotAvailableInGeneratedCode(string filePath)
        {
            const string code = @"
class C
{
    public bool MyProperty { get; private set; }
    public C(bool f)
    {
        MyProperty = f;
    }
}";

            var document = TestHelpers
                .GetDocument(code, this.LanguageName)
                .WithFilePath(filePath);

            NoDiagnostic(document, DiagnosticIds.UseGetterOnlyAutoProperty);
        }

        [Test]
        public void NotAvailableInGeneratedCode1()
        {
            VerifyNotAvailableInGeneratedCode("TemporaryGeneratedFile_TestDocument.cs");
        }

        [Test]
        public void NotAvailableInGeneratedCode2()
        {
            VerifyNotAvailableInGeneratedCode("AssemblyInfo.cs");
        }

        [Test]
        public void NotAvailableInGeneratedCode3()
        {
            VerifyNotAvailableInGeneratedCode("TestDocument.designer.cs");
        }

        [Test]
        public void NotAvailableInGeneratedCode4()
        {
            VerifyNotAvailableInGeneratedCode("TestDocument.g.cs");
        }

        [Test]
        public void NotAvailableInGeneratedCode5()
        {
            VerifyNotAvailableInGeneratedCode("TestDocument.g.i.cs");
        }

        [Test]
        public void NotAvailableInGeneratedCode6()
        {
            VerifyNotAvailableInGeneratedCode("TestDocument.AssemblyAttributes.cs");
        }

        [Test]
        public void NotAvailableAutoGeneratedCode()
        {
            const string code = @"
// <auto-generated>
class C
{
    public bool MyProperty { get; private set; }
    public C(bool f)
    {
        MyProperty = f;
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseExpressionBodiedMember);
        }
    }
}
