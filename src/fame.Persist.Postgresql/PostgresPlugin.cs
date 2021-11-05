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
using System.Reflection;

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
                var c = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
                if (c is null)
                {
                    c = cmd;
                    context.Commands.Add(c);
                }
                else
                {
                    c = cmd;
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
                var c = await context.Events.FirstOrDefaultAsync(x => x.RefId == evt.RefId);
                if (c is null)
                {
                    c = evt;
                    context.Events.Add(c);
                }
                else 
                {
                    c = evt;
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
                var c = await context.Queries.FirstOrDefaultAsync(x => x.RefId == query.RefId);
                if (c is null)
                {
                    c = query;
                    context.Queries.Add(c);
                }
                else
                {
                    c = query;
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
                var c = await context.Responses.FirstOrDefaultAsync(x => x.RefId == resp.RefId);
                if (c is null)
                {
                    c = resp;
                    context.Responses.Add(c);
                }
                else
                {
                    c = resp;
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

    public static class Util
    {
        public static IEnumerable<Type> GetAllLoadedCommandTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseCommand).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedEventTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseEvent).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedQueryTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseQuery).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseQuery).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedResponseTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseResponse).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseResponse).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
    }

    public class ContextBase :
        DbContext
    {
        private readonly string _connectionString;

        public DbSet<BaseCommand> Commands { get; set; }
        public DbSet<BaseQuery> Queries { get; set; }
        public DbSet<BaseEvent> Events { get; set; }
        public DbSet<BaseResponse> Responses { get; set; }

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

            builder.Entity<BaseCommand>()
                .HasKey(x => x.RefId);
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
                .HasKey(x => x.RefId);
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
                .HasKey(x => x.RefId);
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
                .HasKey(x => x.RefId);
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
        }
    }

    //pull out the wrapper, save the messages directly, fluent add as needed, and add discriminator stuff
    //https://docs.microsoft.com/en-us/ef/core/modeling/inheritance

    public class MessageWrapper<T>
        where T : BaseMessage
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
        public T Message { get; set; }

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
            T msg)
        {
            Message = msg;
        }

        public MessageWrapper()
        {

        }

        public MessageWrapper(T message)
        {
            SetMessage(message);
        }
    }
}
