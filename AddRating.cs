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

public static class AddRating
{
    [FunctionName("AddRating")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<URating>(requestBody);

        using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
        {
            connection.Open();
            var query = "INSERT INTO Ratings (MovieId, Title, Opinion, Rating, Date, Author) VALUES ((SELECT Id FROM Movies WHERE Title=@Title), @Title, @Opinion, @Rating, @Date, @Author)";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Title", data.Title);
                command.Parameters.AddWithValue("@Opinion", data.Opinion);
                command.Parameters.AddWithValue("@Rating", data.Rating);
                command.Parameters.AddWithValue("@Date", data.Date);
                command.Parameters.AddWithValue("@Author", data.Author);
                await command.ExecuteNonQueryAsync();
            }
        }

        return new OkObjectResult("Rating added successfully");
    }
}

public class URating
{
    public string Title { get; set; }
    public string Opinion { get; set; }
    public int Rating { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; }
}
