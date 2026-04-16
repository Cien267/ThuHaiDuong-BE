namespace ThuHaiDuong.Shared.Constants;

public static class StoryStatus
{
    public const string Draft         = "Draft";
    public const string PendingReview = "PendingReview";
    public const string Approved      = "Approved";
    public const string Rejected      = "Rejected";
    public const string Publishing    = "Publishing";
    public const string Completed     = "Completed";
    public const string Paused        = "Paused";
 
    public static readonly string[] VisibleToClient = [Publishing, Completed];
 
    public static readonly string[] PublishableStatuses = [Publishing, Completed, Paused];
}