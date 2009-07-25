/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Meebey.SmartIrc4net;

namespace Huffelpuff
{
    /// <summary>
    /// Description of AccessControlList.
    /// </summary>
    public class AccessControlList : MarshalByRefObject
    {
        private List<IdentifyUser> identifyPlugins = new List<IdentifyUser>();
        
        public List<IdentifyUser> IdentifyPlugins {
            get { return identifyPlugins; }
        }
        
        // string should be invalid for nicknames, that way we can handle them in paralell without problems of group overtaking!
        private const string groupPrefix = "#";
        
        /// <summary>
        /// For each Group it gold the list of users
        /// </summary>
        private Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// For each User it holds a list of all accessStrings
        /// </summary>
        private Dictionary<string, List<string>> accessList = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// Possible access Strings
        /// </summary>
        private List<string> possibleAccessStrings = new List<string>();
        
        public ReadOnlyCollection<string>  PossibleAccessStrings {
            get {
                return new ReadOnlyCollection<string>(possibleAccessStrings);
            }
        }
        
        private IrcBot bot = null;
        
        public AccessControlList(IrcBot bot)
        {
            this.bot = bot;
        }
        
        private string superUser = "ns/Apophis";
        
        public void Init() {
            bot.AddCommand(new Commandlet("!+access", "!+access <id or" + groupPrefix + "group> <access_string> adds the privilge to use <access_string> to user <nick>.", accessHandler, this, CommandScope.Both, "acl_add"));
            bot.AddCommand(new Commandlet("!-access", "!-access <id or" + groupPrefix + "group> <access_string> removes the privilge to use <access_string> from user <nick>.", accessHandler, this, CommandScope.Both, "acl_remove"));
            bot.AddCommand(new Commandlet("!access", "!access lists all access_strings, which can be used to access restricted functions, they are not the same as commands. !access <id> lists all access strings he owns by himself or inherited through a group", listRestricted, this, CommandScope.Both));
            
            bot.AddCommand(new Commandlet("!groups", "!groups lists all currently active groups. !groups <" + groupPrefix + "group> lists all user in the group.", listGroups, this, CommandScope.Both));
            bot.AddCommand(new Commandlet("!id", "!id shows all my current identifactions. !id <nick> shows their ids.", idDelegate, this, CommandScope.Both));
            
            bot.AddCommand(new Commandlet("!+group", "!+group <group> adds the new empty group <group>.", groupHandler, this, CommandScope.Both, "group_add"));
            bot.AddCommand(new Commandlet("!-group", "!-group <group> drops the group, all members will be lost.", groupHandler, this, CommandScope.Both, "group_remove"));
            bot.AddCommand(new Commandlet("!+user", "!+user <group> <user> adds the user <user> to the group <group>.", groupHandler, this, CommandScope.Both, "group_add_user"));
            bot.AddCommand(new Commandlet("!-user", "!-user <group> <user> removes the user <user> from the group <group>.", groupHandler, this, CommandScope.Both, "group_remove_user"));

            // Get users and their accessstrings
            foreach(string pair in PersistentMemory.Instance.GetValues("acl", "accesslist")) {
                string[] p = pair.Split(new char[] {';'});
                if(!accessList.ContainsKey(p[0])) {
                    accessList.Add(p[0], new List<string>());
                }
                accessList[p[0]].Add(p[1]);
            }
            
            // Get Groups and Users
            foreach(string pair in PersistentMemory.Instance.GetValues("acl", "group")) {
                string[] p = pair.Split(new char[] {';'});
                if(!groups.ContainsKey(p[0])) {
                    groups.Add(p[0], new List<string>());
                }
                groups[p[0]].Add(p[1]);
            }
            


        }
        
        private List<string> GetAllGroups(string nick) {
            List<string> allgroups = new List<string>();
            foreach (string id in Identified(nick)) {
                allgroups.AddRange(GetGroups(id));
            }
            return allgroups;
        }
        
