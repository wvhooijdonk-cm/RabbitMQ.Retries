using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RabbitMQ.Retries {

    /// <summary>
    /// A consumer that handles retries when an exception occurs.
    /// Use IModel.BasicAck() when the message was processed succesfully.
    /// Note: auto acknowledgement is not supported. 
    /// </summary>
    public class RetryConsumer : EventingBasicConsumer {
        public new event EventHandler<BasicDeliverEventArgs> Received;
        private readonly int attemptsPerLayer;
        private readonly int numberOfLayers;
        private IModel channel;
        private string exchangeName;
        private string queueName;

        public RetryConsumer(
            IModel channel, 
            string mainExchangeName, 
            string mainQueueName, 
            int numberOfLayers = 3, 
            int attemptsPerLayer = 3
        ) : base(channel) {
            //TODO: Add validation of input
            this.channel = channel;
            this.exchangeName = mainExchangeName;
            this.queueName= mainQueueName;
            this.attemptsPerLayer = attemptsPerLayer;
            this.numberOfLayers = numberOfLayers;
            base.Received += (sender, eventArgs) => receive(sender, eventArgs);
        }

        private void receive(object sender, BasicDeliverEventArgs eventArgs) {
            if (Received == null)
                return;
            try {
                Received(sender, eventArgs);
            } catch (Exception ex) {
                if (CanRetry(eventArgs)) {
                    Trace.TraceWarning($"An exception is detected: queueing message for retry. Exception: {ex.Message}");
                    Ack(eventArgs);
                    Retry(eventArgs);
                }
                else {
                    Trace.TraceWarning($"An exception (see below) is detected: rejecting message. Exception: {ex.Message}");
                    Reject(eventArgs);
                }
            }
        }

        private static string CreateRetryQueueName(string queueName, int delayInMs) {
            return $"{queueName}.retry.{delayInMs}";
        }
        private static string CreateRetryExchangeName(string exchangeName) {
            return $"{exchangeName}.retry";
        }

        internal void RetryMessage(IBasicProperties properties, BasicDeliverEventArgs eventArgs, int expiration) {
            var retryExchangeName = CreateRetryExchangeName(exchangeName);
            channel.ExchangeDeclare(
                retryExchangeName,
                "fanout",
                durable: true,
                autoDelete: true,
                arguments: new Dictionary<string, object>{
                    { "x-expires", expiration * 2 }
                }
            );
            var retryQueueName = CreateRetryQueueName(queueName, expiration);
            channel.QueueDeclare(retryQueueName, true, false, true, new Dictionary<string, object> {
                { "x-dead-letter-exchange", exchangeName },
                { "x-expires", expiration * 2 }
            });
            channel.QueueBind(
                queue: retryQueueName,
                exchange: retryExchangeName,
                routingKey: "");
            Debug.WriteLine($"Retry {eventArgs.GetRetryCount()}/{MaxRetries}. Expires in {expiration}ms");
            channel.BasicPublish("", retryQueueName, properties, eventArgs.Body);
        }

        internal void Ack(BasicDeliverEventArgs eventArgs) {
            //NOTE: This fails when BasicConsume is done with autoAck=true. Any cure
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }

        internal void Reject(BasicDeliverEventArgs eventArgs) {
            channel.BasicReject(eventArgs.DeliveryTag, false);
        }

        private int MaxRetries {
            get {
                return numberOfLayers * attemptsPerLayer;
            }
        }

        private bool CanRetry(BasicDeliverEventArgs eventArgs) {
            return eventArgs.GetRetryCount() < MaxRetries;
        }
        
        private void Retry(BasicDeliverEventArgs eventArgs) {
            var delayFactor = (eventArgs.GetRetryCount() - eventArgs.GetRetryCount() % attemptsPerLayer) / attemptsPerLayer;
            int newExpiration;
            checked {
                newExpiration = (int)Math.Truncate(eventArgs.GetRetryExpirationBase() * Math.Pow(2, delayFactor));
            }
            eventArgs.SetRetryCount(eventArgs.GetRetryCount() + 1);

            RetryMessage(
                properties: CreateBasicProperties(eventArgs, newExpiration),
                eventArgs: eventArgs,
                expiration: newExpiration
            );
        }

        private IBasicProperties CreateBasicProperties(BasicDeliverEventArgs eventArgs, int expiration) {
            var properties = channel.CreateBasicProperties();
            properties.Headers = eventArgs.GetOrInitializeHeaders();
            properties.Expiration = expiration.ToString();
            return properties;
        }
    }
}
