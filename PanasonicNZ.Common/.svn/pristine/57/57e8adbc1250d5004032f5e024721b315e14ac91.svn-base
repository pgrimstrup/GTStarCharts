using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace PanasonicNZ.Common
{
    public static class NetworkConnection
    {
        private static class ErrorCodes
        {
            public const int ERROR_PATH_NOT_FOUND = 3;
            public const int ERROR_ACCESS_DENIED = 5;
            public const int ERROR_ALREADY_ASSIGNED = 85;
            public const int ERROR_INVALUE_PASSWORD = 86;
            public const int ERROR_BAD_DEVICE_PATH = 330;
            public const int ERROR_BAD_NET_NAME = 67;
            public const int ERROR_BAD_USERNAME = 2202;
            public const int ERROR_INVALID_PASSWORD = 86;
            public const int ERROR_LOGON_FAILURE = 1326;
            public const int ERROR_NO_NETWORK = 1222;
        }

        public static bool Connect(string networkName, NetworkCredential credential = null)
        {
            var netResource = new NetResource
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplayType.Share,
                RemoteName = networkName
            };

            if (credential == null)
            {
                return WNetAddConnection2(netResource, null, null, 0) == 0;
            }
            else
            {
                var username = String.IsNullOrEmpty(credential.Domain) ? credential.UserName : $"{credential.Domain}\\{credential.UserName}";
                return WNetAddConnection2(netResource, credential.Password, username, 0) == 0;
            }
        }

        public static bool Map(string driveName, string networkName, NetworkCredential credential = null)
        {
            var netResource = new NetResource
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplayType.Share,
                RemoteName = networkName,
                LocalName = driveName
            };

            if (credential == null)
            {
                return WNetAddConnection2(netResource, null, null, 0) == 0;
            }
            else
            {
                var username = String.IsNullOrEmpty(credential.Domain) ? credential.UserName : $"{credential.Domain}\\{credential.UserName}";
                return WNetAddConnection2(netResource, credential.Password, username, 0) == 0;
            }
        }

        public static bool Disconnect(string networkName)
        {
            return WNetCancelConnection2(networkName, 0, true) == 0;
        }

        public static int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }

        public static string GetLastErrorMessage()
        {
            Exception ex = new Win32Exception(GetLastError());
            return ex.Message;
        }

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplayType DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    }

    public enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8
    }

    public enum ResourceDisplayType : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        ShareAdmin = 0x08,
        Directory = 0x09,
        Tree = 0x0A,
        NdsContainer = 0x0B
    }
}
