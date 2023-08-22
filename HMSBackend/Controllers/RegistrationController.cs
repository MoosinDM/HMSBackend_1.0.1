using HMSBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Data.SqlClient;

namespace HMSBackend.Controllers
{
    [Authorize]
    [Route("api/register")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration; // Add this line to declare the field

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("newuser")]
        public ActionResult<string> PostPatientData(Register registration)
        {
            try
            {
                // ...
                // Hash the password using BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registration.password);


                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO login (name, username, email, password, mob_no, role, is_default, created_by) VALUES (@name, @username, @email, @password, @mob_no, @role, @is_default, @created_by)", con);
                    cmd.Parameters.AddWithValue("@name", registration.name);
                    cmd.Parameters.AddWithValue("@username", registration.username); // Corrected to "Username"
                    cmd.Parameters.AddWithValue("@email", registration.email);
                    cmd.Parameters.AddWithValue("@password", hashedPassword); // Store the hashed password
                    cmd.Parameters.AddWithValue("@mob_no", registration.mob_no);
                    cmd.Parameters.AddWithValue("@role", registration.role);
                    cmd.Parameters.AddWithValue("@is_default", registration.is_default); // Make sure 'flag' is set correctly in the 'Register' object
                    cmd.Parameters.AddWithValue("@created_by", registration.created_by);


                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok("Data has been Inserted");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }

        }
    }
}
