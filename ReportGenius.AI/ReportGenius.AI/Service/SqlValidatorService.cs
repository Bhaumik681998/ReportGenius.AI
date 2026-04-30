using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class SqlValidatorService
    {
        public bool Validate(string sql, string schema)
        {
            var columns = Extract(schema);

            var words = Regex.Matches(sql, @"\b[a-zA-Z_]+\b")
                .Select(x => x.Value.ToLower());

            return !words.Any(w => w.EndsWith("id") && !columns.Contains(w));
        }

        private List<string> Extract(string schema)
        {
            var list = new List<string>();

            var matches = Regex.Matches(schema, @"\((.*?)\)");

            foreach (Match m in matches)
            {
                foreach (var col in m.Groups[1].Value.Split(','))
                    list.Add(col.Trim().ToLower());
            }

            return list;
        }
    }
}
