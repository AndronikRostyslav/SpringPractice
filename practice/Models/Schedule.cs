using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;

namespace practice.Models
{
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int schedule_id { get; set; }
        public int hall_id { get; set; }
        public int showing_id { get; set; }
        public int movie_id { get; set; }
        public DateOnly show_date { get; set; }
        public decimal price { get; set; }

        public Schedule(int hall_id, int showing_id, int movie_id, DateOnly show_date, decimal price)
        {
            this.hall_id = hall_id;
            this.showing_id = showing_id;
            this.movie_id = movie_id;
            this.show_date = show_date;
            this.price = price;
        }
    }

    public class ScheduleInfo
    {
        public string hall_name { get; set; }
        public int seats_number { get; set; }
        public string title { get; set; }
        public TimeOnly show_time { get; set; }
        public DateOnly show_date { get; set; }
        public decimal price { get; set; }

        public ScheduleInfo(Hall hall, Showing showing, Movie movie, DateOnly show_date, decimal price)
        {
            this.hall_name = hall.hall_name;
            this.seats_number = hall.seats_number;
            this.title = movie.title;
            this.show_time = showing.show_time;
            this.show_date = show_date;
            this.price = price;
        }
    }
}
