using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Wikiled.Console.Arguments
{
    public static class CommandExtension
    {
        public static string GetName(this ICommand command)
        {
            string typeName = command.GetType().Name;
            if (typeName.Contains("Command"))
            {
                return typeName.Remove(typeName.LastIndexOf("Command", StringComparison.Ordinal));
            }

            return typeName;
        }
    }
}
