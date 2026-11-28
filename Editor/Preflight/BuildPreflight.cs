using System;
using System.Collections.Generic;
using System.Linq;
using Fixer33.BuildTools.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Fixer33.BuildTools.Editor.Preflight
{
    /// <summary>
    /// Hooks the preflight into Unity's build button while it is enabled in <see cref="BuildToolsSettings"/>.
    /// When disabled it registers nothing, so other build handlers in the project keep working.
    /// </summary>
    [InitializeOnLoad]
    public static class BuildPreflight
    {
        private static bool _registered;

        static BuildPreflight()
        {
            Refresh();
            BuildToolsSettings.Changed += Refresh;
        }

        private static void Refresh()
        {
            bool enabled = BuildToolsSettings.instance.PreflightEnabled;
            if (enabled == _registered)
                return;

            // null restores Unity's default handler. Only done when this package registered one.
            BuildPlayerWindow.RegisterBuildPlayerHandler(enabled ? HandleBuildPlayer : null);
            _registered = enabled;
        }

        /// <summary>Creates every <see cref="IBuildPreflightCheck"/> in the loaded editor assemblies, sorted by Order.</summary>
        public static List<IBuildPreflightCheck> CreateChecks()
        {
            var checks = new List<IBuildPreflightCheck>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<IBuildPreflightCheck>())
            {
                if (type.IsAbstract || type.IsInterface || type.GetConstructor(Type.EmptyTypes) == null)
                    continue;
                try
                {
                    checks.Add((IBuildPreflightCheck)Activator.CreateInstance(type));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Build preflight: could not create check {type.FullName}: {e.Message}");
                }
            }
            return checks.OrderBy(c => c.Order).ThenBy(c => c.Title).ToList();
        }

        /// <summary>Runs one check, turning an exception into a failed result.</summary>
        public static PreflightCheckResult RunSafe(IBuildPreflightCheck check, BuildPlayerOptions options)
        {
            try
            {
                return check.Run(options);
            }
            catch (Exception e)
            {
                return PreflightCheckResult.Fail($"Exception: {e.Message}");
            }
        }

        private static void HandleBuildPlayer(BuildPlayerOptions options)
        {
            if (Application.isBatchMode)
            {
                if (BuildToolsSettings.instance.PreflightInBatchMode)
                {
                    foreach (var check in CreateChecks())
                    {
                        var result = RunSafe(check, options);
                        if (result.Status == PreflightCheckStatus.Failed)
                            throw new BuildPlayerWindow.BuildMethodException($"Build preflight: '{check.Title}' failed. {result.Message}");
                    }
                }
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
                return;
            }

            BuildPreflightWindow.Open(options);
        }
    }
}
