using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using AIKit.MarkItDown.Worker;

/// <summary>
/// Worker program that handles Markdown conversion using Python's markitdown library.
/// Reads input from stdin, performs conversion, and writes result to stdout.
/// </summary>

try
{
    // Read JSON from stdin
    var json = Console.In.ReadToEnd();
    Console.Error.WriteLine($"Input received: {json.Length} characters");
    var input = JsonSerializer.Deserialize<WorkerInput>(json);
    if (input == null)
        throw new Exception("Invalid input");
    Console.Error.WriteLine($"Input type: {input.Type}, Path: {input.Path}, Uri: {input.Uri}, Data length: {input.Data?.Length}");

    // Initialize Python
    var pythonExe = PythonHelper.GetPythonExecutable()
        ?? throw new Exception("Python not found");
    var dllPath = PythonHelper.GetPythonDllPath(pythonExe)
        ?? throw new Exception("Python DLL not found");
    Runtime.PythonDLL = dllPath;
    Console.Error.WriteLine($"Using Python: {pythonExe}, DLL: {dllPath}");
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Environment.SetEnvironmentVariable("PYTHONNET_RUNTIME", "coreclr");
        Console.Error.WriteLine("Set PYTHONNET_RUNTIME to coreclr");
    }
    PythonEngine.Initialize();
    PythonEngine.BeginAllowThreads();
    Console.Error.WriteLine("Python engine initialized");

    string result;
    using (Py.GIL())
    {
        PythonHelper.DisablePythonLogging();
        Console.Error.WriteLine("Python logging disabled");

        dynamic md_module = Py.Import("markitdown");
        Console.Error.WriteLine("markitdown module imported");

        dynamic MarkItDown;
        try
        {
            MarkItDown = md_module.GetAttr("MarkItDown");
        }
        catch (Exception ex)
        {
            throw new Exception($"MarkItDown class not found in markitdown module. Ensure markitdown[all] is installed in the Python environment used by the Worker. Inner exception: {ex.Message}");
        }
        Console.Error.WriteLine("MarkItDown class retrieved");

        dynamic md = MarkItDown();
        Console.Error.WriteLine("MarkItDown instance created");

        // Create Python objects from string configs
        if (input.Kwargs.TryGetValue("openai_api_key", out var openaiValue) && openaiValue is string apiKey)
        {
            dynamic openai = Py.Import("openai");
            dynamic OpenAI = openai.GetAttr("OpenAI");
            dynamic client = OpenAI(api_key: apiKey);
            input.Kwargs["llm_client"] = client;
            input.Kwargs.Remove("openai_api_key");
            Console.Error.WriteLine("OpenAI client configured");
        }

        if (input.Kwargs.TryGetValue("docintel_key", out var docintelValue) && docintelValue is string key)
        {
            dynamic azure_credentials = Py.Import("azure.core.credentials");
            dynamic AzureKeyCredential = azure_credentials.GetAttr("AzureKeyCredential");
            dynamic credential = AzureKeyCredential(key);
            input.Kwargs["docintel_credential"] = credential;
            input.Kwargs.Remove("docintel_key");
            Console.Error.WriteLine("Azure Document Intelligence credential configured");
        }

        var kwargs = PythonHelper.BuildKwargs(input.Kwargs);
        Console.Error.WriteLine($"Kwargs built: {input.Kwargs.Count} items");

        if (input.Type == "file")
        {
            Console.Error.WriteLine($"Converting file: {input.Path}");
            dynamic r = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(input.Path) }), kwargs);
            result = r.text_content;
            Console.Error.WriteLine($"File conversion completed, result length: {result.Length}");
        }
        else if (input.Type == "stream")
        {
            Console.Error.WriteLine($"Converting stream, extension: {input.Extension}, data length: {input.Data!.Length}");
            var data = Convert.FromBase64String(input.Data!);
            dynamic io = Py.Import("io");
            using var pyStream = io.BytesIO(data);
            kwargs["file_extension"] = new PyString(input.Extension);
            dynamic r = ((PyObject)md).InvokeMethod("convert_stream", new PyTuple(new PyObject[] { pyStream }), kwargs);
            result = r.text_content;
            Console.Error.WriteLine($"Stream conversion completed, result length: {result.Length}");
        }
        else if (input.Type == "uri")
        {
            Console.Error.WriteLine($"Converting URI: {input.Uri}");
            dynamic r = ((PyObject)md).InvokeMethod("convert", new PyTuple(new PyObject[] { new PyString(input.Uri) }), kwargs);
            result = r.text_content;
            Console.Error.WriteLine($"URI conversion completed, result length: {result.Length}");
        }
        else
        {
            throw new Exception("Invalid type");
        }
    }

    var output = new { success = true, result };
    Console.Write(JsonSerializer.Serialize(output));
    Console.Error.WriteLine("Output sent successfully");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Exception: {ex.Message}");
    Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
    var output = new { success = false, error = ex.Message };
    Console.Write(JsonSerializer.Serialize(output));
}