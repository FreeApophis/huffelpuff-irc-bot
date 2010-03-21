#region Auto-generated classes for main database on 2010-03-21 13:34:58Z

//
//  ____  _     __  __      _        _
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from main on 2010-03-21 13:34:58Z
// Please visit http://linq.to/db for more information

#endregion

using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq;
#else   // MONO_STRICT
using DbLinq.Data.Linq;
using DbLinq.Vendor;
#endif  // MONO_STRICT
using System.ComponentModel;

namespace Huffelpuff.Database
{
	public partial class Main : DataContext
	{
		#region Extensibility Method Definitions

		partial void OnCreated();

		#endregion

		public Main(string connectionString)
			: base(connectionString)
		{
			OnCreated();
		}

		public Main(IDbConnection connection)
//		#if MONO_STRICT
//			: base(connection)
//		#else   // MONO_STRICT
			: base(connection, new DbLinq.Sqlite.SqliteVendor())
//		#endif  // MONO_STRICT
		{
			OnCreated();
		}

		public Main(string connection, MappingSource mappingSource)
			: base(connection, mappingSource)
		{
			OnCreated();
		}

		public Main(IDbConnection connection, MappingSource mappingSource)
			: base(connection, mappingSource)
		{
			OnCreated();
		}

		#if !MONO_STRICT
		public Main(IDbConnection connection, IVendor vendor)
			: base(connection, vendor)
		{
			OnCreated();
		}
		#endif  // !MONO_STRICT

		#if !MONO_STRICT
		public Main(IDbConnection connection, MappingSource mappingSource, IVendor vendor)
			: base(connection, mappingSource, vendor)
		{
			OnCreated();
		}
		#endif  // !MONO_STRICT

		public Table<EventLog> EventLogs { get { return GetTable<EventLog>(); } }
		public Table<FactKey> FactKeys { get { return GetTable<FactKey>(); } }
		public Table<FactValue> FactValues { get { return GetTable<FactValue>(); } }
		public Table<IrcLog> IrcLogs { get { return GetTable<IrcLog>(); } }
		public Table<IrcUser> IrcUsers { get { return GetTable<IrcUser>(); } }
		public Table<IrcUserAlias> IrcUserAlias { get { return GetTable<IrcUserAlias>(); } }
		public Table<Setting> Settings { get { return GetTable<Setting>(); } }
		public Table<User> Users { get { return GetTable<User>(); } }
		public Table<UserGroup> UserGroups { get { return GetTable<UserGroup>(); } }
		public Table<UserGroupUser> UserGroupUsers { get { return GetTable<UserGroupUser>(); } }

	}

	[Table(Name = "main.EventLog")]
	public partial class EventLog : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnLevelChanged();
		partial void OnLevelChanging(int value);
		partial void OnMessageChanged();
		partial void OnMessageChanging(string value);

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region int Level

		private int _level;
		[DebuggerNonUserCode]
		[Column(Storage = "_level", Name = "Level", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int Level
		{
			get
			{
				return _level;
			}
			set
			{
				if (value != _level)
				{
					OnLevelChanging(value);
					SendPropertyChanging();
					_level = value;
					SendPropertyChanged("Level");
					OnLevelChanged();
				}
			}
		}

		#endregion

		#region string Message

		private string _message;
		[DebuggerNonUserCode]
		[Column(Storage = "_message", Name = "Message", DbType = "TEXT", AutoSync = AutoSync.Never)]
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				if (value != _message)
				{
					OnMessageChanging(value);
					SendPropertyChanging();
					_message = value;
					SendPropertyChanged("Message");
					OnMessageChanged();
				}
			}
		}

		#endregion

		#region ctor

		public EventLog()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.FactKey")]
	public partial class FactKey : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnHitCountChanged();
		partial void OnHitCountChanging(int value);
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnKeyChanged();
		partial void OnKeyChanging(string value);

		#endregion

		#region int HitCount

