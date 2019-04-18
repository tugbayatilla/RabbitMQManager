using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQManager.CustomCommands
{
    internal static class RabbitMqExtension
    {
        internal static void Publish(this ProducerCommand command, string message)
        {
            using (var connection = command.RabbitMqConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: command.Exchange, type: command.ExchangeType, durable: true);

                var body = Encoding.UTF8.GetBytes(message);
                var basicproperties = channel.CreateBasicProperties();
                basicproperties.DeliveryMode = 1;
                basicproperties.Headers = new Dictionary<string, object>();

                channel.BasicPublish(exchange: command.Exchange,
                                     routingKey: command.RoutingKey,
                                     basicProperties: basicproperties,
                                     body: body);
            }
        }

        internal static void Consume(this ConsumerCommand command, Action<BasicDeliverEventArgs> messageReceived)
        {
            var connection = command.RabbitMqConnectionFactory.CreateConnection();
            var channel = connection.CreateModel();
            {
                if (!string.IsNullOrEmpty(command.Exchange))
                {
                    channel.ExchangeDeclare(exchange: command.Exchange, type: command.ExchangeType, durable: true);
                }

                var arguments = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(command.DeadLetterExchange))
                {
                    arguments.Add("x-dead-letter-exchange", command.DeadLetterExchange);
                }
                //this queue must have same configuration otherwise it throws exception
                channel.QueueDeclare(queue: command.QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: arguments);

                if (!string.IsNullOrEmpty(command.Exchange) && !string.IsNullOrEmpty(command.RoutingKey))
                {
                    channel.QueueBind(queue: command.QueueName,
                                  exchange: command.Exchange,
                                  routingKey: command.RoutingKey);
                }

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    messageReceived(ea);
                };

                channel.BasicConsume(queue: command.QueueName,
                                     autoAck: !command.NoAutoAck,
                                     consumer: consumer);
            }
        }

    }
}
