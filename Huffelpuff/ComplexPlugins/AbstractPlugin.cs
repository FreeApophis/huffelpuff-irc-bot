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
		
		public AbstractPlugin(IrcBot botInstance)
		{
			BotMethods = botInstance;
			BotEvents = new SharedClientSide(botInstance);
		}
	
		public abstract string Name{get;}
		public abstract bool Ready{get;}
		public abstract bool Active{get;}
			
		public abstract void Init();
		public abstract void Activate();
		public abstract void Deactivate();
		//public abstract void DeInit();
		    
		public abstract string AboutHelp();

		
	}
}
