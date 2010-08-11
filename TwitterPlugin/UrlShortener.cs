/*
 *  The Huffelpuff Irc Bot, versatile pluggable bot for IRC chats
 * 
 *  Copyright (c) 2008-2010 Thomas Bruderer <apophis@apophis.ch>
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

using System.Collections.Generic;
using System.Linq;
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
        public static string GetTinyUrl(string url)
        {
            var replace = new Dictionary<string, string>();
            foreach (var word in url.Split(new[] { ' ' }))
            {
                if ((word.Length <= 23) || ((!word.StartsWith("http://")) && (!word.StartsWith("https://")))) continue;

                var tinyurl = new WebClient();
                tinyurl.QueryString.Add("url", HttpUtility.UrlEncode(word));
                var tiny = tinyurl.DownloadString("http://tinyurl.com/api-create.php");

                if (!replace.ContainsKey(word))
                {
                    replace.Add(word, tiny);
                }
            }

            return replace.Aggregate(url, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }

        /// <summary>
        /// U.nu the shortest URLs possible!
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUnuUrl(string url)
        {
            var replace = new Dictionary<string, string>();
            foreach (var word in url.Split(new[] { ' ' }))
            {
                if ((word.Length <= 16) || ((!word.StartsWith("http://")) && (!word.StartsWith("https://")))) continue;

                var unu = new WebClient();
                unu.QueryString.Add("url", HttpUtility.UrlEncode(word));
                var tiny = unu.DownloadString("http://u.nu/unu-api-simple");

                if (!tiny.StartsWith("NO_RECURSION") && !replace.ContainsKey(word))
                {
                    replace.Add(word, tiny);
                }
            }

            return replace.Aggregate(url, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }
    }
}
