using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WBeParkingPDA
{
    public static class CoreDLL
    {
        [DllImport("CoreDLL")]
        public static extern int ReleasePowerRequirement(IntPtr hPowerReq);


        [DllImport("CoreDLL", SetLastError = true)]
        public static extern IntPtr SetPowerRequirement
        (
            string pDevice,
            CEDeviceDriverPowerStates DeviceState,
            DevicePowerFlags DeviceFlags,
            IntPtr pSystemState,
            uint StateFlagsZero
        );

        [DllImport("CoreDLL", SetLastError = true)]
        public static extern IntPtr SetDevicePower
            (
                string pDevice,
                DevicePowerFlags DeviceFlags,
            CEDeviceDriverPowerStates DevicePowerState
            );
        [DllImport("CoreDLL")]
        public static extern int GetDevicePower(string device, DevicePowerFlags flags, out CEDeviceDriverPowerStates PowerState);

        [DllImport("CoreDLL")]
        public static extern int SetSystemPowerState(String stateName, PowerState powerState, DevicePowerFlags flags);


        [DllImport("CoreDLL")]
        public static extern int PowerPolicyNotify(
          PPNMessage dwMessage,
            int option
            //    DevicePowerFlags);
        );


        public class SYSTEM_POWER_STATUS_EX2
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
            public byte Reserved2;
            public byte BackupBatteryFlag;
            public byte BackupBatteryLifePercent;
            public byte Reserved3;
            public uint BackupBatteryLifeTime;
            public uint BackupBatteryFullLifeTime;
            public uint BatteryVoltage;
            public uint BatteryCurrent;
            public uint BatteryAverageCurrent;
            public uint BatteryAverageInterval;
            public uint BatterymAHourConsumed;
            public uint BatteryTemperature;
            public uint BackupBatteryVoltage;
            public byte BatteryChemistry;
        }
        public class SYSTEM_POWER_STATUS_EX
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
            public byte Reserved2;
            public byte BackupBatteryFlag;
            public byte BackupBatteryLifePercent;
            public byte Reserved3;
            public uint BackupBatteryLifeTime;
            public uint BackupBatteryFullLifeTime;
        }

        private const int SW_HIDE = 0x00;
        private const int SW_SHOW = 0x0001;

        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("coredll.dll", CharSet = CharSet.Auto)]
        private static extern bool EnableWindow(IntPtr hwnd, bool enabled);

        public static void ShowTaskbar()
        {
            IntPtr h = FindWindow("HHTaskBar", null);
            ShowWindow(h, SW_SHOW);
            EnableWindow(h, true);

            IntPtr hSIP = FindWindow("MS_SIPBUTTON", null);
            ShowWindow(hSIP, SW_SHOW);
            EnableWindow(hSIP, true);
        }

        public static void HideTaskbar()
        {
            IntPtr h = FindWindow("HHTaskBar", null);
            ShowWindow(h, SW_HIDE);
            EnableWindow(h, false);

            IntPtr hSIP = FindWindow("MS_SIPBUTTON", null);
            ShowWindow(hSIP, SW_HIDE);
            EnableWindow(hSIP, false);
        }

        [DllImport("coredll")]
        public static extern uint GetSystemPowerStatusEx(SYSTEM_POWER_STATUS_EX lpSystemPowerStatus, bool fUpdate);

        [DllImport("coredll")]
        public static extern uint GetSystemPowerStatusEx2(SYSTEM_POWER_STATUS_EX2 lpSystemPowerStatus, uint dwLen, bool fUpdate);

        //[DllImport("CoreDLL")]
        //public static extern int GetSystemPowerStatusEx2(
        //     SYSTEM_POWER_STATUS_EX2 statusInfo, 
        //    int length,
        //    int getLatest
        //        );


        //public static SYSTEM_POWER_STATUS_EX2 GetSystemPowerStatus()
        //{
        //    SYSTEM_POWER_STATUS_EX2 retVal = new SYSTEM_POWER_STATUS_EX2();
        //   int result =  GetSystemPowerStatusEx2( retVal, Marshal.SizeOf(retVal) , 1);
        //    return retVal;
        //}
        // System\CurrentControlSet\Control\Power\Timeouts
        //[DllImport("CoreDLL")]
        //public static extern int SystemParametersInfo
        //(
        //    SPI Action,
        //    uint Param, 
        //    ref int  result, 
        //    int updateIni
        //);

        //[DllImport("CoreDLL")]
        //public static extern int SystemIdleTimerReset();

        //[DllImport("CoreDLL")]
        //public static extern int CeRunAppAtTime(string application, SystemTime startTime);
        //[DllImport("CoreDLL")]
        //public static extern int CeRunAppAtEvent(string application, int EventID);

        //[DllImport("CoreDLL")]
        //public static extern int FileTimeToSystemTime(ref long lpFileTime, SystemTime lpSystemTime);
        //[DllImport("CoreDLL")]
        //public static extern int FileTimeToLocalFileTime(ref long lpFileTime, ref long lpLocalFileTime);

        // For named events
        //[DllImport("CoreDLL", SetLastError = true)]
        //internal static extern bool EventModify(IntPtr hEvent, EVENT ef);

        [DllImport("CoreDLL", SetLastError = true)]
        internal static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("CoreDLL", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("CoreDLL", SetLastError = true)]
        internal static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        //[DllImport("CoreDLL", SetLastError = true)]
        //internal static extern int WaitForMultipleObjects(int nCount, IntPtr[] lpHandles, bool fWaitAll, int dwMilliseconds);
    }

}
