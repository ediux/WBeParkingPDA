using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using ThingMagic;
using System.Threading;
using WBeParkingPDA.Classes;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;

namespace WBeParkingPDA
{
    

    internal static class Utility
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Utility));
        static log4net.ILog transportLogger = log4net.LogManager.GetLogger("Transport");
        public static LogProvider Logger = new LogProvider();
        
        private static PowerManager _powerMgr = null;
       
        static string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"/Config.txt";

        static string dbPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + @"/wbeparking.db";

        public static PowerManager PowerManager
        {
            get
            {
                if (null == _powerMgr)
                {
                    // Create a new instance of the PowerManager class
                    _powerMgr = new PowerManager();

                    // Enable power notifications. This will cause a thread to start
                    // that will fire the PowerNotify event when any power notification 
                    // is received.
                    _powerMgr.EnableNotifications();
                }
                return _powerMgr;
            }
        }

        private static string _readerPortName = "";
        public static string ReaderPortName
        {
            get { return _readerPortName; }
        }

        public static Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> Properties = new Dictionary<string, string>();
            try
            {
                FileStream fs = File.OpenRead(appPath);

                using (StreamReader sr = File.OpenText(appPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((!string.IsNullOrEmpty(line)) &&
                            (!line.StartsWith(";")) &&
                            (!line.StartsWith("#")) &&
                            (!line.StartsWith("'")) &&
                            (!line.StartsWith("/")) &&
                            (!line.StartsWith("*")) &&
                            (line.Contains('=')))
                        {
                            int index = line.IndexOf('=');
                            string key = line.Substring(0, index).Trim();
                            string value = line.Substring(index + 1).Trim();

                            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                                (value.StartsWith("'") && value.EndsWith("'")))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }
                            Properties.Add(key, value);
                        }
                    }
                }
                fs.Close();

                if (Properties.Count == 0)
                {
                    Properties = BuildDefaultProperties();
                    SaveConfigurations(Properties);
                }
            }
            catch (FileNotFoundException)
            {
                logger.Info(appPath + " not found.  Using default properties.");
                Properties = BuildDefaultProperties();
                SaveConfigurations(Properties);
            }

            return Properties;
        }

        /// <summary>
        /// Set the properties on the reader
        /// </summary>
        /// <param name="objReader">Reader</param>
        /// <param name="Properties">configurations</param>
        public static void SetReaderSettings(Reader objReader, Dictionary<string, string> Properties)
        {
            using (ReadMgr.Session rsess = ReadMgr.GetSession())
            {
                //Set tagencoding value
                switch (Properties["tagencoding"].ToLower())
                {
                    case "fm0":
                        rsess.Reader.ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.FM0);
                        break;
                    case "m2":
                        rsess.Reader.ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M2);
                        break;
                    case "m4":
                        rsess.Reader.ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M4);
                        break;
                    case "m8":
                        rsess.Reader.ParamSet("/reader/gen2/tagEncoding", Gen2.TagEncoding.M8);
                        break;
                }
                //Set target value
                switch (Properties["gen2target"].ToLower())
                {
                    case "a":
                        rsess.Reader.ParamSet("/reader/gen2/target", Gen2.Target.A);
                        break;
                    case "b":
                        rsess.Reader.ParamSet("/reader/gen2/target", Gen2.Target.B);
                        break;
                    case "ab":
                        rsess.Reader.ParamSet("/reader/gen2/target", Gen2.Target.AB);
                        break;
                    case "ba":
                        rsess.Reader.ParamSet("/reader/gen2/target", Gen2.Target.BA);
                        break;
                }
                //Set gen2 session value
                switch (Properties["gen2session"].ToLower())
                {
                    case "s0":
                        rsess.Reader.ParamSet("/reader/gen2/session", Gen2.Session.S0);
                        break;
                    case "s1":
                        rsess.Reader.ParamSet("/reader/gen2/session", Gen2.Session.S1);
                        break;
                    case "s2":
                        rsess.Reader.ParamSet("/reader/gen2/session", Gen2.Session.S2);
                        break;
                    case "s3":
                        rsess.Reader.ParamSet("/reader/gen2/session", Gen2.Session.S3);
                        break;
                }
                //Set Q value
                switch (Properties["gen2q"].ToLower())
                {
                    case "dynamicq":
                        rsess.Reader.ParamSet("/reader/gen2/q", new Gen2.DynamicQ()); break;
                    case "staticq":
                        rsess.Reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(Convert.ToByte(Properties["staticqvalue"].ToString()))); break;
                    default:
                        break;
                }
                rsess.Reader.ParamSet("/reader/read/asyncOffTime", Convert.ToInt32(Properties["rfofftime"]));
                rsess.Reader.ParamSet("/reader/read/asyncOnTime", Convert.ToInt32(Properties["rfontime"]));
            }
        }

        internal static Dictionary<string, string> BuildDefaultProperties()
        {

            //PA692的預設值
            Dictionary<string, string> Properties = new Dictionary<string, string>();
            Properties.Add("iswedgeappdisabled", "yes");
            Properties.Add("comport", "COM8");
            Properties.Add("region", "na");
            Properties.Add("displayformat", "hex");
            Properties.Add("scanduration", "3");
            Properties.Add("audiblealert", "yes");
            Properties.Add("metadataseparator", "space");
            Properties.Add("multipletagseparator", "Enter");
            Properties.Add("metadatatodisplay", "timestamp");
            Properties.Add("decodewavefile", @"\Windows\Loudest.wav");
            Properties.Add("startscanwavefile", @"\Windows\Quietest.wav");
            Properties.Add("endscanwavefile", @"\Windows\Splat.wav");
            Properties.Add("prefix", "");
            Properties.Add("suffix", "");
            Properties.Add("readpower", "");
            Properties.Add("tagpopulation", "small");
            Properties.Add("tagselection", "None");
            Properties.Add("selectionaddress", "0");
            Properties.Add("selectionmask", "");
            Properties.Add("ismaskselected", "");
            Properties.Add("tagencoding", "M4");
            Properties.Add("gen2session", "");
            Properties.Add("gen2target", "A");
            Properties.Add("gen2q", "dynamicq");
            Properties.Add("staticqvalue", "");
            Properties.Add("recordhighestrssi", "yes");
            Properties.Add("recordrssioflasttagread", "yes");
            Properties.Add("confighureapp", "demo");
            Properties.Add("powermode", "maxsave");
            Properties.Add("isreading", "no");
            Properties.Add("rfontime", "250");
            Properties.Add("rfofftime", "50");
            return Properties;
        }

        public static void SaveConfigurations(Dictionary<string, string> Properties)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(appPath))
                {
                    foreach (KeyValuePair<string, string> item in Properties)
                    {
                        sw.WriteLine(item.Key + "=" + item.Value);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("In SaveConfigurations: " + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 切換時區對應地區:PA692 預設對應區域為北美
        /// </summary>
        /// <param name="regionName">時區名稱</param>
        public static void SwitchRegion(string regionName)
        {
            switch (regionName.ToLower())
            {
                case "na": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.NA); break;
                case "in": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.IN); break;
                case "au": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.AU); break;
                case "eu": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.EU); break;
                case "eu2": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.EU2); break;
                case "eu3": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.EU3); break;
                case "jp": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.JP); break;
                case "kr": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.KR); break;
                case "kr2": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.KR2); break;
                case "prc": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.PRC); break;
                case "prc2": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.PRC2); break;
                case "nz":
                    // MCS++
