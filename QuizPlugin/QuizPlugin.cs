/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 11.01.2009
 * Zeit: 12:31
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections.Generic;

using Huffelpuff;
using Huffelpuff.ComplexPlugins;

using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of QuizPlugin.
    /// </summary>
    public class QuizPlugin : AbstractPlugin
    {
        public QuizPlugin(IrcBot botInstance) : 
            base(botInstance) {}
        
        public override string Name {
            get {
                return "Quiz Bot";
            }
        }
        
        public override void Activate()
        {
            throw new NotImplementedException();
        }
        
        public override void Deactivate()
        {
            throw new NotImplementedException();
        }
        
        public override string AboutHelp()
        {
            throw new NotImplementedException();
        }
    }
}