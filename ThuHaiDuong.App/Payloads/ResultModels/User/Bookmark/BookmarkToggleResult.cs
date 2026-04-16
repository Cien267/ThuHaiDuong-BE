namespace ThuHaiDuong.Application.Payloads.ResultModels.User.Bookmark;

public class BookmarkToggleResult
{
    public bool IsBookmarked { get; set; }
    public Guid StoryId { get; set; }
}