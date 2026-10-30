namespace Siniskalc.Tests.Homm.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;

    /// <summary>
    /// Checks the conventions every test project follows for its fixtures: a kind (<c>[Category]</c>) on each, and the folder (the namespace)
    /// that goes with the kind. The library is also linked as source into the test projects that cannot reference it.
    /// </summary>
    public static class TestFixtureConventions
    {
        private const string Unit = "Unit";
        private const string MultiUnit = "MultiUnit";
        private const string Integration = "Integration";
        private const string Manual = "Manual";

        /// <summary>
        /// Lists, for each fixture of <paramref name="assembly"/> that breaks a convention, what is wrong with it.
        /// </summary>
        public static IReadOnlyList<string> Violations(Assembly assembly)
        {
            var found = new List<string>();
            foreach (var type in assembly.GetTypes().Where(IsFixture).OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                var kinds = type.GetCustomAttributes<CategoryAttribute>(inherit: true).Select(c => c.Name).Where(IsKind).Distinct().ToList();
                var folders = (type.Namespace ?? string.Empty).Split('.');
                if (kinds.Count != 1)
                {
                    found.Add($"{type.FullName}: has {kinds.Count} kinds ({string.Join(", ", kinds)}), it needs exactly one of Unit, MultiUnit, Integration, Manual");
                    continue;
                }

                var kind = kinds[0];
                var inIntegration = folders.Contains(Integration);
                var inMultiUnit = folders.Contains(MultiUnit);
                if (kind == Integration && !inIntegration)
                {
                    found.Add($"{type.FullName}: an Integration fixture belongs in an Integration folder");
                }
                else if (kind == MultiUnit && !inMultiUnit)
                {
                    found.Add($"{type.FullName}: a MultiUnit fixture belongs in a MultiUnit folder");
                }
                else if (kind == Unit && (inIntegration || inMultiUnit))
                {
                    found.Add($"{type.FullName}: a Unit fixture belongs in the folder of its layer, not under Integration or MultiUnit");
                }
                else if (inIntegration && inMultiUnit)
                {
                    found.Add($"{type.FullName}: the namespace has both an Integration and a MultiUnit folder");
                }
            }

            return found;
        }

        private static bool IsKind(string name) => name is Unit or MultiUnit or Integration or Manual;

        private static bool IsFixture(Type type) =>
            type.IsClass
            && !type.IsAbstract
            && (type.GetCustomAttributes<TestFixtureAttribute>(inherit: true).Any()
                || type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Any(m => m.GetCustomAttributes<TestAttribute>(inherit: true).Any() || m.GetCustomAttributes<TestCaseAttribute>(inherit: true).Any() || m.GetCustomAttributes<TestCaseSourceAttribute>(inherit: true).Any()));
    }
}
