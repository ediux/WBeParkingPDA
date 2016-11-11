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
    /// <summary>
    /// Reader connection manager: Automatically manages Reader Connect and Destroy
    ///
    /// Maintains a single static Reader object to be shared by all users of ReadMgr.
    /// Automatically connects when reader object is requested (GetReader, Session.Reader)
    /// Automatically handles system power transitions by recovering the reader connection
    /// </summary>
    public class ReadMgr
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger("ReadMgr");
        static Reader _rdr;
        private static readonly object readerLock = new object();
        private static CleanupMgr cleanupManager = new CleanupMgr();
        static bool subscriptionsDone = false;
        static IntPtr nullIntPtr = new IntPtr(0);
        static List<IntPtr> rdrPowerReqHandles = new List<IntPtr>();

        public static LogProvider DebugLog = new LogProvider();
        public static LogProvider StatusLog = new LogProvider();
        public static LogProvider PowerLog = new LogProvider();
        public delegate void ReadMgrEventHandler(EventCode code);
        public static event ReadMgrEventHandler ReaderEvent;
        public static string lastErrorMessage = "";



        class CleanupMgr
        {
            public CleanupMgr() { }

            /// <summary>
            /// Cleanup actions to run when application exits
            /// </summary>
            ~CleanupMgr()
            {
                // Be sure to clear out unattended mode requests.
                // Those are reference-counted across the system,
                // so uncanceled ones will prevent the system from
                // ever sleeping again (until rebooted.)
                DisableUnattendedReaderMode();
            }
        }

        public enum EventCode
        {
            /// <summary>
            /// Notify app that power was interrupted.
            /// App should display wait cursor, because we're not sure
            /// how long it will take to process the next power event.
            /// </summary>
            POWER_INTERRUPTED,
            /// Notify app that power was suspended.
            /// App should shut down FTDI peripherals to prevent driver corruption.
            /// </summary>
            POWER_SUSPEND,
            /// <summary>
            /// Power came back
            /// App can stop displaying wait cursor now.
            /// </summary>
            POWER_RESTORED,
            /// <summary>
            /// Reader connection has been shut down.
            /// </summary>
            READER_SHUTDOWN,
            /// <summary>
            /// Reader connection needs to be reset.
            /// App should display wait cursor.
            /// </summary>
            READER_RECOVERING,
            /// <summary>
            /// ReadMgr is done with reader recovery routine.
            /// App can stop displaying wait cursor now
            /// (unless it has its own long wait to indicate.)
            /// </summary>
            READER_RECOVERED,
            /// <summary>
            /// ReadMgr is done with reader recovery routine, but it failed.
            /// App can stop displaying wait cursor now, but should display an error message.
            /// </summary>
            READER_RECOVERY_FAILED,
        }
        static void OnReaderEvent(EventCode code)
        {
            if (null != ReaderEvent) { ReaderEvent(code); }
        }

        public static Reader GetReader()
        {
            CheckConnection();
            return _rdr;
        }

        public static Session GetSession()
        {
            return GetSession(false);
        }
        public static Session GetSession(bool forceDisconnect)
        {
            CheckConnection();
            return new Session(forceDisconnect);
        }

        /// <summary>
        /// Check status of connection and restore, if down
        /// </summary>
        public static void CheckConnection()
        {
            if (!subscriptionsDone)
            {
                //Utilities.PowerManager.PowerNotify += new PowerManager.PowerEventHandler(PowerManager_PowerNotify);
                //DebugLog.Log += new LogProvider.LogHandler(DebugLog_Log_ToDisk);
                subscriptionsDone = true;
            }
            lock (readerLock)
            {
                if (null == _rdr)
                {
                    //Modify for could not turn on issue
                    //CoreDLL.PowerPolicyNotify(PPNMessage.PPN_UNATTENDEDMODE, 1);
                    DebugLog_Log_ToDisk("Set PPN_UNATTENDEDMODE to 1");

                    _rdr = Utility.ConnectReader();
                    //EnableUnattendedReaderMode(Utilities.ReaderPortName);

                    //_rdr.ParamSet("/reader/powerMode", Reader.PowerMode.MEDSAVE);
                    _rdr.ParamSet("/reader/powerMode", Reader.PowerMode.FULL);
                }
            }
        }

        /// <summary>
        /// Set up unattended power management mode so reader 
        /// can be shut down cleanly before handheld suspends.
        /// </summary>
        /// <param name="portName"></param>
        private static void EnableUnattendedReaderMode(string portName)
        {
            DebugLog_Log_ToDisk("EnableUnattendedReaderMode");
            lock (rdrPowerReqHandles)
            {
                if (0 == rdrPowerReqHandles.Count)
                {
                    // See Unattended Mode example at http://stackoverflow.com/questions/336771/how-can-i-run-code-on-windows-mobile-while-being-suspended
                    //Modify for could not turn on issue
                    CoreDLL.PowerPolicyNotify(PPNMessage.PPN_UNATTENDEDMODE, 1);
                    //DebugLog_Log_ToDisk("Set PPN_UNATTENDEDMODE to 1");

                    // Request that devices remain powered in unattended mode
                    // NOTE: If you don't do this, the system suspends anyway 1 minute after you press the power button
                    // (USB reader power light goes out, system publishes a Suspend notification.)
                    // With this request, it seems to run indefinitely.
                    foreach (string devName in new string[]{
                        portName+":",  // RFID Reader
                        "wav1:",  // Speaker
                        "com3:",  // GPS Intermediate Driver
                    })
                    {
                        IntPtr handle = CoreDLL.SetPowerRequirement(devName, CEDeviceDriverPowerStates.D0,
                            DevicePowerFlags.POWER_NAME | DevicePowerFlags.POWER_FORCE,
                            nullIntPtr, 0);
                        rdrPowerReqHandles.Add(handle);
                        DebugLog_Log_ToDisk("Set PowerRequirement " + handle.ToString() + " for " + devName);
                    }
                }
            }
        }
        /// <summary>
        /// Withdraw unattended power management requirements
        /// to allow handheld to fully suspend.
        /// </summary>
        private static void DisableUnattendedReaderMode()
        {
            lock (rdrPowerReqHandles)
            {
                if (0 < rdrPowerReqHandles.Count)
                {
                    foreach (IntPtr handle in rdrPowerReqHandles)
                    {
                        CoreDLL.ReleasePowerRequirement(handle);
                        DebugLog_Log_ToDisk("Released PowerRequirement " + handle.ToString());
                    }
                    rdrPowerReqHandles.Clear();
                    //Modify for could not turn on issue
                    DebugLog_Log_ToDisk("Set PPN_UNATTENDEDMODE to 0");
                    CoreDLL.PowerPolicyNotify(PPNMessage.PPN_UNATTENDEDMODE, 0);
                    //DebugLog_Log_ToDisk("Set PPN_UNATTENDEDMODE to 0");
                }
            }
        }

        static void DebugLog_Log_ToDisk(string message)
        {
            // Utilities.DiskLog.Log("DebugLog: " + message);
        }

        static void ReadMgrDiskLog(string message)
        {
            //     Utilities.DiskLog.Log("ReadMgr: " + message);
        }

        static bool resumeTripped = false;  // Is there an unhandled Resume event?
        static int transitionsSinceResume = 0;  // FTDI driver isn't up until 2 Transition 12010000s after a Resume
        // MCS++ - unused variable
        // The following variable is never used.
        // static bool screenOffTripped = false;  // Is there an unhandled Screen Off event?
        // MCS--
        static int transitionsSinceScreenOff = 0;

        /// <summary>
        /// Automatically manage reader connect/disconnect via using-dispose syntax
        /// </summary>
        public class Session : IDisposable
        {
            bool _disposed;
            bool _forceDisconnect;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="forceDisconnect">Force disconnection when session is over?</param>
            public Session(bool forceDisconnect)
            {
                _disposed = false;
                _forceDisconnect = forceDisconnect;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            public void Dispose(bool disposing)
            {
                // If you need thread safety, use a lock around these 
                // operations, as well as in your methods that use the resource.
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (_forceDisconnect)
                        {
                            //ForceDisconnect();
                        }
                        else
                        {
                            // AllowDisconnect();
                        }
                    }

                    _disposed = true;
                }
            }

            public Reader Reader
            {
                get
                {
                    CheckConnection();
                    return _rdr;
                }
            }
        }
    }

    public class RFIDScanner
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RFIDScanner));
        List<int> ant = new List<int>();
        private Dictionary<string, string> properties;
        private delegate void InsertItemDelegate(object sender, TagReadDataEventArgs tagReads);
        private delegate void ShowStatusDelegate(object sender, ReaderExceptionEventArgs e);

        //private USIClass myUSI;
        private Reader _ObjReader = null;
        static bool bEnable = true;

        // MCS++ For detect voltage
        CoreDLL.SYSTEM_POWER_STATUS_EX2 status;

        private TextBox taginputbox;

        public TextBox TagInputBox { get { return taginputbox; } set { taginputbox = value; } }

        private Label scannerstatuslabel;
        public Label ScannerStatusLabel { get { return scannerstatuslabel; } set { scannerstatuslabel = value; } }

        private Form _form;

        //Sounds
        SoundPlayer playBeep = new SoundPlayer();
        SoundPlayer playStartAudio = new SoundPlayer();
        SoundPlayer playStopAudio = new SoundPlayer();


        public RFIDScanner(Form form)
        {
            _form = form;
            properties = Utility.GetProperties();

        }





        public void EnableReader()
        {
            try
            {
                // Make sure reader is connected
                _ObjReader = ReadMgr.GetReader();

                if (_ObjReader == null)
                {
                    if (scannerstatuslabel != null)
                    {
                        scannerstatuslabel.Text = "RFID Reader Failed!";
                        scannerstatuslabel.Visible = true;
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
                            MessageBox.Show("Battery level is too low to read tags.");
                            return;
                        case 2:     //Battery Voltage low
                            MessageBox.Show("Battery voltage is too low to read tags.");
                            return;
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
                        MessageBox.Show("properties are null");
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
                    logger.Error(ex.ToString());
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
                logger.Error(ex.ToString());
                if (-1 != ex.Message.IndexOf("RFID reader was not found"))
                {
                    MessageBox.Show(ex.Message, "Error");
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
                    logger.Error(ex.Message, ex);
                }

            }

        }

        void _ObjReader_TagRead(object sender, TagReadDataEventArgs e)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new InsertItemDelegate(_ObjReader_TagRead), sender, e);
            }
            else
            {
                if (taginputbox != null)
                {
                    taginputbox.Text = e.TagReadData.EpcString;
                    taginputbox.Visible = true;
                }
            }
        }

        void _ObjReader_ReadException(object sender, ReaderExceptionEventArgs e)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new ShowStatusDelegate(_ObjReader_ReadException), sender, e);
            }
            else
            {
                if (scannerstatuslabel != null)
                {
                    scannerstatuslabel.Text = e.ReaderException.Message;
                    scannerstatuslabel.Visible = true;
                }

            }
        }

        // MCS++ Check battery status
        private int BatteryStatusCheck()
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
                logger.Error("In playStartSound: " + ex.ToString());
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
                logger.Error("In playStopSound: " + ex.ToString());
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
                logger.Error("In playBeepSound: " + ex.ToString());
            }
        }
    }
}
