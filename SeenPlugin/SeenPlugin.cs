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
using System.Data;
using System.Data.SQLite;
using System.Linq;
using apophis.SharpIRC;
using apophis.SharpIRC.IrcClient;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Plugin.Database.Seen;

namespace Plugin
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class SeenPlugin : AbstractPlugin
    {
        public SeenPlugin(IrcBot botInstance) : base(botInstance) { }

        private Main seenData;

        public override void Init()
        {
            seenData = new Main(DatabaseConnection.Create("Seen"));

            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!seen", "The command !seen <nick> tells you when a certain nick was last seen in this channel ", SeenCommand, this));
            BotEvents.OnChannelMessage += MessageHandler;
            BotEvents.OnNickChange += NickChangeHandler;
            BotEvents.OnNames += NamesHandler;
            BotEvents.OnJoin += JoinHandler;
            BotEvents.OnPart += PartHandler;
            BotEvents.OnQuit += QuitHandler;
            BotEvents.OnKick += KickHandler;

            foreach (var entry in seenData.SeenEntries)
            {
                entry.OnStatus = false;
            }

            foreach (string channel in BotMethods.JoinedChannels)
            {
                BotMethods.RfcNames(channel);
            }
            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!seen");
            BotEvents.OnChannelMessage -= MessageHandler;
            BotEvents.OnNickChange -= NickChangeHandler;
            BotEvents.OnNames -= NamesHandler;
            BotEvents.OnJoin -= JoinHandler;
            BotEvents.OnPart -= PartHandler;
            BotEvents.OnQuit -= QuitHandler;
            BotEvents.OnKick -= KickHandler;

            foreach (var entry in seenData.SeenEntries)
            {
                entry.OnStatus = false;
            }
            base.Deactivate();
        }


        public override string AboutHelp()
        {
            return "This is the basic Last Seen Plugin";
        }

        private void SeenCommand(object sender, IrcEventArgs e)
        {
            lock (seenData)
            {
                string destination = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

                if (e.Data.MessageArray.Length > 1)
                {
                    var result = seenData.SeenEntries.Where(s => s.Nick == e.Data.MessageArray[1]).FirstOrDefault();
                    if (result != null)
                    {
                        if (result.OnStatus)
                        {
                            BotMethods.SendMessage(SendType.Message, destination, "The nick '" + e.Data.MessageArray[1] + "' is on right now (Last Action: " + result.LastAction + " (" + result.LastSeenTime + ") Lastmessage: " + result.LastMessage + " !seen#: " + result.TimesSeen + ")");
                        }
                        else
                        {
                            BotMethods.SendMessage(SendType.Message, destination, "The nick '" + e.Data.MessageArray[1] + "' was last seen at " + result.LastSeenTime + " (Last Action: " + result.LastAction + " Lastmessage: " + result.LastMessage + " !seen#: " + result.TimesSeen + ")");
                        }
                        result.TimesSeen++;
                    }
                    else
                    {
                        BotMethods.SendMessage(SendType.Message, destination, "The nick '" + e.Data.MessageArray[1] + "' was never seen");
                    }
                }
                else
                {
                    BotMethods.SendMessage(SendType.Message, destination, "Seen " + seenData.SeenEntries.Count() + " unique nicknames. Use !seen <nick> for query.");
                }

                SaveDb();
            }
        }


        private void NickChangeHandler(object sender, NickChangeEventArgs e)
        {
            lock (seenData)
            {
                var seenResult = seenData.SeenEntries.Where(s => s.Nick == e.OldNickname).FirstOrDefault();

                if (seenResult != null)
                {
                    seenResult.LastSeenTime = DateTime.Now;
                    seenResult.LastAction = "(NICK) Nick changed to: " + e.NewNickname;
                    seenResult.OnStatus = false;
                }

                seenResult = seenData.SeenEntries.Where(s => s.Nick == e.NewNickname).FirstOrDefault();
                if (seenResult != null)
                {
                    seenResult.LastSeenTime = DateTime.Now;
                    seenResult.LastAction = "(NICK) Nick changed from: " + e.OldNickname;
                    seenResult.OnStatus = true;
                }
                else
                {
                    var seenEntry = new SeenEntry();
                    seenData.SeenEntries.InsertOnSubmit(seenEntry);

                    seenEntry.Nick = e.NewNickname;
                    seenEntry.LastSeenTime = DateTime.Now;
                    seenEntry.LastAction = "(NICK) Nick changed from: " + e.OldNickname;
                    seenEntry.LastMessage = "<no message yet>";
                    seenEntry.TimesSeen = 0;
                    seenEntry.OnStatus = true;

                }

                var aliasResult = seenData.AliasEntries.Where(a => a.Nick == e.OldNickname && a.Alias == e.NewNickname).FirstOrDefault();

                if (aliasResult == null)
                {
                    var aliasEntry = new AliasEntry();
                    seenData.AliasEntries.InsertOnSubmit(aliasEntry);

                    aliasEntry.Nick = e.OldNickname;
                    aliasEntry.Alias = e.NewNickname;
                }

                aliasResult = seenData.AliasEntries.Where(a => a.Nick == e.NewNickname && a.Alias == e.OldNickname).FirstOrDefault();
                if (aliasResult == null)
                {
                    var aliasEntry = new AliasEntry();
                    seenData.AliasEntries.InsertOnSubmit(aliasEntry);

                    aliasEntry.Nick = e.NewNickname;
                    aliasEntry.Alias = e.OldNickname;
                }

                SaveDb();
            }
        }

        private void MessageHandler(object sender, IrcEventArgs e)
        {
            lock (seenData)
            {
                var result = seenData.SeenEntries.Where(s => s.Nick == e.Data.Nick).FirstOrDefault();

                if (result != null)
                {
                    result.LastSeenTime = DateTime.Now;
                    result.LastMessage = e.Data.Message;
                }
                SaveDb();
            }
        }

        private void NamesHandler(object sender, NamesEventArgs e)
        {
            lock (seenData)
            {
                foreach (string name in e.UserList)
                {
                    var result = seenData.SeenEntries.Where(s => s.Nick == name).FirstOrDefault();

                    if (result != null)
                    {
                        result.LastSeenTime = DateTime.Now;
                        result.LastAction = "(ON)";
                        result.OnStatus = true;
                    }
                    else
                    {
                        var seenEntry = new SeenEntry();
                        seenData.SeenEntries.InsertOnSubmit(seenEntry);

                        seenEntry.Nick = name;
                        seenEntry.LastSeenTime = DateTime.Now;
                        seenEntry.LastAction = "(ON)";
                        seenEntry.LastMessage = "<no message yet>";
                        seenEntry.TimesSeen = 0;
                        seenEntry.OnStatus = true;
                    }
                }
                SaveDb();
            }
        }

        private void JoinHandler(object sender, JoinEventArgs e)
        {
            lock (seenData)
            {

                var result = seenData.SeenEntries.Where(s => s.Nick == e.Who).FirstOrDefault();

                if (result != null)
                {
                    result.LastSeenTime = DateTime.Now;
                    result.LastAction = "(JOIN)";
                    result.OnStatus = true;
                }
                else
                {
                    var seenEntry = new SeenEntry();
                    seenData.SeenEntries.InsertOnSubmit(seenEntry);

                    seenEntry.Nick = e.Who;
                    seenEntry.LastSeenTime = DateTime.Now;
                    seenEntry.LastAction = "(JOIN)";
                    seenEntry.LastMessage = "<no message yet>";
                    seenEntry.TimesSeen = 0;
                    seenEntry.OnStatus = true;
                }
                SaveDb();
            }
        }

        private void PartHandler(object sender, PartEventArgs e)
        {
            lock (seenData)
            {
                var result = seenData.SeenEntries.Where(s => s.Nick == e.Who).FirstOrDefault();
                if (result != null)
                {
                    result.LastSeenTime = DateTime.Now;
                    result.LastAction = "(PART) " + e.PartMessage;
                    result.OnStatus = false;
                }
                SaveDb();
            }
        }

        private void QuitHandler(object sender, QuitEventArgs e)
        {
            lock (seenData)
            {
                var result = seenData.SeenEntries.Where(s => s.Nick == e.Who).FirstOrDefault();
                if (result != null)
                {
                    result.LastSeenTime = DateTime.Now;
                    result.LastAction = "(QUIT) " + e.QuitMessage;
                    result.OnStatus = false;
                }
                SaveDb();
            }
        }

        private void KickHandler(object sender, KickEventArgs e)
        {
            lock (seenData)
            {
                var result = seenData.SeenEntries.Where(s => s.Nick == e.Who).FirstOrDefault();
                if (result != null)
                {
                    result.LastSeenTime = DateTime.Now;
                    result.LastAction = "(KICK) " + e.KickReason + " (by " + e.Whom + ")";
                    result.OnStatus = false;
                }
                SaveDb();
            }
        }

        private void SaveDb()
        {
            seenData.SubmitChanges();
        }
    }
}