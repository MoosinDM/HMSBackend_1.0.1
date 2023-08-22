using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace HMSBackend.Controllers
{
    [Authorize]
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

        [HttpPost]
        [Route("patient/fetchAll")]
        public ActionResult<DoctorSearchResult> PostPatient(FilterPatientCriteria filterCriteria)
        {
            try
            {
                int page = Convert.ToInt32(filterCriteria.page);
                int pageSize = Convert.ToInt32(filterCriteria.pageSize);
                string sortByColumn = !string.IsNullOrEmpty(filterCriteria.sortBy) ? filterCriteria.sortBy : "patient_id";
                string sortOrder = !string.IsNullOrEmpty(filterCriteria.order) ? filterCriteria.order.ToUpper() : "ASC"; // Default to ascending order

                List<Patient> patientList = new List<Patient>();
                int totalCount = 0;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = $@"
                SELECT *
                FROM (
                    SELECT *,
                        ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber
                    FROM patient_table
                ) AS Subquery
                WHERE RowNumber BETWEEN @StartRow AND @EndRow AND 
                      patient_id LIKE '%' + @patient_id + '%' AND 
                      name LIKE '%' + @name + '%';";

                    string countQuery = @"
                SELECT COUNT(*) FROM patient_table
                WHERE patient_id LIKE '%' + @patient_id + '%' AND 
                      name LIKE '%' + @name + '%';";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@patient_id", filterCriteria.patient_id);
                    cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");

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
                            gender = reader["gender"] as string,
                            doctor_id = reader["doctor_id"] as string,
                            created_by = reader["created_by"] as string
                        };

                        patientList.Add(patient);
                    }
                    reader.Close();

                    SqlCommand cmdCount = new SqlCommand(countQuery, con);
                    cmdCount.Parameters.AddWithValue("@patient_id", filterCriteria.patient_id);
                    cmdCount.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");
                    totalCount = (int)cmdCount.ExecuteScalar(); // Get total count
                }

                DoctorSearchResult result = new DoctorSearchResult
                {
                    TotalCount = totalCount,
                    Patients = patientList
                };

                return Ok(result);
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
        [Route("patient/update")]
        public ActionResult<string> PutPatientData(Patient patient)
        {
            try
            {
                string id = patient.patient_id;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE patient_table SET name = @name, mobile = @mobile, email = @email, age = @age, gender = @gender, address = @address, doctor_id = @doctor_id, updated_by = @updated_by, updated_date = @updated_date WHERE patient_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", patient.name);
                    cmd.Parameters.AddWithValue("@mobile", patient.mobile);
                    cmd.Parameters.AddWithValue("@email", patient.email);
                    cmd.Parameters.AddWithValue("@age", patient.age);
                    cmd.Parameters.AddWithValue("@gender", patient.gender);
                    cmd.Parameters.AddWithValue("@address", patient.address);
                    cmd.Parameters.AddWithValue("@doctor_id", patient.doctor_id);
                    cmd.Parameters.AddWithValue("@updated_by", patient.updated_by);
                    cmd.Parameters.AddWithValue("@updated_date", patient.updated_date);
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
        [Route("patient/delete")]
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




        /// Patient History API and CRUD Operations

        [HttpPost]
        [Route("patient_hist/fetchAll")]
        public ActionResult<DoctorSearchResult> GetPatienthistData(FilterPatientHistCriteria filterPatientHistCriteria)
        {
            try
            {
                int page = Convert.ToInt32(filterPatientHistCriteria.page);
                int pageSize = Convert.ToInt32(filterPatientHistCriteria.pageSize);
                string sortByColumn = !string.IsNullOrEmpty(filterPatientHistCriteria.sortBy) ? filterPatientHistCriteria.sortBy : "patient_hist_id";
                string sortOrder = !string.IsNullOrEmpty(filterPatientHistCriteria.order) ? filterPatientHistCriteria.order.ToUpper() : "ASC"; // Default to ascending order
                string patient_type = !string.IsNullOrEmpty(filterPatientHistCriteria.patient_type) ? filterPatientHistCriteria.patient_type : "OPD";

                List<Patient_hist> patientHistList = new List<Patient_hist>();
                int totalCount = 0;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = $@"
                SELECT *
                FROM (
                    SELECT *,
                        ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber
                    FROM patient_history_table
                ) AS Subquery
                WHERE patient_type = @patient_type AND RowNumber BETWEEN @StartRow AND @EndRow AND 
                      patient_hist_id LIKE '%' + @patient_hist_id + '%' AND 
                      patient_id LIKE '%' + @patient_id + '%';";

                    string countQuery = @"
                SELECT COUNT(*) FROM patient_history_table
                WHERE patient_type = @patient_type AND
                      patient_hist_id LIKE '%' + @patient_hist_id + '%' AND 
                      patient_id LIKE '%' + @patient_id + '%';";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@patient_hist_id", filterPatientHistCriteria.patient_hist_id);
                    cmd.Parameters.AddWithValue("@patient_id", "%" + filterPatientHistCriteria.patient_id + "%");
                    cmd.Parameters.AddWithValue("@patient_type", patient_type);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Patient_hist patientHist = new Patient_hist
                        {
                            patient_hist_id = reader["patient_hist_id"] as string,
                            date = (DateTime)reader["date"],
                            patient_type = reader["patient_type"] as string,
                            patient_id = reader["patient_id"] as string,
                            treatment_type = reader["treatment_type"] as string,
                            specialist = reader["specialist"] as string,
                            patient_description = reader["patient_description"] as string,
                            comment = reader["comment"] as string,
                            doctor_id = reader["doctor_id"] as string,
                            created_by = reader["created_by"] as string
                        };

                        patientHistList.Add(patientHist);
                    }
                    reader.Close();

                    SqlCommand cmdCount = new SqlCommand(countQuery, con);
                    cmdCount.Parameters.AddWithValue("@patient_hist_id", filterPatientHistCriteria.patient_hist_id);
                    cmdCount.Parameters.AddWithValue("@patient_id", "%" + filterPatientHistCriteria.patient_id + "%");
                    cmdCount.Parameters.AddWithValue("@patient_type", patient_type);
                    totalCount = (int)cmdCount.ExecuteScalar(); // Get total count
                }

                DoctorSearchResult result = new DoctorSearchResult
                {
                    TotalCount = totalCount,
                    PatientsHist = patientHistList
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }
        }



        [HttpPost]
        [Route("patient_hist/create")]
        public ActionResult<string> PostPatient_hist(Patient_hist patient_Hist)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO patient_history_table(patient_hist_id, patient_id, patient_type, treatment_type, doctor_id, specialist, patient_description, comment, created_by) VALUES(@patient_hist_id, @patient_id, @patient_type, @treatment_type, @doctor_id, @specialist, @patient_description, @comment, @created_by)", con);
                    cmd.Parameters.AddWithValue("@patient_hist_id", patient_Hist.patient_hist_id);
                    cmd.Parameters.AddWithValue("@patient_id", patient_Hist.patient_id);
                    cmd.Parameters.AddWithValue("@patient_type", patient_Hist.patient_type);
                    cmd.Parameters.AddWithValue("@treatment_type", patient_Hist.treatment_type); // Fixed typo here
                    cmd.Parameters.AddWithValue("@doctor_id", patient_Hist.doctor_id);
                    cmd.Parameters.AddWithValue("@specialist", patient_Hist.specialist);
                    cmd.Parameters.AddWithValue("@patient_description", patient_Hist.patient_description);
                    cmd.Parameters.AddWithValue("@comment", patient_Hist.comment);
                    cmd.Parameters.AddWithValue("@created_by", patient_Hist.created_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok(new { message = "Patient History created successfully" });
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
        [Route("patient_hist/update")]
        public ActionResult<string> PutPatienthist(Patient_hist patient_Hist)
        {
            try
            {
                string id = patient_Hist.patient_hist_id;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    string query = "UPDATE PATIENT_HISTORY_TABLE SET patient_type = @patient_type, treatment_type = @treatment_type, doctor_id = @doctor_id, specialist = @specialist, patient_description = @patient_description, comment = @comment WHERE Patient_hist_id = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@patient_type", patient_Hist.patient_type);
                    cmd.Parameters.AddWithValue("@treatment_type", patient_Hist.treatment_type);
                    cmd.Parameters.AddWithValue("@doctor_id", patient_Hist.doctor_id);
                    cmd.Parameters.AddWithValue("@specialist", patient_Hist.specialist);
                    cmd.Parameters.AddWithValue("@patient_description", patient_Hist.patient_description);
                    cmd.Parameters.AddWithValue("@comment", patient_Hist.comment);
                   // cmd.Parameters.AddWithValue("@patient_hist_id", patient_Hist.patient_hist_id);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    if (i > 0)
                    {
                        return Ok(new { message = "Patient History hase been Updated" });
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