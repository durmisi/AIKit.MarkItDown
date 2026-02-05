using Python.Runtime;
using System.IO;
using System.Diagnostics;

namespace AIKit.MarkItDown
{
    public class MarkDownConverter
    {
        static MarkDownConverter()
        {
            string pythonExe = GetPythonExecutable();
            if (pythonExe != null)
            {
                string dllPath = GetPythonDllPath(pythonExe);
                if (dllPath != null && File.Exists(dllPath))
                {
                    Runtime.PythonDLL = dllPath;
                }
            }

            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
            }
        }

        public string Convert(string filePath)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic markitdown = Py.Import("markitdown");
                    dynamic md = markitdown.MarkItDown();
                    dynamic result = md.convert(filePath);
                    return result.text_content;
                }
                catch (PythonException ex)
                {
                    throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                }
            }
        }

        private static string GetPythonExecutable()
        {
            string command = null;
            if (IsCommandAvailable("python", "--version")) command = "python";
            else if (IsCommandAvailable("python3", "--version")) command = "python3";
            else if (IsCommandAvailable("py", "--version")) command = "py";

            if (command != null)
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = "-c \"import sys; print(sys.executable)\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    return output.Trim();
                }
            }

            return null;
        }

        private static string GetPythonDllPath(string pythonExe)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = "-c \"import sys; print(f'python{sys.version_info.major}{sys.version_info.minor}.dll')\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string dllName = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(dllName))
                {
                    string pythonDir = GetPythonDir(pythonExe);
                    return Path.Combine(pythonDir, dllName);
                }
            }
            catch { }
            return null;
        }

        private static string GetPythonDir(string pythonExe)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = "-c \"import sys; print(sys.executable)\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string exePath = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(exePath))
                {
                    return Path.GetDirectoryName(exePath);
                }
            }
            catch { }
            return Path.GetDirectoryName(pythonExe); // fallback
        }

        private static bool IsCommandAvailable(string command, string args = "")
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
