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
using System.Collections.Generic;

using Meebey.SmartIrc4net;

namespace Huffelpuff.ComplexPlugins
{
	/// <summary>
	/// Description of AbstractPlugin.
	/// </summary>
	public abstract class AbstractPlugin : MarshalByRefObject
	{
		protected IrcBot BotMethods;
		protected SharedClientSide BotEvents;
		protected bool ready;
		protected bool active;
		
		public AbstractPlugin(IrcBot botInstance)
		{
			BotMethods = botInstance;
			BotEvents = new SharedClientSide(botInstance);
						
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(AppDomain_CurrentDomain_DomainUnload);
		}

		void AppDomain_CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
		    AppDomain.CurrentDomain.DomainUnload -= new EventHandler(AppDomain_CurrentDomain_DomainUnload);

		    BotEvents.Unload();
		    
		    this.Deactivate();
		    this.DeInit();
		    
		}
		
		public void InvokeHandler(string HandlerName, IrcEventArgs e) {
		    object[] IrcEventParameters = new object[] {this, e};
		    Console.WriteLine(this.GetType());
		    this.GetType().GetMethod(HandlerName,  BindingFlags.Public |  BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, IrcEventParameters);
		}
	
		public string FullName{
		    get {
		        return this.GetType().FullName;
		    }
		}
		public abstract string Name{get;}
		
		public virtual bool Ready{
		    get {
		        return ready;
		    }
		}
		public virtual bool Active{
		    get {
		        return active;
		    }
		}
			
		public virtual void Init() {
		    ready = true;
		}
		
		public abstract void Activate();
		
		public abstract void Deactivate();
		
		public virtual void DeInit() {
		    ready = false;
		}
		    
		public abstract string AboutHelp();

		
	}
}
