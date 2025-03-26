using FluentAssertions;
using Tolltech.WhoPrometheus;

namespace WhoPrometheusTests;

public class Tests
{
    private WParser wParser;

    [SetUp]
    public void Setup()
    {
        wParser = new WParser();
    }

    [Test]
    [TestCase(@" 17:42:21 up  2:51,  5 users,  load average: 1,65, 1,54, 1,56
USER     TTY      FROM             LOGIN@   IDLE   JCPU   PCPU  WHAT
tolltech tty1     -                14:51    2:51m  0.03s  0.01s -bash
root              192.168.2.3      15:15    2:19m  0.00s  0.98s sshd: root@pts/0")]
    public void Test1(string input)
    {
        var sshClients = wParser.Parse(input);
        sshClients.Length.Should().Be(2);

        sshClients[0].Should().BeEquivalentTo(new SshClientInfo
        {
            User = "tolltech",
            Tty = "tty1",
            From = "-",
            Login = "14:51",
            Idle = "2:51m",
            JCpu = "0.03s",
            PCpu = "0.01s",
            What = "-bash"
        });
        sshClients[1].Should().BeEquivalentTo(new SshClientInfo
        {
            User = "root",
            Tty = "NONE",
            From = "192.168.2.3",
            Login = "15:15",
            Idle = "2:19m",
            JCpu = "0.00s",
            PCpu = "0.98s",
            What = "sshd: root@pts/0"
        });
    }

    [Test]
    public void TestParseClient()
    {
        var client = new SshClientInfo
        {
            User = "tolltech",
            Tty = "tty1",
            From = "-",
            Login = "14:51",
            Idle = "2:51m",
            JCpu = "0.03s",
            PCpu = "0.01s",
            What = "-bash"
        };
        
        var actual = wParser.Parse(client);
        actual.Should().Be(@"{user=""tolltech"",tty=""tty1"",from=""-"",login=""14:51"",idle=""2:51m"",jcpu=""0.03s"",pcpu=""0.01s"",what=""-bash""}");
    }

    [Test]
    public void BigTest()
    {
        var input = @" 18:31:09 up  3:40,  4 users,  load average: 1,58, 1,62, 1,58
USER     TTY      FROM             LOGIN@   IDLE   JCPU   PCPU  WHAT
tolltech tty1     -                14:51    3:40m  0.03s  0.01s -bash
root              192.168.2.3      16:53    3:08m  0.00s  0.56s sshd: root@notty
root              192.168.2.3      17:33    3:08m  0.00s  0.30s sshd: root@pts/1";
        
        var clients = wParser.Parse(input);
        var actuals = new List<string>();
        foreach (var client in clients)
        {
            actuals.Add(wParser.Parse(client));
        }
        
        actuals.Count.Should().Be(3);
    }
}