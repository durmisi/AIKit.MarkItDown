using Python.Runtime;
using System.Diagnostics;

namespace AIKit.MarkItDown;

public class MarkDownConverter
{
    private const long MaxStreamSizeBytes = 100 * 1024 * 1024; // 100 MB
    private const int DefaultTimeoutMs = 30_000;

    static MarkDownConverter()
    {
        string pythonExe = GetPythonExecutable()
            ?? throw new InvalidOperationException("Python 3.8+ not found.");

        string dllPath = GetPythonDllPath(pythonExe)
            ?? throw new InvalidOperationException("Python DLL not found.");

        Runtime.PythonDLL = dllPath;

        if (!PythonEngine.IsInitialized)
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }

    /* ---------------------------------------------------------
     * PUBLIC ASYNC API
     * --------------------------------------------------------- */

    public Task<string> ConvertAsync(
        string filePath,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync(() => ConvertInternal(filePath, config), ct);

    public Task<string> ConvertAsync(
        Stream stream,
        string extension,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync(() => ConvertInternal(stream, extension, config), ct);

    public Task<string> ConvertUriAsync(
        string uri,
        MarkDownConfig? config = null,
        CancellationToken ct = default)
        => RunAsync(() => ConvertUriInternal(uri, config), ct);

    /* ---------------------------------------------------------
     * PUBLIC SYNC API
     * --------------------------------------------------------- */

    public string Convert(
        string filePath,
        MarkDownConfig? config = null)
        => ConvertInternal(filePath, config);

    public string Convert(
        Stream stream,
        string extension,
        MarkDownConfig? config = null)
        => ConvertInternal(stream, extension, config);

    public string ConvertUri(
        string uri,
        MarkDownConfig? config = null)
        => ConvertUriInternal(uri, config);

    /* ---------------------------------------------------------
     * ASYNC EXECUTION WRAPPER
     * --------------------------------------------------------- */

    private static async Task<string> RunAsync(
        Func<string> work,
        CancellationToken ct,
        int timeoutMs = DefaultTimeoutMs)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        return await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            return work();
        }, cts.Token);
    }

    /* ---------------------------------------------------------
     * SYNC PYTHON CORE (GIL OWNERSHIP)
     * --------------------------------------------------------- */

    private string ConvertInternal(string filePath, MarkDownConfig? config)
    {
        config?.Let(ValidateConfigRequirements);

        byte[] fileBytes;
        try
        {
            fileBytes = File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            throw new MarkItDownConversionException($"File not found: Failed to read file '{filePath}'.", ex);
        }
        string extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        using (Py.GIL())
        {
            DisablePythonLogging();

            dynamic markitdown = Py.Import("markitdown");
            dynamic md = markitdown.MarkItDown();

            var kwargs = BuildKwargs(config);

            if (extension == "pdf")
            {
                dynamic io = Py.Import("io");
                using var pyStream = io.BytesIO(fileBytes);
                kwargs["file_extension"] = new PyString(extension);
                kwargs["check_extractable"] = PyObject.FromManagedObject(false);

                dynamic result = ((PyObject)md).InvokeMethod("convert_stream", new PyTuple(new PyObject[] { pyStream }), kwargs);
                return result.text_content;
            }

            dynamic r = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(filePath) }), kwargs);
            return r.text_content;
        }
    }

    private string ConvertInternal(Stream stream, string extension, MarkDownConfig? config)
    {
        if (stream.Length > MaxStreamSizeBytes)
            throw new MarkItDownConversionException("Stream too large.");

        config?.Let(ValidateConfigRequirements);

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        byte[] bytes = ms.ToArray();

        using (Py.GIL())
        {
            DisablePythonLogging();

            dynamic markitdown = Py.Import("markitdown");
            dynamic md = markitdown.MarkItDown();
            dynamic io = Py.Import("io");

            var kwargs = BuildKwargs(config);
            kwargs["file_extension"] = new PyString(extension);

            using var pyStream = io.BytesIO(bytes);
            dynamic result = ((PyObject)md).InvokeMethod("convert_stream", new PyTuple(new PyObject[] { pyStream }), kwargs);
            return result.text_content;
        }
    }

