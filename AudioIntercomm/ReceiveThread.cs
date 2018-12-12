using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AudioIntercomm
{
    class ReceiveThread 
    {
        AudioForm parent = null;
        Thread receiveThread = null;
        bool running = false;
        AudioReflector.AudioReflectClient service = null;
        string username = "";
        string password = "";


        public ReceiveThread(AudioForm parent, string username, string password)
        {
            this.parent = parent;
            service = new AudioReflector.AudioReflectClient();
            receiveThread = new Thread(new ThreadStart(run));
        }

        public void Start()
        {
            running = true;
            if(receiveThread != null)
            {
                receiveThread.Start();
            }
        }

        public void run()
        {
            while(running)
            {
                if(service != null)
                {
                    AudioReflector.AudioInformation audioInfo =service.messageAvailable(username, password);
                    if(audioInfo.type == 1)
                    {

                    }
                }

                Thread.Sleep(2000);
            }
        }
    }
}
