using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using ThingMagic;
using System.Runtime.InteropServices;
using System.Media;

namespace WBeParkingPDA
{
    public class FakeTagReadData : TagReadData
    {

        public FakeTagReadData()
        {
            _epcString = Guid.NewGuid().ToString("N");
        }
        private string _epcString;

        public new string EpcString { get { return _epcString; } }

    }
    //RFID讀取模擬器
    public class FakeRFIDScanner
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(FakeRFIDScanner));

        private TextBox taginputbox;
        private Form _form;

        private Dictionary<string, string> properties;

        // MCS++ For detect voltage
        CoreDLL.SYSTEM_POWER_STATUS_EX2 status;

        private delegate void InsertItemDelegate(object sender, TagReadDataEventArgs tagReads);
        private delegate void ShowStatusDelegate(object sender, ReaderExceptionEventArgs e);

        public TextBox TagInputBox { get { return taginputbox; } set { taginputbox = value; } }

        public event EventHandler<TagReadDataEventArgs> OnAfterTagRead;
        public event EventHandler<ReaderExceptionEventArgs> OnTagReadException;

        SoundPlayer playBeep = new SoundPlayer();
        SoundPlayer playStartAudio = new SoundPlayer();
        SoundPlayer playStopAudio = new SoundPlayer();

        public FakeRFIDScanner(Form form)
        {
            _form = form;
            properties = Utility.GetProperties();
            status = new CoreDLL.SYSTEM_POWER_STATUS_EX2();
        }

        public static Reader ConnectReader()
        {
            try
            {

                //                string readerPort = null;
                //                List<string> ports = new List<string>();

                //                logger.Debug("Starting ConnectReader");

                //                // First, try last known-good port
                //                Dictionary<string, string> properties = Utility.GetProperties();


                //                string lastPort = properties["comport"];

                //                System.Diagnostics.Debug.Write(string.Format("lastPort = {0}", lastPort));

                //                logger.Debug("lastPort = " + lastPort);
                //                if (lastPort != "")
                //                {
                //                    int retries = 3;
                //                    while ((0 < retries))
                //                    {
                //                        logger.Debug("Trying last-known port: " + lastPort
                //                         + " (" + retries.ToString() + " tries left)");

                //                        retries -= 1;
                //                        readerPort = lastPort;

                //                        Thread.Sleep(1000);
                //                    }
                //                }
                //                if (true)
                //                {
                //                    // Next, try all known ports
                //                    // DiskLog.Log("Enumerating serial ports...");
                //                    string[] portnames = System.IO.Ports.SerialPort.GetPortNames();

                //                    //Add Port names
                //                    int newsize = portnames.Count() + 1;
                //                    Array.Resize(ref portnames, newsize);
                //                    portnames[newsize - 1] = "COM8";
                //                    //

                //                    Array.Reverse(portnames);
                //                    ports.AddRange(portnames);
                //                    {
                //                        StringBuilder sb = new StringBuilder();
                //                        sb.Append("Detected Ports:");
                //                        foreach (string port in ports)
                //                        {
                //                            sb.Append(" " + port.ToString());
                //                        }
                //                        // logger.Debug(sb.ToString());
                //                    }
                //#if DEBUG
                //                {
                //                    StringBuilder sb = new StringBuilder();
                //                    sb.Append("Detected Ports:");
                //                    foreach (string port in ports)
                //                    {
                //                        sb.Append(" " + port.ToString());
                //                    }
                //                    // DiskLog.Log(sb.ToString());
                //                }
                //#endif
                //                    foreach (string port in ports)
                //                    {
                //                        switch (port.ToUpper())
                //                        {
                //                            case "COM0":
                //                            case "COM1":  // physical DB-9 port
                //                            case "COM2":  // GPS port
                //                            case "COM3":  // GPS intermediate driver
                //                                //logger.Debug("Skipping port " + port);
                //                                continue;
                //                        }
                //                        //   logger.Debug("Trying port " + port);
                //#if DEBUG
                //                    // DiskLog.Log("Trying port " + port);
                //#endif
                //                        //Connect to the usb reader
                //                        readerPort = port;

                //                    }
                //                }

                // Did we find a reader?
                return null;
            }
            catch (Exception)
            {
                throw;
            }



        }

        public static Reader ConnectReader(string port)
        {
            Reader objReader = null;

            return objReader;
        }
        private static bool isRunning;

        public void EnableReader()
        {
            isRunning = true;
            System.Threading.Thread MTStart = new Thread(new ThreadStart(RandomRFIDSender));
            MTStart.Start();
            MTStart.IsBackground = true;
        }

        private static string EPCID = "";
        private void RandomRFIDSender()
        {
            while (isRunning)
            {
                //104113BD500218B546455443
                if (OnAfterTagRead != null)
                {
                    EPCID = Guid.NewGuid().ToString("N");
                    //FakeTagReadData tagdata = new FakeTagReadData();
                    TagReadData tagdata = new TagReadData();
                    //OnAfterTagRead(this,new TagReadDataEventArgs(tagdata));
                    _ObjReader_TagRead(this, new TagReadDataEventArgs(tagdata));
                    // OnAfterTagRead.Invoke(this, );
                }

                System.Threading.Thread.Sleep(3000);

                //_ObjReader_TagRead(this, new TagReadDataEventArgs(new TagReadData() {  = Encoding.UTF8.GetBytes( Guid.NewGuid().ToString("N")) });
            }

        }

        internal delegate bool BooleanReturnInvoker();
        internal delegate void NoneReturnInvoker();

        public bool DisableReader()
        {
            isRunning = false;
            return true;
        }


        void _ObjReader_TagRead(object sender, TagReadDataEventArgs e)
        {
            try
            {
                if (_form.InvokeRequired)
                {
                    _form.Invoke(new InsertItemDelegate(_ObjReader_TagRead), sender, e);
                }
                else
                {
                    if (taginputbox != null)
                    {
                        //playBeepSound();
                        if (e.TagReadData != null)
                        {
                            try
                            {
                                taginputbox.Text = EPCID;
                                //taginputbox.Text = e.TagReadData.EpcString;
                                logger.Info(string.Format("EPC={0}", EPCID));
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message,ex);
                                taginputbox.Text = "";
                            }

                            taginputbox.Enabled = false;
                            taginputbox.Visible = true;
                            
                        }


                        if (OnAfterTagRead != null)
                        {
                            OnAfterTagRead(sender, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                writeErrorLog(ex.Message, ex);
                //_ObjReader_ReadException(sender, new ReaderExceptionEventArgs(new ReaderException(ex.Message, ex)));
                this.DisableReader();
                //throw ex;
            }

        }

        void _ObjReader_ReadException(object sender, ReaderExceptionEventArgs e)
        {
            try
            {
                //logger.Error(e.ReaderException.Message, e.ReaderException);
                writeErrorLog(e.ReaderException.Message, e.ReaderException);

                if (_form.InvokeRequired)
                {
                    _form.Invoke(new ShowStatusDelegate(_ObjReader_ReadException), sender, e);
                }
                else
                {
                    if (OnTagReadException != null)
                    {
                        OnTagReadException(sender, e);
                    }

                }
            }
            catch (Exception ex)
            {

                writeErrorLog(ex.Message, ex);
                throw ex;
            }

        }

        internal delegate void NoneReturnErrorLogWriterInvoker(object message, Exception ex);
        internal delegate void NoneReturnInfoLogWriterInvoker(object message);

        private int BatteryStatusCheck()
        {
            try
            {
                if (CoreDLL.GetSystemPowerStatusEx2(status, (uint)Marshal.SizeOf(status), true) != 0)
                {
                    if (status.BatteryLifePercent <= 5)
                    {
                        return 1;
                    }
                    else if (status.BatteryVoltage < 3700)
                    {
                        return 2;
                    }
                    else
                        return 0;
                }
            }
            catch (Exception ex)
            {
                writeErrorLog(ex.Message, ex);
                throw ex;
            }

            return -1;
        }

        private void playStartSound()
        {
            try
            {
                playStartAudio.Play();
            }
            catch (Exception ex)
            {
                writeErrorLog("In playStartSound: " + ex.ToString(), ex);
            }
        }

        private void playStopSound()
        {
            try
            {
                playStopAudio.Play();
            }
            catch (Exception ex)
            {
                writeErrorLog("In playStopSound: " + ex.ToString(), ex);
            }
        }

        private void playBeepSound()
        {
            try
            {
                playBeep.Play();
            }
            catch (Exception ex)
            {
                writeErrorLog("In playBeepSound: " + ex.ToString(), ex);
            }
        }

        private void writeErrorLog(object message, Exception ex)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new NoneReturnErrorLogWriterInvoker(writeErrorLog), message, ex);
            }
            else
            {
                logger.Error(message, ex);
            }
        }

        private void writeInfoLog(object message)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new NoneReturnInfoLogWriterInvoker(writeInfoLog), message);
            }
            else
            {
                logger.Info(message);
            }
        }
    }
}
