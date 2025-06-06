using System.Text.RegularExpressions;

namespace AiTextCleaner.Services
{
    public static class AiHeuristicsDetector
    {
        public class AiAnalysisResult
        {
            public string Evaluation { get; set; } = "Не определено";
            public List<string> Notes { get; set; } = new();
        }

        public static string[] CommonPhrases { get; } = {
            "следует отметить",
            "таким образом",
            "на сегодняшний день",
            "необходимо подчеркнуть",
            "представляет собой",
            "в целом",
            "безусловно",
            "в условиях современности",
            "важным аспектом является",
            "не вызывает сомнений",
            "в рамках данного исследования",
            "как уже упоминалось ранее",
            "следовательно",
            "по мнению многих специалистов"
        };

        public static AiAnalysisResult Analyze(string text)
        {
            var result = new AiAnalysisResult();
            var notes = new List<string>();

            // Подсчёт длинных предложений
            var sentences = Regex.Split(text, @"(?<=[\.!?])\s+");
            int longSentences = sentences.Count(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 20);

            // Подсчёт подозрительных фраз
            int suspiciousPhraseCount = CommonPhrases.Count(p => text.ToLower().Contains(p));

            if (longSentences > sentences.Length * 0.3 || suspiciousPhraseCount >= 3)
            {
                result.Evaluation = "Высокая вероятность GPT-стиля";
            }
            else if (longSentences > 0 || suspiciousPhraseCount > 0)
            {
                result.Evaluation = "Возможен GPT-стиль";
            }
            else
            {
                result.Evaluation = "Похоже на человеческий текст";
            }

            if (longSentences > 0)
                notes.Add($"Длинных предложений (>20 слов): {longSentences}");

            if (suspiciousPhraseCount > 0)
                notes.Add($"Обнаружено GPT-фраз: {suspiciousPhraseCount}");

            result.Notes = notes;
            return result;
        }
    }
}
