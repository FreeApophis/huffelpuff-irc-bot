using System.Data.SQLite;
using System.IO;

namespace Huffelpuff.Utils
{
    public class DatabaseConnection
    {
        public static SQLiteConnection Create(string name)
        {
            var fileInfo = new FileInfo(name + ".s3db");

            return new SQLiteConnection(string.Format("Data Source={0};FailIfMissing=true;", fileInfo.FullName));
        }
    }
}
