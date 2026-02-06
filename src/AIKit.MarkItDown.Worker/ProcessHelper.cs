using System.Diagnostics;

namespace AIKit.MarkItDown.Worker;

public static class ProcessHelper
{
    /// <summary>
    /// Runs a command and returns its output.
    /// </summary>
    /// <param name="cmd">The command to run.</param>
    /// <param name="args">The arguments for the command.</param>
    /// <returns>The trimmed output of the command.</returns>
    /// <exception cref="Exception">Thrown if the command fails.</exception>
    public static string Run(string cmd, string args)
    {
        using var p = Process.Start(new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;

        p.WaitForExit();
        if (p.ExitCode != 0)
            throw new Exception($"Command '{cmd} {args}' failed");

        return p.StandardOutput.ReadToEnd().Trim();
    }

    /// <summary>
    /// Checks if a command is available by running it with given args.
    /// </summary>
    /// <param name="cmd">The command to check.</param>
    /// <param name="args">The arguments to use for checking.</param>
    /// <returns>True if the command is available, false otherwise.</returns>
    public static bool IsCommandAvailable(string cmd, string args)
    {
        try { Run(cmd, args); return true; }
        catch { return false; }
    }
}