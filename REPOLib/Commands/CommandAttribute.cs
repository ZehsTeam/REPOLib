using System;

namespace REPOLib.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class REPOLibCommandExecutionAttribute : Attribute
    {
        public bool RequiresDeveloperMode { get; private set; }
        public bool EnabledByDefault { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public REPOLibCommandExecutionAttribute(string name=null, string description=null, bool enabledByDefault=true, bool requiresDeveloperMode=false)
        {
            RequiresDeveloperMode = requiresDeveloperMode;
            EnabledByDefault = enabledByDefault;
            Name = name ?? "";
            Description = description ?? "";
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
