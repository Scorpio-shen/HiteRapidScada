﻿using Scada.Web.Plugins.Chart;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PlgChartCommon")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Rapid SCADA")]
[assembly: AssemblyCopyright("Copyright © 2016-2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("827ac062-9c5e-429f-abbf-368e84cec9b7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(ChartUtils.ChartVersion)]
[assembly: AssemblyFileVersion(ChartUtils.ChartVersion)]
//配置日志
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", ConfigFileExtension = "config", Watch = true)]
