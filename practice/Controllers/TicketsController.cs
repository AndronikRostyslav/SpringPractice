using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using practice.Models;

namespace practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly Context _context;

        public TicketsController(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all tickets from the database.
        /// </summary>
        /// <returns>An asynchronous operation that returns a collection of tickets.</returns>
        [HttpGet("GetAllTickets")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTickets()
        {
            return await _context.Tickets.ToListAsync();
        }

        /// <summary>
        /// Retrieves all tickets booked by the current user.
        /// </summary>
        /// <returns>Returns a list of tickets booked by the current user if successful, or a conflict response if the user is not logged in.</returns>
        [HttpGet("GetCurrentUserTickets")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetCurrentUserTickets()
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserID");

            // Check if the user is logged in
            if (userId == null)
            {
                // Return Conflict if the user is not logged in
                return Conflict("User is not logged in.");
            }

            // Return the tickets booked by the current user
            return await _context.Tickets
                .Where(t => t.client_id == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the details of a ticket by its ID.
        /// </summary>
        /// <param name="id">The ID of the ticket to retrieve details for.</param>
        /// <returns>
        /// Returns the details of the ticket, including show date, show time, hall name, seat number, movie title, movie duration, ticket price, and movie description.
        /// Returns Conflict if the user is not logged in, the ticket does not exist, or the user does not own the ticket.
        /// </returns>
        [HttpGet("GetTicketDetails")]
        public async Task<ActionResult<TicketDetails>> GetTicketDetails(int id)
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserID");

            // Check if the user is logged in
            if (userId == null)
            {
                // Return Conflict if the user is not logged in
                return Conflict("User is not logged in.");
            }

            // Find the ticket by its ID
            var ticket = await _context.Tickets.FindAsync(id);

            // Check if the ticket exists
            if (ticket == null)
            {
                // Return NotFound if the ticket does not exist
                return NotFound();
            }

            // Check if the current user owns the ticket
            if (ticket.client_id != userId)
            {
                // Return Conflict if the current user does not own the ticket
                return Conflict("You do not have permission to access details for this ticket.");
            }

            // Find the schedule for the specified ID
            var scheduleElement = await _context.Schedule.FindAsync(ticket.schedule_id);

            // Check if the schedule exists
            if (scheduleElement == null)
            {
                return Conflict("Schedule with the specified ID does not exist.");
            }

            // Find the hall for the schedule
            var hallElement = await _context.Hall.FindAsync(scheduleElement.hall_id);

            // Check if the hall exists
            if (hallElement == null)
            {
                return Conflict("Hall with the specified ID does not exist.");
            }

            // Find the movie for the schedule
            var movieElement = await _context.Movie.FindAsync(scheduleElement.movie_id);

            // Check if the movie exists
            if (movieElement == null)
            {
                return Conflict("Movie with the specified ID does not exist.");
            }

            // Find the showing time for the schedule
            var showingElement = await _context.Showings.FindAsync(scheduleElement.showing_id);

            // Check if the showing time exists
            if (showingElement == null)
            {
                return Conflict("Showing time with the specified ID does not exist.");
            }

            // Return the ticket details
            return new TicketDetails(scheduleElement.show_date, showingElement.show_time, hallElement.hall_name, ticket.seat, movieElement.title, movieElement.duration, scheduleElement.price, movieElement.description);
        }


        /// <summary>
        /// Retrieves a ticket from the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the ticket to retrieve.</param>
        /// <returns>
        /// An asynchronous operation that returns the ticket with the specified ID, 
        /// or a NotFound response if no ticket with the specified ID is found.
        /// </returns>
        private async Task<ActionResult<Ticket>> GetTicketById(int id)
        {
            // Attempt to find the ticket with the specified ID
            var ticket = await _context.Tickets.FindAsync(id);

            // Check if the ticket exists
            if (ticket == null)
            {
                // If no ticket is found, return a NotFound response
                return NotFound();
            }

            // If the ticket is found, return the ticket information
            return ticket;
        }


        /// <summary>
        /// Books a ticket for the specified schedule and seat.
        /// </summary>
        /// <param name="scheduleId">The ID of the schedule for which the ticket is being booked.</param>
        /// <param name="seat">The seat number being booked.</param>
        /// <returns>Returns the booked ticket if successful, or a conflict response if there is an issue.</returns>
        [HttpPost("BookingTicket")]
        public async Task<ActionResult<Ticket>> BookingTicket(int scheduleId, int seat)
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserID");

            // Check if the user is logged in
            if (userId == null)
            {
                // Return Conflict if the user is not logged in
                return Conflict("User is not logged in.");
            }

            // Find the schedule for the specified ID
            var scheduleElement = await _context.Schedule.FirstOrDefaultAsync(c => c.schedule_id == scheduleId);

            // Check if the schedule exists
            if (scheduleElement == null)
            {
                return Conflict("Schedule with the specified ID does not exist.");
            }

            // Get the current date
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);

            // Check if the show date has already passed
            if (scheduleElement.show_date <= currentDate)
            {
                return Conflict("The show date has already passed.");
            }

            // Find the hall for the schedule
            var hallElement = await _context.Hall.FirstOrDefaultAsync(c => c.hall_id == scheduleElement.hall_id);

            // Check if the hall exists
            if (hallElement == null)
            {
                return Conflict("Hall with the specified ID does not exist.");
            }

            // Check if the seat number is valid
            if (seat <= 0 || seat > hallElement.seats_number)
            {
                return Conflict("Invalid seat number.");
            }

            // Get all tickets for the specified schedule
            var tickets = await _context.Tickets
                .Where(t => t.schedule_id == scheduleId)
                .ToListAsync();

            // Check if the seat is already booked
            if (tickets != null && tickets.Any())
            {
                for (int i = 0; i < tickets.Count; i++)
                {
                    if (tickets[i].seat == seat)
                    {
                        return Conflict("The seat is already booked.");
                    }
                }
            }

            // Create a new ticket
            var ticket = new Ticket(scheduleId, (int)userId, seat);

            // Add the ticket to the context and save changes
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Return the created ticket
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.ticket_id }, ticket);
        }

        /// <summary>
        /// Deletes a ticket by its ID.
        /// </summary>
        /// <param name="id">The ID of the ticket to delete.</param>
        /// <returns>Returns NoContent if the ticket is successfully deleted, NotFound if the ticket does not exist, or a conflict response if the user is not logged in.</returns>
        [HttpDelete("DeleteTicket/{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserID");

            // Check if the user is logged in
            if (userId == null)
            {
                // Return Conflict if the user is not logged in
                return Conflict("User is not logged in.");
            }

            // Find the ticket by its ID
            var ticket = await _context.Tickets.FindAsync(id);

            // Check if the ticket exists
            if (ticket == null)
            {
                // Return NotFound if the ticket does not exist
                return NotFound();
            }

            // Check if the current user owns the ticket
            if (ticket.client_id != userId)
            {
                // Return Conflict if the current user does not own the ticket
                return Conflict("You do not have permission to delete this ticket.");
            }

            // Remove the ticket from the database
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            // Return NoContent to indicate successful deletion
            return NoContent();
        }

    }
}
