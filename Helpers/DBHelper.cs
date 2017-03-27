using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
    public class DBHelper
    {
        private SQLiteConnection connection;

        public DBHelper()
        {
            OpenDBConnection();
        }

        private void OpenDBConnection()
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "MSCognitive.db");
            if (File.Exists(fullPath) == false)
            {
                SQLiteConnection.CreateFile(fullPath);
            }

            connection = new SQLiteConnection($"Data Source={fullPath};Version=3;");
            connection.Open();
        }

        private void CreateTables()
        {
            string query = "create table if not exists Person (id INTEGER PRIMARY KEY AUTOINCREMENT, personId varchar(36), name varchar(100), group_id varchar(36))";
            //false/sdaafds
        }
    }
}
