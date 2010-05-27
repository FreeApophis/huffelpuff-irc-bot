// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from main on 2010-05-28 01:16:25Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace Huffelpuff.Database
{
	using System;
	using System.ComponentModel;
	using System.Data;
#if MONO_STRICT
	using System.Data.Linq;
#else   // MONO_STRICT
	using DbLinq.Data.Linq;
	using DbLinq.Vendor;
#endif  // MONO_STRICT
	using System.Data.Linq.Mapping;
	using System.Diagnostics;
	
	
	public partial class Main : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public Main(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public Main(string connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Main(IDbConnection connection, MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			this.OnCreated();
		}
		
		public Table<EventLog> EventLogs
		{
			get
			{
				return this.GetTable<EventLog>();
			}
		}
		
		public Table<FactKey> FactKeys
		{
			get
			{
				return this.GetTable<FactKey>();
			}
		}
		
		public Table<FactValue> FactValues
		{
			get
			{
				return this.GetTable<FactValue>();
			}
		}
		
		public Table<IrcLog> IrcLogs
		{
			get
			{
				return this.GetTable<IrcLog>();
			}
		}
		
		public Table<IrcUser> IrcUsers
		{
			get
			{
				return this.GetTable<IrcUser>();
			}
		}
		
		public Table<IrcUserAlias> IrcUserAlias
		{
			get
			{
				return this.GetTable<IrcUserAlias>();
			}
		}
		
		public Table<Setting> Settings
		{
			get
			{
				return this.GetTable<Setting>();
			}
		}
		
		public Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}
		
		public Table<UserGroup> UserGroups
		{
			get
			{
				return this.GetTable<UserGroup>();
			}
		}
		
		public Table<UserGroupUser> UserGroupUsers
		{
			get
			{
				return this.GetTable<UserGroupUser>();
			}
		}
	}
	
	#region Start MONO_STRICT
#if MONO_STRICT

	public partial class Main
	{
		
		public Main(IDbConnection connection) : 
				base(connection)
		{
			this.OnCreated();
		}
	}
	#region End MONO_STRICT
	#endregion
#else     // MONO_STRICT
	
	public partial class Main
	{
		
		public Main(IDbConnection connection) : 
				base(connection, new DbLinq.Sqlite.SqliteVendor())
		{
			this.OnCreated();
		}
		
		public Main(IDbConnection connection, IVendor sqlDialect) : 
				base(connection, sqlDialect)
		{
			this.OnCreated();
		}
		
		public Main(IDbConnection connection, MappingSource mappingSource, IVendor sqlDialect) : 
				base(connection, mappingSource, sqlDialect)
		{
			this.OnCreated();
		}
	}
	#region End Not MONO_STRICT
	#endregion
#endif     // MONO_STRICT
	#endregion
	
	[Table(Name="main.EventLog")]
	public partial class EventLog : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private int _level;
		
		private string _message;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnLevelChanged();
		
		partial void OnLevelChanging(int value);
		
		partial void OnMessageChanged();
		
		partial void OnMessageChanging(string value);
		#endregion
		
		
		public EventLog()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_level", Name="Level", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int Level
		{
			get
			{
				return this._level;
			}
			set
			{
				if ((_level != value))
				{
					this.OnLevelChanging(value);
					this.SendPropertyChanging();
					this._level = value;
					this.SendPropertyChanged("Level");
					this.OnLevelChanged();
				}
			}
		}
		
		[Column(Storage="_message", Name="Message", DbType="TEXT", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Message
		{
			get
			{
				return this._message;
			}
			set
			{
				if (((_message == value) 
							== false))
				{
					this.OnMessageChanging(value);
					this.SendPropertyChanging();
					this._message = value;
					this.SendPropertyChanged("Message");
					this.OnMessageChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.FactKey")]
	public partial class FactKey : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _hitCount;
		
		private int _id;
		
		private string _key;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnHitCountChanged();
		
		partial void OnHitCountChanging(int value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnKeyChanged();
		
		partial void OnKeyChanging(string value);
		#endregion
		
		
		public FactKey()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_hitCount", Name="HitCount", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int HitCount
		{
			get
			{
				return this._hitCount;
			}
			set
			{
				if ((_hitCount != value))
				{
					this.OnHitCountChanging(value);
					this.SendPropertyChanging();
					this._hitCount = value;
					this.SendPropertyChanged("HitCount");
					this.OnHitCountChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_key", Name="Key", DbType="VARCHAR(50)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Key
		{
			get
			{
				return this._key;
			}
			set
			{
				if (((_key == value) 
							== false))
				{
					this.OnKeyChanging(value);
					this.SendPropertyChanging();
					this._key = value;
					this.SendPropertyChanged("Key");
					this.OnKeyChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.FactValue")]
	public partial class FactValue : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _factKeyID;
		
		private int _id;
		
		private int _ircUserID;
		
		private string _value;
		
		#region Extensibility Method Declarations
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
		
		
		public FactValue()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_factKeyID", Name="FactKeyID", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int FactKeyID
		{
			get
			{
				return this._factKeyID;
			}
			set
			{
				if ((_factKeyID != value))
				{
					this.OnFactKeyIDChanging(value);
					this.SendPropertyChanging();
					this._factKeyID = value;
					this.SendPropertyChanged("FactKeyID");
					this.OnFactKeyIDChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_ircUserID", Name="IrcUserID", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int IrcUserID
		{
			get
			{
				return this._ircUserID;
			}
			set
			{
				if ((_ircUserID != value))
				{
					this.OnIrcUserIDChanging(value);
					this.SendPropertyChanging();
					this._ircUserID = value;
					this.SendPropertyChanged("IrcUserID");
					this.OnIrcUserIDChanged();
				}
			}
		}
		
		[Column(Storage="_value", Name="Value", DbType="TEXT", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (((_value == value) 
							== false))
				{
					this.OnValueChanging(value);
					this.SendPropertyChanging();
					this._value = value;
					this.SendPropertyChanged("Value");
					this.OnValueChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.IrcLog")]
	public partial class IrcLog : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _eventType;
		
		private int _id;
		
		private string _message;
		
		private System.DateTime _time;
		
		private System.Nullable<int> _userID;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnEventTypeChanged();
		
		partial void OnEventTypeChanging(string value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnMessageChanged();
		
		partial void OnMessageChanging(string value);
		
		partial void OnTimeChanged();
		
		partial void OnTimeChanging(System.DateTime value);
		
		partial void OnUserIDChanged();
		
		partial void OnUserIDChanging(System.Nullable<int> value);
		#endregion
		
		
		public IrcLog()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_eventType", Name="EventType", DbType="VARCHAR(50)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string EventType
		{
			get
			{
				return this._eventType;
			}
			set
			{
				if (((_eventType == value) 
							== false))
				{
					this.OnEventTypeChanging(value);
					this.SendPropertyChanging();
					this._eventType = value;
					this.SendPropertyChanged("EventType");
					this.OnEventTypeChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_message", Name="Message", DbType="TEXT", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Message
		{
			get
			{
				return this._message;
			}
			set
			{
				if (((_message == value) 
							== false))
				{
					this.OnMessageChanging(value);
					this.SendPropertyChanging();
					this._message = value;
					this.SendPropertyChanged("Message");
					this.OnMessageChanged();
				}
			}
		}
		
		[Column(Storage="_time", Name="Time", DbType="TIMESTAMP", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public System.DateTime Time
		{
			get
			{
				return this._time;
			}
			set
			{
				if ((_time != value))
				{
					this.OnTimeChanging(value);
					this.SendPropertyChanging();
					this._time = value;
					this.SendPropertyChanged("Time");
					this.OnTimeChanged();
				}
			}
		}
		
		[Column(Storage="_userID", Name="UserID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> UserID
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value))
				{
					this.OnUserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("UserID");
					this.OnUserIDChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.IrcUser")]
	public partial class IrcUser : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private string _host;
		
		private int _id;
		
		private string _nickname;
		
		private string _user;
		
		#region Extensibility Method Declarations
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
		
		
		public IrcUser()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_host", Name="Host", DbType="VARCHAR(100)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Host
		{
			get
			{
				return this._host;
			}
			set
			{
				if (((_host == value) 
							== false))
				{
					this.OnHostChanging(value);
					this.SendPropertyChanging();
					this._host = value;
					this.SendPropertyChanged("Host");
					this.OnHostChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_nickname", Name="Nickname", DbType="VARCHAR(50)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Nickname
		{
			get
			{
				return this._nickname;
			}
			set
			{
				if (((_nickname == value) 
							== false))
				{
					this.OnNicknameChanging(value);
					this.SendPropertyChanging();
					this._nickname = value;
					this.SendPropertyChanged("Nickname");
					this.OnNicknameChanged();
				}
			}
		}
		
		[Column(Storage="_user", Name="User", DbType="VARCHAR(50)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string User
		{
			get
			{
				return this._user;
			}
			set
			{
				if (((_user == value) 
							== false))
				{
					this.OnUserChanging(value);
					this.SendPropertyChanging();
					this._user = value;
					this.SendPropertyChanged("User");
					this.OnUserChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.IrcUserAlias")]
	public partial class IrcUserAlias : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private int _sourceID;
		
		private int _targetID;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnSourceIDChanged();
		
		partial void OnSourceIDChanging(int value);
		
		partial void OnTargetIDChanged();
		
		partial void OnTargetIDChanging(int value);
		#endregion
		
		
		public IrcUserAlias()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_sourceID", Name="SourceID", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int SourceID
		{
			get
			{
				return this._sourceID;
			}
			set
			{
				if ((_sourceID != value))
				{
					this.OnSourceIDChanging(value);
					this.SendPropertyChanging();
					this._sourceID = value;
					this.SendPropertyChanged("SourceID");
					this.OnSourceIDChanged();
				}
			}
		}
		
		[Column(Storage="_targetID", Name="TargetID", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int TargetID
		{
			get
			{
				return this._targetID;
			}
			set
			{
				if ((_targetID != value))
				{
					this.OnTargetIDChanging(value);
					this.SendPropertyChanging();
					this._targetID = value;
					this.SendPropertyChanged("TargetID");
					this.OnTargetIDChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.Settings")]
	public partial class Setting : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _key;
		
		private string _value;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnKeyChanged();
		
		partial void OnKeyChanging(string value);
		
		partial void OnValueChanged();
		
		partial void OnValueChanging(string value);
		#endregion
		
		
		public Setting()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_key", Name="Key", DbType="VARCHAR(50)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Key
		{
			get
			{
				return this._key;
			}
			set
			{
				if (((_key == value) 
							== false))
				{
					this.OnKeyChanging(value);
					this.SendPropertyChanging();
					this._key = value;
					this.SendPropertyChanged("Key");
					this.OnKeyChanged();
				}
			}
		}
		
		[Column(Storage="_value", Name="Value", DbType="TEXT", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (((_value == value) 
							== false))
				{
					this.OnValueChanging(value);
					this.SendPropertyChanging();
					this._value = value;
					this.SendPropertyChanged("Value");
					this.OnValueChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.User")]
	public partial class User : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _username;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnUsernameChanged();
		
		partial void OnUsernameChanging(string value);
		#endregion
		
		
		public User()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_username", Name="Username", DbType="VARCHAR(100)", AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public string Username
		{
			get
			{
				return this._username;
			}
			set
			{
				if (((_username == value) 
							== false))
				{
					this.OnUsernameChanging(value);
					this.SendPropertyChanging();
					this._username = value;
					this.SendPropertyChanged("Username");
					this.OnUsernameChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.UserGroup")]
	public partial class UserGroup : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private int _id;
		
		private string _name;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnNameChanged();
		
		partial void OnNameChanging(string value);
		#endregion
		
		
		public UserGroup()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_name", Name="Name", DbType="VARCHAR(50)", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				if (((_name == value) 
							== false))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="main.UserGroupUser")]
	public partial class UserGroupUser : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
		
		private System.Nullable<int> _groupID;
		
		private int _id;
		
		private System.Nullable<int> _userID;
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		
		partial void OnGroupIDChanged();
		
		partial void OnGroupIDChanging(System.Nullable<int> value);
		
		partial void OnIDChanged();
		
		partial void OnIDChanging(int value);
		
		partial void OnUserIDChanged();
		
		partial void OnUserIDChanging(System.Nullable<int> value);
		#endregion
		
		
		public UserGroupUser()
		{
			this.OnCreated();
		}
		
		[Column(Storage="_groupID", Name="GroupID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> GroupID
		{
			get
			{
				return this._groupID;
			}
			set
			{
				if ((_groupID != value))
				{
					this.OnGroupIDChanging(value);
					this.SendPropertyChanging();
					this._groupID = value;
					this.SendPropertyChanged("GroupID");
					this.OnGroupIDChanged();
				}
			}
		}
		
		[Column(Storage="_id", Name="ID", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
		[DebuggerNonUserCode()]
		public int ID
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((_id != value))
				{
					this.OnIDChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("ID");
					this.OnIDChanged();
				}
			}
		}
		
		[Column(Storage="_userID", Name="UserID", DbType="INTEGER", AutoSync=AutoSync.Never)]
		[DebuggerNonUserCode()]
		public System.Nullable<int> UserID
		{
			get
			{
				return this._userID;
			}
			set
			{
				if ((_userID != value))
				{
					this.OnUserIDChanging(value);
					this.SendPropertyChanging();
					this._userID = value;
					this.SendPropertyChanged("UserID");
					this.OnUserIDChanged();
				}
			}
		}
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
			if ((h != null))
			{
				h(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(string propertyName)
		{
			System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
			if ((h != null))
			{
				h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
