using Spring2026_Project3_wmfoster.Models;

namespace Spring2026_Project3_wmfoster.ViewModels
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; } = new Actor();
        public List<string> Movies { get; set; } = new();
        public List<ActorTweetItemViewModel> Tweets { get; set; } = new();
        public string OverallSentiment { get; set; } = "";
    }
}