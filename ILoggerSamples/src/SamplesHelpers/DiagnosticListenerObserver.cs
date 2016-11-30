using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SamplesHelpers
{
    public class DiagnosticListenersObserver : IObserver<DiagnosticListener>
    {
        private readonly IDictionary<string, IObserver<KeyValuePair<string, object>>> observers;

        public DiagnosticListenersObserver(IDictionary<string, IObserver<KeyValuePair<string, object>>> observers)
        {
            this.observers = observers;
        }

        public void OnNext(DiagnosticListener value)
        {
            if (observers.ContainsKey(value.Name))
                value.Subscribe(observers[value.Name]);
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
