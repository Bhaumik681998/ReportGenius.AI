using System.Text.RegularExpressions;

namespace ReportGenius.AI.Service
{
    public class PromptNormalizerService
    {
        public string Normalize(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return prompt;

            prompt = prompt.ToLower();

            // entity mapping
            prompt = Regex.Replace(prompt, @"\byuvak|youth|yuva\b", "Kishore");

            // field mapping
            prompt = Regex.Replace(prompt, @"\bmandal|zone|kshetra\b", "Area");

            // capitalize words
            prompt = Regex.Replace(prompt, @"\b[a-z]+\b", m =>
            {
                var w = m.Value;
                return char.ToUpper(w[0]) + w.Substring(1);
            });

            return prompt;
        }
    }
}
