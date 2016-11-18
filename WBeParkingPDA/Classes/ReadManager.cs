using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ThingMagic;
using System.IO;

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
                Utility.PowerManager.PowerNotify += new PowerManager.PowerEventHandler(PowerManager_PowerNotify);
                DebugLog.Log += new LogProvider.LogHandler(DebugLog_Log_ToDisk);
                subscriptionsDone = true;
            }
            lock (readerLock)
            {
                try
                {
                    if (null == _rdr)
                    {
                        //Modify for could not turn on issue
                        CoreDLL.PowerPolicyNotify(PPNMessage.PPN_UNATTENDEDMODE, 1);
                        DebugLog_Log_ToDisk("Set PPN_UNATTENDEDMODE to 1");

                        _rdr = RFIDScanner.ConnectReader();
                        EnableUnattendedReaderMode(Utility.ReaderPortName);

                        //_rdr.ParamSet("/reader/powerMode", Reader.PowerMode.MEDSAVE);
                        _rdr.ParamSet("/reader/powerMode", Reader.PowerMode.FULL);
                    }
                }
                catch (Exception)
                {
                    throw;
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
                    //CoreDLL.PowerPolicyNotify(PPNMessage.PPN_UNATTENDEDMODE, 1);
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
            logger.DebugFormat("DebugLog: {0}", message);            
        }

        static void ReadMgrDiskLog(string message)
        {
            logger.DebugFormat("ReadMgr: {0}", message);
        }

        static bool resumeTripped = false;  // Is there an unhandled Resume event?
        static int transitionsSinceResume = 0;  // FTDI driver isn't up until 2 Transition 12010000s after a Resume
        // MCS++ - unused variable
        // The following variable is never used.
        // static bool screenOffTripped = false;  // Is there an unhandled Screen Off event?
        // MCS--
        static int transitionsSinceScreenOff = 0;
        static void PowerManager_PowerNotify(object sender, PowerManager.PowerEventArgs e)
        {
            try
            {
                // Get the information
                PowerManager.PowerInfo powerInfo = e.PowerInfo;
                string message = powerInfo.Message + " " + powerInfo.Flags.ToString("X");
                PowerLog.OnLog(message);
                ReadMgrDiskLog(message);

                // Power Management Strategy
                //  * Whenever display is shut down (Transition 00400000), shut down reader, too (ForceDisconnect)
                //    * NOTE: Doesn't apply to user turning off backlight (hold power button for 1s) -- that doesn't generate a Transisition 04000000
                //  * After resume, wait until reader is ready, then reconnect
                //    * If Nomad didn't make it to Suspend (Transition 00200000), wait until first Transition 12010000
                //    * If Nomad went through Suspend (Transition 00200000), wait for second Transition 12010000
                switch (powerInfo.Message)
                {
                    case PowerManager.MessageTypes.Resume:
                        resumeTripped = true;
                        transitionsSinceResume = 0;
                        break;
                    case PowerManager.MessageTypes.Transition:
                        switch ((uint)powerInfo.Flags)
                        {
                            case 0x10010000:
                            case 0x00400000:
                                // Backlight Off
                                // MCS++ - unused variable
                                // // The following variable is never used.
                                // screenOffTripped = true;
                                // MCS--
                                transitionsSinceScreenOff = 0;
                                OnReaderEvent(EventCode.POWER_INTERRUPTED);
                                ReadMgrDiskLog("POWER_INTERRUPTED");
                                ForceDisconnect();
                                //Cursor.Current = Cursors.WaitCursor;
                                //StatusLog.OnLog("Power Interrupted");
                                break;
                            case 0x00200000:
                                // Suspend
                                OnReaderEvent(EventCode.POWER_SUSPEND);
                                ReadMgrDiskLog("POWER_SUSPEND");
                                break;
                            case 0x12010000:
                                // Subsystem On
                                transitionsSinceResume += 1;
                                transitionsSinceScreenOff += 1;

                                if (!resumeTripped && (1 == transitionsSinceScreenOff))
                                {
                                    OnReaderEvent(EventCode.POWER_RESTORED);
                                    ReadMgrDiskLog("POWER_RESTORED");
                                    ExecuteReconnect();
                                }
                                if (resumeTripped && (2 == transitionsSinceResume))
                                {
                                    // Reconnect on 2nd powerup transition after Resume
                                    //Cursor.Current = Cursors.WaitCursor;
                                    //StatusLog.OnLog("Reconnecting...");
                                    ExecuteReconnect();
                                }
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("In PowerManager_PowerNotify: " + ex.ToString());
                ReadMgrDiskLog("In PowerManager_PowerNotify: " + ex.ToString());
                //Application.Exit();

                //Modify for could not turn on issue
                ForceDisconnect();
                throw ex;
            }
        }

        private static void ExecuteReconnect()
        {
            try
            {
                OnReaderEvent(EventCode.READER_RECOVERING);
                ReadMgrDiskLog("READER_RECOVERING");
                DebugLog.OnLog("Recovering Reader...");
                //DebugLog.OnLog("Waiting...");
                //Thread.Sleep(3000);
                //MessageBox.Show("Waiting for serial driver recovery.  Press ok to continue.");
                //DebugLog.OnLog("Calling RecoverReader()...");
                ForceReconnect();
                DebugLog.OnLog("Reader Recovered");
                OnReaderEvent(EventCode.READER_RECOVERED);
                ReadMgrDiskLog("READER_RECOVERED");
                //Cursor.Current = Cursors.Default;
                resumeTripped = false;
            }
            catch (Exception ex)
            {
                logger.Error("In ExecuteReconnect(): " + ex.ToString());
                if (-1 != ex.Message.IndexOf("RFID reader was not found"))
                {
                    lastErrorMessage = "Connection failed";
                }
                else
                {
                    lastErrorMessage = ex.Message;
                }
                //StatusLog.OnLog(lastErrorMessage);
                OnReaderEvent(EventCode.READER_RECOVERY_FAILED);
                ReadMgrDiskLog("READER_RECOVERY_FAILED");
                throw ex;
            }
        }

        /// <summary>
        /// Allow, but do not require, disconnection from the reader.
        /// 
        /// For example, if our app is in a maximum power-saving mode, it may choose 
        /// to disconnect as often as possible in order to power down the reader module.
        /// But it is also free to leave the connection open to minimize latency.
        /// </summary>
        public static void AllowDisconnect()
        {
        }
        /// <summary>
        /// Require disconnection from the reader.
        /// 
        /// For example, if we are exiting the application and want the reader to go into maximum shutdown mode.
        /// </summary>
        public static void ForceDisconnect()
        {
            lock (readerLock)
            {
                if (null != _rdr)
                {
                    try
                    {
                        ReadMgrDiskLog("Trying Reader.Destroy");
                        _rdr.Destroy();
                        ReadMgrDiskLog("Destroyed Reader");
                    }
                    catch (IOException ex)
                    {
                        logger.Error("Ignoring IOException in ForceDisconnect(): " + ex.ToString());
                        // IOExceptions are tolerable on Destroy.
                        // They are normal if the serial chipset
                        // has been shut down without the driver being closed
                        // (e.g., host goes to sleep while a connection is open)
                        ReadMgrDiskLog("Caught IOException in ForceDisconnect: " + ex.ToString());
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Caught Exception in ForceDisconnect: " + ex.ToString());
                        ReadMgrDiskLog("Caught Exception in ForceDisconnect: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        _rdr = null;
                        ReadMgrDiskLog("Waiting for SerialPort threads to spin down...");
                        //Modify for could not turn on issue
                        OnReaderEvent(EventCode.READER_SHUTDOWN);
                        //Thread.Sleep(2000);
                        DisableUnattendedReaderMode();
                        //OnReaderEvent(EventCode.READER_SHUTDOWN);
                    }
                }
            }
        }
        public static void ForceReconnect()
        {
            DebugLog.OnLog("Disconnect Reader");
            ForceDisconnect();
            DebugLog.OnLog("Wait for Serial Reset");
            // Wait for serial driver to recover
            //Thread.Sleep(1000);
            DebugLog.OnLog("Reconnect Reader");
            CheckConnection();
            DebugLog.OnLog("Reader Reconnected");
        }

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
                            ForceDisconnect();
                        }
                        else
                        {
                            AllowDisconnect();
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
}
