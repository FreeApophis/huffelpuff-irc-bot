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
using System.Data.SQLite;
using System.Threading;

namespace Huffelpuff.Database
{
    /// <summary>
    /// Description of DatabaseCommon.
    /// </summary>
    public class DatabaseCommon
    {
        private static readonly object O = new object();
        public static Main Db
        {
            get
            {
                lock (O)
                {
                    var mainObjects = AppDomain.CurrentDomain.GetData("DatabaseMainObjects") as Dictionary<int, Main>;

                    if (mainObjects == null)
                    {
                        mainObjects = new Dictionary<int, Main>();
                        AppDomain.CurrentDomain.SetData("DatabaseMainObjects", mainObjects);
                    }

                    Main main;
                    if (!mainObjects.TryGetValue(Thread.CurrentContext.ContextID, out main))
                    {
                        main = new Main(new SQLiteConnection("Data Source=huffelpuff.db;FailIfMissing=true;"));
                        mainObjects.Add(Thread.CurrentContext.ContextID, main);
                    }
                    return main;
                }
            }
        }
    }
}
