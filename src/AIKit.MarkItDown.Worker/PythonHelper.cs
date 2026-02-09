using Python.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AIKit.MarkItDown.Worker;

public static class PythonHelper
{
    /// <summary>
    /// Disables Python logging to reduce noise.
    /// </summary>
    public static void DisablePythonLogging()
        => PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");

    /// <summary>
    /// Finds the Python executable on the system.
    /// Tries venv python first, then PATH commands, then common installation paths.
    /// </summary>
    /// <returns>The path to the Python executable, or null if not found.</returns>
    public static string? GetPythonExecutable()
    {
        // First try venv python relative to project root
        string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string projectRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
        string venvPython = OperatingSystem.IsWindows() 
            ? Path.Combine(projectRoot, ".venv", "Scripts", "python.exe") 
            : Path.Combine(projectRoot, ".venv", "bin", "python");
        if (File.Exists(venvPython) && ProcessHelper.IsCommandAvailable(venvPython, "--version"))
            return venvPython;

        // Then try commands in PATH
        foreach (var cmd in new[] { "python", "python3", "py", "python3.10" })
            if (ProcessHelper.IsCommandAvailable(cmd, "--version"))
                return ProcessHelper.Run(cmd, "-c \"import sys; print(sys.executable)\"");

        // Then try common installation paths
        var commonPaths = new[]
        {
            @"C:\Python312\python.exe",
            @"C:\Python311\python.exe",
            @"C:\Python310\python.exe",
            @"C:\Python39\python.exe",
            @"C:\Python38\python.exe",
            @"C:\Program Files\Python312\python.exe",
            @"C:\Program Files\Python311\python.exe",
            @"C:\Program Files\Python310\python.exe",
            @"C:\Program Files\Python39\python.exe",
            @"C:\Program Files\Python38\python.exe",
            @"C:\Program Files (x86)\Python312\python.exe",
            @"C:\Program Files (x86)\Python311\python.exe",
            @"C:\Program Files (x86)\Python310\python.exe",
            @"C:\Program Files (x86)\Python39\python.exe",
            @"C:\Program Files (x86)\Python38\python.exe",
        };

        foreach (var path in commonPaths)
            if (File.Exists(path) && ProcessHelper.IsCommandAvailable(path, "--version"))
                return path;

        return null;
    }

    /// <summary>
    /// Gets the path to the Python DLL based on the Python executable.
    /// </summary>
    /// <param name="pythonExe">The path to the Python executable.</param>
    /// <returns>The path to the Python DLL, or null if not found.</returns>
    public static string? GetPythonDllPath(string pythonExe)
    {
        if (OperatingSystem.IsWindows())
        {
            string dll = ProcessHelper.Run(pythonExe, "-c \"import sys; print(f'python{sys.version_info.major}{sys.version_info.minor}.dll')\"");
            return string.IsNullOrEmpty(dll) ? null : Path.Combine(Path.GetDirectoryName(pythonExe)!, dll);
        }
        else
        {
            // On Linux/macOS, Python library is libpythonXY.so
            string version = ProcessHelper.Run(pythonExe, "-c \"import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')\"");
            if (string.IsNullOrEmpty(version))
                return null;
            string dll = $"libpython{version}.so";

            // Try LIBDIR first
            string libDir = ProcessHelper.Run(pythonExe, "-c \"import sysconfig; print(sysconfig.get_config_var('LIBDIR') or '')\"");
            if (!string.IsNullOrEmpty(libDir))
            {
                string path = Path.Combine(libDir, dll);
                if (File.Exists(path))
                    return path;
            }

            // Fallback: common library directories
            var commonLibDirs = new[] { "/usr/lib", "/usr/lib64", "/usr/local/lib", "/usr/lib/x86_64-linux-gnu", "/usr/lib/aarch64-linux-gnu" };
            foreach (var dir in commonLibDirs)
            {
                string path = Path.Combine(dir, dll);
                if (File.Exists(path))
                    return path;
            }

            // Last resort: try to find via ldd on python executable
            string pythonPath = ProcessHelper.Run(pythonExe, "-c \"import sys; print(sys.executable)\"");
            if (!string.IsNullOrEmpty(pythonPath) && File.Exists(pythonPath))
            {
                string lddOutput = ProcessHelper.Run("ldd", $"\"{pythonPath}\"");
                if (!string.IsNullOrEmpty(lddOutput))
                {
                    var lines = lddOutput.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("libpython") && line.Contains(".so"))
                        {
                            var parts = line.Split("=>");
                            if (parts.Length > 1)
                            {
                                var libPath = parts[1].Trim().Split(' ')[0];
                                if (File.Exists(libPath))
                                    return libPath;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Builds a Python dictionary from a C# dictionary for kwargs.
    /// </summary>
    /// <param name="config">The configuration dictionary.</param>
    /// <returns>A PyDict containing the kwargs.</returns>
    public static PyDict BuildKwargs(Dictionary<string, object> config)
    {
        var kwargs = new PyDict();

        if (config == null)
            return kwargs;

        foreach (var kvp in config)
        {
            if (kvp.Value is bool b)
                kwargs[kvp.Key] = PyObject.FromManagedObject(b);
            else if (kvp.Value is string s)
                kwargs[kvp.Key] = new PyString(s);
            else
                kwargs[kvp.Key] = PyObject.FromManagedObject(kvp.Value);
        }

        return kwargs;
    }
}