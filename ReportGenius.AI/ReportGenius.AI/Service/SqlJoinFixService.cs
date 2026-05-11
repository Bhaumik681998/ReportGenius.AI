using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class SqlJoinFixService
    {
        public string Fix(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            // Fix wrong join
            sql = Regex.Replace(sql,
                @"(\w+)\.KishoreID\s*=\s*(\w+)\.KId",
                "$1.KishoreID = $2.KishoreID",
                RegexOptions.IgnoreCase);

            sql = Regex.Replace(sql,
                @"(\w+)\.KId\s*=\s*(\w+)\.KishoreID",
                "$1.KishoreID = $2.KishoreID",
                RegexOptions.IgnoreCase);

            return sql;
        }
    }
}
