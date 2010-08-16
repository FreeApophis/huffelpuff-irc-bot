using System;

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
                    switch (PersistentMemory.Instance.GetValue("logger"))
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
                instance.MinLogLevel = Enum.TryParse(PersistentMemory.Instance.GetValue("minloglevel"), out logLevel)
                                           ? logLevel
                                           : Level.Warning;
                bool verbosity;
                instance.Verbose = bool.TryParse(PersistentMemory.Instance.GetValue("verbosity"), out verbosity)
                                           ? verbosity
                                           : false;

                return instance;

            }
        }
    }
}