using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Media;
using NAudio;
using NAudio.Wave;
using System.Threading;

namespace AudioIntercomm
{
    public partial class AudioForm : Form
    {
        List<string> toList = new List<string>();
        MemoryStream stream = null;
        WaveIn sourceStream = null;
        WaveFileWriter waveWriter;
        int count = 0;
        int id = -1;
        AudioReflector.AudioReflectClient service = null;
        Int64[] averageTotal = new Int64[8];
        Int32[] average = new Int32[8];
        int totalCount = 0;
        double lastTimeStamp = 0.0;
        UserDetails details = new UserDetails();

        bool recording = false;

        public AudioForm()
        {
            InitializeComponent();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FileStream stream = new FileStream("details.xml", FileMode.Open);
            XmlSerializer serialization = new XmlSerializer(typeof(UserDetails));
            details = serialization.Deserialize(stream) as UserDetails;
            if (service == null)
            {
                service = new AudioReflector.AudioReflectClient();
            }

            if (service.checkCredantials(details.Username, details.Password, 0))
            {
                UserTextBox.Text = details.Username;
                if (details.ToList != null && details.ToList.Count > 0)
                {
                    ToTextBox.Text = details.ToList[0];
                }
                PasswordTextBox.Text = details.Password;
                RecordButton.Enabled = true;
                SendButton.Enabled = true;
                MessageTextBox.Enabled = true;
            }
        }
        private void RecordButton_Click(object sender, EventArgs e)
        {
            if (!recording)
            {
                RecordButton.Text = "Stop";
                stream = new MemoryStream();
                sourceStream = new WaveIn();
                sourceStream.WaveFormat = new WaveFormat(8000, 1);
                // sourceStream.WaveFormat = new WaveFormat();
                count = 0;
                waveWriter = new WaveFileWriter(stream, sourceStream.WaveFormat);

               
                id = service.createNewStream(details.Username, details.Password, details.ToList.ToArray(), 1);

                sourceStream.DataAvailable += WaveSource_DataAvailable;

                sourceStream.StartRecording();
                sourceStream.RecordingStopped += SourceStream_RecordingStopped;
            }
            else
            {
                RecordButton.Text = "Record";
                StopButton_Click(sender, e);
            }
        }

        int resporne()
        {
            service.endAudio(details.Username, details.Password, id);
            return service.createNewStream(details.Username, details.Password, details.ToList.ToArray(), 1);
        }

        private void SourceStream_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Playback play = new Playback(stream);

            play.play();
            service.Close();
            /*  string message = "ID=" + id + ";Count=" + count + ";To=audiotest1" + ";From=audiotest2";
               count = 0;
               XmppInterface xmpp = new XmppInterface("dperrett", "images");
               xmpp.send(message, "audiotest1@codinggain.com");
           */
        }

        byte[] ChangeEndian(byte[] input)
        {
            byte[] output = new byte[input.Length];
            int k = 0;
            for (int n = 0; n < input.Length; n += 2)
            {
                output[k++] = input[n + 1];
                output[k++] = input[n];
            }

            return output;
        }

        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] outBuffer = new byte[1600];
            int outcount = 0;
            DateTime now = DateTime.Now;
            double milli = now.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            double diff = milli - lastTimeStamp;
            lastTimeStamp = milli;
            timeLabel.Text = "" + diff;
            if (waveWriter == null) return;
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            long sum = 0;
            int size = e.Buffer.Length;
            string label = "";
            int l = 0;
            byte bitmask = 0;
            int rf = 7;
            int inDataCount = 0;
            for (int n = 0; n < size; n += 200)
            {
                for (int k = 0; k < 200; k++)
                {
                    Int16 value = 0;
                    value = e.Buffer[n + k++];
                    int temp = 0xFF00 & ((e.Buffer[n + k]) << 8);
                    value += (short)temp;
                    sum += Math.Abs(value);
                }
                label += l + " : " + (double)(sum / 100) + " : ";
                averageTotal[l++] += sum / 100;
                if ((sum / 100) > 30)
                {
                    bitmask |= (byte)(1 << rf);
                    for (int k = 0; k < 200; k++)
                    {
                        outBuffer[outcount + k] = e.Buffer[inDataCount++];
                    }
                    outcount += 200;
                }
                rf--;
                sum = 0;
            }
            totalCount++;
            string averages = "";
            for (int n = 0; n < 8; n++)
            {
                average[n] = (int)(averageTotal[n] / totalCount);
                averages += n + " : " + average[n] + " :";
            }
            sumLabel.Text = label;
            averageLabel.Text = averages;
            bitmaskLabel.Text = "" + bitmask;


            count++;

            bool dts = (outcount < 1600);
            byte[] copyMsg = new byte[outcount];
            for (int n = 0; n < outcount; n++)
            {
                copyMsg[n] = outBuffer[n];
            }
            String data = Convert.ToBase64String(ChangeEndian(outBuffer));
            service.putAudio(details.Username, details.Password, outcount, id, data, bitmask, dts, 1, false);

            if (count == 30)
            {
                count = 0;
                id = resporne();
            }
            waveWriter.Flush();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            sourceStream.StopRecording();
            if (id > 0)
            {
                service.endAudio(details.Username, details.Password, id);
                id = -1;
            }
        }

        private void label1_Click(object sender, System.EventArgs e)
        {

        }

        private void CheckButton_Click(object sender, EventArgs e)
        {
            details.Username = UserTextBox.Text;
            string to = ToTextBox.Text;
            details.Password = PasswordTextBox.Text;
            if (string.IsNullOrEmpty(to))
            {
                toList.Add("audiotest2");
            }
            else
            {
                toList.Add(to);
            }
            details.ToList = toList;
            if (service == null)
            {
                service = new AudioReflector.AudioReflectClient();
            }

            if (service.checkCredantials(details.Username, details.Password, 0))
            {
                FileStream stream = new FileStream("details.xml", FileMode.Create);
                XmlSerializer serialization = new XmlSerializer(typeof(UserDetails));
                serialization.Serialize(stream, details);

                RecordButton.Enabled = true;
            }
            else
            {
                RecordButton.Enabled = false;
                SendButton.Enabled = false;
                MessageTextBox.Enabled = false;
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (service == null)
            {
                service = new AudioReflector.AudioReflectClient();
            }

            string text = MessageTextBox.Text;
            int _id = service.createNewStream(details.Username, details.Password, details.ToList.ToArray(), 3);
            service.putAudio(details.Username, details.Password, 1, _id, text, 255, false, 3, false);
            service.endAudio(details.Username, details.Password, _id);
        }
    }
}
