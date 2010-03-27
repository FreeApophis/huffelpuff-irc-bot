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
using Huffelpuff.Database;
using Huffelpuff.Utils;
using Meebey.SmartIrc4net;

namespace Huffelpuff.Plugins
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
		
		/// <summary>
		/// Please only implement a Constructor with one Argument of type IrcBot
		/// </summary>
		/// <param name="botInstance"></param>
		public AbstractPlugin(IrcBot botInstance)
		{
			BotMethods = botInstance;
			BotEvents = new SharedClientSide(botInstance);
			
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(AppDomain_CurrentDomain_DomainUnload);
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
			AppDomain.CurrentDomain.DomainUnload -= new EventHandler(AppDomain_CurrentDomain_DomainUnload);

			BotEvents.Unload();
			
			if (this.active)
			{
				this.Deactivate();
			}
			this.DeInit();
			
		}
		
		public void InvokeHandler(string HandlerName, IrcEventArgs e) 
		{
			object[] IrcEventParameters = new object[] {this, e};
			Log.Instance.Log("InovkeHandler in " + this.GetType() + " calls " + HandlerName);
			try
			{
				this.GetType().GetMethod(HandlerName,  BindingFlags.Public |  BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, IrcEventParameters);
			}
			catch (Exception ex)
			{
				Log.Instance.Log("Handler " + HandlerName + "in" + this.GetType() + " has thrown an Exception: \n" + ex.Message + "\n" + ex.InnerException.Message, Level.Error, ConsoleColor.Red);
			}
		}
	
		public string MainClass {
			get {
				string[] parts = FullName.Split(new [] { '.' });
				return parts[parts.Length - 1];
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string FullName
		{
			get 
			{
				return this.GetType().FullName;
			}
		}
		
		public string ShortName
		{
			get 
			{
				return this.GetType().FullName;
			}
		}
		
		public string AssemblyName 
		{
			get
			{
				return this.GetType().Assembly.FullName;
			}
		}
		
		public virtual string Name{
			get 
			{
				return this.GetType().Assembly.FullName;
			}
		}
		
		public virtual bool Ready{
			get 
			{
				return ready;
			}
		}
		public virtual bool Active{
			get 
			{
				return active;
			}
		}
		
		public virtual void Init()
		{
			ready = true;
		}
		
		public virtual void Activate() 
		{
			active = true;
		}
		
		public virtual void Deactivate()
		{
			active = false;
		}
		
		public virtual void DeInit()
		{
			ready = false;
		}
		
		public abstract string AboutHelp();

		
	}
}
