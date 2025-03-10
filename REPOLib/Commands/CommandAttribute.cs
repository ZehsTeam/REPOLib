using System;

namespace REPOLib.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CommandExecutionAttribute : Attribute
    {
        public bool RequiresDeveloperMode { get; private set; }

        public CommandExecutionAttribute(bool requiresDeveloperMode=false)
        {
            RequiresDeveloperMode = requiresDeveloperMode;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CommandInitializerAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class  CommandAliasAttribute : Attribute
    {
        public string Alias { get; private set; }

        public CommandAliasAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
}
