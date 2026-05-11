using ReportGenius.AI.Model;
using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class PromptUnderstandingService
    {
        public PromptIntent Parse(string prompt)
        {
            var intent = new PromptIntent();

            if (Regex.IsMatch(prompt,
                @"top\s+\d+",
                RegexOptions.IgnoreCase))
            {
                var m = Regex.Match(prompt,
                    @"top\s+(\d+)");

                intent.Top =
                    int.Parse(m.Groups[1].Value);
            }

            if (prompt.Contains("Kishore",
                StringComparison.OrdinalIgnoreCase))
            {
                intent.Tables.Add("Kishore");
            }

            if (prompt.Contains("Area",
                StringComparison.OrdinalIgnoreCase))
            {
                intent.Tables.Add("Area");
            }

            return intent;
        }
    }
}
