﻿using HMSBackend.Models;
using HMSBackend.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace HMSBackend.Controllers
{
    //[Authorize]
    [Route("api/")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public InventoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        ///////////////////////////////////////////////////////Category///////////////////////////////////////////////////////
        [HttpPost]
        [Route("category/fetchAll")]
        public ActionResult<string> GetCategory(FilterCategoryCriteria filterCategoryCriteria)
        {
            List<Category> categoryList = new List<Category>();
            try
            {
                int page = Convert.ToInt32(filterCategoryCriteria.page);
                int pageSize = Convert.ToInt32(filterCategoryCriteria.pageSize);
                string sortByColumn = !string.IsNullOrEmpty(filterCategoryCriteria.sortBy) ? filterCategoryCriteria.sortBy : "category_id";
                string sortOrder = !string.IsNullOrEmpty(filterCategoryCriteria.order) ? filterCategoryCriteria.order.ToUpper() : "ASC"; // Default to ascending order

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    //string sqlQuery = $@"SELECT category_id, category_name FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber " +
                    //    "FROM category WHERE RowNumber BETWEEN @StartRow AND @EndRow AND " +
                    //    "category_id LIKE '%' + @category_id + '%' AND " +
                    //                   " category_name LIKE '%' + @category_name + '%' ";

                    string sqlQuery = $@"SELECT category_id, category_name 
                        FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber 
                              FROM category 
                              WHERE category_id LIKE '%' + @category_id + '%' AND 
                                    category_name LIKE '%' + @category_name + '%' ) AS subquery 
                        WHERE RowNumber BETWEEN @StartRow AND @EndRow";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@category_id", filterCategoryCriteria.category_id);
                    cmd.Parameters.AddWithValue("@category_name", filterCategoryCriteria.category_name);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Category category = new Category
                        {
                            category_id = reader["category_id"] as string,
                            category_name = reader["category_name"] as string
                        };

                        categoryList.Add(category);
                    }
                    reader.Close();
                }
                return Ok(categoryList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }
        }
        [HttpPost]
        [Route("category/create")]
        public ActionResult<string> Postcategory(Category category)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO category(category_id,category_name,created_by) VALUES(@category_id,@category_name,@created_by)", con);
                    cmd.Parameters.AddWithValue("@category_id", category.category_id);
                    cmd.Parameters.AddWithValue("@category_name", category.category_name);
                    cmd.Parameters.AddWithValue("@created_by", category.created_by);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    if (i > 0)
                    {
                        return Ok(new { message = "Category added Sucessfuly" });
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
        [Route("category/update")]
        public ActionResult<string> PutCategory(Category category)
        {
            try
            {
                string id = category.category_id;
                
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE category SET category_name = @category_name, updated_by = @updated_by, updated_date = @updated_date WHERE category_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@category_name", category.category_name);
                    cmd.Parameters.AddWithValue("@updated_by", category.updated_by);
                    cmd.Parameters.AddWithValue("@updated_date", category.updated_date);

                    int i = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    if (i > 0)
                    {
                        return Ok(new { message = "Category updated Successfully" });
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




        ////////////////////////////////////Supplier///////////////////////////////////////////////

        [HttpPost]
        [Route("suplier/fetchAll")]
        public ActionResult<string> GetSuplierRecords(FilterSupplierCriteria filterSupplierCriteria)
        {
            List<Supplier> supplierList = new List<Supplier>();
            try
            {
                int page = Convert.ToInt32(filterSupplierCriteria.page);
                int pageSize = Convert.ToInt32(filterSupplierCriteria.pageSize);
                string sortByColumn = !string.IsNullOrEmpty(filterSupplierCriteria.sortBy) ? filterSupplierCriteria.sortBy : "supplier_id";
                string sortOrder = !string.IsNullOrEmpty(filterSupplierCriteria.order) ? filterSupplierCriteria.order : "ASC";

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
                {
                    con.Open();
                    string Query = $@"SELECT supplier_id, supplier_name, contact_name, contact_email, contact_phone, address FROM (
                        SELECT supplier_id, supplier_name, contact_name, contact_email, contact_phone, address, ROW_NUMBER() OVER(ORDER BY {sortByColumn} {sortOrder}) AS RowNumber FROM supplier
                         WHERE supplier_id LIKE '%' + @supplier_id + '%' AND supplier_name LIKE '%' + @supplier_name + '%')
                        AS subquery WHERE RowNumber BETWEEN @StartRow AND @EndRow";

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;
                    SqlCommand cmd = new SqlCommand(Query, con);
                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@supplier_id", filterSupplierCriteria.supplier_id);
                    cmd.Parameters.AddWithValue("@supplier_name", filterSupplierCriteria.supplier_name);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Supplier supplier = new Supplier
                        {
                            supplier_id = reader["supplier_id"] as string,
                            supplier_name = reader["supplier_name"] as string
                        };

                        supplierList.Add(supplier);
                    }
                    reader.Close();
                }
                return Ok(supplierList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", message = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("supplier/create")]
        //public ActionResult<string> PostSupplierData(Supplier supplier)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("HMSEntities")))
        //        {
        //            con.Open();
        //            string Query = "INSERT INTO supplier(supplier_id, supplier_name, contact_name, contact_email, contact_phone, address, created_by) VLAUES(@supplier_id, @supplier_name, @contact_name, @contact_email, @contact_phone, @address, @created_by)";
        //            SqlCommand cmd = new SqlCommand(Query, con);
        //            cmd.Parameters.AddWithValue("@supplier_id", supplier.supplier_id);
        //            cmd.Parameters.AddWithValue("@supplier_name", supplier.supplier_name);
        //            cmd.Parameters.AddWithValue("@contact_name", supplier.contact_name);
        //            cmd.Parameters.AddWithValue("@contact_email", supplier.contact_email);
        //            cmd.Parameters.AddWithValue("@contact_phone", supplier.contact_phone);
        //            cmd.Parameters.AddWithValue("@address", supplier.address);

        //            int i = cmd.EndExecuteNonQuery();
        //            cmd.Dispose();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = "An error occurred", message = ex.Message });
        //    }
        //}


    }
}