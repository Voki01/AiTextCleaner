using System.Text.RegularExpressions;

namespace AiTextCleaner.Services
{
    public static class TextCleanerPro
    {
        public static (string cleaned, List<string> removedItems) Clean(string input)
        {
            List<string> removed = new();
            string result = input;

            // 1. Удаление RTF-тегов и блоков
            string[] rtfPatterns =
            {
                @"\\[a-z]+\d*",                 // \rtf1, \ansi, \viewkind4 и т.д.
                @"{\\\*?[^}]+}",               // {*...} служебные блоки
                @"\\'3f",                      // кодовые символы
                @"{\s*{.*?}}",                 // вложенные фигурные скобки {{ ... }}
                @"{[^{}]{0,25}}",              // короткие RTF-блоки вида { Calibri; }
            };
            foreach (var pat in rtfPatterns)
            {
                if (Regex.IsMatch(result, pat))
                {
                    removed.Add($"RTF/служебные обрывки: {pat}");
                    result = Regex.Replace(result, pat, "");
                }
            }

            // 2. Удаление XML-блоков Word
            result = Regex.Replace(result, @"<w:.*?>.*?</w:.*?>", m =>
            {
                removed.Add($"Word XML: {m.Value}");
                return "";
            }, RegexOptions.Singleline);

            // 3. Удаление одиночных служебных слов
            string[] garbageWords = { "PAGE", "MERGEFORMAT" };
            foreach (var word in garbageWords)
            {
                var pattern = $@"\b{word}\b";
                if (Regex.IsMatch(result, pattern, RegexOptions.IgnoreCase))
                {
                    removed.Add($"Служебное слово: {word}");
                    result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase);
                }
            }

            // 4. Удаление HTML-тегов и сущностей
            result = Regex.Replace(result, "<.*?>", "");
            result = Regex.Replace(result, @"&[a-z]+;", " ");
            removed.Add("HTML-теги и сущности");

            // 5. Удаление zero-width символов
            string patternZeroWidth = "[\u200B-\u200F\uFEFF]";
            if (Regex.IsMatch(result, patternZeroWidth))
            {
                removed.Add("Zero-width символы");
                result = Regex.Replace(result, patternZeroWidth, "");
            }

            // 6. Замена "умных" кавычек
            result = result.Replace("“", "\"").Replace("”", "\"")
                           .Replace("‘", "'").Replace("’", "'");

            // 7. Очистка нестандартных пробелов
            result = Regex.Replace(result, @"[ \t\u00A0\u2000-\u200B]+", " ");
            result = Regex.Replace(result, @"[\r\n]{2,}", "\n");
            result = Regex.Replace(result, @" {2,}", " ");
            removed.Add("Лишние пробелы и переносы");

            // 8. Удаление управляющих ASCII символов
            result = new string(result.Where(c => !char.IsControl(c) || c == '\n' || c == '\r').ToArray());
            removed.Add("Управляющие ASCII-символы");

            return (result.Trim(), removed);
        }
    }
}
