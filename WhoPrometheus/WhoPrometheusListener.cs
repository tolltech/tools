using System.Diagnostics;
using System.Net;
using System.Text;
using Vostok.Logging.Abstractions;

namespace Tolltech.WhoPrometheus;

public class WhoPrometheusListener : IWhoPrometheusListener
{
    private readonly IWParser wParser;
    private readonly ILog log = LogProvider.Get();
    private readonly int port
        ;

    public WhoPrometheusListener(WhoPrometheusSettings settings, IWParser wParser)
    {
        this.wParser = wParser;
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
        var wInfo = await GetWInfo();
        //# HELP wg-easy and wireguard metrics
        // 
        //# HELP wireguard_latest_handshake_seconds UNIX timestamp seconds of the last handshake
        //# TYPE wireguard_latest_handshake_seconds gauge
        //wireguard_latest_handshake_seconds{interface="wg0",enabled="true",address="10.8.0.2",name="tolltech"} 13509.351
        //wireguard_latest_handshake_seconds{interface="wg0",enabled="true",address="10.8.0.3",name="Kate"} 4.351

        //log.Info($"Get w info:\r\n{wInfo}");
        
        var sshClients = wParser.Parse(wInfo);
        
        
        
        const string metricName = "tolls_ssh_clients";
        var sb = new StringBuilder();
        sb.AppendLine($"# HELP ssh metrics");
        sb.AppendLine();
        sb.AppendLine($"# HELP ssh metrics from w command");
        sb.AppendLine($"# TYPE {metricName} gauge");
        foreach (var sshClientInfo in sshClients)
        {
            sb.AppendLine($"{metricName}{SShClientInfoToMetric(sshClientInfo)} 1");
        }

        const string totalMetricsName = "tolls_ssh_clients_count";
        sb.AppendLine();
        sb.AppendLine($"# HELP ssh metrics from w command");
        sb.AppendLine($"# TYPE {totalMetricsName} gauge");
        sb.AppendLine($"{totalMetricsName}{{ssh_clients=\"ssh_clients\"}} {sshClients.Length}");
        
        return sb.ToString();
    }

    private string SShClientInfoToMetric(SshClientInfo clientInfo)
    {
        return wParser.Parse(clientInfo);
    }
    
    private async Task<string> GetWInfo()
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