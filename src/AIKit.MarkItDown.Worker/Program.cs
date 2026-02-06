using Python.Runtime;
using System.Diagnostics;
using System.Text.Json;

public class Program
{
    public static void Main()
    {
        try
        {
            // Read JSON from stdin
            var json = Console.In.ReadToEnd();
            var input = JsonSerializer.Deserialize<WorkerInput>(json);
            if (input == null)
                throw new Exception("Invalid input");

            // Initialize Python
            var pythonExe = GetPythonExecutable()
                ?? throw new Exception("Python not found");
            var dllPath = GetPythonDllPath(pythonExe)
                ?? throw new Exception("Python DLL not found");
            Runtime.PythonDLL = dllPath;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            string result;
            using (Py.GIL())
            {
                DisablePythonLogging();
                dynamic markitdown = Py.Import("markitdown");
                dynamic md = markitdown.MarkItDown();

                // Create Python objects from string configs
                if (input.Kwargs.ContainsKey("openai_api_key") && input.Kwargs["openai_api_key"] is string apiKey)
                {
                    dynamic openai = Py.Import("openai");
                    dynamic client = openai.OpenAI(api_key: apiKey);
                    input.Kwargs["llm_client"] = client;
                    input.Kwargs.Remove("openai_api_key");
                }

                if (input.Kwargs.ContainsKey("docintel_key") && input.Kwargs["docintel_key"] is string key)
                {
                    dynamic azure_credentials = Py.Import("azure.core.credentials");
                    dynamic credential = azure_credentials.AzureKeyCredential(key);
                    input.Kwargs["docintel_credential"] = credential;
                    input.Kwargs.Remove("docintel_key");
                }

                var kwargs = BuildKwargs(input.Kwargs);

                if (input.Type == "file")
                {
                    dynamic r = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(input.Path) }), kwargs);
                    result = r.text_content;
                }
                else if (input.Type == "stream")
                {
                    var data = Convert.FromBase64String(input.Data!);
                    dynamic io = Py.Import("io");
                    using var pyStream = io.BytesIO(data);
                    kwargs["file_extension"] = new PyString(input.Extension);
                    dynamic r = ((PyObject)md).InvokeMethod("convert_stream", new PyTuple(new PyObject[] { pyStream }), kwargs);
                    result = r.text_content;
                }
                else if (input.Type == "uri")
                {
                    dynamic r = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(input.Uri) }), kwargs);
                    result = r.text_content;
                }
                else
                {
                    throw new Exception("Invalid type");
                }
            }

            var output = new { success = true, result };
            Console.Write(JsonSerializer.Serialize(output));
        }
        catch (Exception ex)
        {
            var output = new { success = false, error = ex.Message };
            Console.Write(JsonSerializer.Serialize(output));
        }
    }

    private static void DisablePythonLogging()
        => PythonEngine.RunSimpleString("import logging; logging.disable(logging.WARNING)");

    private static string? GetPythonExecutable()
    {
        // First try commands in PATH
        foreach (var cmd in new[] { "python", "python3", "py" })
            if (IsCommandAvailable(cmd, "--version"))
                return Run(cmd, "-c \"import sys; print(sys.executable)\"");

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
            if (File.Exists(path) && IsCommandAvailable(path, "--version"))
                return path;

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
            throw new Exception($"Command '{cmd} {args}' failed");

        return p.StandardOutput.ReadToEnd().Trim();
    }

    private static bool IsCommandAvailable(string cmd, string args)
    {
        try { Run(cmd, args); return true; }
        catch { return false; }
    }

    private static PyDict BuildKwargs(Dictionary<string, object> config)
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

    private record WorkerInput(string Type, string? Path, string? Data, string? Extension, string? Uri, Dictionary<string, object> Kwargs);
}