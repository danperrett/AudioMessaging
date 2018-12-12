using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioIntercomm
{
    class AudioInterface
    {
        AudioReflector.AudioReflectClient audioclient = new AudioReflector.AudioReflectClient();
        double version = 0.0;
        public AudioInterface(string username)
        {
            version = audioclient.getVersion();
            int id = audioclient.createNewStream(username, "images", null, 1);
        }

        public void sendToServer(byte[] data, int length)
        {

        }
    }
}
