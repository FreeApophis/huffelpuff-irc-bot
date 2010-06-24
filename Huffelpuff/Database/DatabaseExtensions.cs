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

namespace Huffelpuff.Database
{
	/// <summary>
	/// Description of DatabaseExtensions.
	/// </summary>
	public static class DatabaseExtensions
	{
		public static string GetKey(this IEnumerable<Setting> settings, string key)
		{
			var setting = settings.Where(s => s.Key == key).SingleOrDefault();
			return setting == null ? null : setting.Value;
		}

		public static bool SetKey(this IEnumerable<Setting> settings, string key, string value)
		{
			var setting = settings.Where(s => s.Key == key).SingleOrDefault();
			var result = setting != null;
			
			if (result)
			{
				setting.Key = key;
				setting.Value = value;
			} 
			else
			{
				setting = new Setting();
				setting.Key = key;
				setting.Value = value;
				DatabaseCommon.Db.Settings.InsertOnSubmit(setting);
			}
			
			DatabaseCommon.Db.SubmitChanges();
			return result;
		}
		
		public static bool RemoveKey(this IEnumerable<Setting> settings, string key)
		{
			var setting = settings.Where(s => s.Key == key).SingleOrDefault();
			var result = setting != null;
			if (result)
			{
				DatabaseCommon.Db.Settings.DeleteOnSubmit(setting);
				DatabaseCommon.Db.SubmitChanges();
			}
			return result;
		}
		
	}
}
