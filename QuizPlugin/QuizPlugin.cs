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


using System.IO;
using Huffelpuff;
using Huffelpuff.Plugins;
using Huffelpuff.Utils;
using Plugin.Database.Quiz;
using SharpIrc;

namespace Plugin
{
    /// <summary>
    /// Description of QuizPlugin.
    /// </summary>
    public class QuizPlugin : AbstractPlugin
    {
        public QuizPlugin(IrcBot botInstance) :
            base(botInstance)
        { }

        public override string Name
        {
            get
            {
                return "Quiz Bot";
            }
        }
        private Main quizData;

        public override void Init()
        {
            quizData = new Main(DatabaseConnection.Create("Quiz"));

            //BotMethods.AddExportedCommand(new Commandlet("import-kewlquiz", "Import Kewlquiz File", ImportHandler, this));

            base.Init();
        }

        private void ImportHandler(object sender, IrcEventArgs e)
        {
            if (e != null) { return; }

            var importer = new KewlQuizImport();

            importer.ImportFile(new FileInfo("Z:\\Chats\\mirc\\kewlquiz\\Apophis.txt"), quizData);
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override string AboutHelp()
        {
            return "";
        }
    }
}