
using System.Runtime.InteropServices;
using System.Diagnostics;

using Microsoft.Win32;

namespace PcMQTT
{
    
    class PowerManager
    {

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
        
        static public void hibernate()
        {
            SetSuspendState(true, true, true);
        }
        static public void sleep()
        {
            SetSuspendState(false, true, true);
        }
        static public void shutdown()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        public delegate void OnResumeEventHandler();
        public delegate void OnSuspendEventHandler();
        public delegate void OnShutdownEventHandler();
        
        public event OnSuspendEventHandler OnSuspend;
        public event OnResumeEventHandler OnResume;
        public event OnShutdownEventHandler OnShutdown;


        void OnPowerChange(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend) OnSuspend();
            else if (e.Mode == PowerModes.Resume) OnResume();
        }

        public PowerManager(
            OnResumeEventHandler onResume,
            OnSuspendEventHandler onSuspend,
            OnShutdownEventHandler onShutdown
        )
        {
            OnResume   += onResume;
            OnSuspend  += onSuspend;
            OnShutdown += onShutdown;

            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerChange);
            SystemEvents.SessionEnding    += new SessionEndingEventHandler((sender, e) => OnShutdown());
        }

    }

}
