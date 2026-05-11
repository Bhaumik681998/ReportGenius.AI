namespace ReportGenius.AI.Service
{
    public class SemanticMapperService
    {
        private readonly Dictionary<string, string[]> _map = new()
        {
            { "Kishore", new[]
                {
                    "yuvak","yuva","youth","boy"
                }
            },

            { "Area", new[]
                {
                    "kshetra","zone","mandal"
                }
            },

            { "Status", new[]
                {
                    "active","inactive","enabled"
                }
            }
        };

        public string Normalize(string prompt)
        {
            prompt = prompt.ToLower();

            foreach (var item in _map)
            {
                foreach (var word in item.Value)
                {
                    prompt = prompt.Replace(
                        word.ToLower(),
                        item.Key);
                }
            }

            return prompt;
        }
    }
}
