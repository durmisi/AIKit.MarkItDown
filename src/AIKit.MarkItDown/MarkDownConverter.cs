using Python.Runtime;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace AIKit.MarkItDown
{
    public class MarkDownResult
    {
        public string? Text { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class MarkDownConfig
    {
        public string? DocIntelEndpoint { get; set; }
        public PyObject? LlmClient { get; set; }
        public string? LlmModel { get; set; }
        public string? LlmPrompt { get; set; }
        public bool KeepDataUris { get; set; }
        public bool EnablePlugins { get; set; }
        public string? DocIntelKey { get; set; }
        public List<string> Plugins { get; set; } = new List<string>();
    }

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

        public MarkDownResult Convert(string filePath)
        {
            using (Py.GIL())
            {
                try
                {
                    PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");
                    dynamic markitdown = Py.Import("markitdown");
                    dynamic md = markitdown.MarkItDown();
                    dynamic result;
                    string extension = Path.GetExtension(filePath).ToLowerInvariant().TrimStart('.');
                    var kwargs = new PyDict();
                    if (extension == "pdf")
                    {
                        byte[] fileBytes;
                        try
                        {
                            fileBytes = File.ReadAllBytes(filePath);
                        }
                        catch (FileNotFoundException ex)
                        {
                            throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                        }
                        dynamic io = Py.Import("io");
                        using (var stream = io.BytesIO(fileBytes))
                        {
                            kwargs["file_extension"] = new PyString(extension);
                            kwargs["check_extractable"] = PyObject.FromManagedObject(false);
                            result = ((PyObject)md.convert_stream).Invoke(new PyTuple(new[] { (PyObject)stream }), kwargs);
                        }
                    }
                    else
                    {
                        result = ((PyObject)md.convert).Invoke(new PyTuple(new[] { new PyString(filePath) }), kwargs);
                    }
                    var metadata = new Dictionary<string, object>();
                    try
                    {
                        metadata = ConvertPyDictToDict(result.metadata);
                    }
                    catch
                    {
                        // metadata not available in this version
                    }
                    return new MarkDownResult
                    {
                        Text = result.text_content,
                        Title = result.title,
                        Metadata = metadata
                    };
                }
                catch (PythonException ex)
                {
                    throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                }
            }
        }

        public MarkDownResult Convert(string filePath, MarkDownConfig config = null)
        {
            using (Py.GIL())
            {
                try
                {
                    PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");
                    dynamic markitdown = Py.Import("markitdown");
                    dynamic md = markitdown.MarkItDown();
                    dynamic result;
                    string extension = Path.GetExtension(filePath).ToLowerInvariant().TrimStart('.');
                    var kwargs = new PyDict();
                    PopulateKwargs(kwargs, config);
                    if (extension == "pdf")
                    {
                        byte[] fileBytes;
                        try
                        {
                            fileBytes = File.ReadAllBytes(filePath);
                        }
                        catch (FileNotFoundException ex)
                        {
                            throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                        }
                        dynamic io = Py.Import("io");
                        using (var stream = io.BytesIO(fileBytes))
                        {
                            kwargs["file_extension"] = new PyString(extension);
                            kwargs["check_extractable"] = PyObject.FromManagedObject(false);
                            result = ((PyObject)md.convert_stream).Invoke(new PyTuple(new[] { (PyObject)stream }), kwargs);
                        }
                    }
                    else
                    {
                        result = ((PyObject)md.convert).Invoke(new PyTuple(new[] { new PyString(filePath) }), kwargs);
                    }
                    var metadata = new Dictionary<string, object>();
                    try
                    {
                        metadata = ConvertPyDictToDict(result.metadata);
                    }
                    catch
                    {
                        // metadata not available in this version
                    }
                    return new MarkDownResult
                    {
                        Text = result.text_content,
                        Title = result.title,
                        Metadata = metadata
                    };
                }
                catch (PythonException ex)
                {
                    throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                }
            }
        }

        public MarkDownResult Convert(Stream stream, string extension, MarkDownConfig config = null)
        {
            using (Py.GIL())
            {
                try
                {
                    PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");
                    dynamic markitdown = Py.Import("markitdown");
                    dynamic md = markitdown.MarkItDown();
                    var kwargs = new PyDict();
                    PopulateKwargs(kwargs, config);
                    dynamic io = Py.Import("io");
                    byte[] bytes;
                    if (stream is MemoryStream ms)
                    {
                        bytes = ms.ToArray();
                    }
                    else
                    {
                        using (var ms2 = new MemoryStream())
                        {
                            stream.CopyTo(ms2);
                            bytes = ms2.ToArray();
                        }
                    }
                    using (var pyStream = io.BytesIO(bytes))
                    {
                        kwargs["file_extension"] = new PyString(extension);
                        dynamic result = ((PyObject)md.convert_stream).Invoke(new PyTuple(new[] { (PyObject)pyStream }), kwargs);
                        var metadata = new Dictionary<string, object>();
                        try
                        {
                            metadata = ConvertPyDictToDict(result.metadata);
                        }
                        catch
                        {
                            // metadata not available in this version
                        }
                        return new MarkDownResult
                        {
                            Text = result.text_content,
                            Title = result.title,
                            Metadata = metadata
                        };
                    }
                }
                catch (PythonException ex)
                {
                    throw new Exception($"MarkItDown conversion failed: {ex.Message}");
                }
            }
        }

        public MarkDownResult ConvertUri(string uri, MarkDownConfig config = null)
        {
            using (Py.GIL())
            {
                var markitdown = Py.Import("markitdown");
                var mdClass = markitdown.GetAttr("MarkItDown");
                var md = mdClass.Invoke();
                var args = new PyObject[] { new PyString(uri) };
                PyDict kwargs = null;
                if (config != null)
                {
                    kwargs = ConvertConfigToPyDict(config);
                }
                var result = md.InvokeMethod("convert", args, kwargs);
                return CreateResultFromPyObject(result);
            }
        }

        private static void PopulateKwargs(PyDict kwargs, MarkDownConfig config)
        {
            if (config != null)
            {
                if (!string.IsNullOrEmpty(config.DocIntelEndpoint)) kwargs["docintel_endpoint"] = new PyString(config.DocIntelEndpoint);
                if (config.LlmClient != null) kwargs["llm_client"] = config.LlmClient;
                if (!string.IsNullOrEmpty(config.LlmModel)) kwargs["llm_model"] = new PyString(config.LlmModel);
                if (!string.IsNullOrEmpty(config.LlmPrompt)) kwargs["llm_prompt"] = new PyString(config.LlmPrompt);
                if (config.KeepDataUris) kwargs["keep_data_uris"] = PyObject.FromManagedObject(config.KeepDataUris);
                if (config.EnablePlugins) kwargs["enable_plugins"] = PyObject.FromManagedObject(config.EnablePlugins);
                if (!string.IsNullOrEmpty(config.DocIntelKey)) kwargs["docintel_key"] = new PyString(config.DocIntelKey);
                if (config.Plugins.Any())
                {
                    foreach (var plugin in config.Plugins)
                    {
                        Py.Import(plugin);
                    }
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

        private static Dictionary<string, object> ConvertPyDictToDict(dynamic pyDict)
        {
            var dict = new Dictionary<string, object>();
            if (pyDict != null)
            {
                foreach (var key in pyDict.keys())
                {
                    dict[key.ToString()] = pyDict[key];
                }
            }
            return dict;
        }

        private MarkDownResult CreateResultFromPyObject(PyObject pyResult)
        {
            var textAttr = pyResult.GetAttr("text_content");
            string text = textAttr.IsNone() ? "" : textAttr.ToString();
            var titleAttr = pyResult.GetAttr("title");
            string title = titleAttr.IsNone() ? "" : titleAttr.ToString();
            var metadataDict = new Dictionary<string, object>();
            try
            {
                var metadataAttr = pyResult.GetAttr("metadata");
                if (!metadataAttr.IsNone())
                {
                    metadataDict = ConvertPyDictToDict(metadataAttr);
                }
            }
            catch
            {
                // metadata not available
            }
            return new MarkDownResult { Text = text, Title = title, Metadata = metadataDict };
        }

        private PyDict ConvertConfigToPyDict(MarkDownConfig config)
        {
            var kwargs = new PyDict();
            if (!string.IsNullOrEmpty(config.DocIntelEndpoint)) kwargs["docintel_endpoint"] = new PyString(config.DocIntelEndpoint);
            if (config.LlmClient != null) kwargs["llm_client"] = config.LlmClient;
            if (!string.IsNullOrEmpty(config.LlmModel)) kwargs["llm_model"] = new PyString(config.LlmModel);
            if (!string.IsNullOrEmpty(config.LlmPrompt)) kwargs["llm_prompt"] = new PyString(config.LlmPrompt);
            if (config.KeepDataUris) kwargs["keep_data_uris"] = PyObject.FromManagedObject(config.KeepDataUris);
            if (config.EnablePlugins) kwargs["enable_plugins"] = PyObject.FromManagedObject(config.EnablePlugins);
            if (!string.IsNullOrEmpty(config.DocIntelKey)) kwargs["docintel_key"] = new PyString(config.DocIntelKey);
            if (config.Plugins.Any())
            {
                foreach (var plugin in config.Plugins)
                {
                    Py.Import(plugin);
                }
            }
            return kwargs;
        }
    }
}
