using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Collections.Concurrent;

namespace fame.Persist.Postgresql
{

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
                //await SaveMessage(msg);
            };
            target.HandleValidationStarted += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleValidationSucceeded += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleValidationFailed += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleExecutionStarted += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleExecutionSucceeded += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleSucceeded += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
            };
            target.HandleFailed += async (object target, IMessage msg) =>
            {
                //await SaveMessage(msg);
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
            try
            {
                using (var context = GetContext())
                {
                    if (token.IsCancellationRequested) return -1;
                    var c = await context.Commands.FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
                    if (c is null)
                    {
                        c = cmd;
                        await context.Commands.AddAsync(c);
                    }
                    else
                    {
                        c.SequenceId = cmd.SequenceId;
                        c = cmd;
                    }
                    if (token.IsCancellationRequested) return -1;
                    await context.SaveChangesAsync();
                    _tokenCache.TryRemove(cmd.RefId, out _);
                }
                return 1;

            }
            catch (Exception ex)
            {
                _logger?.LogError("Could not save {0} {1}", cmd.GetType().FullName, cmd.RefId);
                _logger?.LogError(ex.Message);
                _logger?.LogError(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException.Message);
                    _logger?.LogError(ex.InnerException.StackTrace);
                }
                throw;
            }
        }
        private async Task<int> SaveEvent(
            BaseEvent evt,
            CancellationToken token)
        {

            try
            {
                using (var context = GetContext())
                {
                    if (token.IsCancellationRequested) return -1;
                    var c = await context.Events.FirstOrDefaultAsync(x => x.RefId == evt.RefId);
                    if (c is null)
                    {
                        c = evt;
                        await context.Events.AddAsync(c);
                    }
                    else
                    {
                        c.SequenceId = evt.SequenceId;
                        c = evt;
                    }

                    if (token.IsCancellationRequested) return -1;
                    await context.SaveChangesAsync();
                    _tokenCache.TryRemove(evt.RefId, out _);
                }
                return 2;

            }
            catch (Exception ex)
            {
                _logger?.LogError("Could not save {0} {1}", evt.GetType().FullName, evt.RefId);
                _logger?.LogError(ex.Message);
                _logger?.LogError(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException.Message);
                    _logger?.LogError(ex.InnerException.StackTrace);
                }
                throw;
            }
        }
        private async Task<int> SaveQuery(
            BaseQuery query,
            CancellationToken token)
        {

            try
            {
                using (var context = GetContext())
                {
                    if (token.IsCancellationRequested) return -1;
                    var c = await context.Queries.FirstOrDefaultAsync(x => x.RefId == query.RefId);
                    if (c is null)
                    {
                        c = query;
                        await context.Queries.AddAsync(c);
                    }
                    else
                    {
                        c.SequenceId = query.SequenceId;
                        c = query;
                    }

                    if (token.IsCancellationRequested) return -1;
                    _tokenCache.TryRemove(query.RefId, out _);
                    await context.SaveChangesAsync();
                }
                return 3;

            }
            catch (Exception ex)
            {
                _logger?.LogError("Could not save {0} {1}", query.GetType().FullName, query.RefId);
                _logger?.LogError(ex.Message);
                _logger?.LogError(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException.Message);
                    _logger?.LogError(ex.InnerException.StackTrace);
                }
                throw;
            }
        }
        private async Task<int> SaveResponse(
            BaseResponse resp,
            CancellationToken token)
        {
            try
            {
                using (var context = GetContext())
                {
                    if (token.IsCancellationRequested) return -1;
                    var c = await context.Responses.FirstOrDefaultAsync(x => x.RefId == resp.RefId);
                    if (c is null)
                    {
                        c = resp;
                        await context.Responses.AddAsync(c);
                    }
                    else
                    {
                        c.SequenceId = resp.SequenceId;
                        c = resp;
                    }

                    if (token.IsCancellationRequested) return -1;
                    _tokenCache.TryRemove(resp.RefId, out _);
                    await context.SaveChangesAsync();
                }
                return 4;

            }
            catch (Exception ex)
            {
                _logger?.LogError("Could not save {0} {1}", resp.GetType().FullName, resp.RefId);
                _logger?.LogError(ex.Message);
                _logger?.LogError(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger?.LogError(ex.InnerException.Message);
                    _logger?.LogError(ex.InnerException.StackTrace);
                }
                throw;
            }
        }
    }

}
