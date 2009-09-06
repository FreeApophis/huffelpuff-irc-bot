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


using Huffelpuff.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

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
    /// 
    /// * Added: Cross AppDomain Singleton (appDomain.SetData is used after creating
    ///   the Plugins AppDomain!
    /// </summary>
    public sealed class PersistentMemory : MarshalByRefObject
    {
        private static PersistentMemory instance = null;
        
        public static PersistentMemory Instance {
            get
            {
                if(instance == null)
                {
                    // try to pull out of the AppDomain data
                    instance = AppDomain.CurrentDomain.GetData("PersistentMemoryInstance") as PersistentMemory;

                    // if it wasn't there, we're the first AppDomain
                    // so create the instance
                    if(instance == null)
                        instance = new PersistentMemory();
                }

                return instance;
            }
        }
        
        private static System.Data.DataSet memory;
        private const string filename = "pmem.xml";
        private const string baseTable = "baseTBL";
        private const string baseGroup = "baseGroup";
        private const string baseKey = "baseKey";
        private const string baseValue = "baseValue";
        private const string config = "config";
        
        public const string todoValue = "TODO";
        
        /// <summary>
        /// If you want to manipulate directly on the Dataset, you get the full functionality
        /// </summary>
        public DataSet RawData {
            get {
                return memory;
            }
        }
        public static bool Todo {get; private set;}
        
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
                    Log.Instance.Log(e.Message, Level.Error);
                    Log.Instance.Log(e.StackTrace, Level.Error);
                }
            }
            Flush();
        }
        
        /// <summary>
        /// Makes sure that the Database is written to disk (ie. XML)
        /// </summary>
        public void Flush()
        {
            memory.WriteXml(filename);
        }

        /// <summary>
        /// Does this key-value pair exists?
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if it exists</returns>
        public bool Exists(string key, string value) {
            return Exists(config, key, value);
        }

        /// <summary>
        /// Does this key-value pair exists in domain group?
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if it exists</returns>
        public bool Exists(string group, string key, string value) {
            return (0!=memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "' AND " + baseValue + " = '" + value + "'").Length);
        }
        
        /// <summary>
        /// If a value is not set, this GetValue will write a TODO field into the config file, and also will return TODO instead of null. null wont be a possible value for these keys!
        /// This is meant to be used to create a config file, best to be used during the init of a plugin, because the start up of the bot will be blocked untill all TODO fields are filled!
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueOrTodo(string key) {
            string value = GetValue(key);
            if ((value == null) || (value == todoValue)) {
                Todo=true;
                ReplaceValue(key, todoValue);
                return todoValue;
            }
            return value;
        }
        
        /// <summary>
        /// Returns Value (first if there are multiple) for default domain, or null if it does not exist.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Single Value</returns>
        public string GetValue(string key) {
            return GetValue(config, key);
        }
        
        /// <summary>
        /// If a value is not set, this GetValue will write a TODO field into the config file, and also will return TODO instead of null. null wont be a possible value for these keys!
        /// This is meant to be used to create a config file, best to be used during the init of a plugin, because the start up of the bot will be blocked untill all TODO fields are filled!
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueOrTodo(string group, string key) {
            string value = GetValue(group, key);
            if ((value == null) || (value == todoValue)) {
                Todo=true;
                ReplaceValue(group, key, todoValue);
                return todoValue;
            }
            return value;
        }
        
        /// <summary>
        /// Returns Value (first if there are multiple) for key in domain group, or null if it does not exist.
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>Single Value</returns>
        public string GetValue(string group, string key) {
            List<string> t = GetValues(group, key);
            if (t.Count == 0) {
                return null;
            } else {
                return t[0];
            }
        }

        /// <summary>
        /// If not at least one value is set, this GetValues will write a TODO field into the config file, and also will return TODO instead of empty list. an empty list wont be a possible value for these keys!
        /// This is meant to be used to create a config file, best to be used during the init of a plugin, because the start up of the bot will be blocked untill all TODO fields are filled!
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> GetValuesOrTodo(string key) {
            List<string> values = GetValues(key);
            if ((values.Count == 0) || (values[0] == todoValue)) {
                Todo=true;
                ReplaceValue(key, todoValue);
                values.Add(todoValue);
            }
            return values;
        }
        
        /// <summary>
        /// Returns Values as a List for a key in default domain, or the empty list if it does not exist.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>List of Values</returns>
        public List<string> GetValues(string key) {
            return GetValues(config, key);
        }
        
        /// <summary>
        /// Returns Values as a List for a key in the domain group, or the empty list if it does not exist.
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>List of Values</returns>
        public List<string> GetValues(string group, string key) {
            List<string> values = new List<string>();
            DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "' AND " + baseKey + " = '" + key + "'");
            foreach(DataRow dr in rows) {
                values.Add((string)dr[baseValue]);
            }
            return values;
        }
        
        /// <summary>
        /// Non-Idempotent Set Value (if you set twice,you'll get a list back) in default domain
        /// </summary>
        /// <param name="key">Key Value</param>
        /// <param name="value"></param>
        /// <returns>Returns true when successfull</returns>
        public bool SetValue(string key, string value)
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
        public bool SetValue(string group, string key, string value)
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
        public bool RemoveValue(string key, string value) {
            return RemoveValue(config, key, value);
        }

        /// <summary>
        /// Remove Key Value Pair in default Domain
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least on element was removed</returns>
        public bool RemoveValue(string group, string key, string value) {
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
        public bool RemoveValueStartingWith(string key, string beginOfValue) {
            return RemoveValueStartingWith(config, key, beginOfValue);
        }
        
        /// <summary>
        /// Remove Multiple Values for a certain group and key for values which start with beginofvalue
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="beginOfValue">Value which begins with this String will be removed</param>
        public bool RemoveValueStartingWith(string group, string key, string beginOfValue) {
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
        /// Removes all values with the specified key in default Domain
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if at least one element was removed</returns>
        public bool RemoveKey(string key)
        {
            return RemoveKey(config, key);
        }

        /// <summary>
        /// Removes all values with the specified key in the Domain group
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <returns>True if at least one element was removed</returns>
        public bool RemoveKey(string group, string key)
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
            return false;
        }

        /// <summary>
        /// Removes all values within the specified domain group.
        /// (Be carefull. Deleting 'config' would render the bot without a valid configuration)
        /// </summary>
        /// <param name="group">the domain which should be removed</param>
        /// <returns>True if at least one element was removed</returns>
        public bool RemoveGroup(string group) {
            if (memory.Tables.Contains(baseTable))
            {
                bool removed = false;
                DataRow[] rows = memory.Tables[baseTable].Select(baseGroup + " = '" + group + "'");
                foreach (DataRow dr in rows) {
                    memory.Tables[baseTable].Rows.Remove(dr);
                    removed = true;
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
        public bool ReplaceValue(string key, string value) {
            return ReplaceValue(config, key, value);
        }
        
        /// <summary>
        /// Replaces key with a new value in the Domain group, if multiple old values exist only one new value will exist after this operation
        /// </summary>
        /// <param name="group">Data Domain</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if at least one Value was removed, false if the key is new</returns>
        public bool ReplaceValue(string group, string key, string value) {
            bool removed = RemoveKey(group, key);
            SetValue(group, key, value);
            return removed;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private PersistentMemory()
        {
            if (AppDomain.CurrentDomain.FriendlyName == "Plugins") {
                throw new NotImplementedException("this should not happen");
            }
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
        
        /// <summary>
        /// static Constructor
        /// </summary>
        static PersistentMemory() {}
    }
}
