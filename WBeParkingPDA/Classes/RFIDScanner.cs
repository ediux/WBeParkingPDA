using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using USICF;
using System.Windows.Forms;
using ThingMagic;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Media;

namespace WBeParkingPDA
{
    public class RFIDScanner
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RFIDScanner));

        List<int> ant = new List<int>();
        private Dictionary<string, string> properties;
        private delegate void InsertItemDelegate(object sender, TagReadDataEventArgs tagReads);
        private delegate void ShowStatusDelegate(object sender, ReaderExceptionEventArgs e);

        //private USIClass myUSI;
        private Reader _ObjReader = null;
        //static bool bEnable = true;

        // MCS++ For detect voltage
        CoreDLL.SYSTEM_POWER_STATUS_EX2 status;

        private TextBox taginputbox;

        public TextBox TagInputBox { get { return taginputbox; } set { taginputbox = value; } }

        //private Label scannerstatuslabel;
        //public Label ScannerStatusLabel { get { return scannerstatuslabel; } set { scannerstatuslabel = value; } }

        private Form _form;

        //Sounds
        SoundPlayer playBeep = new SoundPlayer();
        SoundPlayer playStartAudio = new SoundPlayer();
        SoundPlayer playStopAudio = new SoundPlayer();


        public event EventHandler<TagReadDataEventArgs> OnAfterTagRead;
        public event EventHandler<ReaderExceptionEventArgs> OnTagReadException;

        public RFIDScanner(Form form)
        {
            _form = form;
            properties = Utility.GetProperties();
            status = new CoreDLL.SYSTEM_POWER_STATUS_EX2();
        }

        public static Reader ConnectReader()
        {
            try
            {
                Reader objReader = null;
                string readerPort = null;
                List<string> ports = new List<string>();

                logger.Debug("Starting ConnectReader");

                // First, try last known-good port
                Dictionary<string, string> properties = Utility.GetProperties();


                string lastPort = properties["comport"];

                System.Diagnostics.Debug.Write(string.Format("lastPort = {0}", lastPort));

                logger.Debug("lastPort = " + lastPort);
                if (lastPort != "")
                {
                    int retries = 3;
                    while ((0 < retries) && (null == objReader))
                    {
                        logger.Debug("Trying last-known port: " + lastPort
                         + " (" + retries.ToString() + " tries left)");

                        retries -= 1;
                        readerPort = lastPort;
                        objReader = ConnectReader(lastPort);
                        if (null != objReader) { break; }
                        Thread.Sleep(1000);
                    }
                }
                if (null == objReader)
                {
                    // Next, try all known ports
                    // DiskLog.Log("Enumerating serial ports...");
                    string[] portnames = System.IO.Ports.SerialPort.GetPortNames();

                    //Add Port names
                    int newsize = portnames.Count() + 1;
                    Array.Resize(ref portnames, newsize);
                    portnames[newsize - 1] = "COM8";
                    //

                    Array.Reverse(portnames);
                    ports.AddRange(portnames);
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Detected Ports:");
                        foreach (string port in ports)
                        {
                            sb.Append(" " + port.ToString());
                        }
                        // logger.Debug(sb.ToString());
                    }
#if DEBUG
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Detected Ports:");
                    foreach (string port in ports)
                    {
                        sb.Append(" " + port.ToString());
                    }
                    // DiskLog.Log(sb.ToString());
                }
#endif
                    foreach (string port in ports)
                    {
                        switch (port.ToUpper())
                        {
                            case "COM0":
                            case "COM1":  // physical DB-9 port
                            case "COM2":  // GPS port
                            case "COM3":  // GPS intermediate driver
                                //logger.Debug("Skipping port " + port);
                                continue;
                        }
                        //   logger.Debug("Trying port " + port);
#if DEBUG
                    // DiskLog.Log("Trying port " + port);
#endif
                        //Connect to the usb reader
                        readerPort = port;
                        objReader = ConnectReader(port);
                        if (null != objReader)
                        {
                            StringBuilder portBlacklistSB = new StringBuilder();
                            foreach (string portName in ports)
                            {
                                if (portName != readerPort)
                                {
                                    portBlacklistSB.Append(" " + portName + " ");
                                }
                            }
                            string portBlacklist = portBlacklistSB.ToString();

#if DEBUG
                        string message =
                            "Found a reader on port " + readerPort + "\r\n"
                            + "portBlacklist: " + portBlacklist;
                        // DiskLog.Log(message);
#endif
                            break;
                        }
                    }
                }

                // Did we find a reader?
                if (objReader == null)
                {
                    string message = "RFID reader was not found. Please check the USB connection or re-install the FTDI driver";
                    logger.Error(message);
                    throw new Exception(message);
                }
                else
                {
#if DEBUG
                logger.Debug("Saving reader port " + readerPort);
#endif
                    properties["comport"] = readerPort;
                    Utility.SaveConfigurations(properties);
#if DEBUG
                logger.Debug("Connected: " + readerPort);
#endif
                    logger.Info("Connected to reader on port " + readerPort);
                    return objReader;
                }
            }
            catch (Exception)
            {
                throw;
            }


           
        }

        public static Reader ConnectReader(string port)
        {
            Reader objReader = null;
            try
            {
                logger.Info("Starting ConnectReader(" + port + ")...");

                objReader = new SerialReader(@"/" + port);
                logger.Info("Created new SerialReader(" + @"/" + port + ")");

                logger.Info("Set baud rate to 9600");

                objReader.Connect();

                string _readerPortName = port;
                logger.Info("Connected to SerialReader on " + port);
                objReader.Transport += new EventHandler<TransportListenerEventArgs>(objReader_Transport);

            }
            catch (Exception ex)
            {
                logger.Error("In ConnectReader(" + port + "): " + ex.ToString());
                
                objReader = null;
               
            }
            return objReader;
        }

        static void objReader_Transport(object sender, TransportListenerEventArgs e)
        {
            System.Diagnostics.Debug.Write(String.Format(
                  "{0}: {1} (timeout={2:D}ms)",
                  e.Tx ? "TX" : "RX",
                  ByteFormat.ToHex(e.Data, "", " "),
                  e.Timeout
                  ));
        }

        public void EnableReader()
        {
            try
            {
                // Make sure reader is connected
                _ObjReader = ReadMgr.GetReader();

                if (_ObjReader == null)
                {
                    //if (scannerstatuslabel != null)
                    //{
                    //    scannerstatuslabel.Text = "RFID Reader Failed!";
                    //    scannerstatuslabel.Visible = true;
                    //}
                    if (OnTagReadException != null)
                    {
                        OnTagReadException.Invoke(this,new ReaderExceptionEventArgs(new ReaderException("RFID Reader Failed!")));
                    }
                    return;
                }

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // MCS++
                    //Check the battery power level
                    switch (BatteryStatusCheck())
                    {
                        case 1:     //Battery percentage low
                            throw new Exception("Battery level is too low to read tags.");
                            //MessageBox.Show();
                            //return;
                        case 2:     //Battery Voltage low
                            throw new Exception("Battery voltage is too low to read tags.");
                            //MessageBox.Show();
                            //return;
                    }
                    // MCS--

                    properties["isreading"] = "yes";
                    Utility.SaveConfigurations(properties);

                    //disable read power coverage

                    ReadMgr.GetReader().ParamSet("/reader/transportTimeout", 2000);
                    int powerLevel = Convert.ToInt32(properties["readpower"]);
                    ReadMgr.GetReader().ParamSet("/reader/radio/readPower", powerLevel);
                    Utility.SwitchRegion(properties["region"]);
                    // MCS++
#if MCS_NOMAD
                        ReadMgr.GetReader().ParamSet("/reader/antenna/txRxMap", new int[][] { new int[] { 1, 1, 1 } });
                        ant.Add(1);
#else
                    // MCS++ - Will we use 1 or 2 antenna?
                    ReadMgr.GetReader().ParamSet("/reader/antenna/txRxMap", new int[][] { new int[] { 2, 2, 2 } });
                    // MCS--
#endif
                    // MCS--
                    SimpleReadPlan plan = new SimpleReadPlan(ant.ToArray(), TagProtocol.GEN2);
                    ReadMgr.GetReader().ParamSet("/reader/read/plan", plan);

                    //set properties
                    ReadMgr.GetReader().ParamSet("/reader/read/asyncOffTime", 50);
                    ReadMgr.GetReader().ParamSet("/reader/powerMode", Reader.PowerMode.FULL);

                    //set the tag population settings
                    ReadMgr.GetReader().ParamSet("/reader/gen2/target", Gen2.Target.A);//default target
                    string tagPopulation = properties["tagpopulation"];
                    switch (tagPopulation)
                    {
                        case "small":
                            ReadMgr.GetReader().ParamSet("/reader/gen2/q", new Gen2.StaticQ(2));
                            ReadMgr.GetReader().ParamSet("/reader/gen2/session", Gen2.Session.S0);
                            ReadMgr.GetReader().ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M4);
                            break;
                        case "medium":
                            ReadMgr.GetReader().ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
                            ReadMgr.GetReader().ParamSet("/reader/gen2/session", Gen2.Session.S1);
                            ReadMgr.GetReader().ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M4);
                            break;
                        case "large":
                            ReadMgr.GetReader().ParamSet("/reader/gen2/q", new Gen2.StaticQ(6));
                            ReadMgr.GetReader().ParamSet("/reader/gen2/session", Gen2.Session.S1);
                            ReadMgr.GetReader().ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M2);
                            break;
                        default: break;
                    }

                    if (null != properties)
                    {
                        Utility.SetReaderSettings(ReadMgr.GetReader(), properties);
                    }
                    else
                        throw new Exception("properties are null");
                        //MessageBox.Show("properties are null");
                    //set the read plan and filter
                    TagFilter filter;
                    int addressToRead = int.Parse(properties["selectionaddress"]);
                    Gen2.Bank bank = Gen2.Bank.EPC;
                    switch (properties["tagselection"].ToLower())
                    {
                        case "None":
                        case "epc": bank = Gen2.Bank.EPC; break;
                        case "tid": bank = Gen2.Bank.TID; break;
                        case "user": bank = Gen2.Bank.USER; break;
                        case "reserved": bank = Gen2.Bank.RESERVED; break;
                        default: break;

                    }
                    if ("yes" == properties["ismaskselected"])
                    {
                        filter = new Gen2.Select(true, bank, (uint)addressToRead * 8, (ushort)(properties["selectionmask"].Length * 4), ByteFormat.FromHex(properties["selectionmask"]));
                    }
                    else
                    {
                        filter = new Gen2.Select(false, bank, (uint)addressToRead * 8, (ushort)(properties["selectionmask"].Length * 4), ByteFormat.FromHex(properties["selectionmask"]));
                    }

                    // MCS++
