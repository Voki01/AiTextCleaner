using Microsoft.AspNetCore.Mvc;
using AiTextCleaner.Models;
using AiTextCleaner.Services;
using AiTextCleaner.Services;
using System.Linq;

namespace AiTextCleaner.Controllers
{
    public class CleanerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CleanText(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return RedirectToAction("Index");

            // Очистка текста расширенной версией
            var (cleaned, removedSymbols) = TextCleanerPro.Clean(inputText);

            // Расширенный анализ GPT-стиля
            var aiProResult = AiHeuristicsDetectorPro.Analyze(cleaned);

            // Определение фраз GPT
            var gptPhrases = AiHeuristicsDetectorPro.GptPhrases
                .Where(p => cleaned.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // Модель для отображения
            var model = new CleanResultViewModel
            {
                OriginalText = inputText,
                CleanedText = cleaned,
                RemovedItems = removedSymbols,

                OriginalCharCount = inputText.Length,
                CleanedCharCount = cleaned.Length,
                OriginalWordCount = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                CleanedWordCount = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,

                AiEvaluation = $"GPT-стиль: {aiProResult.GptProbability:F0}%",
                AiNotes = aiProResult.Reasoning,
                HighlightedPhrases = gptPhrases,

                
                GptProbability = aiProResult.GptProbability
            };

            return View("Result", model);
        }
    }
}
