/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 20.12.2008
 * Zeit: 00:56
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;

namespace Huffelpuff.SimplePlugins
{
	/// <summary>
	/// Description of IAccess.
	/// </summary>
	public interface IAccessPlugin : IPlugin
	{
		string WhoIs(string nick);
	}
}
