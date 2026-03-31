using Spring2026_Project3_wmfoster.Models;

namespace Spring2026_Project3_wmfoster.ViewModels
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; } = new Movie();
        public List<string> Actors { get; set; } = new();
        public List<MovieReviewItemViewModel> Reviews { get; set; } = new();
        public string AverageSentiment { get; set; } = "";
    }
}