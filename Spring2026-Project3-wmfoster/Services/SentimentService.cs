using VaderSharp2;

namespace Spring2026_Project3_wmfoster.Services
{
    public class SentimentService
    {
        private readonly SentimentIntensityAnalyzer _analyzer = new();

        public string GetSentimentLabel(string text)
        {
            var score = _analyzer.PolarityScores(text).Compound;

            if (score >= 0.05) return "Positive";
            if (score <= -0.05) return "Negative";
            return "Neutral";
        }

        public string GetAverageSentiment(IEnumerable<string> texts)
        {
            var list = texts.ToList();
            if (!list.Any()) return "Neutral";

            double avg = list.Average(t => _analyzer.PolarityScores(t).Compound);

            if (avg >= 0.05) return "Positive";
            if (avg <= -0.05) return "Negative";
            return "Neutral";
        }
    }
}