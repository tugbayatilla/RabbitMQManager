using System;

namespace RabbitMQManager.Infrastructure.Commands
{
    public abstract class CommandArgument
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsUsed { get; set; }
        public string Description { get; set; }
        public string GetValueAsStringIfExist()
        {
            return Value as string;
        }

        public bool GetValueAsBooleanIfExist() => Value != null ? Convert.ToBoolean(Value) : false;

        public int GetValueAsInt32IfExist()
        {
            int.TryParse(Convert.ToString(Value), out var result);
            return result;
        }

        public int GetValueAsInt32IfExistAndUsed()
        {
            var result = default(int);
            if (IsUsed)
            {
                result = Convert.ToInt32(Value);
            }
            return result;
        }
    }

    public class OptionalCommandArgument : CommandArgument
    {
        public bool SingleType { get; set; }
    }

    public class RequiredCommandArgument : CommandArgument
    { }
}
