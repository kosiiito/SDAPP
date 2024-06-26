using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public static class SearchMovies
{
    [FunctionName("SearchMovies")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        string title = req.Query["title"];
        var movies = new List<Movie>();

        using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
        {
            connection.Open();
            var query = string.IsNullOrEmpty(title) ? "SELECT * FROM Movies" : "SELECT * FROM Movies WHERE Title LIKE @Title";
            using (var command = new SqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(title))
                {
                    command.Parameters.AddWithValue("@Title", $"%{title}%");
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var movie = new Movie
                        {
                            Id = (int)reader["Id"],
                            Title = reader["Title"].ToString(),
                            Year = (int)reader["Year"],
                            Genre = reader["Genre"].ToString(),
                            Description = reader["Description"].ToString(),
                            Director = reader["Director"].ToString(),
                            Actors = reader["Actors"].ToString(),
                            AverageRating = reader["AverageRating"] as float?
                        };
                        movies.Add(movie);
                    }
                }
            }
        }

        return new OkObjectResult(movies);
    }
}

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Genre { get; set; }
    public string Description { get; set; }
    public string Director { get; set; }
    public string Actors { get; set; }
    public float? AverageRating { get; set; }
}
