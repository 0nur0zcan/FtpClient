using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FtpClient
{
    public class RemoteFileInfo
    {
        public string Type { get; private set; }
        public string Permissions { get; private set; }
        //public string Group { get; private set; }
        //public string Owner { get; private set; }
        public string Size { get; private set; } // int
        public string LastModifiedDate { get; private set; } // DateTime
        public string Name { get; private set; }
        public string FullName { get; private set; }

        private Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        /* Match Groups:
           1: object type:
                d : directory
                - : file
           2: Array[3] of permissions (rwx-)
           3: File Size
           4: Last Modified Date
           5: Last Modified Time
           6: File/Directory Name */

        public RemoteFileInfo(string detailedInfo, string parentDir)
        {
            Match match = regex.Match(detailedInfo);
            Type = match.Groups[1].Value;
            Permissions = match.Groups[2].Value;
            Size = match.Groups[3].Value;
            LastModifiedDate = Convert.ToDateTime(match.Groups[4].Value).ToShortDateString();
            Name = match.Groups[6].Value;

            if (parentDir.Equals("/"))
                FullName = parentDir + Name;
            else
                FullName = parentDir + "/" + Name;

            Size = (Convert.ToInt32(Size) > 0) ? Size : "";
        }

        public RemoteFileInfo(string parentDir)
        {   // Empty
            Type = "d";
            Name = "";
            FullName = parentDir;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsDirectory()
        {
            return Type.Equals("d");
        }
    }
}
