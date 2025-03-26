using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Vostok.Logging.Abstractions;

namespace Tolltech.WhoPrometheus;

public class WhoPrometheusListener : IWhoPrometheusListener
{
    private readonly ILog log = LogProvider.Get();

    public async Task StartListeningAsync(int port)
    {
        log.Info("Starting WhoPrometheusListener");
        
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        log.Info($"Listening {port}");
        
        while (listener.IsListening)
        {
            var context = await listener.GetContextAsync();
            var response = context.Response;
            var responseString = await GetInfo();
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentType = "text/plain";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
    }

    private async Task<string> GetInfo()
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ipconfig",
                Arguments = string.Empty,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        var sb = new StringBuilder();
        proc.Start();
        while (!proc.StandardOutput.EndOfStream)
        {
            sb.AppendLine(await proc.StandardOutput.ReadLineAsync());
        }
        
        return sb.ToString();
    }

    private void WriteResponse(NetworkStream networkStream, string message)
    {
        var contentLength = Encoding.UTF8.GetByteCount(message);

        var response = $@"HTTP/1.1 200 OK
Content-Type: text/plain; charset=UTF-8
Content-Length: {contentLength}

{message}";
        var responseBytes = Encoding.UTF8.GetBytes(response);

        networkStream.Write(responseBytes, 0, responseBytes.Length);
    }
}