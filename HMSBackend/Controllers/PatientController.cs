using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Protocol;

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

        [HttpPost]
        [Route("patient/fetchAll")]
        public ActionResult<IEnumerable<Patient>> PostPatient(FilterPatientCriteria filterCriteria)
        {
            List<Patient> patientList = new List<Patient>();

            try
            {
                int page = filterCriteria.page;
                int pageSize = filterCriteria.pageSize;
                string sortBy = filterCriteria.sortBy;
                string order = filterCriteria.order;

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
                    WHERE RowNumber BETWEEN @StartRow AND @EndRow AND "+
                    "patient_id LIKE '%' + @patient_id + '%' AND " +
                                   "      name LIKE '%' + @name + '%' ";

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


        //[HttpPost]
        //[Route("patient/filter")]
        //public ActionResult<IEnumerable<Patient>> PostFilterData(FilterPatientCriteria filterCriteria)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
        //        {
        //            con.Open();

        //            string query = "SELECT * " +
        //                           "FROM patient_table " +
        //                           "WHERE patient_id LIKE '%' + @patient_id + '%' OR " +
        //                           "      name LIKE '%' + @name + '%' ";

        //            SqlCommand cmd = new SqlCommand(query, con);
        //            cmd.Parameters.AddWithValue("@patient_id", filterCriteria.patient_id);
        //            cmd.Parameters.AddWithValue("@name", "%" + filterCriteria.name + "%");

        //            SqlDataReader reader = cmd.ExecuteReader();

        //            List<Patient> patients = new List<Patient>();

        //            while (reader.Read())
        //            {
        //                Patient patient = new Patient
        //                {
        //                    patient_id = reader["patient_id"] as string,
        //                    name = reader["name"] as string,
        //                    mobile = reader["mobile"] as string,
        //                    email = reader["email"] as string,
        //                    age = (int)reader["age"],
        //                    gender = reader["gender"] as string, // Correct the column name
        //                    doctor_id = reader["doctor_id"] as string,
        //                    created_by = reader["created_by"] as string
        //                };

        //                patients.Add(patient);
        //            }
        //            reader.Close();
        //            return Ok(patients);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "An error occurred", message = ex.Message });
        //    }

        //}


        
        /// Patient History API and CRUD Operations

        [HttpPost]
        [Route("patient_hist/fetchAll")]
        public ActionResult<string> GetPatienthistData(FilterPatientHistCriteria filterPatientHistCriteria)
        {
            List<Patient_hist> patient_hist = new List<Patient_hist>();
            try
            {
                int page = filterPatientHistCriteria.page;
                int pageSize = filterPatientHistCriteria.pageSize;
                string sortBy = filterPatientHistCriteria.sortBy;
                string order = filterPatientHistCriteria.order;

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    string sqlQuery = "SELECT *, ROW_NUMBER() OVER(ORDER BY {sortBy} {order}) AS RowNumber " +
                                   "FROM patient_history_table " +
                                   "WHERE patient_hist_id LIKE '%' + @patient_hist_id + '%' AND " +
                                   "      patient_id LIKE '%' + @patient_id + '%' ";
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@patient_hist_id", filterPatientHistCriteria.patient_hist_id);
                    cmd.Parameters.AddWithValue("@patient_id", "%" + filterPatientHistCriteria.patient_id + "%");
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
                            specialist = reader["specialist"] as string,
                            patient_description = reader["patient_description"] as string,
                            comment = reader["comment"] as string,
                            doctor_id = reader["doctor_id"] as string,
                            created_by = reader["created_by"] as string
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

        //[HttpPost]
        //[Route("patient_hist/create")]
        //public ActionResult<string> PostPatient_hist(Patient_hist patient_Hist)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
        //        {
        //            con.Open();
        //            SqlCommand cmd = new SqlCommand("INSERT INTO patient_history_table(patient_hist_id, patient_id, patient_type, treatment_type, doctor_id, specialist, details, comment, created_by) VALUES(@patient_hist_id, @patient_id, @patient_type, @treatment_type, @doctor_id, @specialist, @details, @comment, @created_by)");
        //            cmd.Parameters.AddWithValue("@patient_hist_id", patient_Hist.patient_hist_id);
        //            cmd.Parameters.AddWithValue("@patient_id", patient_Hist.patient_id);
        //            cmd.Parameters.AddWithValue("@patient_type", patient_Hist.patient_type);
        //            cmd.Parameters.AddWithValue("@teatment_type", patient_Hist.treatment_type);
        //            cmd.Parameters.AddWithValue("@doctor_id", patient_Hist.doctor_id);
        //            cmd.Parameters.AddWithValue("@specialist", patient_Hist.specialist);
        //            cmd.Parameters.AddWithValue("@details", patient_Hist.patient_description);
        //            cmd.Parameters.AddWithValue("@comment", patient_Hist.comment);
        //            cmd.Parameters.AddWithValue("@created_by", patient_Hist.created_by);

        //            int i = cmd.ExecuteNonQuery();
        //            cmd.Dispose();

        //            if (i > 0)
        //            {
        //                return Ok(new { message = "Patient created successfully" });
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
        [Route("patient_hist/update/{id}")]
        public ActionResult<string> PutPatienthist(string id, Patient_hist patient_Hist)
        {
            try
            {
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

        //[HttpPost]
        //[Route("patient_hist/filter")]
        //public ActionResult<string> PostPatienthistFilter(FilterPatientHistCriteria filterPatientHistCriteria)
        //{
        //    using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
        //    {
        //        con.Open();
        //        string query = "SELECT * " +
        //                           "FROM patient_history_table " +
        //                           "WHERE patient_hist_id LIKE '%' + @patient_hist_id + '%' OR " +
        //                           "      patient_id LIKE '%' + @patient_id + '%' ";

        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@patient_hist_id", filterPatientHistCriteria.patient_hist_id);
        //        cmd.Parameters.AddWithValue("@patient_id", "%" + filterPatientHistCriteria.patient_id + "%");

        //        SqlDataReader reader = cmd.ExecuteReader();

        //        List<Patient_hist> patients = new List<Patient_hist>();

        //        while (reader.Read())
        //        {
        //            Patient_hist patient_hist = new Patient_hist
        //            {
        //                patient_hist_id = reader["patient_hist_id"] as string,
        //                date = (DateTime)reader["date"],
        //                patient_type = reader["patient_type"] as string,
        //                patient_id = reader["patient_id"] as string,
        //                treatment_type = reader["treatment_type"] as string,
        //                specialist = reader["specialist"] as string,
        //                patient_description = reader["patient_description"] as string,
        //                comment = reader["comment"] as string,
        //                doctor_id = reader["doctor_id"] as string,
        //                created_by = reader["created_by"] as string
        //            };

        //            patients.Add(patient_hist);
        //        }
        //        reader.Close();
        //        return Ok(patients);



        //    }
        //}

    }
}