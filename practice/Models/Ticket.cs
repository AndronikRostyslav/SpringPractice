using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ticket_id {  get; set; }
        public int schedule_id { get; set; }
        public int client_id { get; set; }
        public int seat {  get; set; }

        public Ticket (int schedule_id, int client_id, int seat)
        {
            this.schedule_id = schedule_id;
            this.client_id = client_id;
            this.seat = seat;
        }
    }

    public class TicketDetails
    {
        public DateOnly show_date {  get; set; }
        public TimeOnly show_time { get; set; }
        public string hall_name { get; set; }
        public int seat { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public decimal price { get; set; }
        public string description { get; set; }

        public TicketDetails(DateOnly show_date, TimeOnly show_time, string hall_name, int seat, string title, int duration, decimal price, string description)
        {
            this.show_date = show_date;
            this.show_time = show_time;
            this.hall_name = hall_name;
            this.seat = seat;
            this.title = title;
            this.duration = duration;
            this.price = price;
            this.description = description;
        }

    }
}
