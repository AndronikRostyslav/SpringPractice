using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using practice.Models;
using System.Text.RegularExpressions;

namespace practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly Context _context;

        public ClientsController(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of clients from the database and returns them as ClientDisplay objects.
        /// </summary>
        /// <returns>A list of clients as ClientDisplay objects.</returns>
        /// 
        [HttpGet("GetClients")]
        public async Task<ActionResult<IEnumerable<ClientDisplay>>> GetClients()
        {
            // Retrieve all clients from the database
            var clients = await _context.Clients.ToListAsync();

            // Create an array of ClientDisplay objects
            ClientDisplay[] clientDisplays = new ClientDisplay[clients.Count];

            // Populate the array with ClientDisplay objects based on the retrieved clients
            for (int i = 0; i < clientDisplays.Length; i++)
            {
                clientDisplays[i] = new ClientDisplay(clients[i]);
            }

            // Return the list of clients as ClientDisplay objects
            return clientDisplays;
        }

        /// <summary>
        /// Registers a new client with the provided information.
        /// </summary>
        /// <param name="request">The request containing the client's registration data.</param>
        /// <returns>
        /// If successful, returns the newly created client as a ClientDisplay object with a status code of 201 (Created).
        /// If the request is invalid, returns a BadRequest response with an error message.
        /// If a client with the same login already exists, returns a Conflict response with an error message.
        /// If the password and confirmation password do not match, returns a Conflict response with an error message.
        /// If any required field is missing, returns a Conflict response with an error message.
        /// If the name or surname contains characters other than letters, returns a Conflict response with an error message.
        /// If the login or password contains spaces, returns a Conflict response with an error message.
        /// </returns>
        [HttpPost("ClientRegistration")]
        public async Task<ActionResult<ClientDisplay>> ClientRegistration([FromBody] RegisterClientRequest request)
        {
            // Check if the request is null
            if (request == null)
            {
                return BadRequest("Invalid client data");
            }

            // Check if a client with the same login already exists
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.login == request.Login);
            if (existingClient != null)
            {
                return Conflict("A client with this login already exists.");
            }

            // Check if the password and confirmation password match
            if (request.Password != request.ConfirmPassword)
            {
                return Conflict("The password and confirmation password do not match.");
            }

            // Check if all required fields are filled
            if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.Surname) && string.IsNullOrEmpty(request.Login) && string.IsNullOrEmpty(request.Password))
            {
                return Conflict("All fields must be filled.");
            }

            // Check if the name and surname contain only letters
            if (!IsValidNameOrSurname(request.Name) && !IsValidNameOrSurname(request.Surname))
            {
                return Conflict("The name and surname must contain only letters.");
            }

            // Check if the login or password contain spaces
            if (request.Login.Contains(" ") || request.Password.Contains(" "))
            {
                return Conflict("The values of fields must not contain spaces.");
            }

            // Hash the password
            var hashedPassword = PasswordHasher.HashPassword(request.Password);

            // Create a new client object with the provided information
            Client client = new Client(request.Name, request.Surname, request.Login, hashedPassword);

            // Add the client to the database
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Return the newly created client as a ClientDisplay object
            return CreatedAtAction(nameof(GetClientById), new { id = client.client_id }, client);
        }

        /// <summary>
        /// Retrieves a client by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the client.</param>
        /// <returns>
        /// If a client with the specified ID exists, returns the client information as a ClientDisplay object with a status code of 200 (OK).
        /// If no client with the specified ID is found, returns a NotFound response with a status code of 404.
        /// </returns>
        private async Task<ActionResult<ClientDisplay>> GetClientById(int id)
        {
            // Attempt to find the client with the specified ID
            var client = await _context.Clients.FindAsync(id);

            // Check if the client exists
            if (client == null)
            {
                // If no client is found, return a NotFound response
                return NotFound();
            }

            // If the client is found, return the client information as a ClientDisplay object
            return new ClientDisplay(client);
        }


        /// <summary>
        /// Authenticates a client based on the provided login credentials.
        /// </summary>
        /// <param name="request">The login request containing the client's login and password.</param>
        /// <returns>
        /// If the login is successful, returns the authenticated client's information as a ClientDisplay object with a status code of 200 (OK).
        /// If the request data is invalid, returns a BadRequest response with a status code of 400.
        /// If the login or password is incorrect, returns an Unauthorized response with a status code of 401.
        /// </returns>
        [HttpPost("ClientLogin")]
        public async Task<ActionResult<ClientDisplay>> ClientLogin([FromBody] LoginClientRequest request)
        {
            if (request == null)
            {
                // Return BadRequest if the request data is invalid
                return BadRequest("Invalid client data");
            }

            // Check if the login exists
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.login == request.Login);

            // Verify client and password
            if (existingClient == null || !PasswordHasher.VerifyPassword(request.Password, existingClient.password))
            {
                // Return Unauthorized if the login or password is incorrect
                return Unauthorized();
            }

            // Set session variables for the authenticated user
            HttpContext.Session.SetInt32("UserID", existingClient.client_id);
            HttpContext.Session.SetString("AccessRights", existingClient.access_rights.ToString());

            // Return the authenticated client's information
            return new ClientDisplay(existingClient);
        }


        /// <summary>
        /// Retrieves the currently logged-in client's information.
        /// </summary>
        /// <returns>
        /// If the user is logged in, returns the current user's information as a ClientDisplay object with a status code of 200 (OK).
        /// If the user is not logged in, returns a Conflict response with a status code of 409.
        /// If the current user's information is not found in the database, returns a NotFound response with a status code of 404.
        /// </returns>
        [HttpGet("CurrentUser")]
        public async Task<ActionResult<ClientDisplay>> GetCurrentUser()
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserID");

            // Check if the user is logged in
            if (userId == null)
            {
                // Return Conflict if the user is not logged in
                return Conflict("User is not logged in.");
            }

            // Retrieve the current user's information from the database
            var currentUser = await _context.Clients.FirstOrDefaultAsync(c => c.client_id == userId);

            // Check if the current user is found in the database
            if (currentUser == null)
            {
                // Return NotFound if the current user's information is not found
                return NotFound();
            }

            // Return the current user's information
            return new ClientDisplay(currentUser);
        }


        /// <summary>
        /// Logs out the currently logged-in client by removing their session data.
        /// </summary>
        /// <remarks>
        /// This method clears the "UserID" and "AccessRights" entries from the current session.
        /// </remarks>
        [HttpPost("LogOut")]
        public void LogOut()
        {
            // Remove the user ID from the session
            HttpContext.Session.Remove("UserID");

            // Remove the access rights from the session
            HttpContext.Session.Remove("AccessRights");
        }


        /// <summary>
        /// Validates that a given string contains only letters.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <returns>True if the string contains only letters, otherwise false.</returns>
        /// <remarks>
        /// This method uses a regular expression to check if the input string consists solely of alphabetic characters (both uppercase and lowercase). It returns true if the string matches the pattern, indicating that it contains only letters; otherwise, it returns false.
        /// </remarks>
        private bool IsValidNameOrSurname(string value)
        {
            return Regex.IsMatch(value, @"^[a-zA-Z]+$");
        }

    }
}
