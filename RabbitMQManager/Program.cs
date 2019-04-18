using ArchPM.Core.Notifications;
using ArchPM.Core.Notifications.Notifiers;
using System;
using RabbitMQManager.Infrastructure;
// ReSharper disable All

namespace RabbitMQManager
{
    class Program
    {
        private static void Main(string[] args)
        {
            INotification notification = new NotificationManager();
            notification.RegisterNotifier(NotifyTo.CONSOLE, new ConsoleNotifier());
            notification.RegisterNotifier(NotifyTo.CONSOLE, new FileNotifier());

            var commandParser = new CommandParser(notification);

            var kernel = new Kernel(notification, commandParser);
            Environment.ExitCode = kernel.Run(args);

            if(Environment.ExitCode == 666)
                Console.Read();
        }
    }
}
