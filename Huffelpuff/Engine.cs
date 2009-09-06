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
using System.Threading;
using Meebey.SmartIrc4net;

using Huffelpuff.Tools;

namespace Huffelpuff
{
    class Engine
    {
        public static void Main(string[] args)
        {
            Tool.RunOnMono();
            IrcBot bot = new IrcBot();
            
            // check for basic settings
            PersistentMemory.Instance.GetValuesOrTodo("ServerHost");
            PersistentMemory.Instance.GetValuesOrTodo("ServerPort");
            PersistentMemory.Instance.GetValueOrTodo("nick");
            PersistentMemory.Instance.GetValueOrTodo("realname");
            PersistentMemory.Instance.GetValueOrTodo("username");
            
            if(PersistentMemory.Todo) {
                PersistentMemory.Instance.Flush();
                Log.Instance.Log("Edit your config file: there are some TODOs left.", Level.Fatal);
                bot.Exit();
            }
            bot.Start();  /*blocking*/
        }
    }
}
