using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Impl;
using RabbitMQ.Retries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiver {
    /// <summary>
    /// https://m.alphasights.com/exponential-backoff-with-rabbitmq-78386b9bec81
    /// </summary>
    class Program {
        private static void Install(string exchangeName, string queueName) {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchangeName, "fanout", durable: false);
                channel.QueueDeclare(queueName, true, false, false, null);
                //channel.ExchangeDeclare(CreateRetryExchangeName(exchangeName), "fanout", durable: true);
            }
        }
        
        public static void Main() {
            //Install("work_exchange", "work_queue");
            //return;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueBind(queue: "work_queue",
                                  exchange: "work_exchange",
                                  routingKey: "");
                
                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new RetryConsumer(channel, "work_exchange", "work_queue");
                consumer.Received += (model, ea) => {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"Received: {message}");
                    throw new NotSupportedException();
                    //channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queue: "work_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

    }
}
