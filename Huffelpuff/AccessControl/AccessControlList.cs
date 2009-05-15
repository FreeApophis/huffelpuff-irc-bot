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
    public class AccessControlList
    {
        private List<IdentifyUser> identifyPlugins = new List<IdentifyUser>();
        
        // string should be invalid for nicknames, that way we can handle them in paralell without problems of group overtaking!
        private const string GroupPrefix = "#";
        
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
        
        public void Init() {
            bot.AddCommand(new Commandlet("!+access", "!+access <nick/" + GroupPrefix + "group> <access_string> adds the privilge to use <access_string> to user <nick>.", accessHandler, this, CommandScope.Both, "acl_add"));
            bot.AddCommand(new Commandlet("!-access", "!-access <nick/" + GroupPrefix + "group> <access_string> removes the privilge to use <access_string> from user <nick>.", accessHandler, this, CommandScope.Both, "acl_remove"));
            
            bot.AddCommand(new Commandlet("!restricted", "!restricted lists all access_strings, which can be used to access restricted functions, they are not the same as commands.", listRestricted, this, CommandScope.Both));
            bot.AddCommand(new Commandlet("!groups", "!groups lists all currently active groups.", listGroups, this, CommandScope.Both));
            
            bot.AddCommand(new Commandlet("!+group", "!+group <group> adds the new empty group <group>.", groupHandler, this, CommandScope.Both, "group_add"));
            bot.AddCommand(new Commandlet("!-group", "!-group <group> drops the group, all members will be lost.", groupHandler, this, CommandScope.Both, "group_remove"));
            bot.AddCommand(new Commandlet("!+user", "!+user <group> <user> adds the user <user> to the group <group>.", groupHandler, this, CommandScope.Both, "group_add_user"));
            bot.AddCommand(new Commandlet("!-user", "!-user <group> <user> removes the user <user> from the group <group>.", groupHandler, this, CommandScope.Both, "group_remove_user"));

            // Get users and their accessstrings
            foreach(string pair in PersistentMemory.GetValues("acl", "accesslist")) {
                string[] p = pair.Split(new char[] {';'});
                if(!accessList.ContainsKey(p[0])) {
                    accessList.Add(p[0], new List<string>());
                }
                accessList[p[0]].Add(p[1]);
            }
            
            // Get Groups and Users
            foreach(string pair in PersistentMemory.GetValues("acl", "group")) {
                string[] p = pair.Split(new char[] {';'});
                if(!groups.ContainsKey(p[0])) {
                    groups.Add(p[0], new List<string>());
                }
                groups[p[0]].Add(p[1]);
            }
            


        }
        
        private List<string> GetGroups(string nick) {
            List<string> temp = new List<string>();
            
            foreach(string group in groups.Keys) {
                if(groups[group].Contains(nick)) {
                    temp.Add(group);
                }
            }
            
            // TODO: Dynamic Groups here
            // example: if <nick> is voice, add group #+ 
            // everyone = *
            
            return temp;
        }
        
        private void accessHandler(object sender, IrcEventArgs e) {
            
        }

        private void groupHandler(object sender, IrcEventArgs e) {
                    
        }

        private void listRestricted(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            foreach(string line in bot.ListToLines(possibleAccessStrings, 350)) {
                bot.SendMessage(SendType.Message, sendto, line);
            }
        }
        
        private void listGroups(object sender, IrcEventArgs e) {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel))?e.Data.Nick:e.Data.Channel;
            foreach(string line in bot.ListToLines(groups.Keys, 350)) {
                bot.SendMessage(SendType.Message, sendto, line);
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
        
        public void AddAccessRight(string identity, string accessString)
        {
            if (possibleAccessStrings.Contains(accessString)) {
                if (!accessList.ContainsKey(identity)) {
                    accessList.Add(identity, new List<string>());
                }
                accessList[identity].Add(accessString);
                PersistentMemory.SetValue("acl", "accesslist", identity + ";" + accessString);
            } else {
                throw new NotSupportedException();
            }
        }
        
        public void RemoveAccessRight(string identity, string accessString)
        {
            if (accessList.ContainsKey(identity)) {
                accessList[identity].Remove(accessString);
                PersistentMemory.RemoveValue("acl", "accesslist", identity + ";" + accessString);
            }
        }
                
        public bool Access(string nick, string command)
        {
            if (!Identiefied(nick)) {
                //bot.SendMessage(SendType.Notice, nick, "Your nick is not identiefied.");
                return false;
            }

            if (accessList.ContainsKey(nick)) {
                foreach(string accessString in accessList[nick]) {
                    if (command == accessString)
                        return true;
                }
            }
            
            foreach(string group in GetGroups(nick)) {
                if (accessList.ContainsKey(GroupPrefix + group)) {
                    foreach(string accessString in accessList[GroupPrefix + group]) {
                        if (command == accessString)
                            return true;
                    }
                }
            }
            
            bot.SendMessage(SendType.Notice, nick, "you tried to access function: '" + command + "' but you don't have the required privileges");
            return false;
        }
        
        public bool Identiefied(string nick) {
            foreach(IdentifyUser id in identifyPlugins) {
                if (id.Identified(nick)) {
                    return true;
                }
            }
            return false;
        }
    }
    

    
}
