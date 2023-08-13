using HMSBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;


namespace HMSBackend.Controllers
{
    //[Authorize]
    [Route("api/")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DoctorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("doctor/fetchAll")]
        public ActionResult<IEnumerable<Doctor>> GetDoctor()
        {
            List<Doctor> doctorList = new List<Doctor>();

            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = "SELECT doctor_id, name, mobile, email, specialist, qualification, address FROM doctor_table";
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Doctor doctor = new Doctor
                        {
                            doctor_id = reader["doctor_id"] as string,
                            name = reader["name"] as string,
                            mob_no = reader["mobile"] as string,
                            email = reader["email"] as string,
                            qualification = reader["qualification"] as string,
                            address = reader["address"] as string
                        };

                        string specialist = reader["specialist"] as string;
                        doctor.specialist = specialist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        doctorList.Add(doctor);
                    }
                    reader.Close();

                }
                return Ok(doctorList);
            }
            catch (Exception ex)
            {
                return BadRequest("Fieled to fetch doctors data" + ex.Message);
            }
        }

        //POST
        [HttpPost]
        [Route("doctor/create")]
        public ActionResult<string> PostDoctorData(Doctor doctorDetails)
        {
            try
            {

                string specialistJson = JsonConvert.SerializeObject(doctorDetails.specialist);

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO doctor_table(doctor_id, name, mobile, specialist, email, qualification, address) VALUES(@doctor_id, @name, @mob_no, @specialist, @email, @qualification, @address)", con);
                    cmd.Parameters.AddWithValue("@doctor_id", doctorDetails.doctor_id);
                    cmd.Parameters.AddWithValue("@name", doctorDetails.name);
                    cmd.Parameters.AddWithValue("@mob_no", doctorDetails.mob_no);
                    cmd.Parameters.AddWithValue("@specialist", specialistJson);
                    cmd.Parameters.AddWithValue("@email", doctorDetails.email);
                    cmd.Parameters.AddWithValue("@qualification", doctorDetails.qualification);
                    cmd.Parameters.AddWithValue("@address", doctorDetails.address);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if(i > 0)
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
                return StatusCode(500, "An error occurred" + ex.Message);
            }
        }

        [HttpPut]
        [Route("doctor/update/{id}")]
        public ActionResult<string> PutDoctorData(string id, Doctor doctorDetails)
        {
            try
            {
                string specialistJson = JsonConvert.SerializeObject(doctorDetails.specialist);

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE doctor_table SET name = @name, mobile = @mob_no, specialist = @specialist, email = @email, qualification = @qualification, address = @address WHERE doctor_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", doctorDetails.name);
                    cmd.Parameters.AddWithValue("@mob_no", doctorDetails.mob_no);
                    cmd.Parameters.AddWithValue("@specialist", specialistJson);
                    cmd.Parameters.AddWithValue("@email", doctorDetails.email);
                    cmd.Parameters.AddWithValue("@qualification", doctorDetails.qualification);
                    cmd.Parameters.AddWithValue("@address", doctorDetails.address);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok("Data has been updated");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        [HttpDelete]
        [Route("doctor/delete")]
        public ActionResult<string> DeleteDoctorData(string id)
        {
            try
            {

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE doctor_table WHERE doctor_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok("Data has been Deleted");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }


    }
}
