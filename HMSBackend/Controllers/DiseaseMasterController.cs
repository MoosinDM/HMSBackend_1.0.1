using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Globalization;

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
        [HttpPost]
        [Route("diseasemaster/fetchAll")]
        public ActionResult<string> GetDiseaseMaster(FilterDiseaseMasterCriteria filterCriteria)
        {
            try
            {
                int page = Convert.ToInt32(filterCriteria.page);
                int pageSize = Convert.ToInt32(filterCriteria.pageSize);
                string sortByColumn = !string.IsNullOrEmpty(filterCriteria.sortBy) ? filterCriteria.sortBy : "disease_id";
                string sortOrder = !string.IsNullOrEmpty(filterCriteria.order) ? filterCriteria.order.ToUpper() : "ASC"; // Default to ascending order
      
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    string query = $@"
                    SELECT *
                    FROM (
                        SELECT *,
                            ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber
                        FROM disease_master
                    ) AS Subquery
                    WHERE RowNumber BETWEEN @StartRow AND @EndRow AND " +
                    "disease_id LIKE '%' + @disease_id + '%' ";
                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@disease_id", filterCriteria.disease_id);

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
                    SqlCommand cmd = new SqlCommand("INSERT INTO disease_master(disease_id, specialist, disease_type, sub_disease_type, treatment_type, created_by) Values(@disease_id, @specialist, @disease_type, @sub_disease_type, @treatment_type, @created_by)", con);
                    cmd.Parameters.AddWithValue("@disease_id", disease_Master.disease_master_id);
                    cmd.Parameters.AddWithValue("@specialist", disease_Master.specialist);
                    cmd.Parameters.AddWithValue("disease_type", disease_Master.disease_type);
                    cmd.Parameters.AddWithValue("sub_disease_type", disease_Master.sub_disese_type);
                    cmd.Parameters.AddWithValue("treatment_type", disease_Master.treatment_type);
                    cmd.Parameters.AddWithValue("created_by", disease_Master.created_by);

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

        [HttpPut]
        [Route("disease_Master/update")]
        public ActionResult<string> PutdiseaseMaster(string disease_id, Disease_master disease_Master)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE disease_master SET specialist = @specialist, disease_type = @disease_type, sub_disease_type = @sub_disease_type, treatment_type = @treatment_type, updated_by = @updated_by WHERE disease_id = @disease_id", con);
                    cmd.Parameters.AddWithValue("@disease_id", disease_id);
                    cmd.Parameters.AddWithValue("@specialist", disease_Master.specialist);
                    cmd.Parameters.AddWithValue("@disease_type", disease_Master.disease_type);
                    cmd.Parameters.AddWithValue("@sub_disease_type", disease_Master.sub_disese_type);
                    cmd.Parameters.AddWithValue("@treatment_type", disease_Master.treatment_type);
                    cmd.Parameters.AddWithValue("@updated_by", disease_Master.updated_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new { message = "Disease Updated Succesful" });
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
