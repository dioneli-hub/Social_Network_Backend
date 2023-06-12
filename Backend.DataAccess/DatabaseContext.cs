using Backend.DataAccess.Configurations;
using Backend.Domain;
using Microsoft.EntityFrameworkCore;

namespace Backend.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new PostConfiguration().Configure(modelBuilder.Entity<Post>());
            new PostCommentConfiguration().Configure(modelBuilder.Entity<PostComment>());
            new PostLikeConfiguration().Configure(modelBuilder.Entity<PostLike>());
            new UserConfiguration().Configure(modelBuilder.Entity<User>());
            new UserFollowerConfiguration().Configure(modelBuilder.Entity<UserFollower>());
            new ApplicationFileConfiguration().Configure(modelBuilder.Entity<ApplicationFile>());

        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }
        public DbSet<ApplicationFile> ApplicationFiles { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Post> Posts { get; set; }

    }
}