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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using apophis.SharpIRC;
using apophis.SharpIRC.IrcClient;
using Huffelpuff.Properties;
using Huffelpuff.Utils;
using Plugin.Database.Huffelpuff;

namespace Huffelpuff.AccessControl
{
    /// <summary>
    /// Description of AccessControlList.
    /// </summary>
    public class AccessControlList : MarshalByRefObject
    {
        private readonly List<IdentifyUser> identifyPlugins = new List<IdentifyUser>();

        public List<IdentifyUser> IdentifyPlugins
        {
            get { return identifyPlugins; }
        }

        // string should be invalid for nicknames, that way we can handle them in paralell without problems of group overtaking!
        private const string GroupPrefix = "#";

        /// <summary>
        /// For each Group it holds the list of users
        /// </summary>
        private readonly Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

        /// <summary>
        /// For each User it holds a list of all accessStrings
        /// </summary>
        private readonly Dictionary<string, List<string>> accessList = new Dictionary<string, List<string>>();

        /// <summary>
        /// Possible access Strings
        /// </summary>
        private readonly List<string> possibleAccessStrings = new List<string>();

        public ReadOnlyCollection<string> PossibleAccessStrings
        {
            get
            {
                return new ReadOnlyCollection<string>(possibleAccessStrings);
            }
        }

        private readonly IrcBot bot;

        public AccessControlList(IrcBot bot)
        {
            this.bot = bot;
        }

        private readonly string superUser = Settings.Default.Botmaster;

        public void Init()
        {
            bot.AddCommand(new Commandlet("!+access", "!+access <id or" + GroupPrefix + "group> <access_string> adds the privilge to use <access_string> to user <nick>.", AccessHandler, this, CommandScope.Both, "acl_add"));
            bot.AddCommand(new Commandlet("!-access", "!-access <id or" + GroupPrefix + "group> <access_string> removes the privilge to use <access_string> from user <nick>.", AccessHandler, this, CommandScope.Both, "acl_remove"));
            bot.AddCommand(new Commandlet("!access", "!access lists all access_strings, which can be used to access restricted functions, they are not the same as commands. !access <id|nick|group> lists all access strings he owns by himself or inherited through a group", ListRestricted, this, CommandScope.Both));

            bot.AddCommand(new Commandlet("!groups", "!groups lists all currently active groups.", ListGroups, this, CommandScope.Both));
            bot.AddCommand(new Commandlet("!users", "!users <group> lists all currently active users in group <group>.", ListUsers, this, CommandScope.Both));
            bot.AddCommand(new Commandlet("!id", "!id shows all my current identifactions. !id <nick> shows their ids.", IDDelegate, this, CommandScope.Both));

            bot.AddCommand(new Commandlet("!+group", "!+group <group> adds the new empty group <group>.", GroupHandler, this, CommandScope.Both, "group_add"));
            bot.AddCommand(new Commandlet("!-group", "!-group <group> drops the group, all members will be lost.", GroupHandler, this, CommandScope.Both, "group_remove"));
            bot.AddCommand(new Commandlet("!+user", "!+user <group> <user> adds the user <user> to the group <group>.", GroupHandler, this, CommandScope.Both, "group_add_user"));
            bot.AddCommand(new Commandlet("!-user", "!-user <group> <user> removes the user <user> from the group <group>.", GroupHandler, this, CommandScope.Both, "group_remove_user"));

            // Get users and their accessstrings

            foreach (var entry in bot.MainBotData.AclEntries)
            {
                if (!accessList.ContainsKey(entry.Identity))
                {
                    accessList.Add(entry.Identity, new List<string>());
                }
                accessList[entry.Identity].Add(entry.AccessString);
            }


            // Get Groups and Users
            foreach (var aclGroup in bot.MainBotData.AclGroups)
            {
                if (!groups.ContainsKey(aclGroup.GroupName))
                {
                    groups.Add(aclGroup.GroupName, new List<string>());
                }
                groups[aclGroup.GroupName].Add(aclGroup.GroupID);
            }
        }


