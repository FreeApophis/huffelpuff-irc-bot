/*
 * $Id: NonRfcChannelUser.cs 274 2008-06-02 19:10:11Z meebey $
 * $URL: svn://svn.qnetp.net/smartirc/SmartIrc4net/trunk/src/IrcClient/NonRfcChannelUser.cs $
 * $Rev: 274 $
 * $Author: meebey $
 * $Date: 2008-06-02 21:10:11 +0200 (Mon, 02 Jun 2008) $
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class NonRfcChannelUser : ChannelUser
    {
        private bool _IsHalfop;    //+h - halfop (%)
        private bool _IsOwner;    //+q - owner (*)
        private bool _IsAdmin;    //+a - admin/protect (!)
        
                /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircuser"> </param>
        internal NonRfcChannelUser(string channel, IrcUser ircuser) : base(channel, ircuser)
        {
        }

#if LOG4NET
        ~NonRfcChannelUser()
        {
            Logger.ChannelSyncing.Debug("NonRfcChannelUser ("+Channel+":"+IrcUser.Nick+") destroyed");
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsAdmin {
            get {
                return _IsAdmin;
            }
            set {
                _IsAdmin = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsHalfop {
            get {
                return _IsHalfop;
            }
            set {
                _IsHalfop = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsOwner {
            get {
                return _IsOwner;
            }
            set {
                _IsOwner = value;
            }
        }

    }
}
