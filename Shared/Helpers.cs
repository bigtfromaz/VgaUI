using System.Globalization;
using System.Text.Json;

namespace VgaUI.Shared;
public class Helpers
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };
    public static string Serialize(Object obj)
    {
        return JsonSerializer.Serialize(obj, serializerOptions);
    }
    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json) ?? throw new Exception($"Helper.Deserialize returned a null value for: {json}");
    }
    public static string Plurality(int num, string str)
    {
        if (num > 1) return str + "s";
        else return str;
    }
    public static string MoneyDisplay(decimal amount)
    {
        if (amount <= 0) return "-";
        return Money(amount);
    }
    public static string MoneyDisplay(int amount)
    {
        if (amount <= 0) return "-";
        return Money(amount);
    }
    public static string Money(int amount) => amount.ToString("C", CultureInfo.CurrentCulture);
    public static string Money(decimal amount) => amount.ToString("C", CultureInfo.CurrentCulture);
    public static string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        return (num % 100) switch
        {
            11 or 12 or 13 => num + "th",
            _ => (num % 10) switch
            {
                1 => num + "st",
                2 => num + "nd",
                3 => num + "rd",
                _ => num + "th",
            },
        };
    }
    public static string ConcatenateWithPlus(List<string> words) => string.Join(" + ", words);
    public static string GetCombinedUniqueKey(DateOnly DateOfPlay, string CourseName)
    {
        return $"{DateOfPlay:yyyy-MM-dd}_{CourseName}";

    }
}
