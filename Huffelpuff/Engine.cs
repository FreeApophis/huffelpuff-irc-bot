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
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Huffelpuff.Plugins;
using Huffelpuff.Properties;
using Huffelpuff.Utils;

namespace Huffelpuff
{
    class Engine
    {
        public static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                IrcBot bot;

                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "--install":
                            ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                            return;
                        case "--uninstall":
                            ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                            return;
                        case "--simulator":

                            break;
                        case "--config":
                            Tool.Configure();
                            return;
                        case "--help":
                            return;
                        case "--plugin-command":
                            bot = new IrcBot();
                            var manager = new BotPluginManager(bot, "plugins");
                            manager.StartUp();
                            bot.CallExportedCommand(args[1], string.Concat(args.Skip(2)), manager);
                            manager.ShutDown();
                            return;
                    }
                }
                Tool.RunOnMono();

                do
                {
                    bot = GetBot();

                    if (bot == null)
                    {
                        break;
                    }

                } while (bot.Start());

                Settings.Default.Save();
            }
            else
            {

                try
                {
                    var servicesToRun = new ServiceBase[] { new ServiceEngine { Bot = GetBot() } };
                    ServiceBase.Run(servicesToRun);
                }
                catch (Exception exception)
                {
                    Log.Instance.Log(exception);
                }
            }
        }

        private static IrcBot GetBot()
        {
            var bot = new IrcBot();
            return bot;
        }
    }
}


