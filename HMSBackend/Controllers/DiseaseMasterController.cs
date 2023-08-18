using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace HMSBackend.Controllers
{
    [Route("api")]
    [ApiController]
    public class DiseaseMasterController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DiseaseMasterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        [Route("diseasemaster/fetchAll")]
        public ActionResult<string> GetDiseaseMaster()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM disease_master_table", con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Disease_master> diseaseList = new List<Disease_master>();
                    while (reader.Read())
                    {
                        Disease_master disease = new Disease_master
                        {
                            disease_master_id = reader["disease_id"] as string,
                            disease_type = reader["disease_type"] as string,
                            specialist = reader["specialist"] as string,
                            sub_disese_type = reader["sub_disease_type"] as string,
                            treatment_type = reader["treatment_type"] as string,
                            created_by = reader["created_by"] as string,
                            created_date = reader["created_date"] as DateTime?,
                            updated_by = reader["updated_by"] as string,
                            updated_date = reader["updated_date"] as DateTime?
                        };

                        diseaseList.Add(disease);
                    }
                    reader.Close();

                    return Ok(diseaseList);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }
        }

        [HttpPost]
        [Route("diseaseMaster/create")]
        public ActionResult<string> PostDiseaseMaster(Disease_master disease_Master)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO disease_master_table(disease_id, specialist, disease_type, sub_disease_type, treatment_type, created_by, updated_by) Values(@disease_id, @specialist, @disease_type, @sub_disease_type, @treatment_type, @created_by, @updated_by)", con);
                    cmd.Parameters.AddWithValue("@disease_id", disease_Master.disease_master_id);
                    cmd.Parameters.AddWithValue("@specialist", disease_Master.specialist);
                    cmd.Parameters.AddWithValue("disease_type", disease_Master.disease_type);
                    cmd.Parameters.AddWithValue("sub_disease_type", disease_Master.sub_disese_type);
                    cmd.Parameters.AddWithValue("treatment_type", disease_Master.treatment_type);
                    cmd.Parameters.AddWithValue("created_by", disease_Master.created_by);
                    cmd.Parameters.AddWithValue("updated_by", disease_Master.updated_by);

                    int i = cmd.ExecuteNonQuery();

                    if (i > 0)
                    {
                        return Ok(new { message = "disease created successfully" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Error." });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }
        }

        //[HttpPut]
        //[Route("disease_Master/update")]
    }
}
