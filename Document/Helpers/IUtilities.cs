using Document.Models;

namespace Document.Helpers
{
    public interface IUtilities
    {
       Task< Requester> GetLoggedInUser();
        public void SendMail(string from, string tos, string ccs, string bccs, string subject, string body);
        public string GetMail(string userName);
        public  bool WriteLog(string strFileName, string strMessage);
       
    }
}