using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RabbitMQ.Retries {
    public sealed class ConsumerScope {
        private IModel Channel;
        private string exchangeName;
        private string queueName;

        public ConsumerScope(
            IModel channel,
            string exchangeName,
            string queueName
        ) {
            this.Channel = channel;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
        }

        
       
    }
}