		private int _hitCount;
		[DebuggerNonUserCode]
		[Column(Storage = "_hitCount", Name = "HitCount", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int HitCount
		{
			get
			{
				return _hitCount;
			}
			set
			{
				if (value != _hitCount)
				{
					OnHitCountChanging(value);
					SendPropertyChanging();
					_hitCount = value;
					SendPropertyChanged("HitCount");
					OnHitCountChanged();
				}
			}
		}

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Key

		private string _key;
		[DebuggerNonUserCode]
		[Column(Storage = "_key", Name = "\"Key\"", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never, CanBeNull = false)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (value != _key)
				{
					OnKeyChanging(value);
					SendPropertyChanging();
					_key = value;
					SendPropertyChanged("Key");
					OnKeyChanged();
				}
			}
		}

		#endregion

		#region ctor

		public FactKey()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.FactValue")]
	public partial class FactValue : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnFactKeyIDChanged();
		partial void OnFactKeyIDChanging(int value);
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnIrcUserIDChanged();
		partial void OnIrcUserIDChanging(int value);
		partial void OnValueChanged();
		partial void OnValueChanging(string value);

		#endregion

		#region int FactKeyID

		private int _factKeyID;
		[DebuggerNonUserCode]
		[Column(Storage = "_factKeyID", Name = "FactKeyID", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int FactKeyID
		{
			get
			{
				return _factKeyID;
			}
			set
			{
				if (value != _factKeyID)
				{
					OnFactKeyIDChanging(value);
					SendPropertyChanging();
					_factKeyID = value;
					SendPropertyChanged("FactKeyID");
					OnFactKeyIDChanged();
				}
			}
		}

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region int IrcUserID

		private int _ircUserID;
		[DebuggerNonUserCode]
		[Column(Storage = "_ircUserID", Name = "IrcUserID", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int IrcUserID
		{
			get
			{
				return _ircUserID;
			}
			set
			{
				if (value != _ircUserID)
				{
					OnIrcUserIDChanging(value);
					SendPropertyChanging();
					_ircUserID = value;
					SendPropertyChanged("IrcUserID");
					OnIrcUserIDChanged();
				}
			}
		}

		#endregion

		#region string Value

		private string _value;
		[DebuggerNonUserCode]
		[Column(Storage = "_value", Name = "Value", DbType = "TEXT", AutoSync = AutoSync.Never, CanBeNull = false)]
		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value != _value)
				{
					OnValueChanging(value);
					SendPropertyChanging();
					_value = value;
					SendPropertyChanged("Value");
					OnValueChanged();
				}
			}
		}

		#endregion

		#region ctor

		public FactValue()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.IrcLog")]
	public partial class IrcLog : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnEventTypeChanged();
		partial void OnEventTypeChanging(string value);
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnMessageChanged();
		partial void OnMessageChanging(string value);
		partial void OnTimeChanged();
		partial void OnTimeChanging(DateTime value);
		partial void OnUserIDChanged();
		partial void OnUserIDChanging(int? value);

		#endregion

		#region string EventType

		private string _eventType;
		[DebuggerNonUserCode]
		[Column(Storage = "_eventType", Name = "EventType", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never, CanBeNull = false)]
		public string EventType
		{
			get
			{
				return _eventType;
			}
			set
			{
				if (value != _eventType)
				{
					OnEventTypeChanging(value);
					SendPropertyChanging();
					_eventType = value;
					SendPropertyChanged("EventType");
					OnEventTypeChanged();
				}
			}
		}

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Message

		private string _message;
		[DebuggerNonUserCode]
		[Column(Storage = "_message", Name = "Message", DbType = "TEXT", AutoSync = AutoSync.Never)]
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				if (value != _message)
				{
					OnMessageChanging(value);
					SendPropertyChanging();
					_message = value;
					SendPropertyChanged("Message");
					OnMessageChanged();
				}
			}
		}

		#endregion

		#region DateTime Time

		private DateTime _time;
		[DebuggerNonUserCode]
		[Column(Storage = "_time", Name = "\"Time\"", DbType = "TIMESTAMP", AutoSync = AutoSync.Never, CanBeNull = false)]
		public DateTime Time
		{
			get
			{
				return _time;
			}
			set
			{
				if (value != _time)
				{
					OnTimeChanging(value);
					SendPropertyChanging();
					_time = value;
					SendPropertyChanged("Time");
					OnTimeChanged();
				}
			}
		}

