using System.IO;
using Plugin.Database.Quiz;

namespace Plugin
{
    internal interface IQuizImport
    {
        void ImportFile(FileInfo file, Main db);
    }
}