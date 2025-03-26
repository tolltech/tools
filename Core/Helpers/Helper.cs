namespace Tolltech.Core.Helpers;

public static class Helper
{
    public static TValue? SafeGet<TKey, TValue>(this IDictionary<TKey, TValue> src, TKey key)
    {
        return src.TryGetValue(key, out var result) ? result : default;
    }
    
    public static int ToInt(this string? src, int defaultValue = 0)
    {
        if (src == null) return defaultValue;
        
        return int.Parse(src);
    }
}