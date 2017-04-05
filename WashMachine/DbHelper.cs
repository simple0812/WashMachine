using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using WashMachine.Models;

namespace WashMachine
{
    public static class DbHelper
    {
        static DbHelper()
        {
            CreateTables();
        }
        private static readonly string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "test.db");

        public static SQLiteConnection GetDbConnection()
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
        }

        private static void CreateTables()
        {
            using (var conn = GetDbConnection())
            {
                conn.CreateTable<WashFlow>();
            }
        }
    }
}
