using System.Diagnostics;
using System.Net;
using System.Text;
using Vostok.Logging.Abstractions;

namespace Tolltech.WhoPrometheus;

public class WhoPrometheusListener : IWhoPrometheusListener
{
    private readonly ILog log = LogProvider.Get();
    private readonly int port
        ;

    public WhoPrometheusListener(WhoPrometheusSettings settings)
    {
        port = settings.Port;
    }

    public async Task StartListeningAsync()
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
            try
            {
                var responseString = await GetInfo();
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                log.Error(e, $"WhePrometheusException");
            }
            finally
            {
                response.Close();
            }
        }
    }

    private async Task<string> GetInfo()
    {
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

        var sb = new StringBuilder();
        proc.Start();
        while (!proc.StandardOutput.EndOfStream)
        {
            sb.AppendLine(await proc.StandardOutput.ReadLineAsync());
        }
        
        return sb.ToString();
    }
}