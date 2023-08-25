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
        public ActionResult<DoctorSearchResult> PostDoctor(FilterCriteria filterCriteria)
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
                        "name LIKE '%' + @name + '%' AND " +
                        "specialist LIKE '%' + @specialist + '%'";
        
            string countQuery = @"
                SELECT COUNT(*) FROM doctor_table
                WHERE doctor_id LIKE '%' + @doctor_id + '%' AND " +
                "name LIKE '%' + @name + '%' AND " +
                "specialist LIKE '%' + @specialist + '%';";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@doctor_id", filterCriteria.doctor_id);
                    cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");
                    cmd.Parameters.AddWithValue("@specialist", '%' + filterCriteria.specialist + "%");

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Doctor> doctorList = new List<Doctor>();
                    while (reader.Read())
                    {
                        Doctor doctor = new Doctor
                        {
                            doctor_id = reader["doctor_id"] as string,
                            name = reader["name"] as string,
                            mobile = reader["mobile"] as string,
                            email = reader["email"] as string,
                            qualification = reader["qualification"] as string,
                            address = reader["address"] as string,
                            specialist = JArray.Parse(reader["specialist"].ToString()).ToObject<string[]>()
                        };

                        doctorList.Add(doctor);
                    }
                    reader.Close();

                    cmd = new SqlCommand(countQuery, con);
                    cmd.Parameters.AddWithValue("@doctor_id", filterCriteria.doctor_id);
                    cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");
                    cmd.Parameters.AddWithValue("@specialist", '%' + filterCriteria.specialist + "%");
                    int totalCount = (int)cmd.ExecuteScalar();

                    DoctorSearchResult result = new DoctorSearchResult
                    {
                        TotalCount = totalCount,
                        Doctors = doctorList
                    };

                    return Ok(result);
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
                    SqlCommand cmd = new SqlCommand("INSERT INTO doctor_table(doctor_id, name, mobile, specialist, email, qualification, address, created_by) VALUES(@doctor_id, @name, @mobbile, @specialist, @email, @qualification, @address, @created_by)", con);
                    cmd.Parameters.AddWithValue("@doctor_id", doctorDetails.doctor_id);
                    cmd.Parameters.AddWithValue("@name", doctorDetails.name);
                    cmd.Parameters.AddWithValue("@mobile", doctorDetails.mobile);
                    cmd.Parameters.AddWithValue("@specialist", specialistJson);
                    cmd.Parameters.AddWithValue("@email", doctorDetails.email);
                    cmd.Parameters.AddWithValue("@qualification", doctorDetails.qualification);
                    cmd.Parameters.AddWithValue("@address", doctorDetails.address);
                    cmd.Parameters.AddWithValue("@created_by", doctorDetails.created_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new { message = "Doctor has been Inserted" });
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


        //Auto Generate ID need to test
        //[HttpPost]
        //[Route("doctor/create")]
        //public ActionResult<string> PostDoctorData(Doctor doctorDetails)
        //{
        //    try
        //    {
        //        string specialistJson = JsonConvert.SerializeObject(doctorDetails.specialist);

        //        // Create an instance of UniqueIdGenerator
        //        UniqueIdGenerator idGenerator = new UniqueIdGenerator();

        //        // Generate the unique doctor_id with the desired format
        //        string doctorId = idGenerator.GenerateUniqueId("D");

        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
        //        {
        //            con.Open();
        //            SqlCommand cmd = new SqlCommand("INSERT INTO doctor_table(doctor_id, name, mobile, specialist, email, qualification, address, created_by) VALUES(@doctor_id, @name, @mob_no, @specialist, @email, @qualification, @address, @created_by)", con);
        //            cmd.Parameters.AddWithValue("@doctor_id", doctorId);
        //            cmd.Parameters.AddWithValue("@name", doctorDetails.name);
        //            cmd.Parameters.AddWithValue("@mob_no", doctorDetails.mob_no);
        //            cmd.Parameters.AddWithValue("@specialist", specialistJson);
        //            cmd.Parameters.AddWithValue("@email", doctorDetails.email);
        //            cmd.Parameters.AddWithValue("@qualification", doctorDetails.qualification);
        //            cmd.Parameters.AddWithValue("@address", doctorDetails.address);
        //            cmd.Parameters.AddWithValue("@created_by", doctorDetails.created_by);

        //            int i = cmd.ExecuteNonQuery();
        //            cmd.Dispose();

        //            if (i > 0)
        //            {
        //                return Ok(new { message = "Doctor has been Inserted" });
        //            }
        //            else
        //            {
        //                return BadRequest(new { message = "Error" });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "An error occurred", message = ex.Message });
        //    }
        //}



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
                    SqlCommand cmd = new SqlCommand("UPDATE doctor_table SET name = @name, mobile = @mobile, specialist = @specialist, email = @email, qualification = @qualification, address = @address, updated_by = @updated_by, updated_date = @updated_date WHERE doctor_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", doctorDetails.name);
                    cmd.Parameters.AddWithValue("@mobile", doctorDetails.mobile);
                    cmd.Parameters.AddWithValue("@specialist", specialistJson);
                    cmd.Parameters.AddWithValue("@email", doctorDetails.email);
                    cmd.Parameters.AddWithValue("@qualification", doctorDetails.qualification);
                    cmd.Parameters.AddWithValue("@address", doctorDetails.address);
                    cmd.Parameters.AddWithValue("@updated_by", doctorDetails.updated_by);
                    cmd.Parameters.AddWithValue("@updated_date", doctorDetails.updated_date);

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
