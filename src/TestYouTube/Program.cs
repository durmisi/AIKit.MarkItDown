// See https://aka.ms/new-console-template for more information
using AIKit.MarkItDown;

var converter = new MarkDownConverter();
Console.WriteLine("Testing YouTube URL conversion...");
try
{
    var result = converter.ConvertUri("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    Console.WriteLine($"Success: {result.Length} chars");
    Console.WriteLine("Content:");
    Console.WriteLine(result);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed: {ex.Message}");
}