public static void ValidateConfigRequirements(MarkDownConfig config)
{
    if (config == null)
        return;

    using (Py.GIL())
    {
        // Azure Document Intelligence
        if (!string.IsNullOrEmpty(config.DocIntelEndpoint) ||
            !string.IsNullOrEmpty(config.DocIntelKey))
        {
            try
            {
                Py.Import("azure.ai.documentintelligence");
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException(
                    "Azure Document Intelligence package not installed. " +
                    "Install 'azure-ai-documentintelligence'.", ex);
            }
        }

        // OpenAI / LLM support
        if (!string.IsNullOrEmpty(config.OpenAiApiKey) ||
            !string.IsNullOrEmpty(config.LlmModel))
        {
            try
            {
                Py.Import("openai");
            }
            catch (PythonException ex)
            {
                throw new MarkItDownConversionException(
                    "OpenAI package not installed. Install 'openai'.", ex);
            }
        }

        // Plugins
        if (config.Plugins != null && config.Plugins.Any())
        {
            foreach (var plugin in config.Plugins)
            {
                try
                {
                    Py.Import(plugin);
                }
                catch (PythonException ex)
                {
                    throw new MarkItDownConversionException(
                        $"Plugin '{plugin}' not installed or failed to import.", ex);
                }
            }
        }
    }
}

public static dynamic CreateOpenAiClient(string apiKey)
{
    using (Py.GIL())
    {
        try
        {
            dynamic openai = Py.Import("openai");
            dynamic client = openai.OpenAI(api_key: apiKey);
            return client;
        }
        catch (PythonException ex)
        {
            throw new MarkItDownConversionException(
                "Failed to create OpenAI client.", ex);
        }
    }
}


    private string ConvertUriInternal(string uri, MarkDownConfig? config)
    {
        config?.Let(ValidateConfigRequirements);

        using (Py.GIL())
        {
            DisablePythonLogging();

            dynamic markitdown = Py.Import("markitdown");
            dynamic md = markitdown.MarkItDown();

            var kwargs = BuildKwargs(config);
            dynamic result = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(uri) }), kwargs);
            return result.text_content;
        }
    }

    /* ---------------------------------------------------------
     * HELPERS
     * --------------------------------------------------------- */

    private static void DisablePythonLogging()
        => PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");

    private static PyDict BuildKwargs(MarkDownConfig? config)
    {
        var kwargs = new PyDict();

        if (config == null)
            return kwargs;

        if (!string.IsNullOrEmpty(config.DocIntelEndpoint))
            kwargs["docintel_endpoint"] = new PyString(config.DocIntelEndpoint);

        if (!string.IsNullOrEmpty(config.DocIntelKey))
            kwargs["docintel_key"] = new PyString(config.DocIntelKey);

        if (!string.IsNullOrEmpty(config.OpenAiApiKey))
            kwargs["openai_api_key"] = new PyString(config.OpenAiApiKey);

        if (!string.IsNullOrEmpty(config.LlmModel))
            kwargs["llm_model"] = new PyString(config.LlmModel);

        if (!string.IsNullOrEmpty(config.LlmPrompt))
            kwargs["llm_prompt"] = new PyString(config.LlmPrompt);

        if (config.KeepDataUris)
            kwargs["keep_data_uris"] = PyObject.FromManagedObject(true);

        if (config.EnablePlugins)
            kwargs["enable_plugins"] = PyObject.FromManagedObject(true);

        return kwargs;
    }

    /* ---------------------------------------------------------
     * PYTHON DISCOVERY (unchanged)
     * --------------------------------------------------------- */

    private static string? GetPythonExecutable()
    {
        foreach (var cmd in new[] { "python", "python3", "py" })
            if (IsCommandAvailable(cmd, "--version"))
                return Run(cmd, "-c \"import sys; print(sys.executable)\"");

        return null;
    }

    private static string? GetPythonDllPath(string pythonExe)
    {
        string dll = Run(pythonExe, "-c \"import sys; print(f'python{sys.version_info.major}{sys.version_info.minor}.dll')\"");
        return string.IsNullOrEmpty(dll) ? null : Path.Combine(Path.GetDirectoryName(pythonExe)!, dll);
    }

    private static string Run(string cmd, string args)
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
            throw new InvalidOperationException($"Command '{cmd} {args}' failed with exit code {p.ExitCode}");

        return p.StandardOutput.ReadToEnd().Trim();
    }

    private static bool IsCommandAvailable(string cmd, string args)
    {
        try { Run(cmd, args); return true; }
        catch { return false; }
    }
}

/* ---------------------------------------------------------
 * SMALL EXTENSION
 * --------------------------------------------------------- */

internal static class FuncExtensions
{
    public static void Let<T>(this T value, Action<T> action) => action(value);
}
