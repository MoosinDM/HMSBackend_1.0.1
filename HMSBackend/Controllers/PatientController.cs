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
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();

                    string sqlQuery = "SELECT * FROM patient_table";
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
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
                            reg_by = reader["reg_by"] as string
                        };

                        patientList.Add(patient);
                    }
                    reader.Close();
                }

                return Ok(patientList);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to fetch patient data: " + ex.Message);
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
                    SqlCommand cmd = new SqlCommand("INSERT INTO patient_table(patient_id, name, mobile, email, age, gender, address, doctor_id, reg_by) VALUES(@patient_id, @name, @mobile, @email, @age, @gender, @address, @doctor_id, @reg_by)", con);
                    cmd.Parameters.AddWithValue("@patient_id", newPatientId); // Use the auto-generated ID
                    cmd.Parameters.AddWithValue("@name", patientDetails.name);
                    cmd.Parameters.AddWithValue("@mobile", patientDetails.mobile);
                    cmd.Parameters.AddWithValue("@email", patientDetails.email);
                    cmd.Parameters.AddWithValue("@age", patientDetails.age);
                    cmd.Parameters.AddWithValue("@gender", patientDetails.gender);
                    cmd.Parameters.AddWithValue("@address", patientDetails.address);
                    cmd.Parameters.AddWithValue("@doctor_id", patientDetails.doctor_id);
                    cmd.Parameters.AddWithValue("@reg_by", patientDetails.reg_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    if (i > 0)
                    {
                        return Ok("Patient created successfully");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to create patient: " + ex.Message);
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
                        return Ok("Data hase been Updated");
                    }
                    else
                    {
                        return BadRequest("Error");
                    }
                }
            }
            catch(Exception ex)
            {
                return BadRequest("failed to update patient data: " + ex);
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
                        return Ok("Data has Deleted.");
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