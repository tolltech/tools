// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

var proc = new Process 
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "w",
        Arguments = string.Empty,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
    }
};

proc.Start();
while (!proc.StandardOutput.EndOfStream)
{
    var line = proc.StandardOutput.ReadLine();
    Console.WriteLine(line);
}

