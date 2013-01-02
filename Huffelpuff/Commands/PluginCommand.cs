using System;
using Huffelpuff.Plugins;

namespace Huffelpuff.Commands
{
    public class PluginCommand : MarshalByRefObject
    {
        public string ExportedCommand { get; private set; }

        public string HelpText { get; private set; }

        public string HandlerName { get; private set; }

        public object Owner { get; private set; }

        public string SourcePlugin { get; private set; }


        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial charakter. like "!example" </param>
        /// <param name="helptext">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="plugin">this (the class where this command is provided)</param>
        public PluginCommand(string command, string helptext, EventHandler<CommandEventArgs> handler, AbstractPlugin plugin)
        {
            ExportedCommand = command;
            HelpText = helptext;

            HandlerName = handler.Method.Name;
            Owner = plugin.FullName;
            SourcePlugin = plugin.FullName;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}