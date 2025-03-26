namespace Tolltech.WhoPrometheus;

public interface IWhoPrometheusListener
{
    Task StartListeningAsync(int port);
}