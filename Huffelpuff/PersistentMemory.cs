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
using System.IO;
using System.Xml;
using System.Data;
using System.Collections.Generic;

namespace Huffelpuff
{
	/// <summary>
	/// Description of PersistentMemory.
	/// </summary>
	public sealed class PersistentMemory
	{
		private static PersistentMemory instance = new PersistentMemory();
		private static System.Data.DataSet memory;
		private const string filename = "pmem.xml";
		private const string baseTable = "baseTBL";
		private const string baseGroup = "baseGroup";
		private const string baseKey = "baseKey";
		private const string baseValue = "baseValue";
		private const string config = "config";
		
		/// <summary>
		/// If you want to manipulate directly on the Dataset, you get the full functionality
		/// </summary>
		public static DataSet RawData {
			get {
				return memory;
			}
		}
		
		public void CreateBase()
		{
			if (!memory.Tables.Contains(baseTable))
			{
				try {
				    memory.Tables.Add(new DataTable(baseTable));
				    memory.Tables[baseTable].Columns.Add(new DataColumn(baseGroup));
				    memory.Tables[baseTable].Columns.Add(new DataColumn(baseKey));
				    memory.Tables[baseTable].Columns.Add(new DataColumn(baseValue));
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
			Flush();
		}
		
		public static void Flush()
		{
			memory.WriteXml(filename);
		}

		public static bool Exists(string key, string value) {
			return Exists(config, key, value);
		}

		public static bool Exists(string group, string key, string value) {
			return (0!=memory.Tables[baseTable].Select(baseGroup + " = " + group + " AND " + baseKey + " = " + key + " AND " + baseValue + " = " + value).Length);
		}
		
		public static string GetValue(string key) {
			return GetValue(config, key);
		}

		public static string GetValue(string group, string key) {
			List<string> t = GetValues(group, key);
			if (t.Count == 0)
				return null;
			else
				return t[0];
		}

		public static List<string> GetValues(string key) {
			return GetValues(config, key);
		}

		public static List<string> GetValues(string group, string key) {
			List<string> vals = new List<string>();
			DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "'");
			foreach(DataRow dr in rows) {
				vals.Add((string)dr[baseValue]);
			}			
			return vals;
		}
		
		/// <summary>
		/// Non-Idempotent Set Value (if you set twice,you'll get a list back)
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool SetValue(string key, string value)
		{
			return SetValue(config, key, value);
		}
		
		public static bool SetValue(string group, string key, string value)
		{
			if (memory.Tables.Contains(baseTable))
			{
				DataRow dr = memory.Tables[baseTable].NewRow();
				dr[baseGroup] = group;
				dr[baseKey] = key;
				dr[baseValue] = value;
				memory.Tables[baseTable].Rows.Add(dr);
				return true;
			}		    
			else 
				return false;
		}
		
		public static bool RemoveValue(string key, string value) {
			return RemoveValue(config, key, value);
		}
		
		public static bool RemoveValue(string group, string key, string value) {		
			if (memory.Tables.Contains(baseTable))
			{
				bool removed = false;
				DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "' AND " + baseValue + " = '" + value + "'");
				foreach (DataRow dr in rows) {
					memory.Tables[baseTable].Rows.Remove(dr);
					removed = true;
				}
				return removed;
			}		    
			else 
				return false;
		}
		
		public static bool RemoveKey(string key)
		{
			return RemoveKey(config, key);
		}

		public static bool RemoveKey(string group, string key)
		{
			if (memory.Tables.Contains(baseTable))
			{
				bool removed = false;
				DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "'");
				foreach (DataRow dr in rows) {
					memory.Tables[baseTable].Rows.Remove(dr);
					removed = true;
				}
				return removed;
			}		    
			else 
				return false;
		}

		private PersistentMemory()
		{				
			FileInfo fi = new FileInfo(PersistentMemory.filename);
			XmlDataDocument xdoc = new XmlDataDocument();
			if (fi.Exists) {		
				xdoc.DataSet.ReadXml(filename);
				memory = xdoc.DataSet;
			} else {
				memory = new DataSet();
			}
			CreateBase();
		}
	}
}
