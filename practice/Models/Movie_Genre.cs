namespace practice.Models
{
    public class Movie_Genre
    {
        public int movie_id { get; set; }
        public int genre_id { get; set; }

        public Movie_Genre(int movie_id, int genre_id)
        {
            this.movie_id = movie_id;
            this.genre_id = genre_id;
        }
    }
}
