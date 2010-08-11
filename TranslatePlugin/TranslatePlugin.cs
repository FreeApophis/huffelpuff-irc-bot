/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class TranslatePlugin : AbstractPlugin
    {
        public TranslatePlugin(IrcBot botInstance) : base(botInstance) { }

        private readonly Dictionary<string, string> languages = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) {
            { "af", "Afrikaans" },
            { "sq", "Albanian" },
            { "am", "Amharic" },
            { "ar", "Arabic" },
            { "hy", "Armenian" },
            { "az", "Azerbaijani" },
            { "eu", "Basque" },
            { "be", "Belarusian" },
            { "bn", "Bengali" },
            { "bh", "Bihari" },
            { "bg", "Bulgarian" },
            { "my", "Burmese" },
            { "ca", "Catalan" },
            { "chr", "Cherokee" },
            { "zh", "Chinese" },
            { "zh-CN", "Simplified Chinese" },
            { "zh-TW", "Traditional Chinese" },
            { "hr", "Croatian" },
            { "cs", "Czech" },
            { "da", "Danish" },
            { "dv", "Dhivehi" },
            { "nl", "Dutch" },
            { "en", "English" },
            { "eo", "Esperanto" },
            { "et", "Estonian" },
            { "tl", "Filipino" },
            { "fi", "Finnish" },
            { "fr", "French" },
            { "gl", "Galician" },
            { "ka", "Georgian" },
            { "de", "German" },
            { "el", "Greek" },
            { "gn", "Guarani" },
            { "gu", "Gujarati" },
            { "iw", "Hebrew" },
            { "hi", "Hindi" },
            { "hu", "Hungarian" },
            { "is", "Icelandic" },
            { "id", "Indonesian" },
            { "iu", "Inuktitut" },
            { "it", "Italian" },
            { "ja", "Japanese" },
            { "kn", "Kannada" },
            { "kk", "Kazakh" },
            { "km", "Khmer" },
            { "ko", "Korean" },
            { "ku", "Kurdish" },
            { "ky", "Kyrgyz" },
            { "lo", "Laothian" },
            { "lv", "Latvian" },
            { "lt", "Lithuanian" },
            { "mk", "Macedonian" },
            { "ms", "Malay" },
            { "ml", "Malayalam" },
            { "mt", "Maltese" },
            { "mr", "Marathi" },
            { "mn", "Mongolian" },
            { "ne", "Nepali" },
            { "no", "Norwegian" },
            { "or", "Oriya" },
            { "ps", "Pashto" },
            { "fa", "Persian" },
            { "pl", "Polish" },
            { "pt-PT", "Portuguese" },
            { "pa", "Punjabi" },
            { "ro", "Romanian" },
            { "ru", "Russian" },
            { "sa", "Sanskrit" },
            { "sr", "Serbian" },
            { "sd", "Sindhi" },
            { "si", "Sinhalese" },
            { "sk", "Slovak" },
            { "sl", "Slovenian" },
            { "es", "Spanish" },
            { "sw", "Swahili" },
            { "sv", "Swedish" },
            { "tg", "Tajik" },
            { "ta", "Tamil" },
            /*          { "tl", "Tagalog" }, == FILIPINO */
            { "te", "Telugu" },
            { "th", "Thai" },
            { "bo", "Tibetan" },
            { "tr", "Turkish" },
            { "uk", "Ukrainian" },
            { "ur", "Urdu" },
            { "uz", "Uzbek" },
            { "ug", "Uighur" },
            { "vi", "Vietnamese" },
        };

        private readonly DataTable activeTranslations = new DataTable("ActiveTranslation");

        public override string AboutHelp()
        {
            return "Translates any discussion";
        }

        public override void Init()
        {
            activeTranslations.Columns.Add(new DataColumn("Nick", typeof(string)));
            activeTranslations.Columns.Add(new DataColumn("Channel", typeof(string)));
            activeTranslations.Columns.Add(new DataColumn("Language", typeof(string)));

            base.Init();
        }

        public override void Activate()
        {


            BotEvents.OnChannelMessage += BotEvents_OnChannelMessage;

            BotEvents.OnKick += (sender, e) => EndTranslate(e.Who, e.Channel);
            BotEvents.OnPart += (sender, e) => EndTranslate(e.Who, e.Channel);
            BotEvents.OnQuit += (sender, e) => EndTranslate(e.Who, "all");
            BotEvents.OnNickChange += (sender, e) => EndTranslate(e.OldNickname, "all");

            BotMethods.AddCommand(new Commandlet("!translate", "Translates any channel conversation into your native language. Usage: !translate <language>", TranslateTrigger, this, CommandScope.Public));
            BotMethods.AddCommand(new Commandlet("!languages", "Shows supported languages.", LanguageTrigger, this));
            BotMethods.AddCommand(new Commandlet("!detect", "What language is this?", DetectTrigger, this));
            BotMethods.AddCommand(new Commandlet("!endtranslate", "Aborts any translation service for you.", EndTranslateTrigger, this));

            base.Activate();
        }

        public override void Deactivate()
        {
            BotEvents.OnChannelMessage -= BotEvents_OnChannelMessage;

            BotMethods.RemoveCommand("!translate");
            BotMethods.RemoveCommand("!languages");
            BotMethods.RemoveCommand("!detect");
            BotMethods.RemoveCommand("!endtranslate");

            base.Deactivate();
        }

        private void TranslateTrigger(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                if ((e.Data.MessageArray[1].ToLower() == "off") || (e.Data.MessageArray[1].ToLower() == "end"))
                {
                    EndTranslate(e.Data.Nick, e.Data.Channel);
                }
                if (activeTranslations.Select("(Nick='" + e.Data.Nick + "') AND (Channel='" + e.Data.Channel + "')").Length > 0)
                {
                    EndTranslate(e.Data.Nick, e.Data.Channel);
                    BotMethods.SendMessage(SendType.Notice, e.Data.Nick, "" + IrcConstants.IrcBold + IrcConstants.IrcColor + (int)IrcColors.Brown + "You requested more than one language translation in a channel: the old request was canceled.");
                }

                if (languages.ContainsKey(e.Data.MessageArray[1]))
                {
                    DataRow dr = activeTranslations.NewRow();
                    dr["Nick"] = e.Data.Nick;
                    dr["Channel"] = e.Data.Channel;
                    dr["Language"] = e.Data.MessageArray[1];
                    activeTranslations.Rows.Add(dr);
                }
                if (languages.ContainsValue(e.Data.MessageArray[1]))
                {
                    foreach (KeyValuePair<string, string> kvp in languages)
                    {
                        if (kvp.Value.ToLower() == e.Data.MessageArray[1].ToLower())
                        {
                            DataRow dr = activeTranslations.NewRow();
                            dr["Nick"] = e.Data.Nick;
                            dr["Channel"] = e.Data.Channel;
                            dr["Language"] = kvp.Key;
                            activeTranslations.Rows.Add(dr);
                            break;
                        }
                    }
                }
            }
        }

        private void EndTranslateTrigger(object sender, IrcEventArgs e)
        {
            if ((e.Data.MessageArray.Length > 1) && (e.Data.MessageArray[1] == "all"))
            {
                EndTranslate(e.Data.Nick, "all");
            }
            else
            {
                EndTranslate(e.Data.Nick, e.Data.Channel);
            }

        }

        private void EndTranslate(string nick, string channel)
        {
            var dataRows = channel == "all" ? activeTranslations.Select("(Nick='" + nick + "')") : activeTranslations.Select("(Nick='" + nick + "') AND (Channel='" + channel + "')");

            foreach (var dr in dataRows)
            {
                activeTranslations.Rows.Remove(dr);
            }
        }

        private void DetectTrigger(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length > 1)
            {
                var lang = DetectLanguage(e.Data.Message.Substring(8));
                if (languages.ContainsKey(lang))
                {
                    string natlang;
                    try
                    {
                        natlang = TranslateText("This Text was in " + languages[lang] + ".", "en", lang);
                    }
                    catch (Exception)
                    {
                        natlang = "";
                    }
                    BotMethods.SendMessage(SendType.Message, sendto, "This Text was in " + languages[lang] + " - " + natlang);
                }
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Usage to detect the language is !detect <some foreign text>");
            }
        }

        private void LanguageTrigger(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            foreach (var line in languages.Values.ToLines(400, ", ", "Languages: ", ""))
            {
                BotMethods.SendMessage(SendType.Message, sendto, line);
            }
        }

        private void BotEvents_OnChannelMessage(object sender, IrcEventArgs e)
        {
            var langs = new Dictionary<string, string>();

            if (activeTranslations.Rows.Count <= 0) return;

            var sourceLang = DetectLanguage(e.Data.Message);

            foreach (var dr in activeTranslations.Select("Channel='" + e.Data.Channel + "'", "Language ASC").Where(dr => !langs.ContainsKey((string)dr["Language"])))
            {
                langs.Add((string)dr["Language"], null);
            }
            var it = new List<string>(langs.Keys);
            foreach (string targetLang in it)
            {
                if (sourceLang == null)
                {
                    string prefix = "" + IrcConstants.IrcBold + "[" + IrcConstants.IrcColor + (int)IrcColors.LightRed + "failed:src:orig" + IrcConstants.IrcColor + "]<" + e.Data.Nick + "> " + IrcConstants.IrcNormal;
                    langs[targetLang] = prefix + e.Data.Message;
                }
                else
                {
                    try
                    {
                        string tt = TranslateText(e.Data.Message, sourceLang, targetLang);
                        if (tt == null)
                        {
                            string prefix = "" + IrcConstants.IrcBold + "[" + IrcConstants.IrcColor + (int)IrcColors.LightRed + "failed:trans:" + languages[sourceLang] + IrcConstants.IrcColor + "]<" + e.Data.Nick + "> " + IrcConstants.IrcNormal;
                            langs[targetLang] = prefix + e.Data.Message;
                        }
                        else
                        {
                            string prefix = "" + IrcConstants.IrcBold + "[" + IrcConstants.IrcColor + (int)IrcColors.Blue + languages[sourceLang] + IrcConstants.IrcColor + "]<" + e.Data.Nick + "> " + IrcConstants.IrcNormal;
                            langs[targetLang] = prefix + tt;
                        }
                    }
                    catch (Exception ex)
                    {
                        string prefix;
                        if (ex.Message == "invalid translation language pair")
                        {
                            prefix = "" + IrcConstants.IrcBold + "[" + IrcConstants.IrcColor + (int)IrcColors.LightRed + languages[sourceLang] + " -> " + languages[targetLang] + " not supported yet" + IrcConstants.IrcColor + "]<" + e.Data.Nick + "> " + IrcConstants.IrcNormal;
                        }
                        else
                        {
                            prefix = "" + IrcConstants.IrcBold + "[" + IrcConstants.IrcColor + (int)IrcColors.LightRed + "failed:" + ex.Message + IrcConstants.IrcColor + "]<" + e.Data.Nick + "> " + IrcConstants.IrcNormal;
                        }
                        langs[targetLang] = prefix + e.Data.Message;
                    }
                }
            }

            foreach (DataRow dr in activeTranslations.Select("Channel='" + e.Data.Channel + "'"))
            {
                BotMethods.SendMessage(SendType.Notice, (string)dr["Nick"], langs[(string)dr["Language"]]);
            }
        }

        private static string TranslateText(string input, string sourceLanguage, string targetLanguage)
        {
            var url = String.Format("http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q={0}&langpair={1}", input, sourceLanguage + "|" + targetLanguage);
            var webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
            var result = webClient.DownloadString(url);
            var jsonObj = JSON.JsonDecode(result);

            if (jsonObj is Hashtable)
            {
                if (((Hashtable)jsonObj)["responseData"] == null)
                {
                    throw new Exception((string)((Hashtable)jsonObj)["responseDetails"]);
                }
                return HttpUtility.HtmlDecode(((string)((Hashtable)((Hashtable)jsonObj)["responseData"])["translatedText"]));
            }
            return null;
        }

        private static string DetectLanguage(string input)
        {
            string url = String.Format("http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&q={0}", input);

            var webClient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
            var result = webClient.DownloadString(url);
            var jsonObj = JSON.JsonDecode(result);
            return jsonObj is Hashtable ? (string)((Hashtable)((Hashtable)jsonObj)["responseData"])["language"] : null;
        }
    }
}
