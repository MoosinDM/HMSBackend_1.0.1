using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HMSBackend.Models;

namespace HMSBackend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration, ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(Login login)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities"));
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM login WHERE username = @username", con);
            da.SelectCommand.Parameters.AddWithValue("@username", login.username);

            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                string hashedPasswordFromDatabase = dt.Rows[0]["password"].ToString();
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(login.password, hashedPasswordFromDatabase);

                if (isPasswordValid)
                {
                    _logger.LogInformation("Login successful for user: {0}", login.username);

                    // Get user details from DataTable
                    int userId = (int)dt.Rows[0]["login_id"];
                    string userName = dt.Rows[0]["name"].ToString();
                    string userEmail = dt.Rows[0]["email"].ToString();
                    bool isDefault = Convert.ToBoolean(dt.Rows[0]["is_default"]);
                    string mob_no = (string)dt.Rows[0]["mob_no"];

                    // Generate JWT token
                    var token = GenerateJwtToken(userId, userName, userEmail, isDefault, mob_no);

                    // Return user details and token as response
                    return Ok(new
                    {
                        Token = token
                    });
                }
            }

            _logger.LogWarning("Login failed for user: {0}", login.username);
            return BadRequest("Invalid user");
        }

        // ... rest of the code ...

        // Method to generate JWT token
        //private string GenerateJwtToken(int userId, string userName)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //            new Claim(ClaimTypes.Name, userName),
        //            new Claim("UserId", userId.ToString()) // Add UserId claim
        //        }),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}

        //private string GenerateJwtToken(int userId, string userName, string email, bool isDefault)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, userName),
        //        new Claim("UserId", userId.ToString()),
        //        new Claim(ClaimTypes.Email, email),
        //        new Claim(ClaimTypes.isDefault, isDefault)
        //    };

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}

        private string GenerateJwtToken(int userId, string userName, string email, bool isDefault, string mob_no)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

            var claims = new List<Claim>
            {
                new Claim("Name", userName),
                new Claim("UserId", userId.ToString()),
                new Claim("Email", email),
                new Claim("isDefault", isDefault.ToString()), // Convert bool to string
                new Claim("MobilePhone", mob_no)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        //Password Change

        [HttpPatch]
        [Route("password/change/{id}")]
        public async Task<ActionResult> ResetPassword(int id, ResetPassword reset)
        {
            try
            {
                // Hash the new password
                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(reset.password);

                // Update the password in the database
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE login SET password = @password, is_default = 'false' WHERE login_id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@password", newPasswordHash);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Password reset successful
                            return Ok("Password reset successful");
                        }
                        else
                        {
                            // Password update failed
                            return BadRequest("Password update failed. No rows were affected.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and return an error response
                _logger.LogError(ex, "An error occurred while resetting the password.");
                return StatusCode(500, $"An error occurred while resetting the password. Error: {ex.Message}");
            }
        }
    }
}

