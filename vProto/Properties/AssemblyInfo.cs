using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("vProto")]
[assembly: AssemblyDescription("Network protocol for the .NET Framework.")]
[assembly: AssemblyCompany("Vercas")]
[assembly: AssemblyProduct("vProto")]
[assembly: AssemblyCopyright("Copyright © 2013 Alexandru-Mihai Maftei")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
#if NET_4_5
[assembly: AssemblyConfiguration("Debug @ .NET 4.5")]
#elif NET_4_0
[assembly: AssemblyConfiguration("Debug @ .NET 4.0")]
#endif
#else
#if NET_4_5
[assembly: AssemblyConfiguration("Release @ .NET 4.5")]
#elif NET_4_0
[assembly: AssemblyConfiguration("Release @ .NET 4.0")]
#endif
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b7275194-3b44-4b9e-b6af-088ad51de304")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
