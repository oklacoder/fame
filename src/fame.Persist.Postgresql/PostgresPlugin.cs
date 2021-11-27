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
using System.Linq;

namespace fame.Persist.Postgresql
{

    public class PostgresPlugin :
        IFamePlugin
    {
        private PostgresPluginConfig _config = null;
        public bool? IsConfigured => _config is not null;
        public bool? IsProcessing => 
            commandQueueIsProcessing || 
            eventQueueIsProcessing || 
            queryQueueIsProcessing || 
            responseQueueIsProcessing;

        public int? QueuedMessages =>
            new[]
            {
                _commandQueue?.Count ?? 0,
                _eventQueue?.Count ?? 0,
                _queryQueue?.Count ?? 0,
                _responseQueue?.Count ?? 0,
            }.Sum();

        private ILogger<PostgresPlugin> _logger;
                
        ConcurrentQueue<BaseCommand> _commandQueue;
        bool commandQueueIsProcessing = false;
        ConcurrentQueue<BaseEvent> _eventQueue;
        bool eventQueueIsProcessing = false;
        ConcurrentQueue<BaseQuery> _queryQueue;
        bool queryQueueIsProcessing = false;
        ConcurrentQueue<BaseResponse> _responseQueue;
        bool responseQueueIsProcessing = false;

        private async Task<int> QueueCommand(BaseCommand command)
        {
            _commandQueue.Enqueue(command);
            if (commandQueueIsProcessing is not true)
                await ProcessCommandQueue();
            return 0;
        }
        private async Task<int> ProcessCommandQueue()
        {
            commandQueueIsProcessing = true;
            while (_commandQueue.TryDequeue(out var cmd))
            {
                await SaveCommand(cmd);
            }
            commandQueueIsProcessing = false;
            return 0;
        }
        
        private async Task<int> QueueEvent(BaseEvent evt)
        {
            _eventQueue.Enqueue(evt);
            if (eventQueueIsProcessing is not true)
                await ProcessEventQueue();
            return 0;
        }
        private async Task<int> ProcessEventQueue()
        {
            eventQueueIsProcessing = true;
            while (_eventQueue.TryDequeue(out var evt))
            {
                await SaveEvent(evt);
            }
            eventQueueIsProcessing = false;
            return 0;
        }

        private async Task<int> QueueQuery(BaseQuery query)
        {
            _queryQueue.Enqueue(query);
            if (queryQueueIsProcessing is not true)
                await ProcessQueryQueue();
            return 0;
        }
        private async Task<int> ProcessQueryQueue()
        {
            queryQueueIsProcessing = true;
            while (_queryQueue.TryDequeue(out var query))
            {
                await SaveQuery(query);
            }
            queryQueueIsProcessing = false;
            return 0;
        }

        private async Task<int> QueueResponse(BaseResponse response)
        {
            _responseQueue.Enqueue(response);
            if (responseQueueIsProcessing is not true)
                await ProcessResponseQueue();
            return 0;
        }
        private async Task<int> ProcessResponseQueue()
        {
            responseQueueIsProcessing = true;
            while(_responseQueue.TryDequeue(out var response))
            {
                await SaveResponse(response);
            }
            responseQueueIsProcessing = false;
            return 0;
        }

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

            _commandQueue = new ConcurrentQueue<BaseCommand>();
            _eventQueue = new ConcurrentQueue<BaseEvent>(); 
            _queryQueue = new ConcurrentQueue<BaseQuery>();
            _responseQueue = new ConcurrentQueue<BaseResponse>();
        }

        public void Enroll(IOperator target)
        {
            if (IsConfigured is not true)
                throw new InvalidOperationException($"Cannot enroll an operator in a plugin ({nameof(PostgresPlugin)}) that has not been configured.");

            target.HandleStarted += async (object target, IMessage msg) =>
            {//this is good - it forces the db to generate a SequenceId when the message is first seen
                await SaveMessage(msg);
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

            _ = msg switch
            {
                BaseCommand c => await QueueCommand(c),
                BaseEvent e => await QueueEvent(e),
                BaseQuery q => await QueueQuery(q),
                BaseResponse r => await QueueResponse(r),
                _ => 0
            };
        }

        private async Task<int> SaveCommand(
            BaseCommand cmd)
        {
            try
            {
                using (var context = GetContext())
                {
                    var c = await context.Commands
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.RefId == cmd.RefId);
                    if (c is null)
                    {
                        await context.Commands.AddAsync(cmd);
                    }
                    else
                    {
                        cmd.SequenceId = c.SequenceId;
                        context.Commands.Update(cmd);
                    }
                    await context.SaveChangesAsync();
                }

                var str = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
                return 1;

            }
            catch (Exception ex)
            {
                var str2 = Newtonsoft.Json.JsonConvert.SerializeObject(cmd);
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
            BaseEvent evt)
        {

            try
            {
                using (var context = GetContext())
                {
                    var c = await context.Events
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.RefId == evt.RefId);
                    if (c is null)
                    {
                        await context.Events.AddAsync(evt);
                    }
                    else
                    {
                        evt.SequenceId = c.SequenceId;
                        context.Events.Update(evt);
                    }

                    await context.SaveChangesAsync();
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
            BaseQuery query)
        {

            try
            {
                using (var context = GetContext())
                {
                    var c = await context.Queries
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.RefId == query.RefId);
                    if (c is null)
                    {
                        await context.Queries.AddAsync(query);
                    }
                    else
                    {
                        query.SequenceId = c.SequenceId;
                        context.Queries.Update(query);
                    }

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
            BaseResponse resp)
        {
            try
            {
                using (var context = GetContext())
                {
                    var c = await context.Responses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.RefId == resp.RefId);
                    if (c is null)
                    {
                        await context.Responses.AddAsync(resp);
                    }
                    else
                    {
                        resp.SequenceId = c.SequenceId;
                        context.Responses.Update(resp);
                    }

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
