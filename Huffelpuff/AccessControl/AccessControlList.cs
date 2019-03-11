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
using SharpIrc;
using SharpIrc.IrcClient;
using Huffelpuff.Commands;
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
        public List<IdentifyUser> IdentifyPlugins { get; } = new List<IdentifyUser>();

        // string should be invalid for nicknames, that way we can handle them in parallel without problems of group overtaking!
        private const string GroupPrefix = "#";

        /// <summary>
        /// For each Group it holds the list of users
        /// </summary>
        private readonly Dictionary<string, List<string>> _groups = new Dictionary<string, List<string>>();

        /// <summary>
        /// For each User it holds a list of all accessStrings
        /// </summary>
        private readonly Dictionary<string, List<string>> _accessList = new Dictionary<string, List<string>>();

        /// <summary>
        /// Possible access Strings
        /// </summary>
        private readonly List<string> _possibleAccessStrings = new List<string>();

        public ReadOnlyCollection<string> PossibleAccessStrings => new ReadOnlyCollection<string>(_possibleAccessStrings);

        private readonly IrcBot _bot;

        public AccessControlList(IrcBot bot)
        {
            _bot = bot;
        }

        private readonly string _superUser = Settings.Default.Botmaster;

        public void Init()
        {
            _bot.AddCommand(new Commandlet("!+access", "!+access <id or" + GroupPrefix + "group> <access_string> adds the privilege to use <access_string> to user <nick>.", AccessHandler, this, CommandScope.Both, "acl_add"));
            _bot.AddCommand(new Commandlet("!-access", "!-access <id or" + GroupPrefix + "group> <access_string> removes the privilege to use <access_string> from user <nick>.", AccessHandler, this, CommandScope.Both, "acl_remove"));
            _bot.AddCommand(new Commandlet("!access", "!access lists all access_strings, which can be used to access restricted functions, they are not the same as commands. !access <id|nick|group> lists all access strings he owns by himself or inherited through a group", ListRestricted, this));

            _bot.AddCommand(new Commandlet("!groups", "!groups lists all currently active groups.", ListGroups, this));
            _bot.AddCommand(new Commandlet("!users", "!users <group> lists all currently active users in group <group>.", ListUsers, this));
            _bot.AddCommand(new Commandlet("!id", "!id shows all my current identifications. !id <nick> shows their ids.", IDDelegate, this));

            _bot.AddCommand(new Commandlet("!+group", "!+group <group> adds the new empty group <group>.", GroupHandler, this, CommandScope.Both, "group_add"));
            _bot.AddCommand(new Commandlet("!-group", "!-group <group> drops the group, all members will be lost.", GroupHandler, this, CommandScope.Both, "group_remove"));
            _bot.AddCommand(new Commandlet("!+user", "!+user <group> <user> adds the user <user> to the group <group>.", GroupHandler, this, CommandScope.Both, "group_add_user"));
            _bot.AddCommand(new Commandlet("!-user", "!-user <group> <user> removes the user <user> from the group <group>.", GroupHandler, this, CommandScope.Both, "group_remove_user"));

            // Get users and their access strings

            foreach (var entry in _bot.MainBotData.AclEntries)
            {
                if (!_accessList.ContainsKey(entry.Identity))
                {
                    _accessList.Add(entry.Identity, new List<string>());
                }
                _accessList[entry.Identity].Add(entry.AccessString);
            }


            // Get Groups and Users
            foreach (var aclGroup in _bot.MainBotData.AclGroups)
            {
                if (!_groups.ContainsKey(aclGroup.GroupName))
                {
                    _groups.Add(aclGroup.GroupName, new List<string>());
                }
                _groups[aclGroup.GroupName].Add(aclGroup.GroupID);
            }
        }


        private IEnumerable<string> GetAllGroups(string nick)
        {
            var allGroups = new List<string>();
            foreach (var id in Identified(nick))
            {
                allGroups.AddRange(GetGroups(id));
            }
            return allGroups;
        }

        private IEnumerable<string> GetGroups(string nick)
        {
            var temp = new List<string>();

            foreach (var group in _groups.Keys)
            {
                if (_groups[group].Contains(nick))
                {
                    temp.Add(group);
                }
            }

            foreach (string channel in _bot.MainBotData.Channels.Select(c => c.ChannelName))
            {
                var user = (NonRfcChannelUser)_bot.GetChannelUser(channel, nick);
                if (user == null) { continue; }
                if (user.IsVoice)
                {
                    temp.Add("#+" + channel);
                }
                if (_bot.SupportNonRfc && user.IsHalfop)
                {
                    temp.Add("#%" + channel);
                    temp.Add("#+" + channel);
                }
                if (user.IsOp)
                {
                    temp.Add("#@" + channel);
                    if (_bot.SupportNonRfc)
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
            string nickToId = e.Data.Nick;
            if (e.Data.MessageArray.Length > 1)
            {
                nickToId = e.Data.MessageArray[1];
            }

            foreach (string line in Identified(nickToId).ToLines(350, ", ", "IDs of the Nick '" + nickToId + "': ", " END."))
            {
                _bot.SendMessage(SendType.Message, e.SendBackTo(), line);
            }
            foreach (string line in GetAllGroups(nickToId).ToLines(350, ", ", "Nick is in Groups: ", " END."))
            {
                _bot.SendMessage(SendType.Message, e.SendBackTo(), line);
            }
        }



        private void AccessHandler(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 2)
            {
                if (e.Data.MessageArray[0] == "!+access")
                {
                    if (AddAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2]))
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Access right '" + e.Data.MessageArray[2] + "' added to '" + e.Data.MessageArray[1] + "'!");
                    }
                    else
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to add access right '" + e.Data.MessageArray[2] + "' to '" + e.Data.MessageArray[1] + "'! (Try !access for a list of access strings)");
                    }
                    return;
                }
                if (e.Data.MessageArray[0] == "!-access")
                {
                    if (RemoveAccessRight(e.Data.MessageArray[1], e.Data.MessageArray[2]))
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Access right '" + e.Data.MessageArray[2] + "' removed from '" + e.Data.MessageArray[1] + "'!");
                    }
                    else
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to remove Access right '" + e.Data.MessageArray[2] + "' from '" + e.Data.MessageArray[1] + "'!");
                    }
                }
                return;
            }
            _bot.SendMessage(SendType.Message, e.SendBackTo(), "Too few arguments! Try !help.");
        }

        private void GroupHandler(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                if (e.Data.MessageArray[0] == "!+group")
                {
                    if (AddGroup(EnsureGroupPrefix(e.Data.MessageArray[1])))
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' added!");
                    }
                    else
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to add group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                    }

                    return;
                }
                if (e.Data.MessageArray[0] == "!-group")
                {
                    if (RemoveGroup(EnsureGroupPrefix(e.Data.MessageArray[1])))
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "' removed!");
                    }
                    else
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to remove group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
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
                            _bot.SendMessage(SendType.Message, e.SendBackTo(), "User '" + e.Data.MessageArray[2] + "' added to group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        else
                        {
                            _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to add user '" + e.Data.MessageArray[2] + "' to group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        return;
                    case "!-user":
                        if (RemoveUser(EnsureGroupPrefix(e.Data.MessageArray[1]), e.Data.MessageArray[2]))
                        {
                            _bot.SendMessage(SendType.Message, e.SendBackTo(), "User '" + e.Data.MessageArray[2] + "' removed from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        else
                        {
                            _bot.SendMessage(SendType.Message, e.SendBackTo(), "Failed to remove user '" + e.Data.MessageArray[2] + "' from group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "'!");
                        }
                        return;
                }
            }
            _bot.SendMessage(SendType.Message, e.SendBackTo(), "Too few arguments! Try !help.");
        }

        private bool AddGroup(string group)
        {
            if (!_groups.ContainsKey(group))
            {
                _groups.Add(group, new List<string>());
                return true;
            }
            return false;
        }

        private bool RemoveGroup(string group)
        {
            if (_groups.ContainsKey(group))
            {
                _groups.Remove(group);

                var toRemove = _bot.MainBotData.AclGroups.Where(g => g.GroupName == group).ToArray();
                _bot.MainBotData.AclGroups.DeleteAllOnSubmit(toRemove);
                _bot.MainBotData.SubmitChanges();
                return true;
            }
            return false;
        }

        private bool AddUser(string group, string id)
        {
            if (_groups.ContainsKey(group))
            {
                if (!_groups[group].Contains(id))
                {
                    _groups[group].Add(id);

                    var aclGroup = new AclGroup { GroupID = id, GroupName = group };
                    _bot.MainBotData.AclGroups.InsertOnSubmit(aclGroup);
                    _bot.MainBotData.SubmitChanges();

                    return true;
                }
            }
            return false;
        }

        private bool RemoveUser(string group, string id)
        {
            if (_groups.ContainsKey(group))
            {
                if (_groups[group].Contains(id))
                {
                    _groups[group].Remove(id);

                    var toRemove = _bot.MainBotData.AclGroups.Where(g => g.GroupName == group && g.GroupID == id).ToArray();
                    _bot.MainBotData.AclGroups.DeleteAllOnSubmit(toRemove);
                    _bot.MainBotData.SubmitChanges();

                    return true;
                }
            }
            return false;
        }

        private void ListRestricted(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                string id = e.Data.MessageArray[1];
                ListAccessRights(e.SendBackTo(), id);
            }
            else
            {
                foreach (string line in _possibleAccessStrings.ToLines(350))
                {
                    _bot.SendMessage(SendType.Message, e.SendBackTo(), line);
                }
            }
        }


        void ListAccessRights(string sendTo, string name)
        {
            var message = new List<string>();
            if (name.StartsWith(GroupPrefix) && _accessList.ContainsKey(name))
            {
                message.AddRange(_accessList[name].ToLines(350, ", ", "Group Rights: ", ""));
            }
            else
            {
                foreach (string id in Identified(name))
                {
                    if (id == _superUser)
                    {
                        message.Add("User Rights from '{0}': This user is bot master".Fill(id));
                    }
                    else if (_accessList.ContainsKey(id))
                    {
                        message.AddRange(_accessList[id].ToLines(350, ", ", "User Rights from '{0}': ".Fill(id), ""));
                    }
                    foreach (string group in GetGroups(id))
                    {
                        if (_accessList.ContainsKey(group))
                        {
                            message.AddRange(_accessList[group].ToLines(350, ", ", "inherited rights from group '{0}': ".Fill(group), ""));
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
                _bot.SendMessage(SendType.Message, sendTo, line);
            }
        }

        private void ListGroups(object sender, IrcEventArgs e)
        {
            foreach (string line in _groups.Keys.ToLines(350, ", ", "Groups: ", " (Special Groups: #@, #%, #+, #*) END."))
            {
                _bot.SendMessage(SendType.Message, e.SendBackTo(), line);
            }
        }

        private void ListUsers(object sender, IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1)
            {
                if (_groups.ContainsKey(EnsureGroupPrefix(e.Data.MessageArray[1])))
                {
                    foreach (string line in _groups[EnsureGroupPrefix(e.Data.MessageArray[1])].ToLines(350, ", ", "Users in Group '" + EnsureGroupPrefix(e.Data.MessageArray[1]) + "': ", " END."))
                    {
                        _bot.SendMessage(SendType.Message, e.SendBackTo(), line);
                    }
                }
                else
                {
                    _bot.SendMessage(SendType.Message, e.SendBackTo(), "No such group.");
                }
            }
            else
            {
                _bot.SendMessage(SendType.Message, e.SendBackTo(), "Too few arguments! Try !help.");
            }
        }

        public void AddAccessString(string accessString)
        {
            if (!_possibleAccessStrings.Contains(accessString))
                _possibleAccessStrings.Add(accessString);
        }

        public List<string> NoAccessList()
        {
            var temp = new List<string>(_possibleAccessStrings);
            foreach (var acc in from sublist in _accessList.Values from acc in sublist where temp.Contains(acc) select acc)
            {
                temp.Remove(acc);
            }
            return temp;
        }

        public void AddIdentifyPlugin(IdentifyUser identify)
        {
            IdentifyPlugins.Add(identify);
        }

        public bool AddAccessRight(string identity, string accessString)
        {
            if (_possibleAccessStrings.Contains(accessString))
            {
                if (!_accessList.ContainsKey(identity))
                {
                    _accessList.Add(identity, new List<string>());
                }
                _accessList[identity].Add(accessString);

                var aclEntry = new AclEntry { AccessString = accessString, Identity = identity };
                _bot.MainBotData.AclEntries.InsertOnSubmit(aclEntry);
                _bot.MainBotData.SubmitChanges();

                return true;
            }
            return false;
        }

        public bool RemoveAccessRight(string identity, string accessString)
        {
            if (_accessList.ContainsKey(identity))
            {
                _accessList[identity].Remove(accessString);

                var toRemove = _bot.MainBotData.AclEntries.Where(e => e.Identity == identity && e.AccessString == accessString).ToArray();
                _bot.MainBotData.AclEntries.DeleteAllOnSubmit(toRemove);
                _bot.MainBotData.SubmitChanges();

                return true;
            }
            return false;
        }

        public bool Access(string nick, string command, bool warn)
        {
            if (Identified(nick).Any(id => (id == _superUser) || (TryAccess(id, command))))
            {
                return true;
            }
            if (warn)
            {
                _bot.SendMessage(SendType.Message, nick, "you tried to access function: '" + command + "' but you don't have the required privileges");
            }
            return false;
        }

        private bool TryAccess(string id, string command)
        {
            if (_accessList.ContainsKey(id))
            {
                if (_accessList[id].Any(accessString => command == accessString))
                {
                    return true;
                }
            }

            foreach (string group in GetGroups(id))
            {
                if (_accessList.ContainsKey(group))
                {
                    if (_accessList[group].Any(accessString => command == accessString))
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

            ids.AddRange(IdentifyPlugins.Select(id => id.Identified(nick)).Where(id => id != null));

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
