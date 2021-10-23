using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fame
{
    public interface IMessage
    {
        public Guid RefId { get; set; }
        public DateTime DateTimeUtc { get; set; }
    }


    public interface IOperator
    {
        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        abstract Task<T> Handle<T>(IMessage cmd)
            where T : BaseResponse;
        public Task<T> SafeHandle<T>(IMessage cmd)
            where T : BaseResponse;
    }

    public abstract class BaseCommandOperator :
        IOperator
    {
        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var cmd = msg as BaseCommand;
            if (cmd is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required", 
                        nameof(msg), 
                        typeof(IOperator).FullName, 
                        typeof(BaseCommand).FullName));
            return await Handle<T>(cmd);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var cmd = msg as BaseCommand;
            if (cmd is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await SafeHandle<T>(cmd);
        }


        public abstract Task<T> Handle<T>(BaseCommand cmd)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseCommand cmd)
            where T : BaseResponse
        {
            T resp = default;
            try
            {
                HandleStarted?.Invoke(this, cmd);

                if (!cmd.Validate(out var messages))
                {
                    HandleInvalid?.Invoke(this, cmd);
                    resp = System.Text.Json.JsonSerializer.Deserialize<T>(
                        System.Text.Json.JsonSerializer.Serialize(
                            BaseResponse.Invalid(messages, cmd)));
                    return resp;
                }

                await Handle<T>(cmd);

                HandleSucceeded?.Invoke(this, cmd);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, cmd);
                throw;
            }
            finally
            {
                HandleFinished?.Invoke(this, cmd);
            }
            return resp;
        }
    }
    public abstract class BaseQueryOperator :
        IOperator
    {

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var query = msg as BaseQuery;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await Handle<T>(query);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var query = msg as BaseQuery;
            if (query is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await SafeHandle<T>(query);
        }


        public abstract Task<T> Handle<T>(BaseQuery query)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseQuery query)
            where T : BaseResponse
        {
            T resp = default;
            try
            {
                HandleStarted?.Invoke(this, query);

                await Handle<T>(query);

                HandleSucceeded?.Invoke(this, query);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, query);
                throw;
            }
            finally
            {
                HandleFinished?.Invoke(this, query);
            }
            return resp;
        }
    }

    public abstract class BaseEventOperator :
        IOperator
    {

        public event EventHandler<IMessage> HandleStarted;
        public event EventHandler<IMessage> HandleInvalid;
        public event EventHandler<IMessage> HandleSucceeded;
        public event EventHandler<IMessage> HandleFailed;
        public event EventHandler<IMessage> HandleFinished;

        async Task<T> IOperator.Handle<T>(IMessage msg)
        {
            var evt = msg as BaseEvent;
            if (evt is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await Handle<T>(evt);
        }
        async Task<T> IOperator.SafeHandle<T>(IMessage msg)
        {
            var evt = msg as BaseEvent;
            if (evt is null)
                throw new ArgumentException(
                    string.Format(
                        "Argument {0} for {1}.Handle could not be coerced to a type compatible with base type {2} as required",
                        nameof(msg),
                        typeof(IOperator).FullName,
                        typeof(BaseCommand).FullName));
            return await SafeHandle<T>(evt);
        }


        public abstract Task<T> Handle<T>(BaseEvent evt)
            where T : BaseResponse;

        public async Task<T> SafeHandle<T>(BaseEvent evt)
            where T : BaseResponse
        {
            T resp = default;
            try
            {
                HandleStarted?.Invoke(this, evt);

                await Handle<T>(evt);

                HandleSucceeded?.Invoke(this, evt);
            }
            catch (Exception ex)
            {
                HandleFailed?.Invoke(this, evt);
                throw;
            }
            finally
            {
                HandleFinished?.Invoke(this, evt);
            }
            return resp;
        }
    }
}
