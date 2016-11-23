using System;
using System.Diagnostics.Tracing;
using Newtonsoft.Json;

namespace EventSourceSample
{
    [EventSource(Name = "Microsoft-Diagnostics-Correlation-DiagnosticSource")]
    public class MyEventSource : EventSource
    {
        public static MyEventSource Log = new MyEventSource();

        public void RequestStart(string method, string path, string correlationId, string requestId)
        {
            WriteEvent(1, method, path, correlationId, requestId);
        }

        public void RequestStop(string status)
        {
            WriteEvent(2, status);
        }

        public void DependencyStart(string method, string path, string correlationId, string requestId, string childRequestId)
        {
            WriteEvent(3, method, path, correlationId, requestId, childRequestId);
        }

        public void DependencyStop(string status)
        {
            WriteEvent(4, status);
        }
    }

    public class MyEventListener : EventListener
    {
        public static JsonSerializerSettings settings = new JsonSerializerSettings { Formatting = Formatting.Indented };

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-Diagnostics-Correlation-DiagnosticSource")
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            try
            {
                Console.WriteLine(JsonConvert.SerializeObject(eventData, settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