#if MCS_NOMAD
                        SimpleReadPlan srp;
                        if (properties["tagselection"].ToLower() == "none")
                        {
                            srp = new SimpleReadPlan(new int[] { 1 }, TagProtocol.GEN2, null, 0);
                        }
                        else
                        {
                            srp = new SimpleReadPlan(new int[] { 1 }, TagProtocol.GEN2, filter, 0);
                        }
                        ReadMgr.GetReader().ParamSet("/reader/read/plan", srp);
#else
                    // MCS++ - use antenna 2
                    SimpleReadPlan srp;
                    if (properties["tagselection"].ToLower() == "none")
                    {
                        //srp = new SimpleReadPlan(new int[] { 1 }, TagProtocol.GEN2, null, 0);
                        srp = new SimpleReadPlan(new int[] { 2 }, TagProtocol.GEN2, null, 0);
                    }
                    else
                    {
                        //srp = new SimpleReadPlan(new int[] { 1 }, TagProtocol.GEN2, filter, 0);
                        srp = new SimpleReadPlan(new int[] { 2 }, TagProtocol.GEN2, filter, 0);
                    }
                    ReadMgr.GetReader().ParamSet("/reader/read/plan", srp);
                    // MCS--
#endif
                    // MCS--
                    ReadMgr.GetReader().ReadException += _ObjReader_ReadException;
                    ReadMgr.GetReader().TagRead += _ObjReader_TagRead;
                    ReadMgr.GetReader().StartReading();
                }
                catch (Exception ex)
                {
                    
                    writeErrorLog(ex.Message, ex);
                    ReadMgr.GetReader().ParamSet("/reader/powerMode", Reader.PowerMode.MAXSAVE);
                    properties["isreading"] = "no";
                    Utility.SaveConfigurations(properties);
                    throw ex;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {              
                writeErrorLog(ex.ToString(), ex);
                if (-1 != ex.Message.IndexOf("RFID reader was not found"))
                {
                    //MessageBox.Show(ex.Message, "Error");
                    throw ex;
                }
                else
                {
                    properties["isreading"] = "no";
                    Utility.SaveConfigurations(properties);
                    throw ex;
                }
            }
        }

        internal delegate bool BooleanReturnInvoker();
        internal delegate void NoneReturnInvoker();

        public bool DisableReader()
        {
            try
            {
                if (_form.InvokeRequired)
                {
                    return (bool)_form.Invoke(new BooleanReturnInvoker(DisableReader));
                }
                else
                {
                    if (properties["isreading"] == "yes")
                    {

                        logger.Debug("Calling StopReading from StopReads");
                        ReadMgr.GetReader().StopReading();
                        logger.Debug("Called StopReading from StopReads");
                        ReadMgr.GetReader().TagRead -= _ObjReader_TagRead;
                        ReadMgr.GetReader().ReadException -= _ObjReader_ReadException;
                        ReadMgr.GetReader().ParamSet("/reader/powerMode", Reader.PowerMode.MINSAVE);
                        properties["isreading"] = "no";
                        properties["powermode"] = "minsave";
                        Utility.SaveConfigurations(properties);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {                
                writeErrorLog(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Stop read while exception
        /// </summary>
        private void ExceptionStopReads()
        {
            if (_form != null && _form.InvokeRequired)
            {
                _form.Invoke(new NoneReturnInvoker(ExceptionStopReads));
            }
            else
            {
                try
                {
                    ReadMgr.GetReader().TagRead -= _ObjReader_TagRead;
                    ReadMgr.GetReader().ReadException -= _ObjReader_ReadException;
                    properties["isreading"] = "no";
                    properties["powermode"] = "minsave";
                    Utility.SaveConfigurations(properties);
                }
                catch (Exception ex)
                {
                    writeErrorLog(ex.Message, ex);
                    throw ex;
                }

            }

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
                        playBeepSound();

                        taginputbox.Text = e.TagReadData.EpcString;
                        taginputbox.Enabled = false;
                        taginputbox.Visible = true;
                        logger.Info(string.Format("EPC={0}", e.TagReadData.Epc));

                        if (OnAfterTagRead != null)
                        {
                            OnAfterTagRead.Invoke(sender, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                writeErrorLog(ex.Message, ex);
                _ObjReader_ReadException(sender, new ReaderExceptionEventArgs(new ReaderException(ex.Message, ex)));
                this.DisableReader();
                throw ex;
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
                        OnTagReadException.Invoke(sender, e);
                    }

                }
            }
            catch (Exception ex)
            {
               
                writeErrorLog(ex.Message, ex);
                throw ex;
            }

        }

        // MCS++ Check battery status
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
                writeErrorLog("In playBeepSound: " + ex.ToString(),ex);    
            }
        }
        internal delegate void NoneReturnErrorLogWriterInvoker(object message, Exception ex);
        internal delegate void NoneReturnInfoLogWriterInvoker(object message);

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
