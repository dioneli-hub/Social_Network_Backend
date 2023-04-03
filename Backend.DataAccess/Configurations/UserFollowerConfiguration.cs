using Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.DataAccess.Configurations
{
    public class UserFollowerConfiguration : IEntityTypeConfiguration<UserFollower>
    {
        public void Configure(EntityTypeBuilder<UserFollower> builder) 
        {
            builder.HasKey(x => new 
            { 
                x.UserId, 
                x.FollowerId 
            });

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserFollowers)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Follower)
                .WithMany(x => x.UserFollowsTo)
                .HasForeignKey(x => x.FollowerId);
        }
    }
}
