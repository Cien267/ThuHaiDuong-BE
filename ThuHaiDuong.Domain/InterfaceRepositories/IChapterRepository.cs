using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Domain.InterfaceRepositories;

public interface IChapterRepository
{
    Task<int> GetMaxChapterNumberAsync(Guid storyId);
 
    Task<bool> ChapterNumberExistsAsync(Guid storyId, int chapterNumber, Guid? excludeId = null);
 
    Task<Chapter?> GetChapterForReadingAsync(Guid chapterId);
 
    Task<Chapter?> GetChapterForReadingByNumberAsync(Guid storyId, int chapterNumber);
    Task<Chapter?> GetPrevChapterAsync(Chapter chapter);
    Task<Chapter?> GetNextChapterAsync(Chapter chapter);
 
    Task SyncStoryCountersAsync(Guid storyId);
}