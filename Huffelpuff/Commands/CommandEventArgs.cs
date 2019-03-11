using System;

namespace Huffelpuff.Commands
{
    [Serializable]
    public class CommandEventArgs : EventArgs
    {
        public string Parameter { get; }

        public CommandEventArgs()
        {
            Parameter = null;
        }

        public CommandEventArgs(string parameter)
        {
            Parameter = parameter;
        }
    }
}
