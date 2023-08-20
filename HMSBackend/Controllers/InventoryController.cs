using HMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace HMSBackend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public InventoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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
                              WHERE (category_id LIKE @category_id OR @category_id IS NULL) AND 
                                    (category_name LIKE @category_name OR @category_name IS NULL)) AS subquery 
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
    }
}
