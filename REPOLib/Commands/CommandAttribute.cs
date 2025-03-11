using System;

namespace REPOLib.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class REPOLibCommandExecutionAttribute : Attribute
    {
        public bool RequiresDeveloperMode { get; private set; }

        public REPOLibCommandExecutionAttribute(bool requiresDeveloperMode=false)
        {
            RequiresDeveloperMode = requiresDeveloperMode;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class REPOLibCommandInitializerAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class  REPOLibCommandAliasAttribute : Attribute
    {
        public string Alias { get; private set; }

        public REPOLibCommandAliasAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
}
