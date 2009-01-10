/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 06.10.2008
 * Zeit: 18:35
 * 
 * Sie k�nnen diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader �ndern.
 */

using System;
using System.Collections.Generic;

namespace Huffelpuff.SimplePlugins
{
	/// <summary>
	/// Description of IPlugins.
	/// </summary>
	public interface IPlugin
	{
		    string Name{get;}
		    bool Ready{get;}
		    bool Active{get;}
			
		    void Init(IrcBot botInstance);
		    void Activate();
		    void Deactivate();
		    void DeInit();
		    
		    string AboutHelp();
	}
}
