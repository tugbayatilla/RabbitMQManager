using ArchPM.Core;
using ArchPM.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RabbitMQManager.Infrastructure.Commands
{
    public class HelpCommand : Command
    {
        public HelpCommand() 
        {
            OptionalArguments.Add(new OptionalCommandArgument() { Name = "all", IsFlag = true });
            OptionalArguments.Add(new OptionalCommandArgument() { Name = "cmd" });
        }

        public override string Name => "help";

        private IEnumerable<Command> _definedCommands;

        private IEnumerable<Command> DefinedCommands
        {
            get
            {
                if (_definedCommands == null)
                {
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    _definedCommands = currentAssembly.GetProvider<Command>().ToList();

                    _definedCommands.ForEach(p => { p.Notification = Notification; });
                }

                return _definedCommands;
            }
        }

        protected override void ExecuteCommand()
        {
            if (OptionalArguments.First(p => p.Name == "all").IsUsed)
            {
                DefinedCommands.ForEach(p =>
                {
                    p.Help();
                });
            }
            else
            {
                var cmd = OptionalArguments.First(p => p.Name == "cmd");

                if (cmd.IsUsed)
                {
                    cmd.Value.ThrowExceptionIfNull($"[{nameof(HelpCommand)}]: {cmd.Name} value is not defined!");
                    var definedCommand = DefinedCommands.FirstOrDefault(p => string.Equals(p.Name, cmd.Value.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    definedCommand?.Help();
                }
            }
        }


    }
}
