using UnityEditor;

namespace Fixer33.BuildTools.Editor.Preflight
{
    /// <summary>Outcome of one automatic preflight check.</summary>
    public enum PreflightCheckStatus
    {
        /// <summary>Nothing wrong.</summary>
        Passed,
        /// <summary>Shown to the user, does not block the build.</summary>
        Warning,
        /// <summary>Blocks the build until it is fixed (or the check is re-run and passes).</summary>
        Failed,
    }

    /// <summary>Result returned by <see cref="IBuildPreflightCheck.Run"/>.</summary>
    public readonly struct PreflightCheckResult
    {
        public readonly PreflightCheckStatus Status;
        public readonly string Message;

        public PreflightCheckResult(PreflightCheckStatus status, string message = null)
        {
            Status = status;
            Message = message;
        }

        public static PreflightCheckResult Pass(string message = null) => new(PreflightCheckStatus.Passed, message);
        public static PreflightCheckResult Warn(string message) => new(PreflightCheckStatus.Warning, message);
        public static PreflightCheckResult Fail(string message) => new(PreflightCheckStatus.Failed, message);
    }

    /// <summary>
    /// An automatic check run before a build. Implement it in any editor assembly of the game
    /// (a class with a parameterless constructor); it is found automatically, no registration.
    /// Game-specific rules belong in the game, not in this package.
    /// </summary>
    public interface IBuildPreflightCheck
    {
        /// <summary>Short name shown in the preflight window.</summary>
        string Title { get; }

        /// <summary>Order in the window; lower runs first.</summary>
        int Order => 0;

        /// <summary>Runs the check for the build about to start. Must not start the build or change assets.</summary>
        PreflightCheckResult Run(BuildPlayerOptions options);
    }
}
