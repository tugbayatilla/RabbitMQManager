using System.Linq;
using RabbitMQ.Client;
using RabbitMQManager.Infrastructure.Commands;
// ReSharper disable All

namespace RabbitMQManager.CustomCommands
{
    public abstract class RabbitMqBaseCommand : Command
    {
        protected RabbitMqBaseCommand() : base()
        {
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(HostName), Value = "localhost" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(Port), Value = 5672 });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(UserName), Value = "guest" });
            this.OptionalArguments.Add(new OptionalCommandArgument() { Name = nameof(Password), Value = "guest" });
        }

        public override string Name => "";

        protected internal string HostName => this.OptionalArguments.First(p => p.Name == nameof(HostName)).GetValueAsStringIfExist();
        protected internal int Port => this.OptionalArguments.First(p => p.Name == nameof(Port)).GetValueAsInt32IfExist();
        protected internal string UserName => this.OptionalArguments.First(p => p.Name == nameof(UserName)).GetValueAsStringIfExist();
        protected internal string Password => this.OptionalArguments.First(p => p.Name == nameof(Password)).GetValueAsStringIfExist();

        protected internal ConnectionFactory RabbitMqConnectionFactory => new ConnectionFactory() { HostName = HostName, Port = Port, UserName = UserName, Password = Password };

    }
}
