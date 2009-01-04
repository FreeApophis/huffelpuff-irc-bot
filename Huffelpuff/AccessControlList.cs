/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 19.11.2008
 * Zeit: 02:18
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;

namespace Huffelpuff
{
    /// <summary>
    /// Description of AccessControlList.
    /// </summary>
    public class AccessControlList
    {
        public AccessControlList()
        {
        }
        
        public bool Access(string nick, string command)
        {
            return false;
        }
    }
    
    public class AccessRights
    {
        public virtual bool hasAccess(string Nick) {
            return false;
        }
    }
    
    public class ChannelRightAccess : AccessRights
    {
        
    }
    
    public class NickServAccess : AccessRights
    {    
        
    }
    
    public class PasswordAccess : AccessRights
    {
        
    }
    
    public class GuestAccess : AccessRights
    {
        public override bool hasAccess(string Nick) {
            return true;
        }
    }
    
}
