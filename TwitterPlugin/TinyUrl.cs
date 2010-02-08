/*
 *  <project description>
 * 
 *  Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 *  File created by apophis at 04.07.2009 16:31
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
using System.Net;
using System.Web;

namespace Plugin
{
    /// <summary>
    /// Urlshortener make a short redirect url to an arbitrary long external url.
    /// </summary>
    public class UrlShortener
    {
        /// <summary>
        /// TinyUrl, the big grandfater of the urlshortneres, not the shortest ones.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetTinyUrl(string url) {
            Dictionary<string, string> replace = new Dictionary<string, string>();
            foreach(string word in url.Split(new char[] {' '})) {
                if ((word.Length > 23) && (word.StartsWith("http://"))) {
                    WebClient tinyurl = new WebClient();
                    tinyurl.QueryString.Add("url", HttpUtility.UrlEncode(word));
                    string tiny = tinyurl.DownloadString("http://tinyurl.com/api-create.php");
                    if (!replace.ContainsKey(word)) {
                        replace.Add(word, tiny);
                    }
                }
            }
            
            foreach(KeyValuePair<string, string> kvp in replace) {
                url = url.Replace(kvp.Key, kvp.Value);
            }
            
            return url;
        }

        /// <summary>
        /// U.nu the shortest URLs possible!
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUnuUrl(string url) {
            Dictionary<string, string> replace = new Dictionary<string, string>();
            foreach(string word in url.Split(new char[] {' '})) {
                if ((word.Length > 16) && (word.StartsWith("http://"))) {
                    WebClient unu = new WebClient();
                    unu.QueryString.Add("url", HttpUtility.UrlEncode(word));
                    string tiny = unu.DownloadString("http://u.nu/unu-api-simple");
                    if (!replace.ContainsKey(word)) {
                        replace.Add(word, tiny);
                    }
                }
            }
            
            foreach(KeyValuePair<string, string> kvp in replace) {
                url = url.Replace(kvp.Key, kvp.Value);
            }
            
            return url;
        }
    }
}
