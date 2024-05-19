using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice.Models
{
    public class Genre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int genre_id { get; set; }
        public string genre_name { get; set; }

        public Genre(string genre_name)
        {
            this.genre_name = genre_name;
        }
    }
}
