using System.Collections.Generic;
using System.IO;
using System.Linq;
using RabbitMQManager.Infrastructure.Commands;
// ReSharper disable All

namespace RabbitMQManager.CustomCommands
{
    public class ProducerCommand : RabbitMqBaseCommand
    {
        public ProducerCommand() : base()
        {
            this.RequiredArguments.Add(new RequiredCommandArgument() { Name = nameof(Data), Value = null, Description= "can be file path,folder path or raw text according to GetDataAs" });
            this.RequiredArguments.Add(new RequiredCommandArgument() { Name = nameof(Exchange), Value = null, Description = "name of the exchange" });
            this.RequiredArguments.Add(new RequiredCommandArgument() { Name = nameof(RoutingKey), Value = null });


            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(ExchangeType), Value = "direct", Description="direct, fanout" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(DeliveryMode), Value = 1, Description= "Non-persistent (1) or persistent (2)" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(UseDataAsText), IsFlag = true, Description= "when you send data in the command, use it" });
        }

        public override string Name => "producer";

        internal string Data => this.RequiredArguments.First(p => p.Name == nameof(Data)).GetValueAsStringIfExist();
        internal string Exchange => this.RequiredArguments.First(p => p.Name == nameof(Exchange)).GetValueAsStringIfExist();
        internal string RoutingKey => this.RequiredArguments.First(p => p.Name == nameof(RoutingKey)).GetValueAsStringIfExist();
        internal string ExchangeType => this.OptionalArguments.First(p => p.Name == nameof(ExchangeType)).GetValueAsStringIfExist();
        internal int DeliveryMode => this.OptionalArguments.First(p => p.Name == nameof(DeliveryMode)).GetValueAsInt32IfExistAndUsed();
        internal bool UseDataAsText => this.OptionalArguments.First(p => p.Name == nameof(UseDataAsText)).IsUsed;


        protected override void ExecuteCommand()
        {
            var fileNames = new List<string>();

            if (UseDataAsText)
            {
                this.Publish(Data);
            }
            else
            {
                var attr = File.GetAttributes(this.Data);
                if (attr.HasFlag(FileAttributes.Directory))
                    fileNames = Directory.GetFiles(this.Data).ToList();
                else
                    fileNames.Add(Data);

                fileNames.ForEach(
                    p =>
                    {
                        //read file 
                        var json = File.ReadAllText(p);
                        //send it to the queue
                        this.Publish(json);

                        Notification.Notify($"{p} published from {Exchange}");
                    }
                );

            }

        }

    }
}
