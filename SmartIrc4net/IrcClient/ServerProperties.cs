/*
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2008 Thomas Bruderer <apophis@pophis.net>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Description of ServerProperties.
    /// </summary>
    public class ServerProperties
    {
        internal ServerProperties()
        {
        }
        
        internal void SetCommands(string commandList)
        {
            foreach(string command in commandList.Split(new char[] {','}))
            {
                switch(command) {
                        case "KNOCK" : _Knock = true;
                        break;
                        case "MAP" : _Map = true;
                        break;
                        case "DCCALLOW" : _DccAllow = true;
                        break;
                        case "USERIP" : _UserIp = true;
                        break;
                    default:
#if LOG4NET
                        Logger.MessageParser.Warn("Unknown Command: " + command);
#endif
                        break;
                }
            }
        }
        
        internal Dictionary<string, string> _Raw = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        
        /// <summary>
        /// Safe Access to All Values Sent in RPL_ISUPPORT
        /// If a Value is not Set this will Return "FALSE" (this can also mean: we don't know)
        /// If a Value has no Parameter this will Return "TRUE"
        /// The Index should be the Identifier sent by the Server like "PREFIX" (not casesensitive)
        /// </summary>
        public string this[string s] {
            get {
                if(_Raw.ContainsKey(s)) {
                    return _Raw[s];
                } else {
                    return "FALSE";
                }
            }
        }
        
        public IEnumerable<string> KnownValues() {
            return _Raw.Keys;
        }
        

        internal string _IrcDaemon = "unknown";
        /// <summary>
        /// Returns the Name of the IrcDaemon
        /// </summary>
        public string IrcDaemon {
            get {
                return _IrcDaemon;
            }
        }
        
        internal string _ChannelTypes = "";
        
        public IEnumerable<char> ChannelTypes {
            get {
                return _ChannelTypes.ToCharArray();
            }
        }
        
        internal string _ChannelModes = "";
        
        public IEnumerable<char> ChannelModes
        {
            get {
                List<char> temp = new List<char>();
                foreach(char c in _ChannelModes) {
                    if (c!=',') {
                        temp.Add(c);
                    }
                }
                return temp;
            }
        }
        
        public IEnumerable<char> GetChannelModes(ChannelModeType channelModeType)
        {
            List<char> temp = new List<char>();
            string[] modes = _ChannelModes.Split(new char[]{','});
            if (modes.Length<4)
                return temp;
            if(channelModeType== ChannelModeType.WithUserhostParameter) {
                AddCharsToList(ref temp, ref modes[0]);
            }
            if(channelModeType== ChannelModeType.WithAlwaysParamter) {
                AddCharsToList(ref temp, ref modes[1]);
            }
            if(channelModeType== ChannelModeType.WithSetOnlyParameter) {
                AddCharsToList(ref temp, ref modes[2]);
            }
            if(channelModeType== ChannelModeType.WithoutParameter) {
                AddCharsToList(ref temp, ref modes[3]);
            }
            return temp;
        }

        void AddCharsToList(ref List<char> temp, ref string modes)
        {
            foreach (char c in modes) {
                if (c != ',') {
                    temp.Add(c);
                }
            }
        }
        
        internal int _MaxChannelModes = -1;
        
        /// <summary>
        /// Returns the maximum number of channel modes with parameter allowed per MODE command (-1 if unknown)
        /// </summary>
        public int MaxChannelModes {
            get {
                return _MaxChannelModes;
            }
        }
        
        internal int _MaxChannels = -1;
        internal string _MaxChannelsByType = "";
        
        
        /// <summary>
        /// Maximum number of channels allowed to join (# channels);
        /// </summary>
        public int MaxChannels {
            get {
                return GetMaxChannels('#');
            }
        }
        
        /// <summary>
        /// Maximum number of channels allowed to join per Channel Type;
        /// </summary>
        /// <param name="channelPrefix">On Which Type of channels (ex. '#')</param>
        /// <returns>Length of Channel ID</returns>
        public int GetMaxChannels(char channelPrefix)
        {
            Dictionary<char, int> pfn = ParsePfxNum(_MaxChannelsByType);
            if (pfn.ContainsKey(channelPrefix)) {
                return pfn[channelPrefix];
            } else {
                return _MaxChannels;
            }
        }
                
        internal int _MaxNickLength = 9;  // Rfc 2812
        
        /// <summary>
        /// Returns the the maximum allowed nickname length.
        /// </summary>
        public int MaxNickLength {
            get {
                return _MaxNickLength;
            }
        }   
        
        internal string _MaxList = "";
        
        /// <summary>
        ///  Returns the maximal number of List entries in a List.
        /// </summary>
        /// <param name="channelPrefix">On Which type of List (ex. Ban: 'b' )</param>
        /// <returns>Maximal Length of List (of type listType)</returns>        
        public int GetMaxList(char listType) {
            Dictionary<char, int> pfn = ParsePfxNum(_MaxList);
            if (pfn.ContainsKey(listType)) {
                return pfn[listType];
            } else {
                return -1;
            }        
        }
        
        internal int _MaxBans = -1;
        /// <summary>
        /// Maximum number of bans per channel.
        /// Note: This Value is either from the MAXBANS or the MAXLIST Value
        /// </summary>
        public int MaxBans {
            get {
                return Math.Max(_MaxBans, GetMaxList('b'));
            }
        }   
        
        internal string _NetworkName = "unknwon";
        
        /// <summary>
        /// Returns the Network Name if known
        /// </summary>
        public string NetworkName {
            get {
                return _NetworkName;
            }
        }
        
        internal bool _BanException = false;
        
        /// <summary>
        /// Returns true if the server support ban exceptions (e mode).
        /// </summary>
        public bool BanException {
            get {
                return _BanException;
            }
        }
        
        internal bool _InviteExceptions = false;
        
        /// <summary>
        /// Returns true if the server support invite exceptions (+I mode).
        /// </summary>
        public bool InviteExceptions {
            get {
                return _InviteExceptions;
            }
        }
        
        
        internal string _StatusMessage = "";
        
        /// <summary>
        /// Returns a list of Prefixes which can be used before a Channel name to message nicks who have a certain status or higher.
        /// </summary>
        public IEnumerable<char> StatusMessage {
            get {
                return _StatusMessage.ToCharArray();
            }
        }

        internal bool _WAllVoices = false;
        
        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLVOICES or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllVoices {
            get {
                bool statusContainsVoice = false;
                foreach(char c in this.StatusMessage) {
                    if(c == '+') statusContainsVoice = true;
                }
                return (_WAllVoices || statusContainsVoice);
            }
        }
        
        internal bool _WAllChannelOps = false;
        
        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLCHANOPS or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllChannelOps {
            get {
                bool statusContainsOp = false;
                foreach(char c in this.StatusMessage) {
                    if(c == '@') statusContainsOp = true;
                }
                return (_WAllChannelOps || statusContainsOp);
            }
        }
        
        internal CaseMappingType _CaseMapping = CaseMappingType.Unknown;
        
        /// <summary>
        /// Returns the used Case Mapping type ascii or rfc1459
        /// </summary>
        public CaseMappingType CaseMapping {
            get {
                return _CaseMapping;
            }
        }
        
        
        internal string _ExtendedListCommand = "";
        
        /// <summary>
        /// Returns an Enum with all List Extentions possible on the server
        /// </summary>
        public EListType ExtendedListCommand {
            get {
                EListType result = 0;
                foreach(char c in _ExtendedListCommand) {
                    result |= (EListType)Enum.Parse(typeof(EListType), c.ToString());
                }
                return result;
            }
        }
        
        internal int _MaxTopicLength = -1;
        
        /// <summary>
        /// Retruns the maximal allowed Length of a channel topic if known (-1 otherwise)
        /// </summary>
        public int MaxTopicLength {
            get {
                return _MaxTopicLength;
            }
        }
        
        internal int _MaxKickLength = -1;
        
        /// <summary>
        /// Retruns the maximal allowed Length of a kick message if known (-1 otherwise)
        /// </summary>
        public int MaxKickLength {
            get {
                return _MaxKickLength;
            }
        }
        
        internal int _MaxChannelLength = 50;  // Rfc 2812
        
        /// <summary>
        /// Retruns the maximal allowed Length of a channel name
        /// </summary>
        public int MaxChannelLength {
            get {
                return _MaxChannelLength;
            }
        }
        
        internal int _ChannelIDLength = 5; // Rfc 2811
        internal string _ChannelIDLengthByType = "";
        
        
        /// <summary>
        /// Returns the ID length for channels (! channels);
        /// </summary>
        public int ChannelIDLength {
            get {
                return GetChannelIDLength('!');
            }
        }
            
        
        /// <summary>
        ///  with an ID. The prefix says for which channel type it is.
        /// </summary>
        /// <param name="channelPrefix">On Which Type of channels (ex. '#')</param>
        /// <returns>Length of Channel ID</returns>
        public int GetChannelIDLength(char channelPrefix) {
            Dictionary<char, int> pfn = ParsePfxNum(_ChannelIDLengthByType);
            if (pfn.ContainsKey(channelPrefix)) {
                return pfn[channelPrefix];
            } else {
                return _ChannelIDLength;
            }
        }
        
        private Dictionary<char, int> ParsePfxNum(string toParse)
        {
            Dictionary<char, int> result = new Dictionary<char, int>();
            foreach(string sr in toParse.Split(new char[] {','}))
            {
                string[] ssr = sr.Split(new char[] {':'});  // ssr[0] list of chars, ssr[1] numeric value
                foreach(char c in ssr[0]) {
                    result.Add(c, int.Parse(ssr[1]));
                }
            }
            return result;
        }

        internal string _IrcStandard = "none";
        
        /// <summary>
        /// Returns the used Irc-Standard if known
        /// </summary>
        public string IrcStandard {
            get {
                return _IrcStandard;
            }
        }
        
        
        internal int _MaxSilence = 0;
        
        /// <summary>
        /// If the server supports the SILENCE command. The number is the maximum number of allowed entries in the list. (0 otherwise)
        /// </summary>
        public int MaxSilence {
            get {
                return _MaxSilence;
            }
        }
        
        internal bool _Rfc2812 = false;
        
        /// <summary>
        /// Server supports Rfc2812 Features beyond Rfc1459
        /// </summary>
        public bool RfC2812 {
            get {
                return _Rfc2812;
            }
        }
        
        internal bool _Penalty = false;
        
        /// <summary>
        /// Server gives extra penalty to some commands instead of the normal 2 seconds per message and 1 second for every 120 bytes in a message.
        /// </summary>
        public bool Penalty {
            get {
                return _Penalty;
            }
        }
        
        internal bool _ForcedNickChange = false;
        
        /// <summary>
        /// Forced nick changes: The server may change the nickname without the client sending a NICK message.
        /// </summary>
        public bool ForcedNickChange {
            get {
                return _ForcedNickChange;
            }
        }
        
        internal bool _SafeList = false;
        
        /// <summary>
        /// The LIST is sent in multiple iterations so send queue won't fill and kill the client connection.
        /// </summary>
        public bool SafeList {
            get {
                return _SafeList;
            }
        }
        
        internal int _MaxAwayLength = -1;
        
        /// <summary>
        /// The maximum length of an away message, returns -1 if not known
        /// </summary>
        public int MaxAwayLength {
            get {
                return _MaxAwayLength;
            }
        }
        
        internal bool _NoQuit = false;
        
        /// <summary>
        /// NOQUIT
        /// </summary>
        public bool NoQuit {
            get {
                return _NoQuit;
            }
        }
        
        internal bool _UserIp = false;
        
        /// <summary>
        /// Returns true if the Server supports the Userip Command
        /// </summary>
        public bool UserIp {
            get {
                return _UserIp;
            }
        }
        
        internal bool _CPrivateMessage = false;
        
        /// <summary>
        /// Returns true if the Server supports the CPrivMsg Command
        /// </summary>
        public bool CPrivateMessage {
            get {
                return _CPrivateMessage;
            }
        }

        internal bool _CNotice = false;
        
        /// <summary>
        /// Returns true if the Server supports the CNotice Command
        /// </summary>
        public bool CNotice {
            get {
                return _CNotice;
            }
        }
        
        internal int _MaxTargets = 1;
        internal string _MaxTargetsByCommand = "";
        
        /// <summary>
        /// Returns the maximum number of targets for PrivMsg and Notice
        /// </summary>        
        public int MaxTargets {
            get {
                return _MaxTargets;
            }
        }

        /// <summary>
        /// Returns the MAXTARGETS String (unparsed);
        /// </summary>
        public string MaxTargetsByCommand {
            get {
                return _MaxTargetsByCommand;
            }
        }
        
        internal bool _Knock = false;
        
        /// <summary>
        /// Returns true if the Server supports the Knock Command
        /// </summary>
        public bool Knock {
            get {
                return _Knock;
                
            }
        }
        
        internal bool _VirtualChannels = false;
        
        /// <summary>
        /// Returns true if the Server supports Virtual Channels
        /// </summary>
        public bool VirtualChannels {
            get {
                return _VirtualChannels;
            }
        }
        
        internal int _MaxWatch = 0;
        
        /// <summary>
        /// Returns how many Users can be on the watch list, returns 0 if the Watch command is not available.
        /// </summary>
        public int MaxWatch {
            get {
                return _MaxWatch;
            }
        }
        
        internal bool _WhoX = false;
        
        /// <summary>
        /// Returns true if the Server  uses WHOX protocol for the Who command.
        /// </summary>
        public bool WhoX {
            get {
                return _WhoX;
            }
        }
        
        internal bool _ModeG = false;
        
        /// <summary>
        /// Returns true if the server supports server side ignores via the +g user mode.
        /// </summary>
        public bool ModeG {
            get {
                return _ModeG;
            }
        }
        
        internal string _Language = "";
        
        /// <summary>
        /// Returns a list of Languages if the Server supports the Language command.
        /// </summary>
        public IEnumerable<string> Languages
        {
            get {
                return _Language.Split(new char[] {','});
            }
        }
        
        
        internal string _NickPrefix ="";
        
        /// <summary>
        /// Returns a Dictionary with Usermodes and Userprefixes for Channels.
        /// If we don't have values from the servers you can assume at least +ov / @+ are supported
        /// However the dictionary will be empty!
        /// Key = Mode, Value = Prefix, ex. NickPrefix['o'] = '@'
        /// Note: Some servers only show the most powerful, others may show all of them. 
        /// </summary>
        public  Dictionary<char, char> NickPrefix {
            get {
                string[] np =_NickPrefix.Split(new char[] {')'});
                Dictionary<char, char> temp = new Dictionary<char, char>();
                int i = 0;
                foreach(char c in np[1]) {
                    i++;    
                    temp.Add(np[0][i], c);
                }
                return temp;
            }
        }
        
        internal int _MaxKeyLength = -1;
        
        /// <summary>
        /// Returns the maximum allowed Key length on this server or -1 if unknown
        /// </summary>
        public int MaxKeyLength {
            get {
                return _MaxKeyLength;
            }
        }
        
        internal int _MaxUserLength = -1;
        
        /// <summary>
        ///  Returns the Maximum allowed User length on this server or -1 if unknwon
        /// </summary>
        public int MaxUserLength {
            get {
                return _MaxUserLength;
            }
        }
        
        internal int _MaxHostLength = -1;

        /// <summary>
        ///  Returns the Maximum allowed Host length on this server or -1 if unknwon
        /// </summary>
        public int MaxHostLength {
            get {
                return _MaxHostLength;
            }
        }

        internal bool _Map = false;
        
        /// <summary>
        /// Returns true if we know this server supports the Map Command
        /// </summary>
        public bool Map {
            get {
                return _Map;
            }
        }
        
        internal bool _DccAllow = false;
        
        /// <summary>
        /// Server Supports the DccAllow Command
        /// </summary>
        public bool DccAllow {
            get {
                return _DccAllow;
            }
        }
    }
    
    public enum CaseMappingType {
        Unknown,
        Rfc1459,
        Rfc1459Strict,
        Ascii
            
    }
    
    [Flags]
    public enum ChannelModeType {
        WithUserhostParameter,
        WithAlwaysParamter,
        WithSetOnlyParameter,
        WithoutParameter
    }

    /// <summary>
    /// M = mask search,
    /// N = !mask search
    /// U = usercount search (< >)
    /// C = creation time search (C< C>)
    /// T = topic search (T< T>) 
    /// </summary>
    [Flags]
    public enum EListType {
        
        M,
        N,
        U,
        C,
        T
    }
}
