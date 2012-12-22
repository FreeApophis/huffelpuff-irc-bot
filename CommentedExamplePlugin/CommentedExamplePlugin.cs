/*
 *  Commented Example Plugin, as a help for Plugin developers
 *  ---------------------------------------------------------
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 03.07.2009 18:54
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

/* 
 * To write a Plugin you need to set a Reference to the Huffelpuff-project 
 * and also to the SharpIRC Project.
 * If you work with the source, make a new Project for your Plugin and add
 * project reference to the Project.
 * If you work with the executable directly, Add a reference to the
 * Huffelpuff.exe And SharpIRC.dll
 * 
 * normally you need the following namesspaces from the project:
 */
using Huffelpuff;
using Huffelpuff.Plugins;


namespace Plugin
{
    /*
     * This Plugin should be an example to anyone who wants to write a plugin.
     * It should describe all the main stepts to write your own plugin, and describes
     * the intentions behinde several design decisions.
     * 
     * 1.) A Plugin will run in another Appdomain thant the mainbot, that was done
     * to make it possible to load, unload and reload Plugins on runtime. This makes
     * several things a bit difficult. But the Infrastructure is Abstracting that
     * away from you. The Plugin infrastrucutre is very simple, but has a very 
     * powerfull access to the IRC bot Infrastructure.
     * 
     */
    public class CommentedExamplePlugin : AbstractPlugin
    {

        /* 
         * The  constructor should always look like this, the base class Constructor
         * sets up most part of the infrastructure to use the Bot from a plugin.
         * It will setup the BotMethods and BotEvents Fields which are the most
         * important parts for the Plugins.
         * You dont need to do anything else in the constructor. Whatever you want
         * to initialize, use Init() for it!
         */
        public CommentedExamplePlugin(IrcBot botInstance) :
            base(botInstance) { }

        /*
         * AboutHelp() is the only function you have to override from the 
         * AbstractPlugin Class. If you implement the above Constructor
         * and this help Plugin. The Plugin is already able to compile and 
         * it will show up in the plugins list. If you move the compiled dll
         * into the plugins folder of your Bot.
         */
        public override string AboutHelp()
        {
            return "This is the help about the whole CommentedExamplePlugin";
        }

        /*
         * Anything you want to do during initialisation, or what your normally
         * want to do in a constructor, you do it here!
         * 
         * This method is only called once during the lifecylce of the object.
         * Either when the bot loads the plugins initially, or when all the
         * plugins get reloaded.
         * 
         * TIPS:
         * * You dont have to override this method for a working Plugin.
         * NEEDED:
         * * Call base.Activate() at the end of the Method.
         * 
         */
        public override void Init()
        {
            // OnTick should be called every 60 seconds.
            TickInterval = 60;

            base.Init();
        }

        /* 
         * TIPS:
         * * You dont have to override this method for a working Plugin.
         * NEEDED:
         * * Call base.Activate() at the end of the Method.
         * 
         */
        public override void Activate()
        {
            base.Activate();
        }

        /* 
         * TIPS:
         * * You dont have to override this method for a working Plugin.
         * NEEDED:
         * * Call base.Activate() at the end of the Method.
         * 
         */
        public override void Deactivate()
        {
            base.Deactivate();
        }

        /* 
         * TIPS:
         * * You dont have to override this method for a working Plugin.
         * NEEDED:
         * * Call base.Activate() at the end of the Method.
         * 
         */
        public override void DeInit()
        {
            base.DeInit();
        }

        /*
         * Often your Plugin needs to perform certain tasks, for that we can
         * offer you the OnTick Event which can be called in intervalls of
         * multiples of 30 seconds. We set in Init() the
         * TickInterval to 60 seconds. this method will therfore be called 
         * every 60 seconds (if the Plugins is activated)
         * 
         * TIPS:
         * * If you do not set TickInterval or set TickInterval = 0;
         *   This Method won't be called at all.
         */
        public override void OnTick()
        {

        }
    }
}