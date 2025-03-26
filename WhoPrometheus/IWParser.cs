using System.Reflection;
using Tolltech.Core.Helpers;
using Vostok.Logging.Abstractions;

namespace Tolltech.WhoPrometheus;

public interface IWParser
{
    SshClientInfo[] Parse(string wCommand);
    string Parse(SshClientInfo clients);
}

public class WParser : IWParser
{
    private readonly ILog log = new SilentLog();
    
    public SshClientInfo[] Parse(string wCommand)
    {
        return parse(wCommand).ToArray();
    }

    public string Parse(SshClientInfo client)
    {
        //{interface="wg0",enabled="true",address="10.8.0.3",name="Kate"}
        var propInfos = typeof(SshClientInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return $"{{{string.Join(',', propInfos.Select(p => $"{p.Name.ToLower()}=\"{p.GetValue(client)}\""))}}}";
    }

    private IEnumerable<SshClientInfo> parse(string wCommand)
    {
        var lines = wCommand.SplitLines();

        var header = lines.Skip(1).Take(1).First();
        var columnInds = new List<int>();
        columnInds.Add(0);
        for (var i = 1; i < header.Length; i++)
        {
            var prev = header[i - 1];
            var current = header[i];
            if (char.IsLetterOrDigit(prev))
            {
                continue;
            }
            
            if (!char.IsLetterOrDigit(current))
            {
                continue;
            }
            
            columnInds.Add(i);
        }

        log.Info($"W Parser: header {header}");
        log.Info($"W Parser: Word inds {string.Join(',', columnInds)}");
        
        foreach (var line in lines.Skip(2))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var words = new List<string>();
            for (var columnIndex = 0; columnIndex < columnInds.Count; columnIndex++)
            {
                var current = columnInds[columnIndex];
                var next = columnInds.Count - 1 > columnIndex ? columnInds[columnIndex + 1] : line.Length;

                log.Info($"W Parser: Try get word from {current} to {next} of line '{line}'");
                
                var word = line.Substring(current, next - current).Trim(' ');
                if (string.IsNullOrWhiteSpace(word)) word = "NONE";
                words.Add(word.Trim(' '));
            }

            yield return new SshClientInfo
            {
                User = words[0],
                Tty = words[1],
                From = words[2],
                Login = words[3],
                Idle = words[4],
                JCpu = words[5],
                PCpu = words[6],
                What = words[7]
            };
        }
    }
}