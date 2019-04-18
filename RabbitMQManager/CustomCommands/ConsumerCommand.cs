using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using ArchPM.Core.Notifications;
using RabbitMQManager.Infrastructure;
using RabbitMQManager.Infrastructure.Commands;
// ReSharper disable All

namespace RabbitMQManager.CustomCommands
{
    internal class ConsumerCommand : RabbitMqBaseCommand
    {
        public ConsumerCommand() : base()
        {
            this.RequiredArguments.Add(new RequiredCommandArgument() { Name = nameof(QueueName), Value = null });

            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(OutputFolder), Value = "", Description = "if defined, output will be recorded into the folder" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(MessageType), Value = "none", Description = "ax, sales, factory, d365, none" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(ExchangeType), Value = "direct" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(OutputFileNameFormat), Value = "SO_{0}.txt" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(Exchange), Value = null, Description = "name of the exchange" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(RoutingKey), Value = null, Description = "the routing key" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(DeadLetterExchange), Value = null, Description = "deadletter exchange name" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(ShowHeaders), SingleType = true, Description = "if you are consuming deadletter, it is a good to use this property" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(NoAutoAck), SingleType = true, Description = "flag for autoack set to false in queue" });
        }

        public override string Name => "consumer";
        protected override bool StaysAlive => true;

        //requireds
        internal string QueueName => this.RequiredArguments.First(p => p.Name == nameof(QueueName)).GetValueAsStringIfExist();

        //optionals
        internal string OutputFolder => this.OptionalArguments.First(p => p.Name == nameof(OutputFolder)).GetValueAsStringIfExist();
        internal string MessageType => this.OptionalArguments.First(p => p.Name == nameof(MessageType)).GetValueAsStringIfExist();
        internal string ExchangeType => this.OptionalArguments.First(p => p.Name == nameof(ExchangeType)).GetValueAsStringIfExist();
        internal string OutputFileNameFormat => this.OptionalArguments.First(p => p.Name == nameof(OutputFileNameFormat)).GetValueAsStringIfExist();
        internal string Exchange => this.OptionalArguments.First(p => p.Name == nameof(Exchange)).GetValueAsStringIfExist();
        internal string RoutingKey => this.OptionalArguments.First(p => p.Name == nameof(RoutingKey)).GetValueAsStringIfExist();
        internal string DeadLetterExchange => this.OptionalArguments.First(p => p.Name == nameof(DeadLetterExchange)).GetValueAsStringIfExist();
        internal Boolean ShowHeaders => this.OptionalArguments.First(p => p.Name == nameof(ShowHeaders)).IsUsed;
        internal Boolean NoAutoAck  => this.OptionalArguments.First(p => p.Name == nameof(NoAutoAck)).IsUsed;

        protected override void ExecuteCommand()
        {
            if (!String.IsNullOrEmpty(OutputFolder) && !Directory.Exists(OutputFolder))
            {
                throw new Exception($"there is no such a folder {OutputFolder}");
            }

            this.Consume(ea =>
            {

                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var uniqueId = Guid.NewGuid().ToString();

                    dynamic entity = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(message);
                    if (entity != null)
                    {
                        var entityDictionary = (IDictionary<string, object>)entity;
                        uniqueId = entity[MessageType];
                    }
                    //switch (MessageType)
                    //{
                    //    case "ax":
                    //        orderNumber = entity.SalesOrderHeader_SALESID;
                    //        break;
                    //    case "sales":
                    //        orderNumber = entity.Order.Id;
                    //        break;
                    //    case "factory":
                    //        eventTimestamp = entity.FactoryOrder.EventTimestamp;
                    //        orderNumber = entity.FactoryOrder.Id;
                    //        break;
                    //    case "d365":
                    //        orderNumber = entity.foreignId;
                    //        break;
                    //    default:
                    //        break;
                    //}
                    


                    if (!String.IsNullOrEmpty(OutputFolder))
                    {
                        //create file
                        var fileName = string.Format(OutputFileNameFormat, uniqueId);
                        var filePath = Path.Combine(OutputFolder, fileName);
                        FileManager.CreateFileWithData(filePath, message);
                    }
                    Notification.Notify($"{uniqueId}");
                    if (ShowHeaders)
                    {
                        foreach (var item in ea.BasicProperties.Headers)
                        {
                            String value = item.Value?.ToString();
                            if (item.Value?.GetType() == typeof(byte[]))
                            {
                                value = Encoding.UTF8.GetString((byte[])item.Value);
                            }
                            Notification.Notify($"{item.Key} : {value}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Notification.Notify(ex, NotifyTo.CONSOLE);
                }

            });


        }

    }
}
