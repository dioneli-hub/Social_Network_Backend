using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.DataAccess.Configurations
{
    internal class ApplicationFileConfiguration : IEntityTypeConfiguration<ApplicationFile>
    {
        public void Configure(EntityTypeBuilder<ApplicationFile> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();

            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.ContentType).IsRequired();
            builder.Property(x => x.FileName).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.ToTable("ApplicationFiles");
        }
    }
}
