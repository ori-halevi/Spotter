using System;
using System.IO;
using System.Text;

namespace Spotter;

/// <summary>
/// Minimal append-only diagnostics.
///
/// The app needs administrator to read drives, and an elevated window cannot be screenshotted or
/// automated from an ordinary process — so when something misbehaves under elevation, a log file
/// is the only channel back out. Deliberately tiny: no levels, no rotation policy beyond a size
/// cap, nothing to configure.
/// </summary>
internal static class Log
{
    private const long MaxBytes = 1024 * 1024;

    private static readonly object Gate = new object();

    /// <summary>
    /// Where this log is written, so Options can point at it:
    /// <c>%LOCALAPPDATA%\Spotter\Spotter.log</c>.
    ///
    /// Deliberately NOT next to the executable, which is wrong twice over: under Program Files an
    /// ordinary user cannot write there at all, and when the app *is* elevated it creates a file
    /// the user afterwards cannot delete. Per-user application data is writable in both cases.
    /// </summary>
    public static string FilePath { get; } = Resolve();

    private static string Resolve()
    {
        try
        {
            string folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Spotter");
            Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, "Spotter.log");
        }
        catch
        {
            // Better beside the exe than no diagnostics at all; Write swallows the failure if
            // that is not writable either.
            return System.IO.Path.Combine(AppContext.BaseDirectory, "Spotter.log");
        }
    }

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length > MaxBytes)
                {
                    File.Delete(FilePath);
                }
                File.AppendAllText(FilePath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
        }
        catch
        {
            // Diagnostics must never take the app down.
        }
    }
}
