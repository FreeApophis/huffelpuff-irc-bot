/*
 *  This is the Factoid Plugin, the usage is very similar to ubottu.
 *  You can generate arbitrary !commands to reply with prefabricated text.
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
using System.Linq;

using Huffelpuff;
using Huffelpuff.Database;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;

namespace FactoidPlugin
{
    /// <summary>
    /// Supports !factoid, !factoid > nick, !factoid | nick
    /// </summary>
    public class FactoidPlugin : AbstractPlugin
    {
        public FactoidPlugin(IrcBot botInstance) : base(botInstance) { }

        public override void Init()
        {

            base.Init();
        }

        public override void Activate()
        {
            BotMethods.AddCommand(new Commandlet("!facts", "Lists all the facts.", listFacts, this, CommandScope.Both));
            BotMethods.AddCommand(new Commandlet("!+fact", "With the command !+fact <fact> <descriptive text> you can add a fact with its description, or you can link a new fact to the same description with !+fact <newfact> <oldfact>. You can add dynamic paramters with %n (where n = 1,2...).", addFact, this, CommandScope.Both, "fact_admin"));
            BotMethods.AddCommand(new Commandlet("!-fact", "With the command !-fact <fact> you can remove a fact.", removeFact, this, CommandScope.Both, "fact_admin"));
            BotEvents.OnQueryMessage += BotEvents_OnQueryMessage;
            BotEvents.OnChannelMessage += BotEvents_OnQueryMessage;

            base.Activate();
        }

        public override void Deactivate()
        {
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
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length > 0 && e.Data.MessageArray[0].StartsWith("!"))
            {
                var factKey = BotMethods.Db.FactKeys.Where(facts => facts.Key == e.Data.MessageArray[0].Substring(1)).SingleOrDefault();
                if (factKey != null)
                {
                    factKey.HitCount++;
                    var factValue = BotMethods.Db.FactValues.Where(facts => facts.FactKeyID == factKey.ID).SingleOrDefault();
                    if (factValue != null)
                    {
                        string answer = factValue.Value;
                        int count = 0;
                        foreach (var parameter in e.Data.MessageArray.Skip(1))
                        {
                            count++;
                            answer = answer.Replace("%" + count.ToString(), parameter);
                        }
                        BotMethods.SendMessage(SendType.Message, sendto, answer);

                    }
                }
                BotMethods.Db.SubmitChanges();
            }
        }

        private void listFacts(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (BotMethods.Db.FactKeys.Count() == 0)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "There are no facts yet, add them with !+fact");
                return;
            }
            foreach (var line in BotMethods.Db.FactKeys.Select(fact => fact.Key).ToLines(350, ", ", "All Facts: ", ""))
            {
                BotMethods.SendMessage(SendType.Message, sendto, line);
            }
        }

        private void addFact(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 3)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'add'! Try '!help !+fact'.");
                return;
            }

            if (BotMethods.Db.FactKeys.Where(facts => facts.Key == e.Data.MessageArray[1]).SingleOrDefault() == null)
            {
                var factKey = new FactKey();
                factKey.Key = e.Data.MessageArray[1];
                BotMethods.Db.FactKeys.InsertOnSubmit(factKey);
                BotMethods.Db.SubmitChanges();

                var factValue = new FactValue();
                factValue.FactKeyID = factKey.ID;
                factValue.IrcUserID = 0;
                factValue.Value = e.Data.Message.Substring(e.Data.MessageArray[0].Length + e.Data.MessageArray[1].Length + 2);
                BotMethods.Db.FactValues.InsertOnSubmit(factValue);
                BotMethods.Db.SubmitChanges();

                BotMethods.SendMessage(SendType.Message, sendto, "New Fact '" + factKey.Key + "' learned. Activate it with !" + factKey.Key + ".");
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Sorry, I know that fact already.");
            }

        }

        private void removeFact(object sender, IrcEventArgs e)
        {
            string sendto = (string.IsNullOrEmpty(e.Data.Channel)) ? e.Data.Nick : e.Data.Channel;

            if (e.Data.MessageArray.Length < 2)
            {
                BotMethods.SendMessage(SendType.Message, sendto, "Too few arguments for 'remove'! Try '!help !-fact'.");
                return;
            }
            var factKey = BotMethods.Db.FactKeys.Where(facts => facts.Key == e.Data.MessageArray[1]).SingleOrDefault();
            if (factKey != null)
            {
                var factValue = BotMethods.Db.FactValues.Where(facts => facts.FactKeyID == factKey.ID).SingleOrDefault();
                if (factValue != null)
                {
                    BotMethods.Db.FactValues.DeleteOnSubmit(factValue);
                }
                BotMethods.Db.FactKeys.DeleteOnSubmit(factKey);

                BotMethods.SendMessage(SendType.Message, sendto, "I forgot Fact '" + factKey.Key + "'.");

                BotMethods.Db.SubmitChanges();
            }
            else
            {
                BotMethods.SendMessage(SendType.Message, sendto, "I don't know fact '" + factKey.Key + "'.");
            }
        }
    }
}