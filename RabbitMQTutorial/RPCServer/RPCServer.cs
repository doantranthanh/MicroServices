using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPCServer
{
    public class RPCServer
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false,
                        arguments: null);

                    channel.BasicQos(prefetchSize:0, prefetchCount:1,global:false);

                    var consumer = new EventingBasicConsumer(channel);

                    channel.BasicConsume(queue: "rpc_queue", noAck: false, consumer: consumer);

                    Console.WriteLine(" [x] Awaiting RPC requests");

                    while (true)
                    {
                        string response = null;

                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body;
                            var props = ea.BasicProperties;
                            var replyProps = channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                var message = Encoding.UTF8.GetString(body);
                                int n = int.Parse(message);
                                Console.WriteLine(" [.] fib ({0})", message);
                                response = Fib(n).ToString();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(" [.] " + ex.Message);
                                response = "";
                            }
                            finally
                            {
                                var responseBytes = Encoding.UTF8.GetBytes(response);
                                channel.BasicPublish(exchange:"",routingKey:props.ReplyTo,basicProperties:replyProps,body:responseBytes);
                                channel.BasicAck(deliveryTag:ea.DeliveryTag,multiple:false);
                            }
                        };
                    }
                }
            }
        }

        private static int Fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return Fib(n - 1) + Fib(n - 2);
        }
    }
}
