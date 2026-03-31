using System.ComponentModel.DataAnnotations;

namespace Spring2026_Project3_wmfoster.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string ImdbLink { get; set; } = "";

        [Required]
        public string Genre { get; set; } = "";

        [Range(1800, 3000)]
        public int YearOfRelease { get; set; }

        public byte[]? Poster { get; set; }

        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}