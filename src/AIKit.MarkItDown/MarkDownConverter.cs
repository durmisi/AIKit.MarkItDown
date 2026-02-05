using Python.Runtime;
using System.Diagnostics;

namespace AIKit.MarkItDown;

public class MarkDownConverter
{
    private const long MaxStreamSizeBytes = 100 * 1024 * 1024; // 100 MB
    private const int ProcessTimeoutMs = 5000;
    private const int PythonInitTimeoutMs = 30000;

    static MarkDownConverter()
    {
        string pythonExe = GetPythonExecutable();
        if (pythonExe == null)
        {
            throw new InvalidOperationException("Python 3.8+ is required but not found. Please install Python and run install.ps1 to install dependencies.");
        }
        string dllPath = GetPythonDllPath(pythonExe);
        if (dllPath == null)
        {
            throw new InvalidOperationException("Python DLL not found. Ensure Python is properly installed.");
        }
        Runtime.PythonDLL = dllPath;

        if (!PythonEngine.IsInitialized)
        {
            PythonEngine.Initialize();
        }
    }

    /// <summary>
    /// Creates a Python OpenAI client instance for internal use.
    /// Requires the 'openai' Python package to be installed.
    /// </summary>
    /// <param name="apiKey">OpenAI API key</param>
    /// <returns>PyObject representing the OpenAI client</returns>
    public static PyObject CreateOpenAiClient(string apiKey)
    {
        using (Py.GIL())
        {
            try
            {
                dynamic openai = Py.Import("openai");
                var client = openai.OpenAI(api_key: apiKey);
                return client;
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException("Failed to create OpenAI client. Ensure 'openai' package is installed.", ex);
            }
        }
    }

    /// <summary>
    /// Validates that required Python packages are installed for advanced features.
    /// </summary>
    /// <param name="config">Configuration to validate against</param>
    public static void ValidateConfigRequirements(MarkDownConfig config)
    {
        using (Py.GIL())
        {
            if (!string.IsNullOrEmpty(config.DocIntelEndpoint) || !string.IsNullOrEmpty(config.DocIntelKey))
            {
                try
                {
                    Py.Import("azure.ai.documentintelligence");
                }
                catch (PythonException)
                {
                    throw new MarkItDownConversionException("Azure Document Intelligence package not installed. Run install.ps1 or install 'azure-ai-documentintelligence'.");
                }
            }

            if (!string.IsNullOrEmpty(config.OpenAiApiKey) || !string.IsNullOrEmpty(config.LlmModel))
            {
                try
                {
                    Py.Import("openai");
                }
                catch (PythonException)
                {
                    throw new MarkItDownConversionException("OpenAI package not installed. Run install.ps1 or install 'openai'.");
                }
            }

            if (config.Plugins.Any())
            {
                foreach (var plugin in config.Plugins)
                {
                    try
                    {
                        Py.Import(plugin);
                    }
                    catch (PythonException ex)
                    {
                        throw new MarkItDownConversionException($"Plugin '{plugin}' not installed or failed to import.", ex);
                    }
                }
            }
        }
    }

    public string Convert(string filePath)
    {
        return Convert(filePath, null);
    }

    public string Convert(string filePath, MarkDownConfig config = null)
    {
        if (config != null)
        {
            ValidateConfigRequirements(config);
        }

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
                        throw new MarkItDownConversionException($"File not found: {filePath}", ex);
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
                return result.text_content;
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException($"MarkItDown conversion failed: {ex.Message}", ex);
            }
        }
    }

    public string Convert(Stream stream, string extension, MarkDownConfig config = null)
    {
        if (config != null)
        {
            ValidateConfigRequirements(config);
        }

        if (stream.Length > MaxStreamSizeBytes)
        {
            throw new MarkItDownConversionException($"Stream size {stream.Length} bytes exceeds maximum allowed size of {MaxStreamSizeBytes} bytes.");
        }

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
                    return result.text_content;
                }
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException($"MarkItDown conversion failed: {ex.Message}", ex);
            }
        }
    }

    public string ConvertUri(string uri, MarkDownConfig config = null)
    {
        if (config != null)
        {
            ValidateConfigRequirements(config);
        }

        using (Py.GIL())
        {
            try
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
                var textAttr = result.GetAttr("text_content");
                return textAttr.IsNone() ? "" : textAttr.ToString();
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException($"MarkItDown URI conversion failed: {ex.Message}", ex);
            }
        }
    }

    private static void PopulateKwargs(PyDict kwargs, MarkDownConfig config)
    {
        if (config != null)
        {
            if (!string.IsNullOrEmpty(config.DocIntelEndpoint)) kwargs["docintel_endpoint"] = new PyString(config.DocIntelEndpoint);
            if (!string.IsNullOrEmpty(config.OpenAiApiKey))
            {
                var llmClient = CreateOpenAiClient(config.OpenAiApiKey);
                kwargs["llm_client"] = llmClient;
            }
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
            if (!process.WaitForExit(ProcessTimeoutMs))
            {
                process.Kill();
                return null;
            }
            string output = process.StandardOutput.ReadToEnd();
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
            if (!process.WaitForExit(ProcessTimeoutMs))
            {
                process.Kill();
                return null;
            }
            string dllName = process.StandardOutput.ReadToEnd().Trim();
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
            if (!process.WaitForExit(ProcessTimeoutMs))
            {
                process.Kill();
                return Path.GetDirectoryName(pythonExe); // fallback
            }
            string exePath = process.StandardOutput.ReadToEnd().Trim();
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

    private PyDict ConvertConfigToPyDict(MarkDownConfig config)
    {
        var kwargs = new PyDict();
        if (!string.IsNullOrEmpty(config.DocIntelEndpoint)) kwargs["docintel_endpoint"] = new PyString(config.DocIntelEndpoint);
        if (!string.IsNullOrEmpty(config.OpenAiApiKey))
        {
            var llmClient = CreateOpenAiClient(config.OpenAiApiKey);
            kwargs["llm_client"] = llmClient;
        }
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