#if MCS_NOMAD
                    // MCS++
                    // Nomad
                    // ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.NZ);
                    // NZ Region not supported on M5ec module, so hack it here.
                    ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.OPEN);
                    ReadMgr.GetReader().ParamSet("/reader/region/hopTable", new int[] {
                    864400,
                    865150, 
                    864900,
                    865900,
                    865400,
                    864650, 
                    865650,
                });
                    // MCS--
#else
                    // MCS++
                    // T41
                    ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.NZ);
                    // MCS--
#endif
                    // MCS--
                    break;
                case "open": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.OPEN); break;
                case "unspec": ReadMgr.GetReader().ParamSet("/reader/region/id", Reader.Region.UNSPEC); break;
                default:
                    throw new ArgumentException("Unknown Region: " + regionName);
            }
        }

        public static void Initializedatabase()
        {
            SQLiteHelper sqlce = null;
            try
            {

                sqlce = new SQLiteHelper(dbPath);

                DataTable dt = sqlce.select(@"SELECT name FROM sqlite_master WHERE type='table';");

                if (dt != null && dt.Rows.Count == 0)
                {
                    //建立新的資料表結構
                    sqlce.select(@"CREATE TABLE CarPurposeTypes (
    Id   INTEGER       PRIMARY KEY ASC
                       NOT NULL,
    Name VARCHAR (100) NOT NULL
                       DEFAULT 未命名,
    Void BOOLEAN       NOT NULL
                       DEFAULT (0) 
);");

                    sqlce.select(@"CREATE TABLE ETCBinding (
    ETCID            VARCHAR (60) PRIMARY KEY ASC
                                  UNIQUE
                                  NOT NULL,
    CarID            VARCHAR (20) NOT NULL,
    CarPurposeTypeID INT          REFERENCES CarPurposeTypes (Id) ON DELETE SET NULL
);
");

                }


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

        }

        public static void GetCarPurposeTypesList(ComboBox ddl)
        {
            SQLiteHelper sqlce = null;
            try
            {
                sqlce = new SQLiteHelper(dbPath);
                DataTable dt = sqlce.select(@"Select Id,Name,Void From CarPurposeTypes Where Void=0");
                
                if (dt != null && dt.Rows.Count > 0)
                {

                    ddl.Items.Clear();
                    ddl.DisplayMember = "Name";
                    ddl.ValueMember = "Id";

                    foreach (DataRow row in dt.Rows)
                    {
                        var objRow = new CarPurposeTypes();
                        objRow.Id = int.Parse(string.Format("{0}", row["Id"]));
                        objRow.Name = (string)row["Name"];
                        objRow.Void = (bool)row["Void"];

                        ddl.Items.Add(objRow);
                    }

                    ddl.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }

        }

        public static void SaveETCTagBinding(string EPCID, string CarId, int PropseId)
        {
            SQLiteHelper sqlce = null;
            try
            {
                sqlce = new SQLiteHelper(dbPath);
                sqlce.execute(@"Insert Into ETCBinding  (ETCID,CarID,CarPurposeTypeID)
Values(@p0,@p1,@p2);", EPCID, CarId, PropseId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

    }

 
   
    #region enums

    /// <summary>
    /// Device Power states
    /// </summary>
    public enum CEDeviceDriverPowerStates : int
    {
        PwrDeviceUnspecified = -1,
        //Full On: full power,  full functionality
        D0 = 0,
        /// <summary>
        /// Low Power On: fully functional at low power/performance
        /// </summary>
        D1 = 1,
        /// <summary>
        /// Standby: partially powered with automatic wake
        /// </summary>
        D2 = 2,
        /// <summary>
        /// Sleep: partially powered with device initiated wake
        /// </summary>
        D3 = 3,
        /// <summary>
        /// Off: unpowered
        /// </summary>
        D4 = 4,
        PwrDeviceMaximum
    }

    [Flags()]
    public enum DevicePowerFlags
    {
        None = 0,
        /// <summary>
        /// Specifies the name of the device whose power should be maintained at or above the DeviceState level.
        /// </summary>
        POWER_NAME = 0x00000001,
        /// <summary>
        /// Indicates that the requirement should be enforced even during a system suspend.
        /// </summary>
        POWER_FORCE = 0x00001000,
        POWER_DUMPDW = 0x00002000
    }

    public enum PowerState
    {
        POWER_STATE_ON = (0x00010000),          // on state
        POWER_STATE_OFF = (0x00020000),         // no power, full off
        POWER_STATE_CRITICAL = (0x00040000),    // critical off
        POWER_STATE_BOOT = (0x00080000),        // boot state
        POWER_STATE_IDLE = (0x00100000),        // idle state
        POWER_STATE_SUSPEND = (0x00200000),     // suspend state
        POWER_STATE_UNATTENDED = (0x00400000),  // Unattended state.
        POWER_STATE_RESET = (0x00800000),       // reset state
        POWER_STATE_USERIDLE = (0x01000000),    // user idle state
        POWER_STATE_PASSWORD = (0x10000000)     // This state is password protected.
    };

    public enum PPNMessage
    {
        PPN_REEVALUATESTATE = 1,
        PPN_POWERCHANGE = 2,
        PPN_UNATTENDEDMODE = 3,
        PPN_SUSPENDKEYPRESSED = 4,
        PPN_POWERBUTTONPRESSED = 4,
        PPN_SUSPENDKEYRELEASED = 5,
        PPN_APPBUTTONPRESSED = 6,

    }

    #endregion enums

    /// <summary>
    /// PowerManager provides several methods to monitor and control 
    /// system and device(example: backlight, audio etc) power states. PowerManager  
    /// does this by wrapping Microsoft's Power Management API. For more information on 
    /// Power Management please refer to MSDN.
    /// </summary>
    /// <remarks>
    /// PowerMangement should be used with extreme caution as it may result in 
    /// unexpected behavior. 
    /// </remarks>
    public class PowerManager : IDisposable
    {
        #region ------------------Enumerations--------------------

        /// <summary>
        /// Defines the System power states
        /// </summary>
        public enum SystemPowerStates : uint
        {
            /// <summary>
            /// On state.
            /// </summary>
            On = 0x00010000,

            /// <summary>
            /// No power, full off.
            /// </summary>
            Off = 0x00020000,

            /// <summary>
            /// Critical off.
            /// </summary>
            Critical = 0x00040000,

            /// <summary>
            /// Boot state.
            /// </summary>
            Boot = 0x00080000,

            /// <summary>
            /// Idle state.
            /// </summary>
            Idle = 0x00100000,

            /// <summary>
            /// Suspend state.
            /// </summary>
            Suspend = 0x00200000,

            /// <summary>
            /// Reset state.
            /// </summary>
            Reset = 0x00800000
        }

        /// <summary>
        /// Defines the System power requirement flags
        /// </summary>
        public enum PowerReqFlags : uint
        {
            POWER_NAME = 0x00000001,
            POWER_FORCE = 0x00001000,
        }

        /// <summary>
        /// Defines the Device power states
        /// </summary>
        public enum DevicePowerStates
        {
            PwrDeviceUnspecified = -1,
            FullOn = 0,		// Full On: full power,  full functionality
            D0 = FullOn,
            LowOn,			// Low Power On: fully functional at low power/performance
            D1 = LowOn,
            StandBy,		// Standby: partially powered with automatic wake
            D2 = StandBy,
            Sleep,			// Sleep: partially powered with device initiated wake
            D3 = Sleep,
            Off,			// Off: unpowered
            D4 = Off,
            PwrDeviceMaximum
        }

        /// <summary>
        /// Defines the Power Status message type.
        /// </summary>
        [FlagsAttribute()]
        public enum MessageTypes : uint
        {
            /// <summary>
            /// System power state transition.
            /// </summary>
            Transition = 0x00000001,

            /// <summary>
            /// Resume from previous state.
            /// </summary>
            Resume = 0x00000002,

            /// <summary>
            /// Power supply switched to/from AC/DC.
            /// </summary>
            Change = 0x00000004,

            /// <summary>
            /// A member of the POWER_BROADCAST_POWER_INFO structure has changed.
            /// </summary>
            Status = 0x00000008
        }

        /// <summary>
        /// Defines the AC power status flags.
        /// </summary>
        public enum ACLineStatus : byte
        {
            /// <summary>
            /// AC power is offline.
            /// </summary>
            Offline = 0x00,

            /// <summary>
            /// AC power is online. 
            /// </summary>
            OnLine = 0x01,

            /// <summary>
            /// AC line status is unknown.
            /// </summary>
            Unknown = 0xff
        }

        /// <summary>
        /// Defines the Battery charge status flags.
        /// </summary>
        [FlagsAttribute()]
        public enum BatteryFlags : byte
        {
            /// <summary>
            /// High
            /// </summary>
            High = 0x01,

            /// <summary>
            /// Low
            /// </summary>
            Low = 0x02,

            /// <summary>
            /// Critical
            /// </summary>
            Critical = 0x04,

            /// <summary>
            /// Charging
            /// </summary>
            Charging = 0x08,

            /// <summary>
            /// Reserved1
            /// </summary>
            Reserved1 = 0x10,

            /// <summary>
            /// Reserved2
            /// </summary>
            Reserved2 = 0x20,

            /// <summary>
            /// Reserved3
            /// </summary>
            Reserved3 = 0x40,

            /// <summary>
            /// No system battery
            /// </summary>
            NoBattery = 0x80,

            /// <summary>
            /// Unknown status
            /// </summary>
            Unknown = High | Low | Critical | Charging | Reserved1 | Reserved2 | Reserved3 | NoBattery
        }

        /// <summary>
        /// Responses from <see cref="WaitForMultipleObjects"/> function.
        /// </summary>
        private enum Wait : uint
        {
            /// <summary>
            /// The state of the specified object is signaled.
            /// </summary>
            Object = 0x00000000,
            /// <summary>
            /// Wait abandoned.
            /// </summary>
            Abandoned = 0x00000080,
            /// <summary>
            /// Wait failed.
            /// </summary>
            Failed = 0xffffffff,
        }


        #endregion -----------------Enumerations-------------------

        #region --------------------Members-----------------------

        /// <summary>
        /// Indicates that an application would like to receive all types of 
        /// power notifications.
        /// </summary>
        private const uint POWER_NOTIFY_ALL = 0xFFFFFFFF;

        /// <summary>
        /// Indicates an infinite wait period
        /// </summary>
        private const int INFINITE = -1;

        /// <summary>
        /// Allocate message buffers on demand and free the message buffers after they are read.
        /// </summary>
        private const int MSGQUEUE_NOPRECOMMIT = 1;

        /// <summary>
        /// Event to wake up the worker thread so that it can close
        /// </summary>
        private AutoResetEvent powerThreadAbort;

        /// <summary>
        /// Flag requesting worker thread closure
        /// </summary>
        private bool abortPowerThread = false;

        /// <summary>
        /// Flag to indicate that the worker thread is running
        /// </summary>
        private bool powerThreadRunning = false;

        ///// <summary>
        ///// Thread interface queue
        ///// </summary>
        //private Queue powerQueue;

        /// <summary>
        /// Handle to the message queue
        /// </summary>
        private IntPtr hMsgQ = IntPtr.Zero;

        /// <summary>
        /// Handle returned from RequestPowerNotifications
        /// </summary>
        private IntPtr hReq = IntPtr.Zero;

        /// <summary>
        /// Boolean used to indicate if the object has been disposed
        /// </summary>
        private bool bDisposed = false;

        /// <summary>
        /// Occurs when there is some PowerNotify information available.
        /// </summary>
        public event PowerEventHandler PowerNotify;

        /// <summary>
        /// EventArgs encapsulating PowerInfo from a power event notification
        /// </summary>
        public class PowerEventArgs : EventArgs
        {
            public PowerInfo PowerInfo;
            public PowerEventArgs(PowerInfo powerInfo)
            {
                this.PowerInfo = powerInfo;
            }
        }
        /// <summary>
        /// Handler delegate format for PowerInfoNotify events
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="eva">Arguments of event</param>
        public delegate void PowerEventHandler(object sender, PowerEventArgs eva);

        #endregion --------------------Members--------------------

        #region -------------------Structures---------------------

        /// <summary>
        /// Contains information about a message queue.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MessageQueueOptions
        {
            /// <summary>
            /// Size of the structure in bytes.
            /// </summary>
            public uint Size;

            /// <summary>
            /// Describes the behavior of the message queue. Set to MSGQUEUE_NOPRECOMMIT to 
            /// allocate message buffers on demand and to free the message buffers after 
            /// they are read, or set to MSGQUEUE_ALLOW_BROKEN to enable a read or write 
            /// operation to complete even if there is no corresponding writer or reader present.
            /// </summary>
            public uint Flags;

            /// <summary>
            /// Number of messages in the queue.
            /// </summary>
            public uint MaxMessages;

            /// <summary>
            /// Number of bytes for each message, do not set to zero.
            /// </summary>
            public uint MaxMessage;

            /// <summary>
            /// Set to TRUE to request read access to the queue. Set to FALSE to request write 
            /// access to the queue.
            /// </summary>
            public uint ReadAccess;
        };

        /// <summary>
        /// Contains information about the power status of the system  
        /// as received from the Power Status message queue.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PowerInfo
        {
            /// <summary>
            /// Defines the event type.
            /// </summary>
            /// <see cref="MessageTypes"/>
            public MessageTypes Message;

            /// <summary>
            /// One of the system power flags.
            /// </summary>
            /// <see cref="SystemPowerStates"/>
            public SystemPowerStates Flags;

            /// <summary>
            /// The byte count of SystemPowerState that follows. 
            /// </summary>
            public uint Length;

            /// <summary>
            /// Levels available in battery flag fields
            /// </summary>
            public uint NumLevels;

            /// <summary>
            /// Number of seconds of battery life remaining, 
            /// or 0xFFFFFFFF if remaining seconds are unknown.
            /// </summary>
            public uint BatteryLifeTime;

            /// <summary>
            /// Number of seconds of battery life when at full charge, 
            /// or 0xFFFFFFFF if full battery lifetime is unknown.
            /// </summary>
            public uint BatteryFullLifeTime;

            /// <summary>
            /// Number of seconds of backup battery life remaining, 
            /// or BATTERY_LIFE_UNKNOWN if remaining seconds are unknown.
            /// </summary>
            public uint BackupBatteryLifeTime;

            /// <summary>
            /// Number of seconds of backup battery life when at full charge, 
            /// or BATTERY_LIFE_UNKNOWN if full battery lifetime is unknown.
            /// </summary>
            public uint BackupBatteryFullLifeTime;

            /// <summary>
            /// AC power status. 
            /// </summary>
            /// <see cref="ACLineStatus"/>
            public ACLineStatus ACLineStatus;

            /// <summary>
            /// Battery charge status. 
            /// </summary>
            /// <see cref="BatteryFlags"/>
            public BatteryFlags BatteryFlag;

            /// <summary>
            /// Percentage of full battery charge remaining. 
            /// This member can be a value in the range 0 (zero) to 100, or 255 
            /// if the status is unknown. All other values are reserved.
            /// </summary>
            public byte BatteryLifePercent;

            /// <summary>
            /// Backup battery charge status. 
            /// </summary>
            public byte BackupBatteryFlag;

            /// <summary>
            /// Percentage of full backup battery charge remaining. 
            /// This value must be in the range of 0 to 100, or BATTERY_PERCENTAGE_UNKNOWN.
            /// </summary>
            public byte BackupBatteryLifePercent;
        };

        #endregion -------------------Structures------------------

        #region ---------------------Methods----------------------

        /// <summary>
        /// Ensures that resources are freed when the garbage collector reclaims the object.
        /// </summary>
        ~PowerManager()
        {
            Dispose();
        }

        /// <summary>
        /// Releases the resources used by the object.
        /// </summary>
        public void Dispose()
        {
            if (!bDisposed)
            {
                // Try disabling notifications and ending the thread
                DisableNotifications();
                bDisposed = true;

                // SupressFinalize to take this object off the finalization queue 
                // and prevent finalization code for this object from executing a second time.
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Sets the system power state to the requested value.
        /// </summary>
        /// <param name="systemState">The system power state to set the device to.</param>
        /// <returns>Win32 error code</returns>
        /// <remarks>Should be used with extreme care since it may result in an unexpected 
        /// application or system behavior.</remarks>
        public int SetSystemPowerState(SystemPowerStates systemState)
        {
            uint nError = 0;

            nError = CESetSystemPowerState(
                IntPtr.Zero,
                (uint)systemState,
                0);

            return (int)nError;
        }

        /// <summary>
        /// Returns the current system power state currently in effect.
        /// </summary>
        /// <param name="systemStateName">Receives the system power state name</param>
        /// <param name="systemState">Receives the system power state</param>
        /// <returns>Win32 error code</returns>
        public int GetSystemPowerState(StringBuilder systemStateName, out SystemPowerStates systemState)
        {
            uint nError = 0;

            nError = CEGetSystemPowerState(systemStateName, (uint)systemStateName.Capacity, out systemState);

            return (int)nError;
        }

        /// <summary>
        /// Requests that the Power Manager change the power state of a device.
        /// </summary>
        /// <param name="deviceName">Specifies the device name, for example, COM1:.</param>
        /// <param name="deviceState">Indicates the device power state</param>
        /// <returns>Win32 error code</returns>
        /// <remarks>Should be used with extreme care since it may result in an unexpected 
        /// application or system behavior.</remarks>
        public int DevicePowerNotify(string deviceName, DevicePowerStates deviceState)
        {
            uint nError = 0;

            nError = CEDevicePowerNotify(deviceName, (uint)deviceState, (uint)PowerReqFlags.POWER_NAME);

            return (int)nError;
        }

        /// <summary>
        /// Activates notification events. An application can now register to PowerNotify and be 
        /// notified when a power notification is received.
        /// </summary>
        public void EnableNotifications()
        {
            // Set the message queue options
            MessageQueueOptions Options = new MessageQueueOptions();

            // Size in bytes ( 5 * 4)
            Options.Size = (uint)Marshal.SizeOf(Options);
            // Allocate message buffers on demand and to free the message buffers after they are read
            Options.Flags = MSGQUEUE_NOPRECOMMIT;
            // Number of messages in the queue.
            Options.MaxMessages = 32;
            // Number of bytes for each message, do not set to zero.
            Options.MaxMessage = 512;
            // Set to true to request read access to the queue.
            Options.ReadAccess = 1;	// True

            // Create the queue and request power notifications on it
            hMsgQ = CECreateMsgQueue("PowerNotifications", ref Options);

            hReq = CERequestPowerNotifications(hMsgQ, POWER_NOTIFY_ALL);

            // If the above succeed
            if (hMsgQ != IntPtr.Zero && hReq != IntPtr.Zero)
            {
                //powerQueue = new Queue();

                // Create an event so that we can kill the thread when we want
                powerThreadAbort = new AutoResetEvent(false);

                // Create the power watcher thread
                Thread pnt = new Thread(new ThreadStart(PowerNotifyThread));
                pnt.IsBackground = true;
                pnt.Start();
            }
        }

        /// <summary>
        /// Disables power notification events.
        /// </summary>
        public void DisableNotifications()
        {
            // If we are already closed just exit
            if (!powerThreadRunning)
                return;

            // Stop receiving power notifications
            if (hReq != IntPtr.Zero)
                CEStopPowerNotifications(hReq);

            // Attempt to end the PowerNotifyThread
            abortPowerThread = true;
            try
            {
                powerThreadAbort.Set();

                // Wait for the thread to stop
                int count = 0;
                while (powerThreadRunning)
                {
                    Thread.Sleep(100);

                    // If it did not stop it time record this and give up
                    if (count++ > 50)
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                // Already-disposed powerThreadAbort implies app terminated without telling us
                // If app already closed, just exit
                return;
            }

        }

        ///// <summary>
        ///// Obtain the next PowerInfo structure
        ///// </summary>
        //public PowerInfo GetNextPowerInfo()
        //{
        //    // Get the next item from the queue in a thread safe manner
        //    lock (powerQueue.SyncRoot)
        //        return (PowerInfo)powerQueue.Dequeue();
        //}

        /// <summary>
        /// Worker thread that creates and reads a message queue for power notifications
        /// </summary>
        private void PowerNotifyThread()
        {
            powerThreadRunning = true;

            // Keep going util we are asked to quit
            while (!abortPowerThread)
            {
                IntPtr[] Handles = new IntPtr[2];

                Handles[0] = hMsgQ;
                Handles[1] = powerThreadAbort.Handle;

                // Wait on two handles because the message queue will never
                // return from a read unless messages are posted.
                Wait res = (Wait)CEWaitForMultipleObjects(
                                         (uint)Handles.Length,
                                         Handles,
                                         false,
                                         INFINITE);

                // Exit the loop if an abort was requested
                if (abortPowerThread)
                    break;

                // Else
                switch (res)
                {
                    // This must be an error - Exit loop and thread
                    case Wait.Abandoned:
                        abortPowerThread = true;
                        break;

                    // Timeout - Continue after a brief sleep
                    case Wait.Failed:
                        Thread.Sleep(500);
                        break;

                    // Read the message from the queue
                    case Wait.Object:
                        {
                            // Create a new structure to read into
                            PowerInfo Power = new PowerInfo();

                            uint PowerSize = (uint)Marshal.SizeOf(Power);
                            uint BytesRead = 0;
                            uint Flags = 0;

                            // Read the message
                            if (CEReadMsgQueue(hMsgQ, ref Power, PowerSize,
                                                ref BytesRead, 0, ref Flags))
                            {
                                // Set value to zero if percentage is not known
                                if ((Power.BatteryLifePercent < 0) || (Power.BatteryLifePercent > 100))
                                    Power.BatteryLifePercent = 0;

                                if ((Power.BackupBatteryLifePercent < 0) || (Power.BackupBatteryLifePercent > 100))
                                    Power.BackupBatteryLifePercent = 0;

                                //// Add the power structure to the queue so that the 
                                //// UI thread can get it
                                //lock (powerQueue.SyncRoot)
                                //    powerQueue.Enqueue(Power);

                                // Fire an event to notify the UI
                                if (PowerNotify != null)
                                    PowerNotify(this, new PowerEventArgs(Power));
                            }

                            break;
                        }
                }
            }

            // Close the message queue
            if (hMsgQ != IntPtr.Zero)
                CECloseMsgQueue(hMsgQ);

            powerThreadRunning = false;
        }


        #endregion -----------------Methods---------------------

        #region ---------Native Power Management Imports----------

        [DllImport("coredll.dll", EntryPoint = "RequestPowerNotifications")]
        private static extern IntPtr CERequestPowerNotifications(IntPtr hMsgQ, uint Flags);

        [DllImport("coredll.dll", EntryPoint = "StopPowerNotifications")]
        private static extern bool CEStopPowerNotifications(IntPtr hReq);

        [DllImport("coredll.dll", EntryPoint = "SetDevicePower")]
        private static extern uint CESetDevicePower(string Device, uint dwDeviceFlags, uint DeviceState);

        [DllImport("coredll.dll", EntryPoint = "GetDevicePower")]
        private static extern uint CEGetDevicePower(string Device, uint dwDeviceFlags, uint DeviceState);

        [DllImport("coredll.dll", EntryPoint = "DevicePowerNotify")]
        private static extern uint CEDevicePowerNotify(string Device, uint DeviceState, uint Flags);

        [DllImport("coredll.dll", EntryPoint = "SetSystemPowerState")]
        private static extern uint CESetSystemPowerState(IntPtr sState, uint StateFlags, uint Options);

        [DllImport("coredll.dll", EntryPoint = "GetSystemPowerState")]
        private static extern uint CEGetSystemPowerState(StringBuilder Buffer, uint Length, out SystemPowerStates Flags);

        [DllImport("coredll.dll", EntryPoint = "CreateMsgQueue")]
        private static extern IntPtr CECreateMsgQueue(string Name, ref MessageQueueOptions Options);

        [DllImport("coredll.dll", EntryPoint = "CloseMsgQueue")]
        private static extern bool CECloseMsgQueue(IntPtr hMsgQ);

        [DllImport("coredll.dll", EntryPoint = "ReadMsgQueue")]
        private static extern bool CEReadMsgQueue(IntPtr hMsgQ, ref PowerInfo Power, uint BuffSize, ref uint BytesRead, uint Timeout, ref uint Flags);

        [DllImport("coredll.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
        private static extern int CEWaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool fWaitAll, int dwMilliseconds);

        #endregion ---------Native Power Management Imports----------
    }
}
