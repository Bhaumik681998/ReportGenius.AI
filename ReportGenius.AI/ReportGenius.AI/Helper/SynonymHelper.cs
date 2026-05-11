namespace ReportGenius.AI.Helper
{
    public static class SynonymHelper
    {
        private static readonly Dictionary<string, string[]> Synonyms = new()
    {
        { "yuvak", new[] { "kishore", "yuva", "youth" } },
        { "mandal", new[] { "group", "team" } },
        { "area", new[] { "zone", "region" } }
    };

        public static string BuildContext(string input)
        {
            var lower = input.ToLower();
            var context = new List<string>();

            foreach (var item in Synonyms)
            {
                foreach (var word in item.Value)
                {
                    if (lower.Contains(word))
                        context.Add($"{word} = {item.Key}");
                }
            }

            return string.Join(", ", context);
        }
    }
}
