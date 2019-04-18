using System;
using System.Collections.Generic;
using System.Text;
using ArchPM.Core;
using ArchPM.Core.Extensions;
using ArchPM.Core.Notifications;

namespace RabbitMQManager.Infrastructure.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Required Arguments to execute to command
        /// </summary>
        public List<RequiredCommandArgument> RequiredArguments { get; } = new List<RequiredCommandArgument>();

        public List<OptionalCommandArgument> OptionalArguments { get; } = new List<OptionalCommandArgument>();

        /// <summary>
        /// 
        /// </summary>
        public virtual INotification Notification { get; set; } = new NullNotification();

        /// <summary>
        /// Name of the command
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <returns>0 if true otherwise 1</returns>
        protected abstract void ExecuteCommand();

        public string CommandPrefix { get; set; } = "-";
        public string ArgumentPrefix { get; set; } = "--";
        protected virtual bool StaysAlive { get; } = false;

        public virtual int Execute()
        {
            var result = ReturnResults.Success.GetValue();
            try
            {
                ValidateRequiredArguments();
                ExecuteCommand();
                if (StaysAlive)
                {
                    Notification.Notify($"[{GetType().Name}]: Stays Alive");
                    Console.Read();
                }
                else
                {
                    Notification.Notify($"[{GetType().Name}]: Execute: Successfully!");
                }

            }
            catch (Exception ex)
            {
                result = ReturnResults.Fail.GetValue();
                Notification.Notify(new Exception($"[{GetType().Name}]: Execute: Failed!", ex), NotifyTo.CONSOLE);
                Console.Read();
            }

            return result;
        }

        protected virtual void ValidateRequiredArguments()
        {
            RequiredArguments.ForEach(p =>
            {
                p.Value.ThrowExceptionIfNull($"[{GetType().Name}]: {p.Name} must be entered!");

            });
        }

        public virtual void Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"[{GetType().Name}] {CommandPrefix}{Name}]");
            if (RequiredArguments.Count > 0)
            {
                sb.AppendLine($"Required");
            }
            foreach (var item in RequiredArguments)
            {
                sb.AppendFormat("\t\t[{0}{1} '{2}']", ArgumentPrefix, item.Name, item.Value ?? "");
                if (!String.IsNullOrEmpty(item.Description))
                {
                    sb.AppendFormat("({0})", item.Description);
                }
                sb.Append("\r\n");
            }

            if (OptionalArguments.Count > 0)
            {
                sb.AppendLine($"Optionals");
            }

            foreach (var item in OptionalArguments)
            {
                if (item.SingleType)
                {
                    sb.AppendFormat("\t\t[{0}{1}]", ArgumentPrefix, item.Name);
                }
                else
                {
                    sb.AppendFormat("\t\t[{0}{1} '{2}']", ArgumentPrefix, item.Name, item.Value ?? "");
                }

                if (!String.IsNullOrEmpty(item.Description))
                {
                    sb.AppendFormat("({0})", item.Description);
                }
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            var help = sb.ToString();
            Notification.Notify(help);
        }

    }
}
