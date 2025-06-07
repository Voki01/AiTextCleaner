using System.Text.RegularExpressions;

namespace AiTextCleaner.Services
{
    public static class TextCleanerPro
    {
        public static (string cleaned, List<string> removedItems) Clean(string input)
        {
            List<string> removed = new();
            string result = input;
            string original = input;

            // 1. Удаление RTF-заголовков и таблиц (fonttbl, colortbl, generator, pict)
            var rtfBlockPattern = @"{\\(rtf|fonttbl|colortbl|\*\\generator|pict)[^{}]*}";
            var rtfBlocks = Regex.Matches(result, rtfBlockPattern);
            if (rtfBlocks.Count > 0)
            {
                removed.Add("RTF-заголовки и бинарные блоки → " + string.Join(", ", rtfBlocks.Select(m => m.Value[..Math.Min(20, m.Value.Length)] + "...")));
                result = Regex.Replace(result, rtfBlockPattern, "");
            }

            // 2. Удаление управляющих RTF-команд (например, \fs22, \pard, \tab, \par)
            var controlWords = Regex.Matches(result, @"\\[a-zA-Z]+\d* ?");
            if (controlWords.Count > 0)
            {
                removed.Add("RTF-управляющие команды → " + string.Join(", ", controlWords.Select(m => m.Value.Trim()).Distinct()));
                result = Regex.Replace(result, @"\\[a-zA-Z]+\d* ?", "");
            }

            // 3. Удаление всех RTF-групп вида {...}
            if (Regex.IsMatch(result, @"{[^{}]*}"))
            {
                removed.Add("RTF-группы → {...}");
                result = Regex.Replace(result, @"{[^{}]*}", "");
            }

            // 4. HTML-сущности (например, &nbsp;)
            var htmlEntities = Regex.Matches(result, @"&[a-z]+;");
            if (htmlEntities.Count > 0)
            {
                removed.Add("HTML-сущности → " + string.Join(", ", htmlEntities.Select(m => m.Value).Distinct()));
                result = Regex.Replace(result, @"&[a-z]+;", " ");
            }

            // 5. HTML-теги
            var htmlTags = Regex.Matches(result, "<.*?>");
            if (htmlTags.Count > 0)
            {
                removed.Add("HTML-теги → " + string.Join(", ", htmlTags.Select(m => m.Value).Distinct()));
                result = Regex.Replace(result, "<.*?>", "");
            }

            // 6. Служебные слова от Word
            string[] junkWords = { "MERGEFORMAT", "PAGE" };
            foreach (var word in junkWords)
            {
                if (Regex.IsMatch(result, $"\\b{word}\\b", RegexOptions.IgnoreCase))
                {
                    removed.Add($"Служебное слово → {word}");
                    result = Regex.Replace(result, $"\\b{word}\\b", "", RegexOptions.IgnoreCase);
                }
            }

            // 7. Нестандартные пробелы
            string[] badSpaces = { "\u00A0", "\u2000", "\u2001", "\u2002", "\u2003", "\u202F" };
            foreach (var space in badSpaces)
            {
                if (result.Contains(space))
                {
                    string code = $"U+{(int)space[0]:X4}";
                    removed.Add($"Нестандартный пробел → {code}");
                    result = result.Replace(space, " ");
                }
            }

            // 8. Zero-width символы
            var zwPattern = "[\u200B-\u200F\uFEFF]";
            var zwMatch = Regex.Matches(result, zwPattern);
            if (zwMatch.Count > 0)
            {
                removed.Add("Zero-width символы → " + string.Join(", ", zwMatch.Select(m => $"U+{(int)m.Value[0]:X4}").Distinct()));
                result = Regex.Replace(result, zwPattern, "");
            }

            // 9. Множественные пробелы
            if (Regex.IsMatch(result, @"\s{2,}"))
            {
                removed.Add("Лишние пробелы → заменены на один");
                result = Regex.Replace(result, @"\s{2,}", " ");
            }

            // 10. Лишние переводы строк
            if (Regex.IsMatch(result, @"[\r\n]{2,}"))
            {
                removed.Add("Лишние переносы строк → объединены");
                result = Regex.Replace(result, @"[\r\n]{2,}", "\n");
            }

            // 11. Умные кавычки
            string beforeQuotes = result;
            result = result.Replace("“", "\"").Replace("”", "\"").Replace("‘", "'").Replace("’", "'");
            if (beforeQuotes != result)
                removed.Add("Умные кавычки → заменены на обычные");

            // 12. Управляющие символы ASCII (до 32), кроме \n \r
            string beforeControl = result;
            result = new string(result.Where(c => !char.IsControl(c) || c == '\n' || c == '\r').ToArray());
            if (beforeControl != result)
                removed.Add("Управляющие ASCII-символы → удалены");

            result = result.Trim();

            if (result == original)
                removed.Clear();

            return (result, removed.Distinct().ToList());
        }
    }
}
