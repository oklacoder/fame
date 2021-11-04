using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace fame.Persist.Postgresql
{
    public class PostgresPluginConfig
    {
        public const string PostgresPluginConfig_Key = "PostgresServer";
        public string PostgresqlConnection { get; set; }
    }

    public class PostgresPlugin :
        IFamePlugin
    {
        private PostgresPluginConfig _config = null;
        public bool? IsConfigured => _config is not null;

        private ILogger<PostgresPlugin> _logger;

        private ConcurrentDictionary<Guid, CancellationTokenSource> _tokenCache;

        public void Configure(
            IConfiguration config, 
            ILoggerFactory logger)
        {
            _logger = logger?.CreateLogger<PostgresPlugin>();
            _config = new PostgresPluginConfig();
            config.GetSection(PostgresPluginConfig.PostgresPluginConfig_Key).Bind(_config);

            using (var context = GetContext())
            {
                context.Database.EnsureCreated();
            }

            _tokenCache = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        }

        public void Enroll(IOperator target)
        {
            if (IsConfigured is not true)
                throw new InvalidOperationException($"Cannot enroll an operator in a plugin ({nameof(PostgresPlugin)}) that has not been configured.");

            target.HandleStarted += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleValidationStarted += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleValidationSucceeded += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleValidationFailed += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleExecutionStarted += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleExecutionSucceeded += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleSucceeded += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleFailed += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
            target.HandleFinished += async (object target, IMessage msg) =>
            {
                await SaveMessage(msg);
            };
        }

        private ContextBase GetContext()
        {
            return new ContextBase(_config.PostgresqlConnection);
        }

        private async Task SaveMessage(IMessage msg)
        {
            var typ = msg.GetType();
            if (!typ.IsAssignableTo(typeof(BaseMessage))) return;

            if (_tokenCache.TryGetValue(msg.RefId, out var cancellationSource))
            {
                cancellationSource.Cancel();
            }

            cancellationSource = new CancellationTokenSource();
            _tokenCache[msg.RefId] = cancellationSource;
            var token = cancellationSource.Token;

            _ = msg switch
            {
                BaseCommand c => await SaveCommand(c, token),
                BaseEvent e => await SaveEvent(e, token),
                BaseQuery q => await SaveQuery(q, token),
                BaseResponse r => await SaveResponse(r, token),
                _ => 0
            };
        }

        private async Task<int> SaveCommand(
            BaseCommand cmd,
            CancellationToken token)
        {
            using (var context = GetContext())
            {
                var c = await context.Commands.FirstOrDefaultAsync(x => x.MessageId == cmd.RefId.ToString());
                if (c is null)
                {
                    c = new MessageWrapper(cmd);
                    context.Commands.Add(c);
                }
                else
                {
                    c.SetMessage(cmd);
                }
                if (!token.IsCancellationRequested)
                {
                    _tokenCache.TryRemove(cmd.RefId, out _);
                    await context.SaveChangesAsync();
                }
            }
            return 1;
        }
        private async Task<int> SaveEvent(
            BaseEvent evt,
            CancellationToken token)
        {

            using (var context = GetContext())
            {
                var c = await context.Events.FirstOrDefaultAsync(x => x.MessageId == evt.RefId.ToString());
                if (c is null)
                {
                    c = new MessageWrapper(evt);
                    context.Commands.Add(c);
                }
                else 
                {                    
                    c.SetMessage(evt);
                }
                if (!token.IsCancellationRequested)
                {
                    _tokenCache.TryRemove(evt.RefId, out _);
                    await context.SaveChangesAsync();
                }
            }
            return 2;
        }
        private async Task<int> SaveQuery(
            BaseQuery query,
            CancellationToken token)
        {

            using (var context = GetContext())
            {
                var c = await context.Queries.FirstOrDefaultAsync(x => x.MessageId == query.RefId.ToString());
                if (c is null)
                {
                    c = new MessageWrapper(query);
                    context.Commands.Add(c);
                }
                else
                {
                    c.SetMessage(query);
                }
                if (!token.IsCancellationRequested)
                {
                    _tokenCache.TryRemove(query.RefId, out _);
                    await context.SaveChangesAsync();
                }
            }
            return 3;
        }
        private async Task<int> SaveResponse(
            BaseResponse resp,
            CancellationToken token)
        {

            using (var context = GetContext())
            {
                var c = await context.Responses.FirstOrDefaultAsync(x => x.MessageId == resp.RefId.ToString());
                if (c is null)
                {
                    c = new MessageWrapper(resp);
                    context.Commands.Add(c);
                }
                else
                {
                    c.SetMessage(resp);
                }
                if (!token.IsCancellationRequested)
                {
                    _tokenCache.TryRemove(resp.RefId, out _);
                    await context.SaveChangesAsync();
                }
            }
            return 4;
        }
    }

    public class ContextBase :
        DbContext
    {
        private readonly string _connectionString;

        public DbSet<MessageWrapper> Commands { get; set; }
        public DbSet<MessageWrapper> Queries { get; set; }
        public DbSet<MessageWrapper> Events { get; set; }
        public DbSet<MessageWrapper> Responses { get; set; }

        public ContextBase()
        {

        }
        public ContextBase(
            string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {

            builder.Entity<MessageWrapper>()
                .HasKey(x => x.SequenceId);

            //builder.Entity<MessageWrapper>()
            //    .Property(x => x.MessageId)
            //    .HasComputedColumnSql();
            //builder.Entity<MessageWrapper>()
            //    .Property(x => x.MessageType)
            //    .HasComputedColumnSql();
            //builder.Entity<MessageWrapper>()
            //    .Property(x => x.DateTimeUtc)
            //    .HasComputedColumnSql();

            builder.Entity<MessageWrapper>()
                .HasIndex(x => x.MessageId);
            builder.Entity<MessageWrapper>()
                .HasIndex(x => x.MessageType);
        }

        protected override void OnConfiguring(
            DbContextOptionsBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                builder.UseNpgsql(_connectionString);
            }
        }
    }

    public class MessageWrapper 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SequenceId { get; set; }

        public string MessageId
        {
            get => Message?.RefId.ToString();
            protected set { }
        }
        public string MessageType
        {
            get => Message?.GetType().FullName;
            protected set { }
        }

        [Column(TypeName = "jsonb")]
        public BaseMessage Message { get; set; }

        public DateTime? DateTimeUtc
        {
            get => Message?.DateTimeUtc;
            protected set { }
        }

        public T GetObjectAsMessage<T>()
            where T : BaseMessage    
        {
            return Message as T;
        }

        public void SetMessage(
            BaseMessage msg)
        {
            Message = msg;
        }

        public MessageWrapper()
        {

        }

        public MessageWrapper(
            BaseMessage msg)
        {
            SetMessage(msg);
        }
    }
}
