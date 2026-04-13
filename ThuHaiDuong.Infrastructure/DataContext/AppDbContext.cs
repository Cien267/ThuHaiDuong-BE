using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Domain.Entities;
using System.Threading.Tasks;

namespace ThuHaiDuong.Infrastructure.DataContext
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<AffiliateClick> AffiliateClicks { get; set; }
        public virtual DbSet<AffiliateLink>  AffiliateLinks { get; set; }
        public virtual DbSet<AffiliateLinkStory>  AffiliateLinkStories { get; set; }
        public virtual DbSet<Bookmark> Bookmarks { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Chapter> Chapters { get; set; }
        public virtual DbSet<ChapterView> ChapterViews { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<DailyStoryStat> DailyStoryStats { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Story> Stories { get; set; }
        public virtual DbSet<StoryCategory> StoryCategories { get; set; }
        public virtual DbSet<StoryTag> StoryTags { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<UserReadingProgress> UserReadingProgresses { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public async Task<int> CommitChangeAsync()
        {
            return await SaveChangesAsync();
        }

        public DbSet<TEntity> SetEntity<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildUserModel();
            modelBuilder.BuildAffiliateClickModel();
            modelBuilder.BuildAffiliateLinkModel();
            modelBuilder.BuildAffiliateLinkStoryModel();
            modelBuilder.BuildBookmarkModel();
            modelBuilder.BuildCategoryModel();
            modelBuilder.BuildChapterModel();
            modelBuilder.BuildChapterViewModel();
            modelBuilder.BuildCommentModel();
            modelBuilder.BuildDailyStoryStatModel();
            modelBuilder.BuildRatingModel();
            modelBuilder.BuildStoryModel();
            modelBuilder.BuildStoryCategoryModel();
            modelBuilder.BuildStoryTagModel();
            modelBuilder.BuildSubscriptionModel();
            modelBuilder.BuildTagModel();
            modelBuilder.BuildUserReadingProgressModel();
            modelBuilder.BuildRefreshTokenModel();
        }
    }
}
