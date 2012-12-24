using System;
using Huffelpuff.Properties;

namespace Huffelpuff.Utils
{
    public static class Log
    {

        private static Logger instance;

        public static Logger Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                if (!Environment.UserInteractive)
                {
                    instance = new WindowsServiceLogger();
                }
                else
                {
                    switch (Settings.Default.Logger)
                    {
                        case "console":
                            instance = new ConsoleLogger();
                            break;
                        case "null":
                            instance = new NullLogger();
                            break;
                        default:
                            instance = new ConsoleLogger();
                            break;
                    }
                }

                Level logLevel;
                instance.MinLogLevel = Enum.TryParse(Settings.Default.LogLevel, out logLevel) ? logLevel : Level.Warning;
                instance.Verbose = Settings.Default.Verbosity;
                return instance;
            }
        }
    }
}