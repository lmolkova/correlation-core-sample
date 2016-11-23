using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Correlation.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Nest;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SamplesHelpers
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddElasicSearch(this ILoggerFactory factory)
        {
            factory.AddProvider(new ElasticSerachProvider());
            return factory;
        }
    }

    public class ElasticSerachProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ElastiSearchLogger(categoryName);
        }
    }

    public class ElastiSearchLogger : ILogger
    {
        private readonly ElasticClient client;
        private const string IndexName = "myindex";
        private readonly string categoryName;
        public ElastiSearchLogger(string categoryName)
        {
            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node);
            client = new ElasticClient(settings);
            this.categoryName = categoryName;
        }

        private Scope<IDictionary<string,object>> scope = null;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var document = new Dictionary<string,object>
            {
                ["Message"] = formatter(state, exception),
                ["LogLevel"] = logLevel,
                ["Exception"] = exception,
                ["EventId"] = eventId,
                ["Timestamp"] = DateTime.UtcNow,
                ["CategoryName"] = categoryName
            };
            if (scope?.State != null)
                document["Context"] = scope?.State;
            else
            {
                var ctx = ContextResolver.GetRequestContext<CorrelationContext>();
                if (ctx != null)
                    document["Context"] = ctx;
            }
            var formattedState = state as FormattedLogValues;
            if (formattedState != null)
                for (int i = 0; i < formattedState.Count - 1; i++)
                    document.Add(formattedState[i].Key, formattedState[i].Value);

            client.Index(document, idx => idx.Index(IndexName));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (!(state is IDictionary<string, object>))
                return new Scope<TState>(state);

            scope = new Scope<IDictionary<string, object>>(state as IDictionary<string,object>);
            return scope;
        }

        private class Scope<TState> : IDisposable
        {
            public TState State { get; private set; }

            public Scope(TState state)
            {
                this.State = state;
            }

            public void Dispose()
            {
                var disposable = State as IDisposable;
                disposable?.Dispose();
                State = default(TState);
            }
        }
    }

    public static class ElasticSearchLogger
    {
        public static IDisposable BeginScope<TState>(this ILogger logger, TState state)
        {
            return logger.BeginScope(state);
        }
    }
}
