using System;
using System.Configuration;
using System.Data;
using System.DirectoryServices;
using System.Globalization;
using System.Text;
namespace Document
{
    public class Authentication
    {
        private string _path;
        private string _filterAttribute;
        public Authentication(string path)
        {
            _path = path;
        }
        public bool IsAuthenticated(string domain, string username, string pwd)
        {
            string domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);
            //Bind to the native AdsObject to force authentication.
            object obj = entry.NativeObject;
            //  string fnamee=entry.Properties["FullName"].Value.ToString();
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(SAMAccountName=" + username + ")";
            search.PropertiesToLoad.Add("cn");
            SearchResult result = search.FindOne();
            if (null == result)
            {
                return false;
            }
            //Update the new path to the user in the directory.
            _path = result.Path;
            _filterAttribute = (string)result.Properties["cn"][0];
            return true;
        }
        public string getUserDir(string domain, string username, string pwd)
        {
            string domainAndUsername = domain + @"\" + username;
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);
            //Bind to the native AdsObject to force authentication.
            object obj = entry.NativeObject;
            //string fnamee = entry.Properties["FullName"].Value.ToString();
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(SAMAccountName=" + username + ")";
            search.PropertiesToLoad.Add("CN");
            SearchResult result = search.FindOne();
            string f = result.Properties["cn"][0].ToString();
            return f;

        }
        public string GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(_path);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();
            try
            {
                SearchResult result = search.FindOne();
                int propertyCount = result.Properties["memberOf"].Count;
                string dn;
                int equalsIndex, commaIndex;
                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (string)result.Properties["memberOf"][propertyCounter];
                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
        }
        public static string CheckInput(string parameter)
        {
            string re = null;
            string[] blackList = {"--",";--",";","/*","*/","@@",
                                         "char","nchar","varchar","nvarchar",
                                         "alter","begin","cast","create","cursor","declare","delete","drop","end","exec","execute",
                                         "fetch","insert","kill","open",
                                         "select", "sys","sysobjects","syscolumns",
                                         "table","update"};
            CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;
            for (int i = 0; i < blackList.Length; i++)
            {
                if (comparer.IndexOf(parameter, blackList[i], CompareOptions.IgnoreCase) >= 0)
                {
                    re = "error";
                }
                else
                {
                    re = "";
                }
            }
            return re;
        }


    }
}