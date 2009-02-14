/*
 *  The Seen Plugin tells you when a certain Nick was last used
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Linq;
using System.Xml;
using System.IO;

using Huffelpuff;
using Huffelpuff.SimplePlugins;

using Meebey.SmartIrc4net;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class SeenPlugin : IPlugin
    {
        
        
        public string Name {
            get {
                return Assembly.GetExecutingAssembly().FullName;
            }
        }
        
        private bool ready = false;
        public bool Ready {
            get {
                return ready;
            }
        }
        
        private bool active;
        public bool Active {
            get {
                return active;
            }
        }
        
        private IrcBot bot;
        private DataSet db;
        
        private const string filename = "seen.db";
        private const string seentable = "seen";
        private const string aliastable = "alias";
        public void Init(IrcBot botInstance)
        {
            bot = botInstance;
            // Seen: Nick, LastSeenTime, LastAction, LastMessage
            
            FileInfo fi = new FileInfo(filename);
            XmlDataDocument xdoc = new XmlDataDocument();

            if (fi.Exists) {
                xdoc.DataSet.ReadXml(filename, XmlReadMode.ReadSchema);
                db = xdoc.DataSet;
            } else {
                db = new DataSet();
            }
            if (!db.Tables.Contains(seentable)) {
                db.Tables.Add(seentable);
                db.Tables[seentable].Columns.Add("Nick", typeof(string));
                db.Tables[seentable].Columns.Add("LastSeenTime", typeof(DateTime));
                db.Tables[seentable].Columns.Add("LastAction", typeof(string));
                db.Tables[seentable].Columns.Add("LastMessage", typeof(string));
                db.Tables[seentable].Columns.Add("TimesSeen", typeof(int));
                db.Tables[seentable].Columns.Add("OnStatus", typeof(bool));
                // TODO: Add More than one channel handling
                //db.Tables[seentable].Columns.Add("Channel", typeof(string));

                db.Tables.Add(aliastable);
                db.Tables[aliastable].Columns.Add("Nick", typeof(string));
                db.Tables[aliastable].Columns.Add("Alias", typeof(string));
            }
            
            ready = true;
        }
        
        public void Activate()
        {
            bot.AddPublicCommand(new Commandlet("!seen", "The command !seen <nick> tells you when a certain nick was last seen in this channel ", seenCommand, this));
            bot.OnChannelMessage += new IrcEventHandler(messageHandler);
            bot.OnNickChange += new NickChangeEventHandler(nickChangeHandler);
            bot.OnNames += new NamesEventHandler(namesHandler);
            bot.OnJoin += new JoinEventHandler(joinHandler);
            bot.OnPart += new PartEventHandler(partHandler);
            bot.OnQuit += new QuitEventHandler(quitHandler);
            bot.OnKick += new KickEventHandler(kickHandler);
            
            foreach(DataRow dr in db.Tables[seentable].Rows) {
                dr["OnStatus"] = false;
            }
            
            foreach(string channel in PersistentMemory.GetValues("channel"))
            {
                bot.RfcNames(channel);
            }
            
            active = true;
        }
        
        public void Deactivate()
        {
            bot.RemovePublicCommand("!seen");
            bot.OnChannelMessage -= new IrcEventHandler(messageHandler);
            bot.OnNickChange -= new NickChangeEventHandler(nickChangeHandler);
            bot.OnNames -= new NamesEventHandler(namesHandler);
            bot.OnJoin -= new JoinEventHandler(joinHandler);
            bot.OnPart -= new PartEventHandler(partHandler);
            bot.OnQuit -= new QuitEventHandler(quitHandler);
            bot.OnKick -= new KickEventHandler(kickHandler);
            
            foreach(DataRow dr in db.Tables[seentable].Rows) {
                dr["OnStatus"] = false;
            }

            active = false;
        }
        
        public void DeInit()
        {
            ready = false;
        }
        
        public string AboutHelp()
        {
            return "This is the basic Last Seen Plugin";
        }
        
        private void seenCommand(object sender, IrcEventArgs e) {
            string destination = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;

            if(e.Data.MessageArray.Length > 1) {
                DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Data.MessageArray[1] + "'");
                if(datarows.Length > 0) {
                    if((bool)datarows[0]["OnStatus"]) {
                        bot.SendMessage(SendType.Notice, destination, "The nick '" + e.Data.MessageArray[1] + "' is on right now (Last Action: " + datarows[0]["LastAction"] + " (" + datarows[0]["LastSeenTime"] + ") Lastmessage: " + datarows[0]["LastMessage"] + " !seen#: " + datarows[0]["TimesSeen"] + ")");
                    } else {
                        bot.SendMessage(SendType.Notice, destination, "The nick '" + e.Data.MessageArray[1] + "' was last seen at " + datarows[0]["LastSeenTime"] + " (Last Action: " + datarows[0]["LastAction"] + " Lastmessage: " + datarows[0]["LastMessage"] + " !seen#: " + datarows[0]["TimesSeen"] + ")");
                    }
                    datarows[0]["TimesSeen"] = ((int)datarows[0]["TimesSeen"] + 1);
                } else {
                    bot.SendMessage(SendType.Notice, destination, "The nick '" + e.Data.MessageArray[1] + "' was never seen");
                }
            } else {
                bot.SendMessage(SendType.Notice, destination, "Seen " + db.Tables[seentable].Rows.Count + " unique nicknames. Use !seen <nick> for query.");
            }
            
            SaveDB();
        }
        
        
        private void nickChangeHandler(object sender, NickChangeEventArgs e) {         
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.OldNickname + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(NICK) Nick changet to: " + e.NewNickname;
                datarows[0]["OnStatus"] = false;
            }
            
            datarows = db.Tables[seentable].Select("Nick='" + e.NewNickname + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(NICK) Nick changed from: " + e.OldNickname;
                datarows[0]["OnStatus"] = true;
            } else {
                DataRow dr = db.Tables[seentable].NewRow();
                dr["Nick"] = e.NewNickname;
                dr["LastSeenTime"] = DateTime.Now;
                dr["LastAction"] = "(NICK) Nick changed from: " + e.OldNickname;
                dr["LastMessage"] = "<no message yet>";
                dr["TimesSeen"] = 0;
                dr["OnStatus"] = true;
                db.Tables[seentable].Rows.Add(dr);
            }

            datarows = db.Tables[aliastable].Select("(Nick='" + e.OldNickname + "') AND (" + " Alias='" + e.NewNickname + "')");
            if (datarows.Length == 0) {
                DataRow dr = db.Tables[aliastable].NewRow();
                dr["Nick"] = e.OldNickname;
                dr["Alias"] = e.NewNickname;
                db.Tables[aliastable].Rows.Add(dr);
            }
            
            datarows = db.Tables[aliastable].Select("(Nick='" + e.NewNickname + "') AND (" + " Alias='" + e.OldNickname + "')");
            if (datarows.Length == 0) {
                DataRow dr = db.Tables[aliastable].NewRow();
                dr["Nick"] = e.NewNickname;
                dr["Alias"] = e.OldNickname;
                db.Tables[aliastable].Rows.Add(dr);
            }

            SaveDB();
        }

        private void messageHandler(object sender, IrcEventArgs e) {
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Data.Nick + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastMessage"] = e.Data.Message;
            }   
            SaveDB();
        }

        private void namesHandler(object sender, NamesEventArgs e) {
            foreach(string name in e.UserList) {
                DataRow[] datarows = db.Tables[seentable].Select("Nick='" + name + "'");
                if (datarows.Length > 0) {
                    datarows[0]["LastSeenTime"] = DateTime.Now;
                    datarows[0]["LastAction"] = "(ON)";
                    datarows[0]["OnStatus"] = true;
                } else {
                    DataRow dr = db.Tables[seentable].NewRow();
                    dr["Nick"] = name;
                    dr["LastSeenTime"] = DateTime.Now;
                    dr["LastAction"] = "(ON)";
                    dr["LastMessage"] = "<no message yet>";
                    dr["TimesSeen"] = 0;
                    dr["OnStatus"] = true;
                    db.Tables[seentable].Rows.Add(dr);
                }
            }
            SaveDB();
        }
        
        private void joinHandler(object sender, JoinEventArgs e) {
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Who + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(JOIN)";
                datarows[0]["OnStatus"] = true;
            } else {
                DataRow dr = db.Tables[seentable].NewRow();
                dr["Nick"] = e.Who;
                dr["LastSeenTime"] = DateTime.Now;
                dr["LastAction"] = "ON";
                dr["LastMessage"] = "<no message yet>";
                dr["TimesSeen"] = 0;
                dr["OnStatus"] = true;
                db.Tables[seentable].Rows.Add(dr);
            }
            SaveDB();
        }

        private void partHandler(object sender, PartEventArgs e) {
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Who + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(PART) " + e.PartMessage;
                datarows[0]["OnStatus"] = false;
            }   
            SaveDB();
        }

        private void quitHandler(object sender, QuitEventArgs e) {
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Who + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(QUIT) " + e.QuitMessage;
                datarows[0]["OnStatus"] = false;
            }   
            SaveDB();
        }

        private void kickHandler(object sender, KickEventArgs e) {
            DataRow[] datarows = db.Tables[seentable].Select("Nick='" + e.Who + "'");
            if (datarows.Length > 0) {
                datarows[0]["LastSeenTime"] = DateTime.Now;
                datarows[0]["LastAction"] = "(KICK) " + e.KickReason + " (by " + e.Whom + ")";
                datarows[0]["OnStatus"] = false;
            }
            SaveDB();
        }
        
        private void SaveDB() {
            db.WriteXml(filename, XmlWriteMode.WriteSchema);
        }
    }
}