        private List<string> GetGroups(string nick) {
            List<string> temp = new List<string>();
            
            foreach(string group in groups.Keys) {
                if(groups[group].Contains(nick)) {
                    temp.Add(group);
                }
            }
            
            foreach(string channel in PersistentMemory.Instance.GetValues("channel"))
            {
                NonRfcChannelUser user = (NonRfcChannelUser)bot.GetChannelUser(channel, nick);
                if (user==null) { continue; }
                if (user.IsVoice) {
                    temp.Add("#+" + channel);
                }
                if (bot.SupportNonRfc && user.IsHalfop) {
                    temp.Add("#%" + channel);
                    temp.Add("#+" + channel);
                }
                if (user.IsOp) {
                    temp.Add("#@" + channel);
                    if (bot.SupportNonRfc) {
                        temp.Add("#%" + channel);
                    }
                    temp.Add("#+" + channel);
                }
            }
            
            // TODO: Dynamic Groups here
            // example: if <nick> is voice, add group #+ #@ #%
            // everyone = #*
            
            return temp;
        }

        private void idDelegate(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            string nicktToID = e.Data.Nick;
            if (e.Data.MessageArray.Length>1) {
                nicktToID = e.Data.MessageArray[1];
            }
            
            foreach(string line in bot.ListToLines(this.Identified(nicktToID), 350, ", ", "IDs of the Nick '" + nicktToID + "': ", " END.")) {
                bot.SendMessage(SendType.Notice, sendto, line);
            }
            foreach(string line in bot.ListToLines(this.GetAllGroups(nicktToID), 350, ", ", "Nick is in Groups: ", " END.")) {
                bot.SendMessage(SendType.Notice, sendto, line);
            }
        }

        
        