        private IEnumerable<string> GetAllGroups(string nick)
        {
            var allgroups = new List<string>();
            foreach (var id in Identified(nick))
            {
                allgroups.AddRange(GetGroups(id));
            }
            return allgroups;
        }

        private IEnumerable<string> GetGroups(string nick)
        {
            var temp = new List<string>();

            foreach (var group in groups.Keys)
            {
                if (groups[group].Contains(nick))
                {
                    temp.Add(group);
                }
            }

            foreach (string channel in bot.MainBotData.Channels.Select(c => c.ChannelName))
            {
                var user = (NonRfcChannelUser)bot.GetChannelUser(channel, nick);
                if (user == null) { continue; }
                if (user.IsVoice)
                {
                    temp.Add("#+" + channel);
                }
                if (bot.SupportNonRfc && user.IsHalfop)
                {
                    temp.Add("#%" + channel);
                    temp.Add("#+" + channel);
                }
                if (user.IsOp)
                {
                    temp.Add("#@" + channel);
                    if (bot.SupportNonRfc)
                    {
                        temp.Add("#%" + channel);
                    }
                    temp.Add("#+" + channel);
                }
                temp.Add("#*" + channel);
                temp.Add("#*");
            }
            return temp;
        }

        private void IDDelegate(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            string nicktToID = e.Data.Nick;
            if (e.Data.MessageArray.Length > 1)
            {
                nicktToID = e.Data.MessageArray[1];
            }

            foreach (string line in Identified(nicktToID).ToLines(350, ", ", "IDs of the Nick '" + nicktToID + "': ", " END."))
            {
                bot.SendMessage(SendType.Message, sendto, line);
            }
            foreach (string line in GetAllGroups(nicktToID).ToLines(350, ", ", "Nick is in Groups: ", " END."))
            {
                bot.SendMessage(SendType.Message, sendto, line);
            }
        }



