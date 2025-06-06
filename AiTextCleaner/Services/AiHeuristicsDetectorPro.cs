using System.Text.RegularExpressions;

namespace AiTextCleaner.Services
{
    public static class AiHeuristicsDetectorPro
    {
        // Расширенный список GPT-подобных фраз
        public static string[] GptPhrases { get; } =
        {
            "следует отметить", "таким образом", "на сегодняшний день",
            "необходимо подчеркнуть", "представляет собой", "в целом", "безусловно",
            "в условиях современности", "важным аспектом является", "не вызывает сомнений",
            "в рамках данного исследования", "как уже упоминалось ранее", "следовательно",
            "по мнению многих специалистов"
        };

        public class ProAiAnalysisResult
        {
            public double GptProbability { get; set; } // От 0 до 100
            public List<string> Reasoning { get; set; } = new();
        }

        public static ProAiAnalysisResult Analyze(string text)
        {
            var result = new ProAiAnalysisResult();
            var reasons = new List<string>();

            // Разбиение на предложения
            var sentences = Regex.Split(text, @"(?<=[\.!?])\s+").Where(s => s.Length > 0).ToList();
            int totalWords = 0;
            int totalWordLength = 0;

            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                totalWords += words.Length;
                totalWordLength += words.Sum(w => w.Length);
            }

            double avgSentenceLength = totalWords / (double)sentences.Count;
            double avgWordLength = totalWordLength / (double)totalWords;

            // Подсчёт GPT-фраз
            int phraseHits = GptPhrases.Count(p => text.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);

            // Оценка
            double score = 0;

            if (avgSentenceLength > 20)
            {
                score += 30;
                reasons.Add($"Средняя длина предложения {avgSentenceLength:F1} слов — похоже на GPT.");
            }

            if (avgWordLength > 5.5)
            {
                score += 20;
                reasons.Add($"Средняя длина слова {avgWordLength:F1} букв — формальный стиль.");
            }

            if (phraseHits >= 3)
            {
                score += 30;
                reasons.Add($"Обнаружено GPT-фраз: {phraseHits}.");
            }
            else if (phraseHits > 0)
            {
                score += 15;
                reasons.Add($"Обнаружено 1–2 GPT-фразы.");
            }

            result.GptProbability = Math.Min(score, 100);
            result.Reasoning = reasons;

            return result;
        }
    }
}
