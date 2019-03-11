using System;
using Huffelpuff.Plugins;

namespace Huffelpuff.Commands
{
    public class PluginCommand : MarshalByRefObject
    {
        public string ExportedCommand { get; }

        public string HelpText { get; }

        public string HandlerName { get; }

        public object Owner { get; }

        public string SourcePlugin { get; }


        /// <summary>
        /// A commandlet represents the abstract idea of the typical IRC command represented with a start-character (!) and a string for identify. All the parsing will be done
        /// by the bot, you only get exactly the events you registered to. For example: you can register an event which is only thrown if used in a private message by certain users,
        /// or in a certain channel.
        /// </summary>
        /// <param name="command">the command string including initial character. like "!example" </param>
        /// <param name="helpText">A help for this certain command which should be displayed by the !help command</param>
        /// <param name="handler">The name of the method which should be called, can be private</param>
        /// <param name="plugin">this (the class where this command is provided)</param>
        public PluginCommand(string command, string helpText, EventHandler<CommandEventArgs> handler, AbstractPlugin plugin)
        {
            ExportedCommand = command;
            HelpText = helpText;

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