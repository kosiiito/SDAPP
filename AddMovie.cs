using System.IO;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;

public static class AddMovie
{
    [FunctionName("AddMovie")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        log.LogInformation($"Request Body: {requestBody}");
        var data = JsonConvert.DeserializeObject<Movie2>(requestBody);

        try
        {
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("SQL connection string is null or empty.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            log.LogInformation($"Connection String: {connectionString}");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                log.LogInformation("Database connection opened.");

                var query = "INSERT INTO Movies (Title, Year, Genre, Description, Director, Actors) VALUES (@Title, @Year, @Genre, @Description, @Director, @Actors)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", data.Title);
                    command.Parameters.AddWithValue("@Year", data.Year);
                    command.Parameters.AddWithValue("@Genre", data.Genre);
                    command.Parameters.AddWithValue("@Description", data.Description);
                    command.Parameters.AddWithValue("@Director", data.Director);
                    command.Parameters.AddWithValue("@Actors", data.Actors);

                    log.LogInformation("Executing query.");
                    await command.ExecuteNonQueryAsync();
                    log.LogInformation("Query executed successfully.");
                }
            }

            return new OkObjectResult("Movie added successfully");
        }
        catch (SqlException sqlEx)
        {
            log.LogError($"SQL error occurred: {sqlEx.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            log.LogError($"General error occurred: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

public class Movie2 {
    public string Title { get; set; }
    public int Year { get; set; }
    public string Genre { get; set; }
    public string Description { get; set; }
    public string Director { get; set; }
    public string Actors { get; set; }
}
