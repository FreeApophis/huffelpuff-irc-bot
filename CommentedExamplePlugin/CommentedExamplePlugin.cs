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
using System;
using System.Collections.Generic;

/* 
 * To write a Plugin you need to set a Reference to the Huffelpuff-project
 *  If you work with the source, make a new Project for your Plugin and add
 *  project reference to the Project.
 *  If you work with the executable directly, Add a reference to the
 *  Huffelpuff.exe
 * 
 *  normally you need the following namesspaces from the project:
 */
using Huffelpuff;
using Huffelpuff.Plugins;
using Meebey.SmartIrc4net;


namespace Plugin
{
    /*
     * This Plugin should be an example to anyone who wants to write a plugin.
     * It should describe all the main stepts to write an own plugin, and describes
     * the intentions behinde several design decisions.
     * 
     * 1.) A Plugin will run in another Appdomain thant the mainbot, that was done
     * to make it possible to load, unload and reload Plugins on runtime. This makes
     * several things a bit difficult.
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
            base(botInstance) {}
        
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
        */
        public override void Init()
        {
            
            base.Init();
        }
    }
}