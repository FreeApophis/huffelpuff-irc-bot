/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 08.02.2009
 * Zeit: 01:11
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Huffelpuff;
using Huffelpuff.ComplexPlugins;

using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class TranslatePlugin : AbstractPlugin
    {
        public TranslatePlugin(IrcBot botInstance) : base(botInstance) {}
        
        
        public override string AboutHelp()
        {
            return "Translates any discussion";
        }
        
        public override void Activate()
        {
            BotEvents.OnChannelMessage += new IrcEventHandler(BotEvents_OnChannelMessage);
            
            base.Activate();
        }
        
        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= new IrcEventHandler(BotEvents_OnChannelMessage);
            
            base.Deactivate();
        }
        
        private void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            string sourceLang = DetectLanguage(e.Data.Message);
            string targetLang = "de";
            if (sourceLang!=null) {
                string translation = TranslateText(e.Data.Message, sourceLang + "|" + targetLang);
                if (translation != null) {
                    BotMethods.SendMessage(SendType.Notice, e.Data.Channel, "[" + sourceLang + "->" + targetLang + "] " + translation);
                } else {
                    BotMethods.SendMessage(SendType.Notice, e.Data.Channel, "[" + sourceLang + "->" + targetLang + " failed]" + e.Data.Message);
                }
            } else {
                BotMethods.SendMessage(SendType.Notice, e.Data.Channel, "[translation failed: no source language] " + e.Data.Message);
            }
        }
        
        public string TranslateText(string input, string languagePair)
        {
            string url = String.Format("http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q={0}&langpair={1}", input, languagePair);

            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            string result = webClient.DownloadString(url);
            object jsonObj = JSON.JsonDecode(result);
            if (jsonObj is Hashtable) {
                return ((string)((Hashtable)((Hashtable)jsonObj)["responseData"])["translatedText"]);
            } else {
                return null;
            } 
        }
        
        public string DetectLanguage(string input)
        {
            string url = String.Format("http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&q={0}", input);

            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            string result = webClient.DownloadString(url);
            object jsonObj = JSON.JsonDecode(result);
            if (jsonObj is Hashtable) {
                return ((string)((Hashtable)((Hashtable)jsonObj)["responseData"])["language"]);
            } else {
                return null;
            } 
        }
        
        
        
    }
}
