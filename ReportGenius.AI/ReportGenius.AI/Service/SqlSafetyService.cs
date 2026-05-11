using System.Text.RegularExpressions;

public class SqlSafetyService
{
    public bool IsSafeQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        var cleaned = sql.ToLower().Trim();

        var match = Regex.Match(cleaned, @"\b(select|with)\b[\s\S]*", RegexOptions.IgnoreCase);

        if (!match.Success)
            return false;

        cleaned = match.Value;

        if (!(cleaned.StartsWith("select") || cleaned.StartsWith("with")))
            return false;

        string[] blocked =
        {
            "insert","update","delete","drop","alter","truncate","exec","xp_"
        };

        foreach (var word in blocked)
        {
            if (Regex.IsMatch(cleaned, $@"\b{word}\b", RegexOptions.IgnoreCase))
                return false;
        }

        return true;
    }
}