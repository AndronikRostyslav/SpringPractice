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

    public class Movie_Genre_Info
    {
        public int movie_id { get; set;}
        public string title { get; set;}
        public string genre_name { get; set;}

        public Movie_Genre_Info (int movie_id, string title, string genre_name)
        {
            this.movie_id = movie_id;
            this.title = title;
            this.genre_name = genre_name;
        }
    }
}


