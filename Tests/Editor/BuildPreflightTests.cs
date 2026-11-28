using System.Linq;
using Fixer33.BuildTools.Editor.Preflight;
using NUnit.Framework;
using UnityEditor;

namespace Fixer33.BuildTools.Editor.Tests
{
    public class BuildPreflightTests
    {
        private class PassingCheck : IBuildPreflightCheck
        {
            public string Title => "Test: passing";
            public int Order => -1000;
            public PreflightCheckResult Run(BuildPlayerOptions options) => PreflightCheckResult.Pass();
        }

        private class ThrowingCheck : IBuildPreflightCheck
        {
            public string Title => "Test: throwing";
            public PreflightCheckResult Run(BuildPlayerOptions options) => throw new System.InvalidOperationException("boom");
        }

        [Test]
        public void CreateChecks_FindsChecksAndSortsByOrder()
        {
            var checks = BuildPreflight.CreateChecks();

            Assert.That(checks.Any(c => c is PassingCheck), Is.True);
            Assert.That(checks.Any(c => c is ThrowingCheck), Is.True);
            Assert.That(checks.First(), Is.InstanceOf<PassingCheck>(), "Lowest Order runs first");
        }

        [Test]
        public void RunSafe_ExceptionBecomesFailedResult()
        {
            var result = BuildPreflight.RunSafe(new ThrowingCheck(), default);

            Assert.That(result.Status, Is.EqualTo(PreflightCheckStatus.Failed));
            StringAssert.Contains("boom", result.Message);
        }
    }
}
