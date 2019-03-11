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
using System.Reflection;
using SharpIrc;
using Huffelpuff.Utils;

namespace Huffelpuff.Plugins
{
    /// <summary>
    /// Description of AbstractPlugin.
    /// </summary>
    public abstract class AbstractPlugin : MarshalByRefObject
    {
        protected IrcBot BotMethods;
        protected SharedClientSide BotEvents;
        private bool _ready;
        private bool _active;

        public const int MinTickInterval = 30;

        private int _tickInterval;

        /// <summary>
        /// the OnTick Event is called every TickInterval seconds. TickInterval set to 0 (Standard) when you don't need a tick event.
        /// </summary>
        public int TickInterval
        {
            get => _tickInterval;
            protected set
            {
                if (value <= 0)
                {
                    _tickInterval = 0;
                }
                else if (value < MinTickInterval)
                {
                    _tickInterval = MinTickInterval;
                }
                else
                {
                    _tickInterval = value - (value % MinTickInterval);
                }
            }
        }

        private int _timeUntilTick;
        public bool ShallITick(int secs)
        {
            if (!BotMethods.IsConnected)
                return false;

            if (_tickInterval == 0)
                return false;

            _timeUntilTick -= secs;
            if (_timeUntilTick <= 0)
            {
                _timeUntilTick = _tickInterval;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Please only implement a Constructor with one Argument of type IrcBot
        /// </summary>
        /// <param name="botInstance"></param>
        protected AbstractPlugin(IrcBot botInstance)
        {
            BotMethods = botInstance;
            BotEvents = new SharedClientSide(botInstance);

            AppDomain.CurrentDomain.DomainUnload += AppDomain_CurrentDomain_DomainUnload;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// If we get unloaded we want to make sure that no references to this appDomain survive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AppDomain_CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.DomainUnload -= AppDomain_CurrentDomain_DomainUnload;

            BotEvents.Unload();

            if (_active)
            {
                Deactivate();
            }
            DeInit();

        }

        public void InvokeHandler(string handlerName, EventArgs e)
        {
            var ircEventParameters = new object[] { this, e };
            Log.Instance.Log("Invoke handler in " + GetType() + " calls " + handlerName);
            try
            {
                GetType().GetMethod(handlerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(this, ircEventParameters);
            }
            catch (Exception ex)
            {
                Log.Instance.Log("Handler " + handlerName + "in" + GetType() + " has thrown an Exception: \n" + ex.Message + "\n" + ex.InnerException?.Message, Level.Error, ConsoleColor.Red);
            }
        }

        public string MainClass
        {
            get
            {
                string[] parts = FullName.Split('.');
                return parts[parts.Length - 1];
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string FullName => GetType().FullName;

        public string ShortName => GetType().FullName;

        public string AssemblyName => GetType().Assembly.FullName;

        public virtual string Name => GetType().Assembly.FullName;

        public virtual bool Ready => _ready;

        public virtual bool Active => _active;

        public virtual void Init()
        {
            _ready = true;
        }

        public virtual void Activate()
        {
            _active = true;
        }

        public virtual void Deactivate()
        {
            _active = false;
        }

        public virtual void DeInit()
        {
            _ready = false;
        }

        public virtual void OnTick()
        {

        }

        public abstract string AboutHelp();


    }
}
