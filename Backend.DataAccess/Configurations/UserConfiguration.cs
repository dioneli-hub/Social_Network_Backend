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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();

            builder.Property(x => x.FirstName).IsRequired();
            builder.Property(x => x.LastName).IsRequired();
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.SaltHash).IsRequired();

            builder.HasOne(x => x.Avatar)
                .WithMany()
                .HasForeignKey(x => x.AvatarFileId);

            builder.ToTable("Users");
        }
    }
}
