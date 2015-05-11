using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlServerCe;


namespace ProcessTracker
{
    
    public class PTManager
    {
        private static object observer = null;

        public DataView ProcessInfoView { get; set; }
        private static string currentProcessName = string.Empty;
        private static DateTime startDateTime = default(DateTime);

        public PTManager()
        {
            currentProcessName = string.Empty;
            startDateTime = default(DateTime);
            SubscribeToWindowEvents();
        }

        ~PTManager() {
            Console.WriteLine("Manager has been deallocated.");
        }

        public void SetObserver(object obs) {
            observer = obs;
        }

        public DataTable GetDateTable() {
            DataTable table = new DataTable("Dates");
            SqlCeDataAdapter adapter = PTDatabase.GetAdaperForDatesView();
            adapter.Fill(table);
            return table;
        }

        public DataTable GetProcessInfoTableForDate(DateTime datetime) {
            DataTable table = new DataTable("ProcessInfos");
            PTDate date = PTDatabase.GetDateForDateTime(datetime);
            SqlCeDataAdapter adapter = PTDatabase.GetAdaperForProcessInfosViewForDate(date);
            adapter.Fill(table);
            return table;
        }

        public void CleanUp() {
            List<PTRecord> records = MakeRecords("ProcessTracker", DateTime.Now);
            records.ForEach(PTDatabase.HandleRecord);
            Console.WriteLine("[{0} {1}] Window Name: {2}: Got Focus", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), "ProcessTracker");
            UnhookWinEvent(windowEventHook);
        }

        public string processName 
        {
            get 
            {
                return currentProcessName;
            }
        }

        public DateTime startTime
        {
            get 
            {
                return startDateTime;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;

        private const int EVENT_SYSTEM_FOREGROUND = 3;

        private static IntPtr windowEventHook;

        private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        private static void WindowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            StringBuilder title = new StringBuilder(1024);
            var length = GetWindowText(hwnd, title, 1024);
            List<PTRecord> records = MakeRecords(title.ToString(), DateTime.Now);
            records.ForEach(PTDatabase.HandleRecord);
            Console.WriteLine("[{0} {1}] Window Name: {2}: Got Focus", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), title);
            if (observer != null && observer is PTObserver)
            {
                PTObserver obs = (PTObserver)observer;
                obs.update();
            }
        }

        static WinEventProc eventProc;

        private static void SubscribeToWindowEvents()
        {

             eventProc = new WinEventProc(PTManager.WindowEventCallback);

            if (windowEventHook == IntPtr.Zero)
            {
                windowEventHook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND, // eventMin
                    EVENT_SYSTEM_FOREGROUND, // eventMax
                    IntPtr.Zero,             // hmodWinEventProc
                    eventProc,               // lpfnWinEventProc
                    0,                       // idProcess
                    0,                       // idThread
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                if (windowEventHook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public static List<PTRecord> MakeRecords(string newProcessName, DateTime newDateTime) {
            List<PTRecord> records = new List<PTRecord>();
            if (currentProcessName != string.Empty)
            {
                if (startDateTime.Date == newDateTime.Date)
                    records.Add(new PTRecord(currentProcessName, startDateTime.Date, newDateTime - startDateTime));
                else
                {
                    int differenceInDays = (newDateTime - startDateTime).Days;
                    for (int i = 0; i <= differenceInDays; i++)
                    {
                        DateTime dateTime = startDateTime + new TimeSpan(i, 0, 0, 0);
                        TimeSpan time = default(TimeSpan);
                        if (startDateTime.Date == dateTime.Date)
                            time = (startDateTime.Date + new TimeSpan(1, 0, 0, 0)) - startDateTime;
                        else if (newDateTime.Date != dateTime.Date)
                            time = new TimeSpan(1, 0, 0, 0);
                        else
                            time = newDateTime - dateTime.Date;
                        records.Add(new PTRecord(currentProcessName, dateTime.Date, time));
                    }
                }
            }
            currentProcessName = newProcessName;
            startDateTime = newDateTime;
            return records;
        }
    }
}