        private void AccessHandler(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length > 2)
            {
                if (e.Data.MessageArray[0] == "!+access")
                {
                    if (AddAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2]))
                    {
                        bot.SendMessage(SendType.Message, sendto, "Accessright '" + e.Data.MessageArray[2] + "' added to '" + e.Data.MessageArray[1] + "'!");
                    }
                    else
                    {
                        bot.SendMessage(SendType.Message, sendto, "Failed to add Accessright '" + e.Data.MessageArray[2] + "' to '" + e.Data.MessageArray[1] + "'! (Try !access for a list of access strings)");
                    }
                    return;
                }
                if (e.Data.MessageArray[0] == "!-access")
                {
                    if (RemoveAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2]))
                    {
                        bot.SendMessage(SendType.Message, sendto, "Accessright '" + e.Data.MessageArray[2] + "' removed from '" + e.Data.MessageArray[1] + "'!");
                    }
                    else
                    {
                        bot.SendMessage(SendType.Message, sendto, "Failed to remove Accessright '" + e.Data.MessageArray[2] + "' from '" + e.Data.MessageArray[1] + "'!");
                    }
                }
                return;
            }
            bot.SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
        }

        private void GroupHandler(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length > 1)
            {
                if (e.Data.MessageArray[0] == "!+group")
                {
                    if (AddGroup(EnsureGroupPrefix(e.Data.MessageArray[1])))
                    {
                        bot.SendMessage(SendType.Message, sendto, "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' added!");
                    }
                    else
                    {
                        bot.SendMessage(SendType.Message, sendto, "Failed to add group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }

                    return;
                }
                if (e.Data.MessageArray[0] == "!-group")
                {
                    if (RemoveGroup(EnsureGroupPrefix(e.Data.MessageArray[1])))
                    {
                        bot.SendMessage(SendType.Message, sendto, "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' removed!");
                    }
                    else
                    {
                        bot.SendMessage(SendType.Message, sendto, "Failed to remove group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }
                    return;
                }
            }
            if (e.Data.MessageArray.Length > 2)
            {
                switch (e.Data.MessageArray[0])
                {
                    case "!+user":
                        if (AddUser(EnsureGroupPrefix(e.Data.MessageArray[1]), e.Data.MessageArray[2]))
                        {
                            bot.SendMessage(SendType.Message, sendto, "User '" + e.Data.MessageArray[2] + "' added to group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        else
                        {
                            bot.SendMessage(SendType.Message, sendto, "Failed to add user '" + e.Data.MessageArray[2] + "' to group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        return;
                    case "!-user":
                        if (RemoveUser(EnsureGroupPrefix(e.Data.MessageArray[1]), e.Data.MessageArray[2]))
                        {
                            bot.SendMessage(SendType.Message, sendto, "User '" + e.Data.MessageArray[2] + "' removed from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        else
                        {
                            bot.SendMessage(SendType.Message, sendto, "Failed to remove user '" + e.Data.MessageArray[2] + "' from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        return;
                }
            }
            bot.SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
        }

        private bool AddGroup(string group)
        {
            if (!groups.ContainsKey(group))
            {
                groups.Add(group, new List<string>());
                return true;
            }
            return false;
        }

        private bool RemoveGroup(string group)
        {
            if (groups.ContainsKey(group))
            {
                groups.Remove(group);

                var toRemove = bot.MainBotData.AclGroups.Where(g => g.GroupName == group).ToArray();
                bot.MainBotData.AclGroups.DeleteAllOnSubmit(toRemove);
                bot.MainBotData.SubmitChanges();
                return true;
            }
            return false;
        }

        private bool AddUser(string group, string id)
        {
            if (groups.ContainsKey(group))
            {
                if (!groups[group].Contains(id))
                {
                    groups[group].Add(id);

                    var aclGroup = new AclGroup { GroupID = id, GroupName = group };
                    bot.MainBotData.AclGroups.InsertOnSubmit(aclGroup);
                    bot.MainBotData.SubmitChanges();

                    return true;
                }
            }
            return false;
        }

        private bool RemoveUser(string group, string id)
        {
            if (groups.ContainsKey(group))
            {
                if (groups[group].Contains(id))
                {
                    groups[group].Remove(id);

                    var toRemove = bot.MainBotData.AclGroups.Where(g => g.GroupName == group && g.GroupID == id).ToArray();
                    bot.MainBotData.AclGroups.DeleteAllOnSubmit(toRemove);
                    bot.MainBotData.SubmitChanges();

                    return true;
                }
            }
            return false;
        }

        private void ListRestricted(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length > 1)
            {
                string id = e.Data.MessageArray[1];
                ListAccessrights(sendto, id);
            }
            else
            {
                foreach (string line in possibleAccessStrings.ToLines(350))
                {
                    bot.SendMessage(SendType.Message, sendto, line);
                }
            }
        }


        void ListAccessrights(string sendto, string name)
        {
            var message = new List<string>();
            if (name.StartsWith(GroupPrefix) && accessList.ContainsKey(name))
            {
                message.AddRange(accessList[name].ToLines(350, ", ", "Group Rights: ", ""));
            }
            else
            {
                foreach (string id in Identified(name))
                {
                    if (id == superUser)
                    {
                        message.Add("User Rights from '{0}': This user is botmaster".Fill(id));
                    }
                    else if (accessList.ContainsKey(id))
                    {
                        message.AddRange(accessList[id].ToLines(350, ", ", "User Rights from '{0}': ".Fill(id), ""));
                    }
                    foreach (string @group in GetGroups(id))
                    {
                        if (accessList.ContainsKey(@group))
                        {
                            message.AddRange(accessList[@group].ToLines(350, ", ", "inherited rights from group '{0}': ".Fill(@group), ""));
                        }
                    }
                }

                if (message.Count == 0)
                {
                    message.Add(name.StartsWith(GroupPrefix)
                                    ? "Group '{0}' does not exist or has no rights set".Fill(name)
                                    : "User '{0}' does not exist or has no rights set".Fill(name));
                }
            }
            foreach (var line in message.ToLines(350, " / "))
            {
                bot.SendMessage(SendType.Message, sendto, line);
            }
        }

        private void ListGroups(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            foreach (string line in groups.Keys.ToLines(350, ", ", "Groups: ", " (Special Groups: #@, #%, #+, #*) END."))
            {
                bot.SendMessage(SendType.Message, sendto, line);
            }
        }

        private void ListUsers(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;
            if (e.Data.MessageArray.Length > 1)
            {
                if (groups.ContainsKey(EnsureGroupPrefix(e.Data.MessageArray[1])))
                {
                    foreach (string line in groups[EnsureGroupPrefix(e.Data.MessageArray[1])].ToLines(350, ", ", "Users in Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "': ", " END."))
                    {
                        bot.SendMessage(SendType.Message, sendto, line);
                    }
                }
                else
                {
                    bot.SendMessage(SendType.Message, sendto, "No such group.");
                }
            }
            else
            {
                bot.SendMessage(SendType.Message, sendto, "Too few arguments! Try !help.");
            }
        }

        public void AddAccessString(string accessString)
        {
            if (!possibleAccessStrings.Contains(accessString))
                possibleAccessStrings.Add(accessString);
        }

        public List<string> NoAccessList()
        {
            var temp = new List<string>(possibleAccessStrings);
            foreach (var acc in from sublist in accessList.Values from acc in sublist where temp.Contains(acc) select acc)
            {
                temp.Remove(acc);
            }
            return temp;
        }

        public void AddIdentifyPlugin(IdentifyUser identify)
        {
            identifyPlugins.Add(identify);
        }

        public bool AddAccessRight(string identity, string accessString)
        {
            if (possibleAccessStrings.Contains(accessString))
            {
                if (!accessList.ContainsKey(identity))
                {
                    accessList.Add(identity, new List<string>());
                }
                accessList[identity].Add(accessString);

                var aclEntry = new AclEntry { AccessString = accessString, Identity = identity };
                bot.MainBotData.AclEntries.InsertOnSubmit(aclEntry);
                bot.MainBotData.SubmitChanges();

                return true;
            }
            return false;
        }

        public bool RemoveAccessRight(string identity, string accessString)
        {
            if (accessList.ContainsKey(identity))
            {
                accessList[identity].Remove(accessString);

                var toRemove = bot.MainBotData.AclEntries.Where(e => e.Identity == identity && e.AccessString == accessString).ToArray();
                bot.MainBotData.AclEntries.DeleteAllOnSubmit(toRemove);
                bot.MainBotData.SubmitChanges();

                return true;
            }
            return false;
        }

        public bool Access(string nick, string command, bool warn)
        {
            if (Identified(nick).Any(id => (id == superUser) || (TryAccess(id, command))))
            {
                return true;
            }
            if (warn)
            {
                bot.SendMessage(SendType.Message, nick, "you tried to access function: '" + command + "' but you don't have the required privileges");
            }
            return false;
        }

        private bool TryAccess(string id, string command)
        {
            if (accessList.ContainsKey(id))
            {
                if (accessList[id].Any(accessString => command == accessString))
                {
                    return true;
                }
            }

            foreach (string group in GetGroups(id))
            {
                if (accessList.ContainsKey(group))
                {
                    if (accessList[group].Any(accessString => command == accessString))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public List<string> Identified(string nick)
        {
            var ids = new List<string>();
            if (nick.Contains("/"))
            {
                ids.Add(nick);
                return ids;
            }

            ids.AddRange(identifyPlugins.Select(id => id.Identified(nick)).Where(idstring => idstring != null));

            return ids;
        }

        private static string EnsureGroupPrefix(string group)
        {
            if (group.StartsWith(GroupPrefix))
            {
                return group;
            }
            return GroupPrefix + group;
        }
    }



}
