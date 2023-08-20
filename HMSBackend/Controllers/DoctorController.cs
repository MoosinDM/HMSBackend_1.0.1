using HMSBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        ////Server side paging and sorting
        [HttpPost]
        [Route("doctor/fetchAll")]
        public ActionResult<IEnumerable<Doctor>> PostDoctor(FilterCriteria filterCriteria)
        {
            try
            {
                int page = Convert.ToInt32(filterCriteria.page);
                int pageSize = Convert.ToInt32(filterCriteria.pageSize);
                string sortBy = filterCriteria.sortBy;
                string order = filterCriteria.order;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = $@"
                    SELECT doctor_id, name, mobile, email, specialist, qualification, address
                    FROM (
                        SELECT *,
                            ROW_NUMBER() OVER(ORDER BY {sortBy} {order}) AS RowNumber
                        FROM doctor_table
                    ) AS Subquery
                    WHERE RowNumber BETWEEN @StartRow AND @EndRow AND " +
                    "doctor_id LIKE '%' + @doctor_id + '%' AND " +
                                   "      name LIKE '%' + @name + '%' AND " +
                                   "specialist LIKE '%' + @specialist + '%'";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@doctor_id", filterCriteria.doctor_id);
                    cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");
                    //cmd.Parameters.AddWithValue("@specialist", filterCriteria.specialist); // Assuming specialist is a string, adjust as needed
                    cmd.Parameters.AddWithValue("@specialist", '%' + filterCriteria.specialist + "%");

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Doctor> doctorList = new List<Doctor>();
                    while (reader.Read())
                    {
                        Doctor doctor = new Doctor
                        {
                            doctor_id = reader["doctor_id"] as string,
                            name = reader["name"] as string,
                            mob_no = reader["mobile"] as string,
                            email = reader["email"] as string,
                            qualification = reader["qualification"] as string,
                            address = reader["address"] as string,
                            // Fetch the specialist data and parse it as a string array
                            specialist = JArray.Parse(reader["specialist"].ToString()).ToObject<string[]>()
                        };

                        doctorList.Add(doctor);
                    }
                    reader.Close();

                    return Ok(doctorList);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
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
                        return Ok(new {meesage = "Doctor has been Inserted"});
                    }
                    else
                    {
                        return BadRequest(new { message = "Error" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }

        }

        [HttpPut]
        [Route("doctor/update")]
        public ActionResult<string> PutDoctorData(Doctor doctorDetails)
        {
            try
            {
                string id = doctorDetails.doctor_id;
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
                        return Ok(new { message = "Doctor has been updated" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Error" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
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
                        return Ok(new { message = "Doctor Deleted Successfuly" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Error" });
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
