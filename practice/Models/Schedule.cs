using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace practice.Models
{
    public class Schedule
    {
        [Key]
        public int schedule_id { get; set; }
        public int hall_id { get; set; }
        public int showing_id { get; set; }
        public int movie_id { get; set; }
        public DateOnly show_date { get; set; }
        public decimal price { get; set; }

    }
}