		#endregion

		#region int? UserID

		private int? _userID;
		[DebuggerNonUserCode]
		[Column(Storage = "_userID", Name = "UserID", DbType = "INTEGER", AutoSync = AutoSync.Never)]
		public int? UserID
		{
			get
			{
				return _userID;
			}
			set
			{
				if (value != _userID)
				{
					OnUserIDChanging(value);
					SendPropertyChanging();
					_userID = value;
					SendPropertyChanged("UserID");
					OnUserIDChanged();
				}
			}
		}

		#endregion

		#region ctor

		public IrcLog()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.IrcUser")]
	public partial class IrcUser : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnHostChanged();
		partial void OnHostChanging(string value);
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnNicknameChanged();
		partial void OnNicknameChanging(string value);
		partial void OnUserChanged();
		partial void OnUserChanging(string value);

		#endregion

		#region string Host

		private string _host;
		[DebuggerNonUserCode]
		[Column(Storage = "_host", Name = "Host", DbType = "VARCHAR(100)", AutoSync = AutoSync.Never)]
		public string Host
		{
			get
			{
				return _host;
			}
			set
			{
				if (value != _host)
				{
					OnHostChanging(value);
					SendPropertyChanging();
					_host = value;
					SendPropertyChanged("Host");
					OnHostChanged();
				}
			}
		}

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Nickname

		private string _nickname;
		[DebuggerNonUserCode]
		[Column(Storage = "_nickname", Name = "Nickname", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never)]
		public string Nickname
		{
			get
			{
				return _nickname;
			}
			set
			{
				if (value != _nickname)
				{
					OnNicknameChanging(value);
					SendPropertyChanging();
					_nickname = value;
					SendPropertyChanged("Nickname");
					OnNicknameChanged();
				}
			}
		}

		#endregion

		#region string User

		private string _user;
		[DebuggerNonUserCode]
		[Column(Storage = "_user", Name = "\"User\"", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never)]
		public string User
		{
			get
			{
				return _user;
			}
			set
			{
				if (value != _user)
				{
					OnUserChanging(value);
					SendPropertyChanging();
					_user = value;
					SendPropertyChanged("User");
					OnUserChanged();
				}
			}
		}

		#endregion

		#region ctor

		public IrcUser()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.IrcUserAlias")]
	public partial class IrcUserAlias : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnSourceIDChanged();
		partial void OnSourceIDChanging(int value);
		partial void OnTargetIDChanged();
		partial void OnTargetIDChanging(int value);

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region int SourceID

