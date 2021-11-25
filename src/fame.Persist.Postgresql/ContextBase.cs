using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Linq;

namespace fame.Persist.Postgresql
{

    public class ContextBase :
        DbContext
    {
        private readonly string _connectionString;
        private readonly ILoggerFactory loggerFactory;

        public DbSet<BaseCommand> Commands { get; set; }
        public DbSet<BaseQuery> Queries { get; set; }
        public DbSet<BaseEvent> Events { get; set; }
        public DbSet<BaseResponse> Responses { get; set; }

        public ContextBase()
        {

        }
        public ContextBase(
            string connectionString,
            ILoggerFactory loggerFactory = null)
        {
            _connectionString = connectionString;
            this.loggerFactory = loggerFactory;

            NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
        }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {

            builder.Entity<BaseCommand>()
                .HasKey(x => x.SequenceId);
            builder.Entity<BaseCommand>()
                .HasIndex(x => x.RefId);
            builder.Entity<BaseCommand>()
                .HasIndex(x => x.UserId);
            builder.Entity<BaseCommand>()
                .HasIndex(x => x.DateTimeUtc);
            builder.Entity<BaseCommand>()
                .Property(x => x.Args)
                .HasColumnType("jsonb");

            Util.GetAllLoadedCommandTypes().ToList().ForEach(x =>
            {
                builder.Entity<BaseCommand>()
                    .HasDiscriminator<string>("Type")
                    .HasValue(x, x.FullName);
            });

            builder.Entity<BaseQuery>()
                .HasKey(x => x.SequenceId);
            builder.Entity<BaseQuery>()
                .HasIndex(x => x.RefId);
            builder.Entity<BaseQuery>()
                .HasIndex(x => x.UserId);
            builder.Entity<BaseQuery>()
                .HasIndex(x => x.DateTimeUtc);
            builder.Entity<BaseQuery>()
                .Property(x => x.Args)
                .HasColumnType("jsonb");

            Util.GetAllLoadedQueryTypes().ToList().ForEach(x =>
            {
                builder.Entity<BaseQuery>()
                    .HasDiscriminator<string>("Type")
                    .HasValue(x, x.FullName);
            });

            builder.Entity<BaseEvent>()
                .HasKey(x => x.SequenceId);
            builder.Entity<BaseEvent>()
                .HasIndex(x => x.RefId);
            builder.Entity<BaseEvent>()
                .HasIndex(x => x.SourceId);
            builder.Entity<BaseEvent>()
                .HasIndex(x => x.SourceUserId);
            builder.Entity<BaseEvent>()
                .HasIndex(x => x.DateTimeUtc);
            builder.Entity<BaseEvent>()
                .Property(x => x.Args)
                .HasColumnType("jsonb");

            Util.GetAllLoadedEventTypes().ToList().ForEach(x =>
            {
                builder.Entity<BaseEvent>()
                    .HasDiscriminator<string>("Type")
                    .HasValue(x, x.FullName);
            });

            builder.Entity<BaseResponse>()
                .HasKey(x => x.SequenceId);
            builder.Entity<BaseResponse>()
                .HasIndex(x => x.RefId);
            builder.Entity<BaseResponse>()
                .HasIndex(x => x.DateTimeUtc);
            builder.Entity<BaseResponse>()
                .Property(x => x.Args)
                .HasColumnType("jsonb");
            builder.Entity<BaseResponse>()
                .Property(x => x.Messages)
                .HasColumnType("jsonb");

            Util.GetAllLoadedResponseTypes().ToList().ForEach(x =>
            {
                builder.Entity<BaseResponse>()
                    .HasDiscriminator<string>("Type")
                    .HasValue(x, x.FullName);
            });
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                builder.UseNpgsql(_connectionString);
            }
            if (loggerFactory != null)
            {
                builder.UseLoggerFactory(loggerFactory);
            }
        }
    }

}
