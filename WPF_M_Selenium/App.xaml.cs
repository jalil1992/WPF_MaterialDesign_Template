using PCKLIB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_M_Selenium
{
    public partial class App : Application
    {
        public static MainWindow g_main_wnd;
        public static UserSetting g_setting;
        public static WRequest g_request = new WRequest();
        public static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            log_error("DISPATCHER UNHANDLED EXCEPTION: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log_error("DOMAIN UNHANDLED EXCEPTION: " + (e.ExceptionObject as Exception).Message + "\n" + (e.ExceptionObject as Exception).StackTrace);
        }

        public static void log_info(string msg)
        {
            try
            {
                logger.Info(msg);
                if (g_main_wnd != null)
                {
                    //g_main_wnd.output_log(msg);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void log_error(string msg, bool msgbox = false)
        {
            try
            {
                logger.Error(msg);
                if (msgbox)
                    MessageBox.Show(msg);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