		private int _sourceID;
		[DebuggerNonUserCode]
		[Column(Storage = "_sourceID", Name = "SourceID", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int SourceID
		{
			get
			{
				return _sourceID;
			}
			set
			{
				if (value != _sourceID)
				{
					OnSourceIDChanging(value);
					SendPropertyChanging();
					_sourceID = value;
					SendPropertyChanged("SourceID");
					OnSourceIDChanged();
				}
			}
		}

		#endregion

		#region int TargetID

		private int _targetID;
		[DebuggerNonUserCode]
		[Column(Storage = "_targetID", Name = "TargetID", DbType = "INTEGER", AutoSync = AutoSync.Never, CanBeNull = false)]
		public int TargetID
		{
			get
			{
				return _targetID;
			}
			set
			{
				if (value != _targetID)
				{
					OnTargetIDChanging(value);
					SendPropertyChanging();
					_targetID = value;
					SendPropertyChanged("TargetID");
					OnTargetIDChanged();
				}
			}
		}

		#endregion

		#region ctor

		public IrcUserAlias()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.Settings")]
	public partial class Setting : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnKeyChanged();
		partial void OnKeyChanging(string value);
		partial void OnValueChanged();
		partial void OnValueChanging(string value);

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Key

		private string _key;
		[DebuggerNonUserCode]
		[Column(Storage = "_key", Name = "\"Key\"", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never, CanBeNull = false)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (value != _key)
				{
					OnKeyChanging(value);
					SendPropertyChanging();
					_key = value;
					SendPropertyChanged("Key");
					OnKeyChanged();
				}
			}
		}

		#endregion

		#region string Value

		private string _value;
		[DebuggerNonUserCode]
		[Column(Storage = "_value", Name = "Value", DbType = "TEXT", AutoSync = AutoSync.Never)]
		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value != _value)
				{
					OnValueChanging(value);
					SendPropertyChanging();
					_value = value;
					SendPropertyChanged("Value");
					OnValueChanged();
				}
			}
		}

		#endregion

		#region ctor

		public Setting()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.\"User\"")]
	public partial class User : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnUsernameChanged();
		partial void OnUsernameChanging(string value);

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Username

		private string _username;
		[DebuggerNonUserCode]
		[Column(Storage = "_username", Name = "Username", DbType = "VARCHAR(100)", AutoSync = AutoSync.Never, CanBeNull = false)]
		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				if (value != _username)
				{
					OnUsernameChanging(value);
					SendPropertyChanging();
					_username = value;
					SendPropertyChanged("Username");
					OnUsernameChanged();
				}
			}
		}

		#endregion

		#region ctor

		public User()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.UserGroup")]
	public partial class UserGroup : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnNameChanged();
		partial void OnNameChanging(string value);

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region string Name

		private string _name;
		[DebuggerNonUserCode]
		[Column(Storage = "_name", Name = "Name", DbType = "VARCHAR(50)", AutoSync = AutoSync.Never)]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (value != _name)
				{
					OnNameChanging(value);
					SendPropertyChanging();
					_name = value;
					SendPropertyChanged("Name");
					OnNameChanged();
				}
			}
		}

		#endregion

		#region ctor

		public UserGroup()
		{
			OnCreated();
		}

		#endregion

	}

	[Table(Name = "main.UserGroupUser")]
	public partial class UserGroupUser : INotifyPropertyChanging, INotifyPropertyChanged
	{
		#region INotifyPropertyChanging handling

		public event PropertyChangingEventHandler PropertyChanging;

		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs("");
		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		#endregion

		#region INotifyPropertyChanged handling

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Extensibility Method Definitions

		partial void OnCreated();
		partial void OnGroupIDChanged();
		partial void OnGroupIDChanging(int? value);
		partial void OnIDChanged();
		partial void OnIDChanging(int value);
		partial void OnUserIDChanged();
		partial void OnUserIDChanging(int? value);

		#endregion

		#region int? GroupID

		private int? _groupID;
		[DebuggerNonUserCode]
		[Column(Storage = "_groupID", Name = "GroupID", DbType = "INTEGER", AutoSync = AutoSync.Never)]
		public int? GroupID
		{
			get
			{
				return _groupID;
			}
			set
			{
				if (value != _groupID)
				{
					OnGroupIDChanging(value);
					SendPropertyChanging();
					_groupID = value;
					SendPropertyChanged("GroupID");
					OnGroupIDChanged();
				}
			}
		}

		#endregion

		#region int ID

		private int _id;
		[DebuggerNonUserCode]
		[Column(Storage = "_id", Name = "ID", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				if (value != _id)
				{
					OnIDChanging(value);
					SendPropertyChanging();
					_id = value;
					SendPropertyChanged("ID");
					OnIDChanged();
				}
			}
		}

		#endregion

		#region int? UserID

		private int? _userID;
		[DebuggerNonUserCode]
		[Column(Storage = "_userID", Name = "UserID", DbType = "INTEGER", AutoSync = AutoSync.Never)]
		public int? UserID
		{
			get
			{
				return _userID;
			}
			set
			{
				if (value != _userID)
				{
					OnUserIDChanging(value);
					SendPropertyChanging();
					_userID = value;
					SendPropertyChanged("UserID");
					OnUserIDChanged();
				}
			}
		}

		#endregion

		#region ctor

		public UserGroupUser()
		{
			OnCreated();
		}

		#endregion

	}
}
