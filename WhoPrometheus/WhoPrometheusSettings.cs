using Tolltech.Core;
using Tolltech.Core.Helpers;

namespace Tolltech.WhoPrometheus;

public class WhoPrometheusSettings : AppSettingsItem
{
    public WhoPrometheusSettings(Dictionary<string, string>? settings)
    {
        KeyValues = settings?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>(); 
    }
    public int Port => KeyValues.SafeGet("Port").ToInt();
}