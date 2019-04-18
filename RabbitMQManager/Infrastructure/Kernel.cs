using System;
using ArchPM.Core.Enums;
using ArchPM.Core.Extensions;
using ArchPM.Core.Notifications;

namespace RabbitMQManager.Infrastructure
{
    public class Kernel
    {
        private readonly INotification _notification;
        private readonly CommandParser _commandParser;

        public Kernel(INotification notification, CommandParser commandParser)
        {
            _notification = notification;
            _commandParser = commandParser;
        }

        public int Run(string[] args)
        {
            try
            {
                _notification.Notify($"[{nameof(Kernel)}]: Started.", NotifyTo.CONSOLE,NotifyTo.FILE);

                var command = _commandParser.Parse(args);
                command.Notification = _notification;
                var result = command.Execute();
                _notification.Notify($@"[{nameof(Kernel)}]: Completed. {command.Name}:{result}:{EnumManager<ReturnResults>.GetName(result)}", NotifyTo.CONSOLE, NotifyTo.FILE);
                return result;
            }
            catch (Exception ex)
            {
                _notification.Notify(new Exception($"[{nameof(Kernel)}]: Run Failed! Use Help", ex), NotifyTo.CONSOLE, NotifyTo.FILE);
                _notification.Notify(ReturnResults.Fail.GetValueAsString(), NotifyTo.CONSOLE, NotifyTo.FILE);
                return 666;
            }

        }
    }
}
