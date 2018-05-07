using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RabbitMQ.Retries {
    public sealed class RetryScope : IDisposable {
        private static int retriesToProcessBeforeDelayIncrease = 2;
        private static int nrOfDelayLayers = 2;
        private BasicDeliverEventArgs EventArgs;
        private readonly ConsumerScope consumerScope;

        public RetryScope(
            ConsumerScope consumerScope,
            BasicDeliverEventArgs eventArgs
        ) {
            this.consumerScope = consumerScope;
            this.EventArgs = eventArgs;
        }

        public void Dispose() {
            
        }
    }
}
