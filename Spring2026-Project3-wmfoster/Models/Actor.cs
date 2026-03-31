using System.ComponentModel.DataAnnotations;

namespace Spring2026_Project3_wmfoster.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Gender { get; set; } = "";

        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        public string ImdbLink { get; set; } = "";

        public byte[]? Photo { get; set; }

        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}