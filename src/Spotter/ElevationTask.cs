using System;
using System.Security.Cryptography;
using System.Text;

namespace Spotter;

/// <summary>
/// "Ask once per location, then never again."
///
/// Reading the MFT needs administrator, and a UAC prompt on every launch gets old fast. This
/// registers a Task Scheduler entry that is allowed to start the app elevated without prompting.
/// Registering it needs administrator once — so nothing is gained that the user did not already
/// grant.
///
/// The task name is derived from the executable's own path, which is what keeps the app portable:
/// copy it somewhere else and that copy prompts once, registers its own task, and is then free
/// too. Same behaviour as the 1.5.1 app, and it reuses that app's Task Scheduler wrapper
/// (<see cref="GClass252"/>) rather than a second implementation of the same COM calls.
///
/// The trade-off, stated where the code is: a standing elevated entry point for one fixed
/// executable path. If that path is somewhere an ordinary process can write, whatever can replace
/// the file inherits elevation. Under Program Files it cannot; under a user profile it can.
/// </summary>
internal static class ElevationTask
{
    /// <summary>The task name, so Options can name what it is offering to remove.</summary>
    public static string Name => TaskName;

    private static string TaskName
    {
        get
        {
            // SHA-256 rather than MD5: this is only a name, not a security decision, but MD5 is
            // unavailable when Windows is in FIPS mode — where it throws rather than degrading —
            // and the whole feature would then silently fall back to a UAC prompt on every launch.
            // Truncated because a task name is for humans to recognise in Task Scheduler.
            string path = (Environment.ProcessPath ?? "unknown").ToLowerInvariant();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(path));
            return "Spotter." + Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Builds the argument string handed to the scheduled task.
    ///
    /// The task substitutes this into its command line as <c>$(Arg0)</c>, so it has to survive a
    /// second round of command-line parsing. Joining on a bare space — which is what this did —
    /// turned <c>--search "my file"</c> into <c>--search my file</c>, and the app then searched
    /// for "my" with a stray "file" argument after it.
    /// </summary>
    private static string JoinArguments(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder joined = new StringBuilder();
        foreach (string arg in args)
        {
            if (joined.Length > 0)
            {
                joined.Append(' ');
            }

            if (arg.Length > 0 && arg.IndexOfAny(new[] { ' ', '\t', '"' }) < 0)
            {
                joined.Append(arg);
                continue;
            }

            // Standard Windows quoting: backslashes are only special immediately before a quote.
            joined.Append('"');
            int backslashes = 0;
            foreach (char c in arg)
            {
                if (c == '\\')
                {
                    backslashes++;
                    continue;
                }
                if (c == '"')
                {
                    joined.Append('\\', backslashes * 2 + 1);
                    backslashes = 0;
                }
                else if (backslashes > 0)
                {
                    joined.Append('\\', backslashes);
                    backslashes = 0;
                }
                joined.Append(c);
            }
            joined.Append('\\', backslashes * 2);
            joined.Append('"');
        }
        return joined.ToString();
    }

    /// <summary>
    /// Starts the elevated copy through the task and returns true if it went. False means there
    /// is no task for this location yet, so the caller should fall back to a UAC prompt.
    /// </summary>
    public static bool TryLaunchElevated(string[] args)
    {
        try
        {
            GClass252.smethod_8(TaskName, JoinArguments(args));
            Log.Write($"launched elevated via task {TaskName}");
            return true;
        }
        catch (Exception ex)
        {
            // Missing task is the normal first-run path, not an error worth surfacing.
            Log.Write("no elevation task for this location yet: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Registers the task for this executable's location. Requires administrator, so it is called
    /// from the already-elevated copy — the one UAC prompt the user has just approved pays for it.
    /// </summary>
    public static void RegisterIfMissing()
    {
        try
        {
            if (Exists())
            {
                return;
            }
            GClass252.smethod_4(TaskName, Environment.ProcessPath, string.Empty);
            Log.Write($"registered elevation task {TaskName} -> {Environment.ProcessPath}");
        }
        catch (Exception ex)
        {
            // Not fatal: the app still works, it just keeps prompting.
            Log.Write("could not register the elevation task: " + ex.Message);
        }
    }

    public static bool Exists()
    {
        try { return GClass252.smethod_10(TaskName); }
        catch { return false; }
    }

    public static void Remove()
    {
        try
        {
            GClass252.smethod_9(TaskName);
            Log.Write($"removed elevation task {TaskName}");
        }
        catch (Exception ex)
        {
            Log.Write("could not remove the elevation task: " + ex.Message);
        }
    }
}
