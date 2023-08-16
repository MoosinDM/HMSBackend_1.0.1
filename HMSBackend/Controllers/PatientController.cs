using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace HMSBackend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UniqueIdGenerator _idGenerator;

        public PatientController(IConfiguration configuration, UniqueIdGenerator idGenerator)
        {
            _configuration = configuration;
            _idGenerator = idGenerator; // Inject the UniqueIdGenerator
        }

        [HttpGet]
        [Route("patient/fetchAll")]
        public ActionResult<IEnumerable<Patient>> GetPatientData()
        {
            List<Patient> patientList = new List<Patient>();

            try
            {
                int page = Convert.ToInt32(Request.Query["page"]);
                int pageSize = Convert.ToInt32(Request.Query["pageSize"]);
                string sortBy = Request.Query["sortBy"];
                string order = Request.Query["order"];

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = $@"
                    SELECT *
                    FROM (
                        SELECT *,
                            ROW_NUMBER() OVER(ORDER BY {sortBy} {order}) AS RowNumber
                        FROM patient_table
                    ) AS Subquery
                    WHERE RowNumber BETWEEN @StartRow AND @EndRow";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Patient patient = new Patient
                        {
                            patient_id = reader["patient_id"] as string,
                            name = reader["name"] as string,
                            mobile = reader["mobile"] as string,
                            email = reader["email"] as string,
                            age = (int)reader["age"],
                            gender = reader["gender"] as string, // Correct the column name
                            doctor_id = reader["doctor_id"] as string,
                            created_by = reader["created_by"] as string
                        };

                        patientList.Add(patient);
                    }
                    reader.Close();
                }

                return Ok(patientList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }

        }

        [HttpPost]
        [Route("patient/create")]
        public ActionResult<string> PostPatientData(Patient patientDetails, [FromServices] UniqueIdGenerator idGenerator)
        {
            try
            {
                string newPatientId = idGenerator.GenerateUniqueId("P"); // Generate the new auto-generated ID with "P" prefix

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO patient_table(patient_id, name, mobile, email, age, gender, address, doctor_id, created_by) VALUES(@patient_id, @name, @mobile, @email, @age, @gender, @address, @doctor_id, @created_by)", con);
                    cmd.Parameters.AddWithValue("@patient_id", newPatientId); // Use the auto-generated ID
                    cmd.Parameters.AddWithValue("@name", patientDetails.name);
                    cmd.Parameters.AddWithValue("@mobile", patientDetails.mobile);
                    cmd.Parameters.AddWithValue("@email", patientDetails.email);
                    cmd.Parameters.AddWithValue("@age", patientDetails.age);
                    cmd.Parameters.AddWithValue("@gender", patientDetails.gender);
                    cmd.Parameters.AddWithValue("@address", patientDetails.address);
                    cmd.Parameters.AddWithValue("@doctor_id", patientDetails.doctor_id);
                    cmd.Parameters.AddWithValue("@created_by", patientDetails.created_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new { message = "Patient created successfully" });
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
        [Route("patient/update/{id}")]
        public ActionResult<string> PutPatientData(string id, Patient patient)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE patient_table SET name = @name, mobile = @mobile, email = @email, age = @age, gender = @gender, address = @address, doctor_id = @doctor_id WHERE patient_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", patient.name);
                    cmd.Parameters.AddWithValue("@mobile", patient.mobile);
                    cmd.Parameters.AddWithValue("@email", patient.email);
                    cmd.Parameters.AddWithValue("@age", patient.age);
                    cmd.Parameters.AddWithValue("@gender", patient.gender);
                    cmd.Parameters.AddWithValue("@address", patient.address);
                    cmd.Parameters.AddWithValue("@doctor_id", patient.doctor_id);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new {message = "Patient hase been Updated" });
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
        [Route("patient/delete/{id}")]
        public ActionResult<string> DeletePatientData(string id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM patient_table WHERE patient_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new {message = "Patient Deleted Successfuly." });
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

        [HttpGet]
        [Route("patient_hist/fetchAll")]
        public ActionResult<string> GetPatienthistData()
        {
            List<Patient_hist> patient_hist = new List<Patient_hist>();
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    string sqlQuery = "SELECT * FROM patient_history_table";
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Patient_hist patient_Hist = new Patient_hist
                        {
                            patient_hist_id = reader["patient_hist_id"] as string,
                            date = (DateTime)reader["date"],
                            patient_type = reader["patient_type"] as string,
                            patient_id = reader["patient_id"] as string,
                            treatment_type = reader["treatment_type"] as string,
                            specialist = reader["specialist"] as string[],
                            patient_description = reader["patient_description"] as string,
                            comment = reader["comment"] as string,
                            doctor_id = reader["doctor_id"] as string

                        };

                        patient_hist.Add(patient_Hist);
                    }
                    reader.Close();
                }

                return Ok(patient_hist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }

        }

        [HttpPost]
        [Route("patient/filter")]
        public ActionResult<IEnumerable<Patient>> PostFilterData(FilterPatientCriteria filterCriteria)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string query = "SELECT * " +
                                   "FROM patient_table " +
                                   "WHERE patient_id LIKE '%' + @patient_id + '%' OR " +
                                   "      name LIKE '%' + @name + '%' ";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@patient_id", filterCriteria.patient_id);
                    cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Patient> patients = new List<Patient>();

                    while (reader.Read())
                    {
                        Patient patient = new Patient
                        {
                            patient_id = reader["patient_id"] as string,
                            name = reader["name"] as string,
                            mobile = reader["mobile"] as string,
                            email = reader["email"] as string,
                            age = (int)reader["age"],
                            gender = reader["gender"] as string, // Correct the column name
                            doctor_id = reader["doctor_id"] as string,
                            created_by = reader["created_by"] as string
                        };

                        patients.Add(patient);
                    }
                    reader.Close();
                    return Ok(patients);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }

        }

    }
}