/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 03.01.2009
 * Zeit: 00:28
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
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
