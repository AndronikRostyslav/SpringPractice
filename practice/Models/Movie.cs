using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice.Models
{
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movie_id {  get; set; }
        public string title { get; set; }
        public decimal budget { get; set; }
        public string description { get; set; }
        public DateTime release_date { get; set; }
        public decimal box_office { get; set; }
        public int duration { get; set; }
        public string tagline { get; set; }
        public double average_rating { get; set; }

        public Movie(string title, decimal budget, string description, DateTime release_date, decimal box_office, int duration, string tagline, double average_rating)
        {
            this.title = title;
            this.budget = budget;
            this.description = description;
            this.release_date = release_date;
            this.box_office = box_office;
            this.duration = duration;
            this.tagline = tagline;
            this.average_rating = average_rating;
        }
    }
}
