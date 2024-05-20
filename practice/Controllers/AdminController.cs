using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using practice.Models;

namespace practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Context _context;

        public AdminController(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new movie to the database.
        /// </summary>
        /// <param name="title">The title of the movie.</param>
        /// <param name="budget">The budget of the movie.</param>
        /// <param name="description">The description of the movie.</param>
        /// <param name="release_date">The release date of the movie.</param>
        /// <param name="box_office">The box office earnings of the movie.</param>
        /// <param name="duration">The duration of the movie in minutes.</param>
        /// <param name="tagline">The tagline of the movie.</param>
        /// <param name="average_rating">The average rating of the movie.</param>
        /// <returns>
        /// Returns the newly created movie. Returns Unauthorized if the user is not logged in, Conflict if the user does not have access rights, and BadRequest if the input data is invalid.
        /// </returns>
        [HttpPost("AddMovie")]
        public async Task<ActionResult<Movie>> AddMovie(string title, decimal budget, string description, DateTime release_date, decimal box_office, int duration, string tagline, double average_rating)
        {
            // Retrieve the access rights from the session
            var access_rights = HttpContext.Session.GetString("AccessRights");

            // Check if the user is logged in
            if (access_rights == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Check if the user has the necessary access rights
            if (access_rights == "False")
            {
                return Conflict("User does not have access rights.");
            }

            // Validate the input data
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Title cannot be empty.");
            }

            if (budget < 0)
            {
                return BadRequest("Budget cannot be negative.");
            }

            if (release_date > DateTime.Today)
            {
                return BadRequest("Release date cannot be in the future.");
            }

            if (box_office < 0)
            {
                return BadRequest("Box office cannot be negative.");
            }

            if (duration <= 0)
            {
                return BadRequest("Duration must be greater than zero.");
            }

            if (average_rating < 0 || average_rating > 10)
            {
                return BadRequest("Average rating must be between 0 and 10.");
            }

            // Create a new movie object
            Movie movie = new Movie(title, budget, description, release_date, box_office, duration, tagline, average_rating);

            // Add the new movie to the database context
            _context.Movie.Add(movie);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the newly created movie
            return CreatedAtAction(nameof(GetMovieById), new { id = movie.movie_id }, movie);
        }


        /// <summary>
        /// Retrieves a movie by its ID.
        /// </summary>
        /// <param name="id">The ID of the movie to retrieve.</param>
        /// <returns>
        /// Returns the movie with the specified ID. If no movie is found, returns NotFound.
        /// </returns>
        [HttpGet("GetMovieById")]
        public async Task<ActionResult<Movie>> GetMovieById(int id)
        {
            // Attempt to find the movie with the specified ID
            var movie = await _context.Movie.FindAsync(id);

            // Check if the movie exists
            if (movie == null)
            {
                // If no movie is found, return a NotFound response
                return NotFound();
            }

            // If the movie is found, return the movie information as a Movie object
            return movie;
        }


        /// <summary>
        /// Deletes a movie by its ID, along with its associated genres and schedules.
        /// </summary>
        /// <param name="id">The ID of the movie to delete.</param>
        /// <returns>
        /// Returns NoContent if the movie and its associations are successfully deleted.
        /// If the user is not logged in, returns Unauthorized.
        /// If the user does not have access rights, returns Conflict.
        /// If the movie is not found, returns NotFound.
        /// </returns>
        [HttpDelete("DeleteMovie/{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            // Retrieve the user's access rights from the session
            var access_rights = HttpContext.Session.GetString("AccessRights");

            // Check if the user is logged in
            if (access_rights == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Check if the user has access rights
            if (access_rights == "False")
            {
                return Conflict("User does not have access rights.");
            }

            // Attempt to find the movie with the specified ID
            var movie = await _context.Movie.FindAsync(id);

            // Check if the movie exists
            if (movie == null)
            {
                return NotFound();
            }

            // Retrieve associated movie genres
            var movie_genres = await _context.Movie_Genres
                .Where(t => t.movie_id == id)
                .ToListAsync();

            // Delete associated movie genres if any exist
            if (movie_genres != null && movie_genres.Count > 0)
            {
                _context.Movie_Genres.RemoveRange(movie_genres);
                await _context.SaveChangesAsync();
            }

            // Retrieve associated schedules
            var schedule = await _context.Schedule
                .Where(t => t.movie_id == id)
                .ToListAsync();

            // Delete associated schedules if any exist
            if (schedule != null && schedule.Count > 0)
            {
                _context.Schedule.RemoveRange(schedule);
                await _context.SaveChangesAsync();
            }

            // Remove the movie
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();

            // Return NoContent to indicate successful deletion
            return NoContent();
        }


        /// <summary>
        /// Adds a genre to a movie by their IDs.
        /// </summary>
        /// <param name="movie_id">The ID of the movie to which the genre will be added.</param>
        /// <param name="genre_id">The ID of the genre to be added to the movie.</param>
        /// <returns>
        /// Returns the movie and genre information if the genre is successfully added to the movie.
        /// If the user is not logged in, returns Unauthorized.
        /// If the user does not have access rights, returns Conflict.
        /// If the movie or genre does not exist, returns Conflict.
        /// </returns>
        [HttpPost("AddMovieGenres")]
        public async Task<ActionResult<Movie_Genre_Info>> AddMovieGenres(int movie_id, int genre_id)
        {
            // Retrieve the user's access rights from the session
            var access_rights = HttpContext.Session.GetString("AccessRights");

            // Check if the user is logged in
            if (access_rights == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Check if the user has access rights
            if (access_rights == "False")
            {
                return Conflict("User does not have access rights.");
            }

            // Attempt to find the movie with the specified ID
            var movie = await _context.Movie.FindAsync(movie_id);

            // Check if the movie exists
            if (movie == null)
            {
                return Conflict("Movie with the specified ID does not exist.");
            }

            // Attempt to find the genre with the specified ID
            var genre = await _context.Genres.FindAsync(genre_id);

            // Check if the genre exists
            if (genre == null)
            {
                return Conflict("Genre with the specified ID does not exist.");
            }

            // Check if the movie already has the specified genre
            var movie_genre = await _context.Movie_Genres
                .Where(t => t.movie_id == movie_id && t.genre_id == genre_id)
                .FirstOrDefaultAsync();

            // If the genre is not already associated with the movie, add it
            if (movie_genre == null)
            {
                _context.Movie_Genres.Add(new Movie_Genre(movie_id, genre_id));
                await _context.SaveChangesAsync();
            }

            // Return the movie and genre information
            return new Movie_Genre_Info(movie_id, movie.title, genre.genre_name);
        }


        /// <summary>
        /// Adds a new schedule for a movie showing in a specified hall.
        /// </summary>
        /// <param name="hall_id">The ID of the hall where the movie will be shown.</param>
        /// <param name="showing_id">The ID of the showing time.</param>
        /// <param name="movie_id">The ID of the movie to be shown.</param>
        /// <param name="show_date">The date of the show in the format YYYY-MM-DD.</param>
        /// <param name="price">The price of the ticket for the show.</param>
        /// <returns>
        /// Returns the created schedule if successful.
        /// If the user is not logged in, returns Unauthorized.
        /// If the user does not have access rights, returns Conflict.
        /// If the date format is invalid, returns BadRequest.
        /// If the hall, showing, or movie does not exist, returns Conflict.
        /// If a schedule already exists for the specified hall, showing, and date, returns Conflict.
        /// If the show date is in the past, returns Conflict.
        /// </returns>
        [HttpPost("AddSchedule")]
        public async Task<ActionResult<Schedule>> AddSchedule(int hall_id, int showing_id, int movie_id, string show_date, decimal price)
        {
            // Retrieve the user's access rights from the session
            var access_rights = HttpContext.Session.GetString("AccessRights");

            // Check if the user is logged in
            if (access_rights == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Check if the user has access rights
            if (access_rights == "False")
            {
                return Conflict("User does not have access rights.");
            }

            // Try to parse the show date
            if (!DateOnly.TryParse(show_date, out DateOnly showDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            // Attempt to find the hall with the specified ID
            var hallElement = await _context.Hall.FindAsync(hall_id);
            if (hallElement == null)
            {
                return Conflict("Hall with the specified ID does not exist.");
            }

            // Attempt to find the showing with the specified ID
            var showingElement = await _context.Showings.FindAsync(showing_id);
            if (showingElement == null)
            {
                return Conflict("Showing with the specified ID does not exist.");
            }

            // Attempt to find the movie with the specified ID
            var movieElement = await _context.Movie.FindAsync(movie_id);
            if (movieElement == null)
            {
                return Conflict("Movie with the specified ID does not exist.");
            }

            // Check if the show date is today or in the future
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
            if (showDate < currentDate)
            {
                return Conflict("The show date must be today or a future date.");
            }

            // Check if a schedule already exists for the specified hall, showing, and date
            var schedule = await _context.Schedule
                .Where(t => t.hall_id == hall_id && t.showing_id == showing_id && t.show_date == showDate)
                .ToArrayAsync();

            if (schedule.Any())
            {
                return Conflict("Schedule already exists for the specified hall, showing, and date.");
            }

            // Create a new schedule
            var newSchedule = new Schedule(hall_id, showing_id, movie_id, showDate, price);
            _context.Schedule.Add(newSchedule);
            await _context.SaveChangesAsync();

            // Return the created schedule
            return CreatedAtAction(nameof(GetMovieById), new { id = newSchedule.schedule_id }, newSchedule);
        }

        /// <summary>
        /// Retrieves the schedule with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the schedule to retrieve.</param>
        /// <returns>
        /// Returns the schedule if found.
        /// If no schedule is found, returns NotFound.
        /// </returns>
        [HttpPost("GetScheduleByID")]
        public async Task<ActionResult<Schedule>> GetScheduleById(int id)
        {
            // Attempt to find the schedule with the specified ID
            var schedule = await _context.Schedule.FindAsync(id);

            // Check if the schedule exists
            if (schedule == null)
            {
                // If no schedule is found, return a NotFound response
                return NotFound();
            }

            // If the schedule is found, return the schedule object
            return schedule;
        }


        /// <summary>
        /// Deletes the schedule with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the schedule to delete.</param>
        /// <returns>
        /// Returns NoContent if the schedule is successfully deleted.
        /// If no schedule is found, returns NotFound.
        /// If the user does not have access rights, returns Conflict.
        /// </returns>
        [HttpDelete("DeleteSchedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            // Retrieve access rights of the user
            var accessRights = HttpContext.Session.GetString("AccessRights");

            // Check if the user is logged in
            if (accessRights == null)
            {
                // Return Unauthorized if the user is not logged in
                return Unauthorized("User is not logged in.");
            }

            // Check if the user has access rights
            if (accessRights == "False")
            {
                // Return Conflict if the user does not have access rights
                return Conflict("User does not have access rights.");
            }

            // Attempt to find the schedule with the specified ID
            var schedule = await _context.Schedule.FindAsync(id);

            // Check if the schedule exists
            if (schedule == null)
            {
                // If no schedule is found, return NotFound
                return NotFound();
            }

            // Retrieve all tickets associated with the schedule
            var tickets = await _context.Tickets
                .Where(t => t.schedule_id == id)
                .ToListAsync();

            // Check if tickets are found
            if (tickets != null)
            {
                // Remove each ticket from the database
                foreach (var ticket in tickets)
                {
                    _context.Tickets.Remove(ticket);
                    await _context.SaveChangesAsync();
                }
            }

            // Remove the schedule from the database
            _context.Schedule.Remove(schedule);
            await _context.SaveChangesAsync();

            // Return NoContent to indicate successful deletion
            return NoContent();
        }

    }
}
