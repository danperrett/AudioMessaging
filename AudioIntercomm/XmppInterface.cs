using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;

namespace AudioIntercomm
{
    class XmppInterface
    {
        string jid_sender = "@tigase.im";
        XmppClientConnection xmpp = null;
        public XmppInterface(string username, string password)
        {
            jid_sender = username + jid_sender; 
            Jid jidSender = new Jid(jid_sender);
            xmpp = new XmppClientConnection(jidSender.Server);
            xmpp.OnMessage += Xmpp_OnMessage;
            xmpp.Open(jidSender.User, password);
         

        }

        private void Xmpp_OnMessage(object sender, Message msg)
        {
          
        }

        public bool send(string message, string jid)
        {
            if(xmpp != null)
            {
                xmpp.OnLogin += delegate (object o) { xmpp.Send(new Message(new Jid(jid), MessageType.chat, message)); };
            }
            return false;
        }
    }
}
