using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace fame.Tests
{
    public sealed class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(_testOutputHelper, _scopeProvider, categoryName);
        }

        public void Dispose()
        {
        }
    }
    public sealed class XUnitLogger<T> : XUnitLogger, ILogger<T>
    {
        public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider)
            : base(testOutputHelper, scopeProvider, typeof(T).FullName)
        {
        }
    }
    public class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;
        private readonly LoggerExternalScopeProvider _scopeProvider;

        public static ILogger CreateLogger(ITestOutputHelper testOutputHelper) => new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");
        public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper) => new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());

        public XUnitLogger(ITestOutputHelper testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _scopeProvider = scopeProvider;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var sb = new StringBuilder();
            sb.Append(GetLogLevelString(logLevel))
              .Append(" [").Append(_categoryName).Append("] ")
              .Append(formatter(state, exception));

            if (exception != null)
            {
                sb.Append('\n').Append(exception);
            }

            // Append scopes
            _scopeProvider.ForEachScope((scope, state) =>
            {
                state.Append("\n => ");
                state.Append(scope);
            }, sb);

            _testOutputHelper.WriteLine(sb.ToString());
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
    }
    public class CommandOperatorTests
    {
        
        [Fact]
        public async void CommandOperator_HappyPath()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.True(resp.IsValid);
            Assert.True(resp.Successful);
        }

        [Fact]
        public async void CommandOperator_Invalidates()
        {
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs
            {
                IsValid = false
            };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.IsValid);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }

        [Fact]
        public async void CommandOperator_Errors()
        {
            var opr = new TestCommandOperator();
            var args = new TestCommandArgs
            {
                ShouldThrow = true
            };
            var msg = new TestCommand(args);

            var resp = await opr.SafeHandle<TestResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
        [Fact]
        public async void CommandOperator_ErrorsWhenTypeConversionFails()
        {
            var opr = new TestCommandOperator();
            var msg = new TestCommand();

            //await Assert.ThrowsAsync<InvalidCastException>(async () => await opr.SafeHandle<JunkResponse>(msg));

            var resp = await opr.SafeHandle<JunkResponse>(msg);

            Assert.NotNull(resp);
            Assert.Equal(msg.RefId, resp.SourceId);
            Assert.False(resp.Successful);
            Assert.NotEmpty(resp.Messages);
        }
    }
}
