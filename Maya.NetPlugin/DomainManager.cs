using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Maya.NetPlugin
{
    public class DomainManager
    {
        private static readonly Lazy<DomainManager> LazyInstance =
            new Lazy<DomainManager>(() => new DomainManager());

        private DomainManager()
        {

        }

        private static DomainManager Instance =>
            LazyInstance.Value;

        private AppDomain _pluginDomain;

        private static string _domainApplicationBase;

        public static void CreateDomain(
            string applicationBase,
            string assemblyName,
            string entryPointClassName,
            string entryPointMethodName)
        {
            _domainApplicationBase = applicationBase;
            var domaininfo = new AppDomainSetup
            {
                ApplicationBase = applicationBase,
            };
            var evidence = AppDomain.CurrentDomain.Evidence;

            UnloadDomain(assemblyName);
            var domain = AppDomain.CreateDomain(assemblyName, evidence, domaininfo);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            domain.AssemblyResolve += DomainOnAssemblyResolve;

            var type = typeof(Proxy);
            var value = (Proxy)domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                type.FullName);

            if (assemblyName == DotNetCommand.AssemblyName)
                Instance._pluginDomain = domain;
            else throw new Exception();

            var path = $"{applicationBase}\\{assemblyName}.dll";

            value.InstantiateObject(path, $"{assemblyName}.{entryPointClassName}");
            value.InvokeMethod($"{entryPointMethodName}");
        }

        private static Assembly DomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var current = Environment.CurrentDirectory;
            var path = Path.Combine(current, @"bin\openmayacs.dll");
            if (args.Name.Contains("openmayacs"))
                return Assembly.LoadFrom(path);

            throw new Exception("Can't find assembly!");
        }

        public static void UnloadDomain(string name)
        {
            AppDomain domain;
            if (name == DotNetCommand.AssemblyName)
                domain = Instance._pluginDomain;
            else throw new Exception();
            try
            {
                if (domain == null)
                    return;
                AppDomain.Unload(domain);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public static void UnloadDomains()
        {
            UnloadDomain(DotNetCommand.AssemblyName);
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = Assembly.Load(args.Name);
            if (assembly != null)
                return assembly;
            throw new Exception("AppDomain assembly load error");
        }

        private class Proxy : MarshalByRefObject
        {
            private Type _type;
            private object _object;
            private static Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFile(assemblyPath);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public void InstantiateObject(string assemblyPath, string typeName, object[] args = null)
            {
                var assembly = GetAssembly(assemblyPath);
                _type = assembly.GetType(typeName);
                _object = Activator.CreateInstance(_type, args);
            }

            public void InvokeMethod(string methodName, object[] args = null)
            {
                var methodinfo = _type.GetMethod(methodName);
                methodinfo.Invoke(_object, args);
            }
        }
    }
}