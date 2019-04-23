using System;
using System.Linq;
using System.Reflection;
using ArchPM.Core;
using ArchPM.Core.Notifications;
using RabbitMQManager.Infrastructure.Commands;

namespace RabbitMQManager.Infrastructure
{
    public class CommandParser
    {
        private readonly INotification _notification;

        public CommandParser(INotification notification)
        {
            notification.ThrowExceptionIfNull(new ArgumentNullException(nameof(notification)));

            _notification = notification;

        }

        private const string CommandPrefix = "-";
        private const string CommandArgumentsPrefix = "--";

        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// [{nameof(CommandParser)}]: Argument:{requiredCommandWithInitializer} needs a value (like {0}:\"blabla\")to be able to execute execute command:{command.Name}
        /// or
        /// [{nameof(CommandParser)}]: Argument:{optionalCommandWithInitializer} needs a value. Command:{command.Name}
        /// </exception>
        public Command Parse(string[] args)
        {
            _notification.Notify($"[{nameof(CommandParser)}]: args parsing...", NotifyTo.FILE);
            args.ThrowExceptionIfNull(new ArgumentNullException(nameof(args)));
            //args.ThrowExceptionIf(p => p.Length == 0, new ArgumentException($"[{nameof(CommandParser)}]: Arguments missing..."));
            if(args.Length == 0)
            {
                args = new[] { "-help", "--all" };
            }


            var argsStr = string.Join(" ", args);
            _notification.Notify($"[{nameof(CommandParser)}]: {argsStr}", NotifyTo.FILE);

            //first argument starting with commandInitializer is the command
            var possibleCommandName = args.FirstOrDefault(p => p.ToLower().StartsWith(CommandPrefix));

            var currentAssembly = Assembly.GetExecutingAssembly();
            //validate args
            var commands = ArchPM.Core.Extensions.Extensions.GetProvider<Command>(currentAssembly);
            var command = commands.FirstOrDefault(p => string.Concat(CommandPrefix, p.Name).ToLower() == possibleCommandName);
            command.ThrowExceptionIfNull($"there is no such a command:{possibleCommandName}");

            // ReSharper disable once PossibleNullReferenceException
            command.Notification = _notification;
            command.ThrowExceptionIfNull(new Exception($"[{nameof(CommandParser)}]: There is no valid command:{possibleCommandName} found"));
            command.CommandPrefix = CommandPrefix;
            command.ArgumentPrefix = CommandArgumentsPrefix;
            _notification.Notify($"[{nameof(CommandParser)}]: Command: {command.Name}", NotifyTo.FILE);

            _notification.Notify($"[{nameof(CommandParser)}]: RequiredArguments: {string.Join(", ", command.RequiredArguments.Select(p => p.Name))}", NotifyTo.FILE);
            foreach (var requiredArgument in command.RequiredArguments)
            {
                var requiredCommandWithInitializer = string.Concat(command.ArgumentPrefix, requiredArgument.Name).ToLower();
                var arg = args.FirstOrDefault(p => p.ToLower() == requiredCommandWithInitializer);
                arg.ThrowExceptionIf(p => string.IsNullOrEmpty(p),
                    new Exception($"[{nameof(CommandParser)}]: Argument:{requiredCommandWithInitializer} is required to execute command:{command.Name}"));

                //find value
                var index = Array.IndexOf(args, arg);
                if (args.Length <= index + 1 || string.IsNullOrEmpty(args[index + 1]) || args[index + 1].StartsWith(CommandArgumentsPrefix))
                {
                    throw new Exception($"[{nameof(CommandParser)}]: Argument:{requiredCommandWithInitializer} needs a value (like {0}:\"blabla\")to be able to execute execute command:{command.Name}");
                }

                //command.RequiredArguments[requiredArgument.Key] = args[index + 1];
                requiredArgument.Value = args[index + 1];

            }

            _notification.Notify($"[{nameof(CommandParser)}]: OptionalArguments: {string.Join(", ", command.OptionalArguments.Select(p => p.Name))}", NotifyTo.FILE);
            foreach (var optionalArgument in command.OptionalArguments.ToList())
            {
                var optionalCommandWithInitializer = string.Concat(command.ArgumentPrefix, optionalArgument.Name).ToLowerInvariant();
                optionalArgument.IsUsed = args.Any(p => p.ToLowerInvariant() == optionalCommandWithInitializer);
                //not in argument continue
                if (!optionalArgument.IsUsed)
                {
                    continue;
                }

                //dont need value
                if (optionalArgument.IsFlag)
                {
                    continue;
                }

                //find value
                var index = args.ToList().FindIndex(p => p.ToLowerInvariant() == optionalCommandWithInitializer);
                if (args.Length <= index + 1 || string.IsNullOrEmpty(args[index + 1]) || args[index + 1].StartsWith(CommandArgumentsPrefix))
                {
                    throw new Exception($"[{nameof(CommandParser)}]: Argument:{optionalCommandWithInitializer} needs a value. Command:{command.Name}");
                }
                optionalArgument.Value = args[index + 1];
            }

            _notification.Notify($"[{nameof(CommandParser)}]: args parsing completed.", NotifyTo.FILE);

            return command;
        }
    }
}
