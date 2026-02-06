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
    var input = JsonSerializer.Deserialize<WorkerInput>(json);
    if (input == null)
        throw new Exception("Invalid input");

    // Initialize Python
    var pythonExe = PythonHelper.GetPythonExecutable()
        ?? throw new Exception("Python not found");
    var dllPath = PythonHelper.GetPythonDllPath(pythonExe)
        ?? throw new Exception("Python DLL not found");
    Runtime.PythonDLL = dllPath;
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Environment.SetEnvironmentVariable("PYTHONNET_RUNTIME", "coreclr");
    }
    PythonEngine.Initialize();
    PythonEngine.BeginAllowThreads();

    string result;
    using (Py.GIL())
    {
        PythonHelper.DisablePythonLogging();
        dynamic md_module = Py.Import("markitdown");
        dynamic MarkItDown = md_module.GetAttr("MarkItDown");
        dynamic md = MarkItDown();

        // Create Python objects from string configs
        if (input.Kwargs.TryGetValue("openai_api_key", out var openaiValue) && openaiValue is string apiKey)
        {
            dynamic openai = Py.Import("openai");
            dynamic OpenAI = openai.GetAttr("OpenAI");
            dynamic client = OpenAI(api_key: apiKey);
            input.Kwargs["llm_client"] = client;
            input.Kwargs.Remove("openai_api_key");
        }

        if (input.Kwargs.TryGetValue("docintel_key", out var docintelValue) && docintelValue is string key)
        {
            dynamic azure_credentials = Py.Import("azure.core.credentials");
            dynamic AzureKeyCredential = azure_credentials.GetAttr("AzureKeyCredential");
            dynamic credential = AzureKeyCredential(key);
            input.Kwargs["docintel_credential"] = credential;
            input.Kwargs.Remove("docintel_key");
        }

        var kwargs = PythonHelper.BuildKwargs(input.Kwargs);

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