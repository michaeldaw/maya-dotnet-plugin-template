using System;
using Autodesk.Maya.OpenMaya;
using System.Collections.Generic;
using Maya.NetPlugin;

[assembly: ExtensionPlugin(typeof(DotNetPlugin), "Any")]
[assembly: MPxCommandClass(typeof(DotNetCommand), "dotnetplugin")]
[assembly: MPxCommandClass(typeof(DotNetUnloadCommand), "dotnetpluginunload")]

/*
Notes:
Load the plugin with the command 'dotnetplugin'. At this point, this assembly
"Maya.NetPlugin" will be held "in-use" by Maya, and you won't be able to overwrite it.
Therefore, this project has been removed from the default build configuration 
(Build menu -> Configuration Manager...). Once it's working properly, it shouldn't 
need to be changed. This assembly will have the .nll.dll extension, indicating it
is a .net plugin.

However, Maya will not retain an handle to the assembly "Maya.NetPlugin.Logic". Use
the command "dotnetpluginunload" to unload the AppDomain created by this class. You 
can then copy a new version of the .dll into the plugin directory without exiting 
Maya, or even unloading the plugin. The project "Maya.NetPlugin.Logic" is also set
to build directory to the plugin folder. 

The target folder is set to %PUBLIC%\mayaplugin. Change this location to suit 
your needs.
*/

namespace Maya.NetPlugin
{
    public class DotNetPlugin : IExtensionPlugin
    {
        public string GetMayaDotNetSdkBuildVersion() => "201353";

        public bool InitializePlugin() => true;

        public bool UninitializePlugin() => true;
    }

    public class DotNetCommand : MPxCommand, IMPxCommand
    {
        // add the path you specify here to the following environment variable
        // MAYA_PLUG_IN_PATH
        // if multiple paths are required, separate them with a semi-colon (;)
        internal readonly string ApplicationBase =
            Environment.ExpandEnvironmentVariables("%public%\\mayaplugin");

        // this is the name of the assembly where the logic of the plugin will
        // be located.
        internal const string AssemblyName =
            "Maya.NetPlugin.Logic";

        // Specify the name of class in the assembly specified above. This class
        // must have a method with the name specified below and will be called
        // to start the logic of the plugin.
        private const string DoIt_Class =
            "MayaCommand";

        private const string DoIt_Method =
            "Run";

        public override void doIt(MArgList args)
        {
            DomainManager.CreateDomain(
               ApplicationBase,
               AssemblyName,
               DoIt_Class,
               DoIt_Method);
        }
    }

    public class DotNetUnloadCommand : MPxCommand, IMPxCommand
    {
        public override void doIt(MArgList args)
        {
            DomainManager.UnloadDomains();
        }
    }
}