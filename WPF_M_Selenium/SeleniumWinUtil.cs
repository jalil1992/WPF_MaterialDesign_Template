// Windows related action utility to be used for Web automation
// David Piao

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OS_Util;
using WPF_M_Selenium;

namespace PCKLIB
{
    class WinUtil
    {
        static int timeout = 10000;
        static object locker = new object();

        private static bool remove_javascript_alert(IntPtr h, IntPtr lp)
        {
            string title = OS_Win.GetWindowText(h, 256);
            if (title == null)
                return true;
            if (title.ToLower().Contains("javascript confirm"))
            {
                OS_Win.SetForegroundWindow(h);
                System.Windows.Forms.SendKeys.SendWait("{TAB}");
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
            return true;
        }
        public static void check_javascript_alert()
        {
            OS_Win.EnumWindows(new OS_Win.EnumWindowsDelegate(remove_javascript_alert), IntPtr.Zero);
        }

        public async static Task<bool> find_open_dlg()
        {
            lock(locker)
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                IntPtr hnd = IntPtr.Zero;
                IntPtr par_hnd = IntPtr.Zero;
                string[] title = { "Open", "Open File" };
                string sel_t = "N";
                w.Start();
                while (w.ElapsedMilliseconds < timeout)
                {
                    foreach (var t in title)
                    {
                        hnd = OS_Win.FindWindow("#32770", t);
                        if (hnd != IntPtr.Zero)
                        {
                            par_hnd = OS_Win.GetParent(hnd);
                            if (par_hnd != null)
                            {
                                sel_t = t;
                                break;
                            }
                        }
                    }
                    if (sel_t != "N")
                        break;
                    Thread.Sleep(100);
                }
                w.Stop();
                if (w.ElapsedMilliseconds >= timeout)
                {
                    return false;
                }
                return true;
            }
        }
        public async static Task<bool> set_upload_file(string path)
        {
            lock(locker)
            {
                try
                {
                    IntPtr hnd = IntPtr.Zero;
                    IntPtr par_hnd = IntPtr.Zero;
                    Stopwatch w = new Stopwatch();
                    string[] title = { "Open", "Open File" };
                    string sel_t = "N";
                    w.Start();
                    while (w.ElapsedMilliseconds < timeout)
                    {
                        foreach (var t in title)
                        {
                            hnd = OS_Win.FindWindow("#32770", t);
                            if (hnd != IntPtr.Zero)
                            {
                                par_hnd = OS_Win.GetParent(hnd);
                                if (par_hnd != null)
                                {
                                    sel_t = t;
                                    break;
                                }
                            }
                        }
                        if (sel_t != "N")
                            break;
                        Thread.Sleep(100);
                    }
                    w.Stop();
                    Thread.Sleep(500);
                    if (w.ElapsedMilliseconds >= timeout)
                    {
                        return false;
                    }

                    w.Start();
                    while (w.ElapsedMilliseconds < 5000)
                    {
                        OS_Win.SendMessage(hnd, OS_Win.WM_ACTIVATE, (IntPtr)0, (IntPtr)0);
                        OS_Win.SetForegroundWindow(hnd);
                        string keys = "";
                        foreach (char key in path)
                            keys += "{" + key + "}";
                        System.Windows.Forms.SendKeys.SendWait(path);
                        break;
                    }
                    w.Stop();

                    OS_Win.SetForegroundWindow(hnd);
                    System.Windows.Forms.SendKeys.SendWait("%+{O}");
                    Thread.Sleep(100);
                    return true;
                }
                catch(Exception ex)
                {
                    App.log_error(ex.Message + "\n" + ex.StackTrace);
                    return false;
                }
            }

        }
    }
}
