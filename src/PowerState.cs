
using System.Runtime.InteropServices;
using System.Diagnostics;

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

    }

}
