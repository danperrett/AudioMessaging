using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioIntercomm
{
    [Serializable]
    public class UserDetails
    {
        private string username = "";
        private string password = "";
        private List<string> toList = new List<string>();
        public UserDetails()
        {

        }

        public UserDetails(string username, string password, List<string> toList)
        {
            this.username = username;
            this.password = password;
            this.toList = toList;
        }

        public string Password { get => password; set => password = value; }
        public string Username { get => username; set => username = value; }
        public List<string> ToList { get => toList; set => toList = value; }
    }
}
