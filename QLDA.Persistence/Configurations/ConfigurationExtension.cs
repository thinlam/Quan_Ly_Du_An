using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Interfaces;

namespace QLDA.Persistence.Configurations;

public static class ConfigurationExtension {
    public static void ConfigureForBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class {
        builder.HasKey("Id");

        if (typeof(Entity<Guid>).IsAssignableFrom(typeof(TEntity))) {
            builder.Property<Guid>("Id")
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        } else if (typeof(Entity<Guid>).IsAssignableFrom(typeof(ITienDo))) {
            builder.Property<int?>("BuocId")
                .HasConversion(
                    toDb => toDb == 0 ? null : toDb,  // Convert 0 to null for database storage (common pattern for optional foreign keys)
                    fromDb => fromDb                   // Direct assignment from database (int?) to model (int?)
                );
        } else if (typeof(Entity<Guid>).IsAssignableFrom(typeof(IQuyetDinh))) {
            builder.Property<string?>("SoQuyetDinh").HasMaxLength(50);
            builder.Property<DateTimeOffset?>("NgayQuyetDinh").HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                    fromDb => fromDb
                );
            // builder.Property<string?>("TrichYeu");
        }
        if (typeof(IMayHaveDelete).IsAssignableFrom(typeof(TEntity))) {
            builder.HasQueryFilter(e => EF.Property<bool>(e, "IsDeleted") == false);
        }
        builder.Property<DateTimeOffset>("CreatedAt")
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasConversion(
                toDb => toDb.ToUniversalTime(),
                fromDb => fromDb
            );

        builder.Property<long>("Index")
            .HasDefaultValueSql("DATEDIFF(SECOND, '19700101', GETUTCDATE())");
        builder.HasIndex("Index")
            .IsClustered(false);

        builder.Property<string>("CreatedBy");
        builder.Property<DateTimeOffset>("CreatedAt");
        builder.Property<string>("UpdatedBy");
        builder.Property<DateTimeOffset?>("UpdatedAt")
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );
        builder.Property<bool>("IsDeleted");
        builder.Property<long>("Index");
    }
    public static void ConfigureForMaterializedPath<TEntity, TKey>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IMaterializedPathEntity<TKey> {
        builder.ConfigureForBase();

        builder.Property<TKey>("ParentId")
            .IsRequired(false);
    }
    public static void ConfigureForEnumDb<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class {

        builder.HasKey("Id");

        builder.Property<string>("Ma")
            .HasMaxLength(50);

        builder.HasIndex("Ma")
            .IsUnique()
            .HasFilter("[Ma] IS NOT NULL AND [Ma] <> ''");

        builder.Property<string>("Ten")
            .HasMaxLength(200);
    }
    public static void ConfigureForDanhMuc<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class {
        builder.ConfigureForBase();

        builder.Property<string>("Ma")
            .HasMaxLength(20);

        builder.HasIndex("Ma")
            .IsUnique()
            .HasFilter("[Ma] IS NOT NULL AND [Ma] <> ''");

        builder.Property<string>("Ten")
            .HasMaxLength(200);

        builder.Property<string>("MoTa")
            .HasMaxLength(400);

    }



}
