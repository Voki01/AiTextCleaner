using System.Text.RegularExpressions;

namespace AiTextCleaner.Services
{
    public static class TextCleaner
    {
        public static (string cleaned, List<string> removedItems) Clean(string input)
        {
            List<string> removed = new();
            string result = input;

            // Удаление Zero-width символов
            string pattern = @"[\u200B-\u200D\uFEFF]";
            if (Regex.IsMatch(result, pattern))
            {
                removed.Add("Zero-width symbols (\\u200B–\\u200D, \\uFEFF)");
                result = Regex.Replace(result, pattern, "");
            }

            // Удаление HTML-тегов
            if (Regex.IsMatch(result, "<.*?>"))
            {
                removed.Add("HTML-теги");
                result = Regex.Replace(result, "<.*?>", "");
            }

            // Замена “умных кавычек” на обычные
            result = result.Replace("“", "\"").Replace("”", "\"")
                           .Replace("‘", "'").Replace("’", "'");

            // Удаление нестандартных пробелов
            string[] badSpaces = { "\u00A0", "\u2000", "\u2001", "\u2002", "\u2003" };
            foreach (var space in badSpaces)
            {
                if (result.Contains(space))
                {
                    removed.Add($"Non-standard space: U+{((int)space[0]).ToString("X4")}");
                    result = result.Replace(space, " ");
                }
            }

            // Удаление двойных и тройных пробелов
            result = Regex.Replace(result, @"\s{2,}", " ");
            removed.Add("Двойные и тройные пробелы");

            // Удаление лишних переводов строк
            result = Regex.Replace(result, @"[\r\n]{2,}", "\n");

            return (result.Trim(), removed);
        }
    }
}
