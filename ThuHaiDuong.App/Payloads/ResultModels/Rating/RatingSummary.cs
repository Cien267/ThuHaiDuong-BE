namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Rating;

public class RatingSummary
{
    public decimal AverageScore { get; set; }
    public int TotalRatings { get; set; }
    public Dictionary<int, int> ScoreDistribution { get; set; } = [];
    // { 1: 5, 2: 10, 3: 20, 4: 50, 5: 100 }
 
    public RatingResult? MyRating { get; set; }
}