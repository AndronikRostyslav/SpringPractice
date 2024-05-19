using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice.Models
{
    /// <summary>
    /// Represents a client in the system.
    /// </summary>
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int client_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string login { get; set; }

        public string password { get; set; }
        public bool access_rights { get; set; } = false;

        public Client(string first_name, string last_name, string login, string password)
        {
            this.first_name = first_name;
            this.last_name = last_name;
            this.login = login;
            this.password = password;
        }
    }

    /// <summary>
    /// Represents a request to register a new client.
    /// </summary>
    public class RegisterClientRequest
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Represents a request to log in a client.
    /// </summary>
    public class LoginClientRequest
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }

    /// <summary>
    /// Represents the display information of a client.
    /// </summary>
    public class ClientDisplay
    {
        public int client_id { get; set; }

        public string? first_name { get; set; }

        public string? last_name { get; set; }

        public string? login { get; set; }

        public ClientDisplay (Client client)
        {
            this.client_id = client.client_id;
            this.first_name = client.first_name;
            this.last_name = client.last_name;
            this.login = client.login;
        }
    }
}
