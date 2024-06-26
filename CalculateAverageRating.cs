using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

public static class CalculateAverageRating
{
    [FunctionName("CalculateAverageRating")]
    public static async Task Run([TimerTrigger("0 30 11 * * *")] TimerInfo myTimer, ILogger log)
    {
        using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
        {
            connection.Open();
            var query = @"
                UPDATE Movies
                SET AverageRating = (
                    SELECT AVG(Rating)
                    FROM Ratings
                    WHERE Ratings.MovieId = Movies.Id
                )";
            using (var command = new SqlCommand(query, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        log.LogInformation($"Average ratings calculated at: {DateTime.Now}");
    }
}
