using CSharpEssentials.UseExpressionBodiedMember;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RoslynNUnitLight;

namespace CSharpEssentials.Tests.UseExpressionBodiedMember
{
    [TestFixture]
    public class UseExpressionBodiedMemberAnalyzerTests : AnalyzerTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override DiagnosticAnalyzer CreateAnalyzer() => new UseExpressionBodiedMemberAnalyzer();

        [Test]
        public void NoDiagnosticWhenThereIsAnAttributeOnAnAccessor()
        {
            const string code = @"
class C
{
    int Property
    {
        [A] get { return 42; }
    }
}";

            NoDiagnostic(code, DiagnosticIds.UseExpressionBodiedMember);
        }
    }
}
