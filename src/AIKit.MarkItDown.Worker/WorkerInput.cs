using System.Collections.Generic;

namespace AIKit.MarkItDown.Worker;

/// <summary>
/// Input model for the worker process.
/// </summary>
/// <param name="Type">The type of conversion: "file", "stream", or "uri".</param>
/// <param name="Path">The file path for file conversions.</param>
/// <param name="Data">The base64-encoded data for stream conversions.</param>
/// <param name="Extension">The file extension for stream conversions.</param>
/// <param name="Uri">The URI for URI conversions.</param>
/// <param name="Kwargs">Additional keyword arguments for the conversion.</param>
record WorkerInput(string Type, string? Path, string? Data, string? Extension, string? Uri, Dictionary<string, object> Kwargs);