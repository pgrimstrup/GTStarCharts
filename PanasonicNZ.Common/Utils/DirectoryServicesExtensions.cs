using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Principal;

namespace PanasonicNZ.Common
{
    public static class DirectoryServicesExtensions
    {
        public static string AccountName(this DirectoryEntry entry)
        {
            return $"{entry.Properties["sAMAccountName"].Value}";
        }

        public static string DisplayName(this DirectoryEntry entry)
        {
            // Prefer display name from Active Directory
            if (entry.Properties.Contains("displayName"))
                return $"{entry.Properties["displayName"].Value}";

            // Otherwise, try for a first name + last name, if both are available
            if (entry.Properties.Contains("givenName") && entry.Properties.Contains("sn"))
                return $"{entry.Properties["givenName"].Value} {entry.Properties["sn"].Value}".Trim();

            // Sometimes we can get a name property
            if (entry.Properties.Contains("name"))
                return $"{entry.Properties["name"].Value}";

            // Fallback to the common name
            return $"{entry.Properties["cn"].Value}";
        }

        public static string EmailAddress(this DirectoryEntry entry)
        {
            if (entry.Properties.Contains("email"))
                return $"{entry.Properties["email"].Value}";

            return null;
        }

        public static string PhoneNumber(this DirectoryEntry entry)
        {
            if (entry.Properties.Contains("phone"))
                return $"{entry.Properties["phone"].Value}";

            return null;
        }

        public static List<string> GetGroups(this DirectoryEntry entry)
        {
            List<string> groups = new List<string>();
            try
            {
                WindowsIdentity ident = new WindowsIdentity(entry.AccountName());
                foreach (IdentityReference group in ident.Groups)
                {
                    try
                    {
                        // Group names include domain specifier, which can be <UserDomainName> or "BUILTIN" (for local machine group)
                        groups.Add(group.Translate(typeof(NTAccount)).ToString());
                    }
                    catch
                    {
                        // Ignore invalid SID for the group - group is probably deleted
                    }
                }
            }
            catch
            {
                // Ignore - account is probably disabled
            }
            return groups;
        }
    }
}
