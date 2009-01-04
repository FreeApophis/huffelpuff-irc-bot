/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.12.2008
 * Zeit: 13:35
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    /// <summary>
    /// Description of Commandlet.
    /// </summary>
    public class Commandlet : MarshalByRefObject
    {
        private string _Command;
        private string _HelpText;
        private IrcEventHandler _Handler;
        private object _Owner;
        private object _ACL;

        public string Command {
            get {
                return _Command;
            }
        }
            
        public string HelpText {
            get {
                return _HelpText;
            }
        }
        
        public IrcEventHandler Handler {
            get {
                return _Handler;
            }
        }
            
        public object Owner {
            get {
                return _Owner;
            }
        }
            
        public Commandlet(string command, string helptext, IrcEventHandler handler, object owner)
        {
            _Command = command;
            _HelpText = helptext;
            _Handler = handler;
            _Owner = owner;
            _ACL = null;
        }
    }
}
