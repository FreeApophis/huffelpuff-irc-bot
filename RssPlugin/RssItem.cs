/*
 * Erstellt mit SharpDevelop.
 * Benutzer: apophis
 * Datum: 27.01.2009
 * Zeit: 00:23
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;

namespace Plugin
{
    /// <summary>
    /// Description of RssItem.
    /// </summary>
    public class RssItem
    {
        public RssItem(string title, string author, DateTime published, string link, string desc, string category, string content)
        {
            this.title = title;    
            this.author = author;    
            this.published = published;  
            this.link = link;
            this.desc = desc;
            this.category = category;
            this.content = content;
        }

        private string link; 
        
        public string Link {
            get {
                return link;
            }
        }
        
        private string desc; 
        
        public string Desc {
            get { 
                return desc; 
            }
        }
        
        private string category;
        
        public string Category {
            get { 
                return category;
            }
        }
        
        private string content;
        
        public string Content {
            get { 
                return content;
            }
        }
        
        private string title;
            
        public string Title {
            get {
                return title;
            }
        }

        private string author;

        public string Author {
            get {
                return author;
            }
        }
        
        private DateTime published;

        public DateTime Published {
            get {
                return published;
            }
        }
    }
}
