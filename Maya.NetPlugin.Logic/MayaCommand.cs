using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maya.NetPlugin.Logic
{
    // this class is specified in the Plugin class of Maya.NetPlugin
    public class MayaCommand
    {

        // this method is specified in the Plugin class of Maya.NetPlugin
        public void Run()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            MGlobal.displayInfo("starting plugin...");
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            MGlobal.displayInfo("unloading...");
        }

        private static void CurrentDomainOnUnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            // write the error the Output Window
            var error = MStreamUtils.stdErrorStream();
            MStreamUtils.writeCharBuffer(error, e.ExceptionObject + "\n");

            // write the error to the status bar
            MGlobal.displayError(e.ExceptionObject.ToString());
        }
    }
}
