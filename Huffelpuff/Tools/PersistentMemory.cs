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
    /// Easy access to persistent data through an easy OO interface.
    /// 
    /// Its mainly intended as a very simple config tool, but can also be used
    /// to track data over multiple sessions!
    /// 
    /// Very simple implementation with DataSet and XML Documents.
    /// Other Interfaces like SQL should be easy to implement.
    /// The usability is very restricted due the simplicity of the DB Scheme.
    /// This is intended! dont use it for huge Databases, use a Database instead!
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
        
        /// <summary>
        /// Makes sure that the Database is written to disk (ie. XML)
        /// </summary>
        public static void Flush()
        {
            memory.WriteXml(filename);
        }

        /// <summary>
        /// Does this key-value pair exists?
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if it exists</returns>
        public static bool Exists(string key, string value) {
            return Exists(config, key, value);
        }

        /// <summary>
        /// Does this key-value pair exists in domain group?
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if it exists</returns>
        public static bool Exists(string group, string key, string value) {
            return (0!=memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "' AND " + baseValue + " = '" + value + "'").Length);
        }
        
        /// <summary>
        /// Returns Value (first if there are multiple) for default domain, or null if it does not exist.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Single Value</returns>
        public static string GetValue(string key) {
            return GetValue(config, key);
        }
        
        /// <summary>
        /// Returns Value (first if there are multiple) for key in domain group, or null if it does not exist.
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>Single Value</returns>
        public static string GetValue(string group, string key) {
            List<string> t = GetValues(group, key);
            if (t.Count == 0) {
                return null;
            } else {
                return t[0];
            }
        }

        /// <summary>
        /// Returns Values as a List for a key in default domain, or the empty list if it does not exist.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>List of Values</returns>
        public static List<string> GetValues(string key) {
            return GetValues(config, key);
        }
        
        /// <summary>
        /// Returns Values as a List for a key in the domain group, or the empty list if it does not exist.
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>List of Values</returns>
        public static List<string> GetValues(string group, string key) {
            List<string> vals = new List<string>();
            DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "'");
            foreach(DataRow dr in rows) {
                vals.Add((string)dr[baseValue]);
            }
            return vals;
        }
        
        /// <summary>
        /// Non-Idempotent Set Value (if you set twice,you'll get a list back) in default domain
        /// </summary>
        /// <param name="key">Key Value</param>
        /// <param name="value"></param>
        /// <returns>Returns true when successfull</returns>
        public static bool SetValue(string key, string value)
        {
            return SetValue(config, key, value);
        }
        
        /// <summary>
        /// Non-Idempotent Set Value (if you set twice,you'll get a list back) in Domain group
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true when successfull</returns>
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
            return false;
        }
        
        /// <summary>
        /// Remove Key Value Pair in default Domain
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least on element was removed</returns>
        public static bool RemoveValue(string key, string value) {
            return RemoveValue(config, key, value);
        }

        /// <summary>
        /// Remove Key Value Pair in default Domain
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least on element was removed</returns>
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
            return false;
        }
        
        /// <summary>
        /// Remove Multiple Values for a certain group and key for values which start with beginofvalue
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="beginOfValue">Value which begins with this String will be removed</param>
        public static bool RemoveValueStartingWith(string key, string beginOfValue) {
            return RemoveValueStartingWith(config, key, beginOfValue);
        }
        
        /// <summary>
        /// Remove Multiple Values for a certain group and key for values which start with beginofvalue
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="beginOfValue">Value which begins with this String will be removed</param>
        public static bool RemoveValueStartingWith(string group, string key, string beginOfValue) {
            if (memory.Tables.Contains(baseTable))
            {
                bool removed = false;
                DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "'");
                foreach (DataRow dr in rows) {
                    if(((string)dr[baseValue]).StartsWith(beginOfValue)) {
                        memory.Tables[baseTable].Rows.Remove(dr);
                        removed = true;
                    }
                }
                return removed;
            }
            return false;
        }
        
        /// <summary>
        /// Replaces key with a new value in the default Domain, if multiple old values exist only one new value will exist after this operation
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least one Value was removed, false if the key is new</returns>
        public static bool ReplaceValue(string key, string value) {
            return ReplaceValue(config, key, value);
        }
        
        /// <summary>
        /// Replaces key with a new value in the Domain group, if multiple old values exist only one new value will exist after this operation
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least one Value was removed, false if the key is new</returns>
        public static bool ReplaceValue(string group, string key, string value) {
            bool removed = RemoveKey(group, key);
            SetValue(group, key, value);
            return removed;
        }
        
        /// <summary>
        /// Removes all values with the specified key in default Domain
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if at least one element was removed</returns>
        public static bool RemoveKey(string key)
        {
            return RemoveKey(config, key);
        }

        /// <summary>
        /// Removes all values with the specified key in the Domain group
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>True if at least one element was removed</returns>
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

        /// <summary>
        /// Constructor
        /// </summary>
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
