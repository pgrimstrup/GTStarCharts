using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace PanasonicNZ.Common
{
    public class ActiveDirectoryData
    {
        [Flags]
        public enum UserFlags
        {
            Script = 1,                  // 0x1
            AccountDisabled = 2,              // 0x2
            HomeDirectoryRequired = 8,           // 0x8 
            AccountLockedOut = 16,             // 0x10
            PasswordNotRequired = 32,           // 0x20
            PasswordCannotChange = 64,           // 0x40
            EncryptedTextPasswordAllowed = 128,      // 0x80
            TempDuplicateAccount = 256,          // 0x100
            NormalAccount = 512,              // 0x200
            InterDomainTrustAccount = 2048,        // 0x800
            WorkstationTrustAccount = 4096,        // 0x1000
            ServerTrustAccount = 8192,           // 0x2000
            PasswordDoesNotExpire = 65536,         // 0x10000
            MnsLogonAccount = 131072,           // 0x20000
            SmartCardRequired = 262144,          // 0x40000
            TrustedForDelegation = 524288,         // 0x80000
            AccountNotDelegated = 1048576,         // 0x100000
            UseDesKeyOnly = 2097152,            // 0x200000
            DontRequirePreauth = 4194304,          // 0x400000
            PasswordExpired = 8388608,           // 0x800000
            TrustedToAuthenticateForDelegation = 16777216, // 0x1000000
            NoAuthDataRequired = 33554432         // 0x2000000
        }

        // Cache lists for searches 
        Dictionary<string, List<string>> userSearchCache = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        Dictionary<string, ADUser> userCache = new Dictionary<string, ADUser>();
        Dictionary<string, List<string>> groupSearchCache = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

        public bool IsMemberOf(string loginName, string groupName)
        {
            var usernames = UserGroupMembers(groupName).ToArray();
            return usernames.Contains(loginName, StringComparer.InvariantCultureIgnoreCase);
        }

        Dictionary<string, List<string>> groupMemberCache = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

        static TimeSpan ClientTimeout = TimeSpan.FromSeconds(5);

        public ActiveDirectoryData()
        {
        }

        public bool IsInDomain
        {
            get { return Environment.UserDomainName != Environment.MachineName; }
        }

        public string FQDN
        {
            get { return IPGlobalProperties.GetIPGlobalProperties().DomainName; }
        }

        public IEnumerable<string> UserNames(string term, int maxcount = 20)
        {
            if (userSearchCache.TryGetValue($"{term}:{maxcount}", out List<string> names))
                return names;

            names = new List<string>();
            if (IsInDomain)
            {
                // Search active directory
                var Root = new DirectoryEntry($"LDAP://{FQDN}", null, null, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | AuthenticationTypes.ReadonlyServer);
                using (var searcher = new DirectorySearcher(Root))
                {
                    searcher.ClientTimeout = ClientTimeout;
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PageSize = maxcount;
                    searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName=*{term}*))";

                    var result = searcher.FindAll();
                    foreach (SearchResult item in result)
                    {
                        var user = item.GetDirectoryEntry();
                        names.Add($"{Environment.UserDomainName}\\{user.AccountName()}");
                    }
                }
            }
            else
            {
                // Search local computer only
                var Root = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");
                foreach (DirectoryEntry child in Root.Children)
                {
                    if (child.SchemaClassName == "User")
                    {
                        if (String.IsNullOrEmpty(term) || child.Name.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            try
                            {
                                var flags = (UserFlags)GetProperty(child, "UserFlags", 0);
                                if ((flags & UserFlags.AccountDisabled) == 0)
                                {
                                    names.Add(child.Name);
                                }
                            }
                            catch
                            {
                                // Ignore invalid users - probable orphaned entries
                            }
                        }
                    }
                }
            }

            userSearchCache.Add($"{term}:{maxcount}", names);
            return names;
        }

        public IEnumerable<string> UserGroups(string term, int maxcount = 20)
        {
            if (groupSearchCache.TryGetValue($"{term}:{maxcount}", out List<string> names))
                return names;

            names = new List<string>();
            if (IsInDomain)
            {
                var Root = new DirectoryEntry($"LDAP://{FQDN}", null, null, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | AuthenticationTypes.ReadonlyServer);
                using (var searcher = new DirectorySearcher(Root))
                {
                    searcher.ClientTimeout = ClientTimeout;
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PageSize = maxcount;
                    searcher.Filter = $"(&(objectClass=group)(sAMAccountName={term}*))";

                    var result = searcher.FindAll();
                    foreach (SearchResult item in result)
                    {
                        var group = item.GetDirectoryEntry();
                        names.Add($"{Environment.UserDomainName}\\{group.AccountName()}");
                    }
                }
            }
            else
            {
                // Search local computer only
                var Root = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");
                foreach (DirectoryEntry child in Root.Children)
                {
                    if (child.SchemaClassName == "Group")
                    {
                        if (String.IsNullOrEmpty(term) || child.Name.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            names.Add(child.Name);
                    }
                }
            }

            groupSearchCache.Add($"{term}:{maxcount}", names);
            return names;
        }

        public IEnumerable<string> UserGroupMembers(string groupName)
        {
            // Split name into domain and name parts
            string domain = Environment.UserDomainName;
            if (groupName.IndexOf('\\') >= 0)
            {
                domain = groupName.Split('\\').First();
                groupName = groupName.Split('\\').Last();
            }

            if (groupMemberCache.TryGetValue($"{domain}\\{groupName}", out List<string> found))
                return found;

            List<string> names = new List<string>();
            if (IsInDomain)
            {
                var Root = new DirectoryEntry($"LDAP://{FQDN}", null, null, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | AuthenticationTypes.ReadonlyServer);

                // Search for the group first
                string distinguishedName = null;
                string primaryGroupId = null;
                using (var searcher1 = new DirectorySearcher(Root))
                {
                    searcher1.ClientTimeout = ClientTimeout;
                    searcher1.SearchScope = SearchScope.Subtree;
                    searcher1.PageSize = 1000;
                    searcher1.Filter = $"(&(objectClass=group)(sAMAccountName={groupName}*))";

                    var result = searcher1.FindOne()?.GetDirectoryEntry();
                    if (result == null)
                        return new String[0];

                    distinguishedName = result.Properties["distinguishedName"][0].ToString();

                    var sid = new SecurityIdentifier(result.Properties["objectSid"][0] as byte[], 0);
                    Regex regRID = new Regex(@"^S.*-(\d+)$");
                    Match matchRID = regRID.Match(sid.Value);
                    primaryGroupId = matchRID.Groups[1].Value;
                }


                using (var searcher = new DirectorySearcher(Root))
                {
                    searcher.ClientTimeout = ClientTimeout;
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.PageSize = 1000;
                    searcher.Filter = $"(&(objectClass=user)(|(memberOf={distinguishedName})(primaryGroupID={primaryGroupId})))";

                    var result = searcher.FindAll();
                    foreach (SearchResult item in result)
                    {
                        var user = item.GetDirectoryEntry();
                        names.Add($"{domain}\\{user.AccountName()}");
                    }
                }
            }
            else
            {
                try
                {
                    // Search local computer only
                    var group = new DirectoryEntry($"WinNT://{domain}/{groupName},group");
                    var members = group.Invoke("Members") as IEnumerable;
                    foreach (object member in members)
                    {
                        var memberentry = new DirectoryEntry(member);
                        try
                        {
                            var flags = (UserFlags)GetProperty(memberentry, "UserFlags", 0);
                            if ((flags & UserFlags.AccountDisabled) == 0)
                                names.Add(memberentry.Name);
                        }
                        catch
                        {
                            // Ignore members that are invalid - possible orphaned entries
                        }
                    }
                }
                catch
                {
                    // Ignore groups that are not found
                }
            }

            groupMemberCache.Add($"{domain}\\{groupName}", names);
            return names;
        }

        public ADUser FindUser(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            // Split name into domain and name parts
            string domain = Environment.UserDomainName;
            if (name.IndexOf('\\') >= 0)
            {
                domain = name.Split('\\').First();
                name = name.Split('\\').Last();
            }

            // Check the cache first
            if (userCache.TryGetValue($"{domain}\\{name}", out ADUser found))
                return found;

            if (IsInDomain)
            {
                try
                {
                    // Search active directory
                    var Root = new DirectoryEntry($"LDAP://{FQDN}", null, null, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind | AuthenticationTypes.ReadonlyServer);
                    using (var searcher = new DirectorySearcher(Root))
                    {
                        searcher.ClientTimeout = ClientTimeout;
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.PageSize = 20;
                        searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={name}))";

                        var result = searcher.FindAll();
                        foreach (SearchResult item in result)
                        {
                            // Return the first user found
                            var user = item.GetDirectoryEntry();
                            var ad = new ADUser(this, user);

                            // Add to cache
                            userCache.Add($"{domain}\\{name}", ad);
                            return ad;
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    userCache.Add($"{domain}\\{name}", null);
                    return null;
                }
            }
            else
            {
                // Search local computer only
                // WinNT does not support first name, last name, email and phone number.
                var user = new DirectoryEntry($"WinNT://{domain}/{name},user", null, null, AuthenticationTypes.Secure | AuthenticationTypes.ServerBind);
                try
                {
                    user.RefreshCache(new string[] { "Name", "FullName" });
                    var ad = new ADUser(this, user);
                    userCache.Add($"{domain}\\{name}", ad);
                    return ad;
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    userCache.Add($"{domain}\\{name}", null);
                    return null;
                }
            }

            return null;
        }


        string GetProperty(SearchResult entry, string prop)
        {
            var values = entry.Properties[prop];
            if (values != null && values.Count > 0)
                return values[0].ToString();
            return null;
        }

        string GetProperty(DirectoryEntry entry, string prop)
        {
            var values = entry.Properties[prop];
            if (values != null && values.Count > 0)
                return values[0].ToString();
            return null;
        }

        int GetProperty(DirectoryEntry entry, string prop, int defaultValue)
        {
            var values = entry.Properties[prop];
            if (values != null && values.Count > 0)
                return Convert.ToInt32(values[0]);
            return defaultValue;
        }

        public class ADUser
        {
            readonly ActiveDirectoryData AD;
            readonly DirectoryEntry Entry;
            List<string> groups;

            internal ADUser(ActiveDirectoryData owner, DirectoryEntry entry)
            {
                AD = owner;
                Entry = entry;
            }

            public string AccountName
            {
                get
                {
                    if (AD.IsInDomain)
                        return Entry.AccountName();
                    else
                        return Entry.Name;
                }
            }

            public string DisplayName {
                get
                {
                    if (AD.IsInDomain)
                        return Entry.DisplayName();
                    else
                        return (string)Entry.InvokeGet("FullName");
                }
            }

            public string Email
            {
                get
                {
                    if (AD.IsInDomain)
                        return Entry.EmailAddress();
                    return null;
                }
            }

            public string PhoneNumber
            {
                get
                {
                    if (AD.IsInDomain)
                        return Entry.PhoneNumber();
                    return null;
                }
            }

            public ADUser LoadGroups()
            {
                if (AD.IsInDomain)
                {
                    this.groups = Entry.GetGroups();
                }
                else
                {
                    this.groups = new List<string>();
                    var adgroups = Entry.Invoke("Groups") as IEnumerable;
                    foreach (var group in adgroups)
                    {
                        var entry = new DirectoryEntry(group);
                        this.groups.Add(entry.Name);
                    }
                }
                return this;
            }

            public bool IsMemberOf(string groupName)
            {
                if (String.IsNullOrEmpty(groupName))
                    return false;

                if (groups == null) LoadGroups();
                return groups.Any(g => g.Equals(groupName, StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
