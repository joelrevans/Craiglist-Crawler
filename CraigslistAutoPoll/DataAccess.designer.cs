﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CraigslistAutoPoll
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="CraigslistDB")]
	public partial class DataAccessDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertVehicleModel(VehicleModel instance);
    partial void UpdateVehicleModel(VehicleModel instance);
    partial void DeleteVehicleModel(VehicleModel instance);
    partial void InsertVehicleMake(VehicleMake instance);
    partial void UpdateVehicleMake(VehicleMake instance);
    partial void DeleteVehicleMake(VehicleMake instance);
    partial void InsertCLSiteSection(CLSiteSection instance);
    partial void UpdateCLSiteSection(CLSiteSection instance);
    partial void DeleteCLSiteSection(CLSiteSection instance);
    partial void InsertCLSubCity(CLSubCity instance);
    partial void UpdateCLSubCity(CLSubCity instance);
    partial void DeleteCLSubCity(CLSubCity instance);
    partial void InsertListing(Listing instance);
    partial void UpdateListing(Listing instance);
    partial void DeleteListing(Listing instance);
    partial void InsertProxy(Proxy instance);
    partial void UpdateProxy(Proxy instance);
    partial void DeleteProxy(Proxy instance);
    partial void InsertCLCity(CLCity instance);
    partial void UpdateCLCity(CLCity instance);
    partial void DeleteCLCity(CLCity instance);
    partial void InsertListingAttribute(ListingAttribute instance);
    partial void UpdateListingAttribute(ListingAttribute instance);
    partial void DeleteListingAttribute(ListingAttribute instance);
    #endregion
		
		public DataAccessDataContext() : 
				base(global::CraigslistAutoPoll.Properties.Settings.Default.CraigslistDBConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DataAccessDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataAccessDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataAccessDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataAccessDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<VehicleModel> VehicleModels
		{
			get
			{
				return this.GetTable<VehicleModel>();
			}
		}
		
		public System.Data.Linq.Table<VehicleMake> VehicleMakes
		{
			get
			{
				return this.GetTable<VehicleMake>();
			}
		}
		
		public System.Data.Linq.Table<CLSiteSection> CLSiteSections
		{
			get
			{
				return this.GetTable<CLSiteSection>();
			}
		}
		
		public System.Data.Linq.Table<CLSubCity> CLSubCities
		{
			get
			{
				return this.GetTable<CLSubCity>();
			}
		}
		
		public System.Data.Linq.Table<Listing> Listings
		{
			get
			{
				return this.GetTable<Listing>();
			}
		}
		
		public System.Data.Linq.Table<Proxy> Proxies
		{
			get
			{
				return this.GetTable<Proxy>();
			}
		}
		
		public System.Data.Linq.Table<CLCity> CLCities
		{
			get
			{
				return this.GetTable<CLCity>();
			}
		}
		
		public System.Data.Linq.Table<ListingAttribute> ListingAttributes
		{
			get
			{
				return this.GetTable<ListingAttribute>();
			}
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.GetFeedList", IsComposable=true)]
		public IQueryable<GetFeedListResult> GetFeedList([global::System.Data.Linq.Mapping.ParameterAttribute(Name="IPFilter", DbType="VarChar(15)")] string iPFilter)
		{
			return this.CreateMethodCallQuery<GetFeedListResult>(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), iPFilter);
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.VehicleModel")]
	public partial class VehicleModel : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    #endregion
		
		public VehicleModel()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="VarChar(255) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.VehicleMake")]
	public partial class VehicleMake : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    #endregion
		
		public VehicleMake()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="VarChar(255) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.CLSiteSection")]
	public partial class CLSiteSection : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
		private bool _Enabled;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    #endregion
		
		public CLSiteSection()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="Char(3) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="Bit NOT NULL")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.CLSubCity")]
	public partial class CLSubCity : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _ParentCity;
		
		private string _SubCity;
		
		private EntityRef<CLCity> _CLCity;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnParentCityChanging(string value);
    partial void OnParentCityChanged();
    partial void OnSubCityChanging(string value);
    partial void OnSubCityChanged();
    #endregion
		
		public CLSubCity()
		{
			this._CLCity = default(EntityRef<CLCity>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentCity", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string ParentCity
		{
			get
			{
				return this._ParentCity;
			}
			set
			{
				if ((this._ParentCity != value))
				{
					if (this._CLCity.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnParentCityChanging(value);
					this.SendPropertyChanging();
					this._ParentCity = value;
					this.SendPropertyChanged("ParentCity");
					this.OnParentCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SubCity", DbType="Char(3) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string SubCity
		{
			get
			{
				return this._SubCity;
			}
			set
			{
				if ((this._SubCity != value))
				{
					this.OnSubCityChanging(value);
					this.SendPropertyChanging();
					this._SubCity = value;
					this.SendPropertyChanged("SubCity");
					this.OnSubCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CLCity_CLSubCity", Storage="_CLCity", ThisKey="ParentCity", OtherKey="Name", IsForeignKey=true)]
		public CLCity CLCity
		{
			get
			{
				return this._CLCity.Entity;
			}
			set
			{
				if ((this._CLCity.Entity != value))
				{
					this.SendPropertyChanging();
					this._CLCity.Entity = value;
					this.SendPropertyChanged("CLCity");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Listing")]
	public partial class Listing : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _Id;
		
		private string _Body;
		
		private string _Title;
		
		private System.DateTime _PostDate;
		
		private string _SiteSection;
		
		private System.DateTime _Timestamp;
		
		private string _City;
		
		private string _SubCity;
		
		private EntitySet<ListingAttribute> _ListingAttributes;
		
		private EntityRef<CLSiteSection> _CLSiteSection;
		
		private EntityRef<CLSubCity> _CLSubCity;
		
		private EntityRef<CLCity> _CLCity;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(long value);
    partial void OnIdChanged();
    partial void OnBodyChanging(string value);
    partial void OnBodyChanged();
    partial void OnTitleChanging(string value);
    partial void OnTitleChanged();
    partial void OnPostDateChanging(System.DateTime value);
    partial void OnPostDateChanged();
    partial void OnSiteSectionChanging(string value);
    partial void OnSiteSectionChanged();
    partial void OnTimestampChanging(System.DateTime value);
    partial void OnTimestampChanged();
    partial void OnCityChanging(string value);
    partial void OnCityChanged();
    partial void OnSubCityChanging(string value);
    partial void OnSubCityChanged();
    #endregion
		
		public Listing()
		{
			this._ListingAttributes = new EntitySet<ListingAttribute>(new Action<ListingAttribute>(this.attach_ListingAttributes), new Action<ListingAttribute>(this.detach_ListingAttributes));
			this._CLSiteSection = default(EntityRef<CLSiteSection>);
			this._CLSubCity = default(EntityRef<CLSubCity>);
			this._CLCity = default(EntityRef<CLCity>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="BigInt NOT NULL", IsPrimaryKey=true)]
		public long Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Body", DbType="VarChar(MAX) NOT NULL", CanBeNull=false)]
		public string Body
		{
			get
			{
				return this._Body;
			}
			set
			{
				if ((this._Body != value))
				{
					this.OnBodyChanging(value);
					this.SendPropertyChanging();
					this._Body = value;
					this.SendPropertyChanged("Body");
					this.OnBodyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Title", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if ((this._Title != value))
				{
					this.OnTitleChanging(value);
					this.SendPropertyChanging();
					this._Title = value;
					this.SendPropertyChanged("Title");
					this.OnTitleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PostDate", DbType="DateTime NOT NULL")]
		public System.DateTime PostDate
		{
			get
			{
				return this._PostDate;
			}
			set
			{
				if ((this._PostDate != value))
				{
					this.OnPostDateChanging(value);
					this.SendPropertyChanging();
					this._PostDate = value;
					this.SendPropertyChanged("PostDate");
					this.OnPostDateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SiteSection", DbType="Char(3) NOT NULL", CanBeNull=false)]
		public string SiteSection
		{
			get
			{
				return this._SiteSection;
			}
			set
			{
				if ((this._SiteSection != value))
				{
					if (this._CLSiteSection.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnSiteSectionChanging(value);
					this.SendPropertyChanging();
					this._SiteSection = value;
					this.SendPropertyChanged("SiteSection");
					this.OnSiteSectionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Timestamp", DbType="DateTime NOT NULL")]
		public System.DateTime Timestamp
		{
			get
			{
				return this._Timestamp;
			}
			set
			{
				if ((this._Timestamp != value))
				{
					this.OnTimestampChanging(value);
					this.SendPropertyChanging();
					this._Timestamp = value;
					this.SendPropertyChanged("Timestamp");
					this.OnTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_City", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				if ((this._City != value))
				{
					if (this._CLCity.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnCityChanging(value);
					this.SendPropertyChanging();
					this._City = value;
					this.SendPropertyChanged("City");
					this.OnCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SubCity", DbType="Char(3)")]
		public string SubCity
		{
			get
			{
				return this._SubCity;
			}
			set
			{
				if ((this._SubCity != value))
				{
					if (this._CLSubCity.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnSubCityChanging(value);
					this.SendPropertyChanging();
					this._SubCity = value;
					this.SendPropertyChanged("SubCity");
					this.OnSubCityChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Listing_ListingAttribute", Storage="_ListingAttributes", ThisKey="Id", OtherKey="ListingID")]
		public EntitySet<ListingAttribute> ListingAttributes
		{
			get
			{
				return this._ListingAttributes;
			}
			set
			{
				this._ListingAttributes.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CLSiteSection_Listing", Storage="_CLSiteSection", ThisKey="SiteSection", OtherKey="Name", IsForeignKey=true)]
		public CLSiteSection CLSiteSection
		{
			get
			{
				return this._CLSiteSection.Entity;
			}
			set
			{
				if ((this._CLSiteSection.Entity != value))
				{
					this.SendPropertyChanging();
					this._CLSiteSection.Entity = value;
					this.SendPropertyChanged("CLSiteSection");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CLSubCity_Listing", Storage="_CLSubCity", ThisKey="SubCity", OtherKey="SubCity", IsForeignKey=true)]
		public CLSubCity CLSubCity
		{
			get
			{
				return this._CLSubCity.Entity;
			}
			set
			{
				if ((this._CLSubCity.Entity != value))
				{
					this.SendPropertyChanging();
					this._CLSubCity.Entity = value;
					this.SendPropertyChanged("CLSubCity");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="CLCity_Listing", Storage="_CLCity", ThisKey="City", OtherKey="Name", IsForeignKey=true)]
		public CLCity CLCity
		{
			get
			{
				return this._CLCity.Entity;
			}
			set
			{
				if ((this._CLCity.Entity != value))
				{
					this.SendPropertyChanging();
					this._CLCity.Entity = value;
					this.SendPropertyChanged("CLCity");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ListingAttributes(ListingAttribute entity)
		{
			this.SendPropertyChanging();
			entity.Listing = this;
		}
		
		private void detach_ListingAttributes(ListingAttribute entity)
		{
			this.SendPropertyChanging();
			entity.Listing = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Proxy")]
	public partial class Proxy : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _IP;
		
		private int _Port;
		
		private bool _Enabled;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIPChanging(string value);
    partial void OnIPChanged();
    partial void OnPortChanging(int value);
    partial void OnPortChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    #endregion
		
		public Proxy()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IP", DbType="VarChar(45) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string IP
		{
			get
			{
				return this._IP;
			}
			set
			{
				if ((this._IP != value))
				{
					this.OnIPChanging(value);
					this.SendPropertyChanging();
					this._IP = value;
					this.SendPropertyChanged("IP");
					this.OnIPChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Port", DbType="Int NOT NULL")]
		public int Port
		{
			get
			{
				return this._Port;
			}
			set
			{
				if ((this._Port != value))
				{
					this.OnPortChanging(value);
					this.SendPropertyChanging();
					this._Port = value;
					this.SendPropertyChanged("Port");
					this.OnPortChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="Bit NOT NULL")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.CLCity")]
	public partial class CLCity : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
		private string _ShortName;
		
		private string _IP;
		
		private bool _Enabled;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnShortNameChanging(string value);
    partial void OnShortNameChanged();
    partial void OnIPChanging(string value);
    partial void OnIPChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    #endregion
		
		public CLCity()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="VarChar(255) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ShortName", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string ShortName
		{
			get
			{
				return this._ShortName;
			}
			set
			{
				if ((this._ShortName != value))
				{
					this.OnShortNameChanging(value);
					this.SendPropertyChanging();
					this._ShortName = value;
					this.SendPropertyChanged("ShortName");
					this.OnShortNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IP", DbType="VarChar(15)")]
		public string IP
		{
			get
			{
				return this._IP;
			}
			set
			{
				if ((this._IP != value))
				{
					this.OnIPChanging(value);
					this.SendPropertyChanging();
					this._IP = value;
					this.SendPropertyChanged("IP");
					this.OnIPChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="Bit NOT NULL")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ListingAttribute")]
	public partial class ListingAttribute : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _ListingID;
		
		private string _Name;
		
		private string _Value;
		
		private int _AttributeID;
		
		private EntityRef<Listing> _Listing;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnListingIDChanging(long value);
    partial void OnListingIDChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnValueChanging(string value);
    partial void OnValueChanged();
    partial void OnAttributeIDChanging(int value);
    partial void OnAttributeIDChanged();
    #endregion
		
		public ListingAttribute()
		{
			this._Listing = default(EntityRef<Listing>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ListingID", DbType="BigInt NOT NULL")]
		public long ListingID
		{
			get
			{
				return this._ListingID;
			}
			set
			{
				if ((this._ListingID != value))
				{
					if (this._Listing.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnListingIDChanging(value);
					this.SendPropertyChanging();
					this._ListingID = value;
					this.SendPropertyChanged("ListingID");
					this.OnListingIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Value", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				if ((this._Value != value))
				{
					this.OnValueChanging(value);
					this.SendPropertyChanging();
					this._Value = value;
					this.SendPropertyChanged("Value");
					this.OnValueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AttributeID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int AttributeID
		{
			get
			{
				return this._AttributeID;
			}
			set
			{
				if ((this._AttributeID != value))
				{
					this.OnAttributeIDChanging(value);
					this.SendPropertyChanging();
					this._AttributeID = value;
					this.SendPropertyChanged("AttributeID");
					this.OnAttributeIDChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Listing_ListingAttribute", Storage="_Listing", ThisKey="ListingID", OtherKey="Id", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public Listing Listing
		{
			get
			{
				return this._Listing.Entity;
			}
			set
			{
				Listing previousValue = this._Listing.Entity;
				if (((previousValue != value) 
							|| (this._Listing.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Listing.Entity = null;
						previousValue.ListingAttributes.Remove(this);
					}
					this._Listing.Entity = value;
					if ((value != null))
					{
						value.ListingAttributes.Add(this);
						this._ListingID = value.Id;
					}
					else
					{
						this._ListingID = default(long);
					}
					this.SendPropertyChanged("Listing");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	public partial class GetFeedListResult
	{
		
		private string _City;
		
		private string _SiteSection;
		
		private string _SubCity;
		
		private System.DateTime _Timestamp;
		
		public GetFeedListResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_City", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				if ((this._City != value))
				{
					this._City = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SiteSection", DbType="Char(3) NOT NULL", CanBeNull=false)]
		public string SiteSection
		{
			get
			{
				return this._SiteSection;
			}
			set
			{
				if ((this._SiteSection != value))
				{
					this._SiteSection = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SubCity", DbType="Char(3)")]
		public string SubCity
		{
			get
			{
				return this._SubCity;
			}
			set
			{
				if ((this._SubCity != value))
				{
					this._SubCity = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Timestamp", DbType="DateTime NOT NULL")]
		public System.DateTime Timestamp
		{
			get
			{
				return this._Timestamp;
			}
			set
			{
				if ((this._Timestamp != value))
				{
					this._Timestamp = value;
				}
			}
		}
	}
}
#pragma warning restore 1591
