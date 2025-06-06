namespace AiTextCleaner.Models
{
    public class CleanResultViewModel
    {
        public string OriginalText { get; set; } = string.Empty;
        public string CleanedText { get; set; } = string.Empty;
        public List<string> RemovedItems { get; set; } = new();
        public string AiEvaluation { get; set; } = string.Empty;
        public List<string> AiNotes { get; set; } = new();

        public int OriginalWordCount { get; set; }
        public int CleanedWordCount { get; set; }
        public int OriginalCharCount { get; set; }
        public int CleanedCharCount { get; set; }

        public List<string> HighlightedPhrases { get; set; } = new();

        // 💡 Новое свойство для прогресс-бара
        public double GptProbability { get; set; }
    }
}
