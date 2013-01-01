/*
 *  This is the Factoid Plugin, the usage is very similar to ubottu.
 *  You can generate arbitrary !commands to reply with prefabricated text.
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
using System.Data.SQLite;
using System.Linq;
using apophis.SharpIRC;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Plugin.Database.Factoid;

namespace Plugin
{
    /// <summary>
    /// Supports !factoid, !factoid > nick, !factoid | nick
    /// </summary>
    public class FactoidPlugin : AbstractPlugin
    {
        public FactoidPlugin(IrcBot botInstance) : base(botInstance) { }

        private Main factoidData;

        public override void Init()
        {
            factoidData = new Main(new SQLiteConnection("Data Source=Factoid.s3db;FailIfMissing=true;"));

            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!facts", "Lists all the facts.", ListFacts, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!+fact", "With the command !+fact <fact> <descriptive text> you can add a fact with its description, or you can link a new fact to the same description with !+fact <newfact> <oldfact>. You can add dynamic paramters with %n (where n = 1,2...).", AddFact, this, CommandScope.Both, "fact_admin"));
            BotMethods.AddCommand(new Commandlet("!-fact", "With the command !-fact <fact> you can remove a fact.", RemoveFact, this, CommandScope.Both, "fact_admin"));
            BotEvents.OnQueryMessage += BotEvents_OnQueryMessage;
            BotEvents.OnChannelMessage += BotEvents_OnQueryMessage;

            base.Activate();
        }

        public override void Deactivate()
        {
            BotMethods.RemoveCommand("!facts");
            BotMethods.RemoveCommand("!+fact");
            BotMethods.RemoveCommand("!-fact");

            BotEvents.OnQueryMessage -= BotEvents_OnQueryMessage;
            BotEvents.OnChannelMessage -= BotEvents_OnQueryMessage;

            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "This is the Factoid Plugin, the usage is very similar to ubottu. You can generate arbitrary !commands to reply with prefabricated text.";
        }

        void BotEvents_OnQueryMessage(object sender, IrcEventArgs e)
        {
            try
            {
                string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

                if (e.Data.MessageArray.Length <= 0 || !e.Data.MessageArray[0].StartsWith("!")) return;

                var fact = factoidData.Facts.Where(facts => facts.FactKey == e.Data.MessageArray[0].Substring(1)).SingleOrDefault();

                if (fact != null)
                {
                    fact.HitCount++;
                    var answer = fact.FactValue;
                    var count = 0;
                    foreach (var parameter in e.Data.MessageArray.Skip(1))
                    {
                        count++;
                        answer = answer.Replace("%" + count, parameter);
                    }
                    BotMethods.SendMessage(SendType.Message, sendto, answer);

                }
                factoidData.SubmitChanges();
            }
            catch (Exception exception)
            {
                Log.Instance.Log(exception);
            }
        }

        private void ListFacts(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (factoidData.Facts.Count() == 0)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "There are no facts yet, add them with !+fact");
                return;
            }
            foreach (var line in factoidData.Facts.Select(fact => fact.FactKey).ToLines(350, ", ", "All Facts: ", ""))
            {
                BotMethods.SendMessage(SendType.Message, sendto, line);
            }
        }

        private void AddFact(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 3)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help !+fact'.");
                return;
            }

            if (factoidData.Facts.Where(facts => facts.FactKey == e.Data.MessageArray[1]).SingleOrDefault() == null)
            {
                var message = e.Data.Message.Substring(e.Data.MessageArray[0].Length + e.Data.MessageArray[1].Length + 2);
                var fact = new Fact { FactKey = e.Data.MessageArray[1], FactValue = message };

                factoidData.Facts.InsertOnSubmit(fact);
                factoidData.SubmitChanges();

                BotMethods.SendMessage(SendType.Message, sendto, "New Fact '" + fact.FactKey + "' learned. Activate it with !" + fact.FactKey + ".");
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Sorry, I know that fact already.");
            }

        }

        private void RemoveFact(object sender, IrcEventArgs e)
        {
            var sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 2)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'remove'! Try '!help !-fact'.");
                return;
            }
            var fact = factoidData.Facts.Where(f => f.FactKey == e.Data.MessageArray[1]).SingleOrDefault();
            if (fact != null)
            {
                factoidData.Facts.DeleteOnSubmit(fact);

                BotMethods.SendMessage(SendType.Message, sendto, "I forgot Fact '" + fact.FactKey + "'.");

                factoidData.SubmitChanges();
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "I don't know fact '" + e.Data.MessageArray[1] + "'.");
            }
        }
    }
}