        private void accessHandler(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if(e.Data.MessageArray.Length > 2) {
                if (e.Data.MessageArray[0] == "!+access") {
                    if(AddAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2])) {
                        bot.SendMessage(SendType.Notice, sendto, "Accessright '" + e.Data.MessageArray[2] + "' added to '" + e.Data.MessageArray[1] + "'!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to add Accessright '" + e.Data.MessageArray[2] + "' to '" + e.Data.MessageArray[1] + "'! (Try !access for a list of access strings)");
                    }
                    return;
                }
                if (e.Data.MessageArray[0] == "!-access") {
                    if(RemoveAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2])) {
                        bot.SendMessage(SendType.Notice, sendto, "Accessright '" + e.Data.MessageArray[2] + "' removed from '" + e.Data.MessageArray[1] + "'!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to remove Accessright '" + e.Data.MessageArray[2] + "' from '" + e.Data.MessageArray[1] + "'!");
                    }
                }
                return;
            }
            bot.SendMessage(SendType.Notice, sendto, "Too few arguments! Try !help.");
        }

        private void groupHandler(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if(e.Data.MessageArray.Length > 1) {
                if (e.Data.MessageArray[0] == "!+group") {
                    if(AddGroup(EnsureGroupPrefix(e.Data.MessageArray[1]))) {
                        bot.SendMessage(SendType.Notice, sendto, "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' added!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to add group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }
                    
                    return;
                } else if (e.Data.MessageArray[0] == "!-group") {
                    if(RemoveGroup(EnsureGroupPrefix(e.Data.MessageArray[1]))) {
                        bot.SendMessage(SendType.Notice, sendto, "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' removed!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to remove group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }
                    return;
                }
            }
            if(e.Data.MessageArray.Length > 2) {
                if (e.Data.MessageArray[0] == "!+user") {
                    if(AddUser(EnsureGroupPrefix(e.Data.MessageArray[1]), e.Data.MessageArray[2])) {
                        bot.SendMessage(SendType.Notice, sendto, "User '" + e.Data.MessageArray[2] + "' added to group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to add user '" + e.Data.MessageArray[2] + "' from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }
                    return;
                } else if (e.Data.MessageArray[0] == "!-user") {
                    if(RemoveUser(EnsureGroupPrefix(e.Data.MessageArray[1]), e.Data.MessageArray[2])) {
                        bot.SendMessage(SendType.Notice, sendto, "User '" + e.Data.MessageArray[2] + "' removed from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    } else {
                        bot.SendMessage(SendType.Notice, sendto, "Failed to remove user '" + e.Data.MessageArray[2] + "' from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }
                    return;
                }
            }
            bot.SendMessage(SendType.Notice, sendto, "Too few arguments! Try !help.");
        }

        private bool AddGroup(string group) {
            if (!groups.ContainsKey(group)) {
                groups.Add(group, new List<string>());
                return true;
            }
            return false;
        }
        
        private bool RemoveGroup(string group) {
            if (groups.ContainsKey(group)) {
                groups.Remove(group);
                PersistentMemory.Instance.RemoveValueStartingWith("acl", "group", group + ";");
                PersistentMemory.Instance.Flush();
                return true;
            }
            return false;
        }
        
        private bool AddUser(string group, string id) {
            if (groups.ContainsKey(group)) {
                if (!groups[group].Contains(id)) {
                    groups[group].Add(id);
                    PersistentMemory.Instance.SetValue("acl", "group", group + ";" + id);
                    PersistentMemory.Instance.Flush();
                    return true;
                }
            }
            return false;
        }

        private bool RemoveUser(string group, string id) {
            if (groups.ContainsKey(group)) {
                if (groups[group].Contains(id)) {
                    groups[group].Remove(id);
                    PersistentMemory.Instance.RemoveValue("acl", "group", group + ";" + id);
                    PersistentMemory.Instance.Flush();
                    return true;
                }
            }
            return false;
        }
        
        private void listRestricted(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            // TODO <paramter> described in the help!
            foreach(string line in bot.ListToLines(possibleAccessStrings, 350)) {
                bot.SendMessage(SendType.Notice, sendto, line);
            }
        }
        
        private void listGroups(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            if (e.Data.MessageArray.Length > 1) {
                if(groups.ContainsKey(EnsureGroupPrefix(e.Data.MessageArray[1]))) {
                    foreach(string line in bot.ListToLines(groups[EnsureGroupPrefix(e.Data.MessageArray[1])], 350, ", ", "User in Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "': ", " END.")) {
                        bot.SendMessage(SendType.Notice, sendto, line);
                    }
                    
                } else {
                    bot.SendMessage(SendType.Notice, sendto, "No such group.");
                }
            } else {
                foreach(string line in bot.ListToLines(groups.Keys, 350, ", ", "Groups: ", " (Special Groups: #@, #%, #+, #*) END.")) {
                    bot.SendMessage(SendType.Notice, sendto, line);
                }
            }
        }

        public void AddAccessString(string accessString) {
            if(!possibleAccessStrings.Contains(accessString))
                possibleAccessStrings.Add(accessString);
        }
        
        public List<string> NoAccessList() {
            List<string> temp = new List<string>(possibleAccessStrings);
            foreach(List<string> sublist in accessList.Values) {
                foreach(string acc in sublist) {
                    if (temp.Contains(acc)) {
                        temp.Remove(acc);
                    }
                }
            }
            return temp;
        }
        
        public void AddIdentifyPlugin(IdentifyUser identify) {
            identifyPlugins.Add(identify);
        }
        
        public bool AddAccessRight(string identity, string accessString)
        {
            if (possibleAccessStrings.Contains(accessString)) {
                if (!accessList.ContainsKey(identity)) {
                    accessList.Add(identity, new List<string>());
                }
                accessList[identity].Add(accessString);
                PersistentMemory.Instance.SetValue("acl", "accesslist", identity + ";" + accessString);
                PersistentMemory.Instance.Flush();
                return true;
            }
            return false;
        }
        
        public bool RemoveAccessRight(string identity, string accessString)
        {
            if (accessList.ContainsKey(identity)) {
                accessList[identity].Remove(accessString);
                PersistentMemory.Instance.RemoveValue("acl", "accesslist", identity + ";" + accessString);
                PersistentMemory.Instance.Flush();
                return true;
            }
            return false;
        }
        
        public bool Access(string nick, string command, bool warn)
        {
            foreach(string id in Identified(nick)) {
                if ((id == superUser) || (TryAccess(id, command))) {
                    return true;
                }
            }
            if (warn) {
                bot.SendMessage(SendType.Notice, nick, "you tried to access function: '" + command + "' but you don't have the required privileges");
            }
            return false;
        }
        
        private bool TryAccess(string id, string command) {
            if (accessList.ContainsKey(id)) {
                foreach(string accessString in accessList[id]) {
                    if (command == accessString)
                        return true;
                }
            }
            
            foreach(string group in GetGroups(id)) {
                if (accessList.ContainsKey(group)) {
                    foreach(string accessString in accessList[group]) {
                        if (command == accessString)
                            return true;
                    }
                }
            }
            
            
            return false;
        }
        
        public List<string> Identified(string nick) {
            List<string> ids = new List<string>();
            foreach(IdentifyUser id in identifyPlugins) {
                string idstring = id.Identified(nick);
                if (idstring!=null) {
                    ids.Add(idstring);
                }
            }
            return ids;
        }
        
        private string EnsureGroupPrefix(string group) {
            if (group.StartsWith(groupPrefix)) {
                return group;
            }
            return groupPrefix + group;
        }
    }



}
