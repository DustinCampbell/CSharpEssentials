using CSharpEssentials.UseExpressionBodiedMember;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace CSharpEssentials.Tests.UseExpressionBodiedMember
{
    [TestFixture]
    public class UseExpressionBodiedMemberAnalyzerTests : AnalyzerTestFixture
    {
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
