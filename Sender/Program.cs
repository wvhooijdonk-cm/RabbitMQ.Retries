using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sender {
    class Program {
        public static void Main(string[] args) {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchange: "work_exchange", type: "fanout");

                var properties = channel.CreateBasicProperties();
                //properties.Expiration = "1000";
                for (int i = 0; i < 100; i++) {
                    var message = $"Test message ({i})";
                    channel.BasicPublish(exchange: "work_exchange",
                                         routingKey: "",
                                         basicProperties: properties,
                                         body: Encoding.UTF8.GetBytes(message));
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }

        private static string GetMessage(string[] args) {
            return ((args.Length > 0)
                   ? string.Join(" ", args)
                   : "info: Hello World!");
        }
    }
}
