using System;
using System.Reflection;
using System.Windows;

namespace PanasonicNZ.Common
{
    // Provides access methods for the Assembly-level attributes
    public static class ApplicationExtensions
    {
        public static string Title(this Application application)
        {
            return Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        }
        public static string Product(this Application application)
        {
            return Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        }
        public static string Description(this Application application)
        {
            return Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        }
        public static string Company(this Application application)
        {
            return Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        }
        public static string Copyright(this Application application)
        {
            return Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        }


        public static Version FileVersion(this Application application)
        {
            string version = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            if (version == null)
                return null;
            return new Version(version);
        }
    }
}
