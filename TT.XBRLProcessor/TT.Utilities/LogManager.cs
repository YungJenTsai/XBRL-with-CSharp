using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace TT.Utilities
{
    public static class LogManager
    {
        static bool isConfigured = false;

        public static ILog GetLogger(Type classType)
        {
            if (isConfigured == false)
            {
                //
                // https://msdn.microsoft.com/en-us/library/system.reflection.assembly.getentryassembly(v=vs.110).aspx
                // The documentation says: The GetEntryAssembly method can return null when a managed assembly has been loaded from an unmanaged application.
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                    assembly = Assembly.GetExecutingAssembly();
                var logRepository = log4net.LogManager.GetRepository(assembly);
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                isConfigured = true;
            }
            return log4net.LogManager.GetLogger(classType);
        }
    }
}
