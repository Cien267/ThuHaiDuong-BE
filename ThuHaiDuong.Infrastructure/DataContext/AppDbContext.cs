using System.Linq.Expressions;
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
        public virtual DbSet<AffiliateLinkChapter>  AffiliateLinkChapters { get; set; }
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
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<ReadingHistory> ReadingHistories { get; set; }
        public async Task<int> CommitChangeAsync()
        {
            return await SaveChangesAsync();
        }

        public DbSet<TEntity> SetEntity<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }
        
        private void UpdateTimestamps()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State            = EntityState.Modified; // SOFT-DELETE - do đổi State thành Modified chứ không phải Deleted
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
        }
        
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property  = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var isNull    = Expression.Equal(
                    property,
                    Expression.Constant(null, typeof(DateTime?)));
                var lambda = Expression.Lambda(isNull, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            
            modelBuilder.BuildUserModel();
            modelBuilder.BuildAffiliateClickModel();
            modelBuilder.BuildAffiliateLinkModel();
            modelBuilder.BuildAffiliateLinkStoryModel();
            modelBuilder.BuildAffiliateLinkChapterModel();
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
            modelBuilder.BuildAuthorModel();
            modelBuilder.BuildReadingHistoryModel();
        }
    }
}
