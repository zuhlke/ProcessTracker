using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;
using System.IO;

namespace ProcessTracker
{

    public struct PTDate {
        public int index;
        public DateTime date;
        public PTDate(int i, DateTime d) {
            this.index = i;
            this.date = d;
        }
    }

    public struct PTProcessInfo {
        public int index;
        public string name;
        public TimeSpan activeTime;
        public PTProcessInfo(int i, string n, TimeSpan at) {
            this.index = i;
            this.name = n;
            this.activeTime = at;
        }
    }

    public struct PTRecord
    {
        public string name;
        public DateTime datetime;
        public TimeSpan time;
        public PTRecord(string n, DateTime dt, TimeSpan t)
        {
            this.name = n;
            this.datetime = dt;
            this.time = t;
        }
    }

    public static class PTDatabase
    {

        private static SqlCeConnection connection = null;

        public static bool CreateSDFFile(string filePath) {
            bool created = false;
            try
            {
                if (!File.Exists(filePath))
                {
                    string connectionString = string.Format("DataSource=\"{0}\"", filePath);
                    SqlCeEngine en = new SqlCeEngine(connectionString);
                    en.CreateDatabase();
                    SqlCeConnection connection = new SqlCeConnection(connectionString);
                    connection.Open();
                    try
                    {
                        var datesTableCreationCommand = connection.CreateCommand();
                        datesTableCreationCommand.CommandText = @"CREATE TABLE Dates (
Idx INTEGER PRIMARY KEY NOT NULL IDENTITY, 
Date DATETIME NOT NULL, 
CONSTRAINT uniqueDate UNIQUE (Date));";
                        datesTableCreationCommand.ExecuteNonQuery();
                        var processInfosTableCreationCommand = connection.CreateCommand();
                        processInfosTableCreationCommand.CommandText = @"CREATE TABLE ProcessInfos (
Idx INTEGER PRIMARY KEY NOT NULL IDENTITY, 
Name NVARCHAR(1024) NOT NULL, 
ActiveTime INTEGER, 
DateIdx INTEGER);";
                        processInfosTableCreationCommand.ExecuteNonQuery();
                    }
                    finally
                    {
                        connection.Close();
                    }
                    created = true;
                }
                else {
                    created = true;
                }
            }
            catch (Exception e)
            {
                created = false;
                throw e;
            }

            return created;
        }

        public static SqlCeConnection Connection() {
            if (connection == null) {
                var sdfPath = "PTDatabase.sdf";
                if(CreateSDFFile(sdfPath))
                    connection = new SqlCeConnection("Data Source = " + sdfPath);
            }
            return connection;
        }

        public static bool CreateConnectionWithSDFFile(string sdfFilePath)
        {
            bool created = false;
            if (CreateSDFFile(sdfFilePath)) {
                connection = new SqlCeConnection("Data Source = " + sdfFilePath);
                created = true;
            }
            return created;
        }
        
        public static List<PTDate> dates {
            get
            {
                List<PTDate> dateList = new List<PTDate>();
                SqlCeConnection connection = null;
                try
                {
                    connection = Connection();
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Dates";
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        PTDate date;
                        date.index = reader.GetInt32(0);
                        date.date = reader.GetDateTime(1);
                        dateList.Add(date);
                    }
                }
                finally
                {
                    connection.Close();
                }
                return dateList;
            }
        }

        public static List<PTProcessInfo> ProcessInfoForDate(PTDate date) {
            List<PTProcessInfo> processInfoList = new List<PTProcessInfo>();
            SqlCeConnection connection = null;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ProcessInfos WHERE DateIdx=@dateIdx";
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                SqlCeDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PTProcessInfo processInfo;
                    processInfo.index = reader.GetInt32(0);
                    processInfo.name = reader.GetString(1);
                    processInfo.activeTime = new TimeSpan(0,reader.GetInt32(2),0);
                    processInfoList.Add(processInfo);
                }
            }
            finally
            {
                connection.Close();
            }
            return processInfoList;
        }

        public static PTProcessInfo GetInfoForProcessOnDate(string processName, PTDate date) {
            PTProcessInfo info = new PTProcessInfo(0, string.Empty, default(TimeSpan));
            SqlCeConnection connection = null;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ProcessInfos WHERE Name=@name AND DateIdx=@dateIdx";
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = processName;
                SqlCeDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    info.index = reader.GetInt32(0);
                    info.name = reader.GetString(1);
                    info.activeTime = new TimeSpan(0, reader.GetInt32(2), 0);
                }
            }
            finally
            {
                connection.Close();
            }
            return info;
        }

        public static bool InsertDate(PTDate date) {
            bool inserted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Dates (Date) VALUES (@date)";
                command.Parameters.Add("@date", SqlDbType.DateTime).Value = date.date;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    inserted = true;
            }
            finally
            {
                connection.Close();
            }
            return inserted;
        }

        public static bool InsertDate(DateTime datetime)
        {
            bool inserted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Dates (Date) VALUES (@date)";
                command.Parameters.Add("@date", SqlDbType.DateTime).Value = datetime.Date;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    inserted = true;
            }
            finally
            {
                connection.Close();
            }
            return inserted;
        }

        public static PTDate GetDateForDateTime(DateTime datetime) {
            SqlCeConnection connection = null;
            PTDate date = new PTDate(0, default(DateTime));
            bool success = false;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Dates WHERE Date=@date";
                command.Parameters.Add("@date", SqlDbType.DateTime).Value = datetime.Date;
                SqlCeDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    date.index = reader.GetInt32(0);
                    date.date = reader.GetDateTime(1);
                    success = true;
                }
            }
            finally
            {
                connection.Close();
            }
            if (!success) 
            {
                InsertDate(datetime);
                date = GetDateForDateTime(datetime);
            }
            return date;
        }

        public static SqlCeDataAdapter GetAdaperForDatesView() {
            SqlCeDataAdapter adapter = null;
            SqlCeConnection connection = null;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Date FROM Dates";
                command.ExecuteNonQuery();
                adapter = new SqlCeDataAdapter(command);
            }
            finally
            {
                connection.Close();
            }
            return adapter;
        }

        public static SqlCeDataAdapter GetAdaperForProcessInfosViewForDate(PTDate date)
        {
            SqlCeDataAdapter adapter = null;
            SqlCeConnection connection = null;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Name, ActiveTime FROM ProcessInfos WHERE DateIdx=@dateIdx";
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                command.ExecuteNonQuery();
                adapter = new SqlCeDataAdapter(command);
                Console.WriteLine(adapter.ToString());
            }
            finally
            {
                connection.Close();
            }
            return adapter;
        }

        public static bool DeleteDate(PTDate date) {
            bool deleted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Dates WHERE Date=@date";
                command.Parameters.Add("@date", SqlDbType.DateTime).Value = date.date;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    deleted = true;
            }
            finally
            {
                connection.Close();
            }
            return deleted;
        }

        public static bool InsertProcessInfo(PTProcessInfo info, PTDate date) {
            bool inserted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ProcessInfos (Name, ActiveTime, DateIdx) VALUES (@name, @active, @dateIdx)";
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = info.name;
                command.Parameters.Add("@active", SqlDbType.Int).Value = info.activeTime.TotalMinutes;
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    inserted = true;
            }
            finally
            {
                connection.Close();
            }
            return inserted;
        }

        public static bool InsertProcessInfo(string name, TimeSpan time, PTDate date)
        {
            bool inserted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ProcessInfos (Name, ActiveTime, DateIdx) VALUES (@name, @active, @dateIdx)";
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                command.Parameters.Add("@active", SqlDbType.Int).Value = time.TotalMinutes;
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    inserted = true;
            }
            finally
            {
                connection.Close();
            }
            return inserted;
        }

        public static bool UpdateProcessTime(string name, PTDate date, TimeSpan time) {
            bool updated = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var getTimeStepCommand = connection.CreateCommand();
                getTimeStepCommand.CommandText = "SELECT ActiveTime FROM ProcessInfos WHERE Name=@name AND DateIdx=@dateIdx";
                getTimeStepCommand.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                getTimeStepCommand.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                SqlCeDataReader reader = getTimeStepCommand.ExecuteReader();
                if (reader.Read()) {
                    int previousTime = reader.GetInt32(0);
                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = "UPDATE ProcessInfos SET ActiveTime=@active WHERE Name=@name AND DateIdx=@dateIdx";
                    updateCommand.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
                    updateCommand.Parameters.Add("@active", SqlDbType.Int).Value = previousTime + time.Minutes;
                    updateCommand.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                    noOfRowAffected = updateCommand.ExecuteNonQuery();
                    if (noOfRowAffected > 0)
                        updated = true;
                }
            }
            finally
            {
                connection.Close();
            }
            return updated;
        }

        public static bool DeleteProcessInfo(PTProcessInfo info, PTDate date)
        {
            bool deleted = false;
            SqlCeConnection connection = null;
            int noOfRowAffected = 0;
            try
            {
                connection = Connection();
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ProcessInfos WHERE Name=@name AND DateIdx=@dateIdx";
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = info.name;
                command.Parameters.Add("@dateIdx", SqlDbType.Int).Value = date.index;
                noOfRowAffected = command.ExecuteNonQuery();
                if (noOfRowAffected > 0)
                    deleted = true;
            }
            finally
            {
                connection.Close();
            }
            return deleted;
        }

        public static void HandleRecord(PTRecord record) {
            PTDate date = PTDatabase.GetDateForDateTime(record.datetime);
            if (!PTDatabase.UpdateProcessTime(record.name, date, record.time))
            {
                PTDatabase.InsertProcessInfo(record.name, record.time, date);
            }
        }

        public static bool ClearAllWithConnection(SqlCeConnection conn) {
            bool allDeleted = false;
            //SqlCeConnection connection = null;
            int datesDeleted = 0, processInfoDeleted = 0;
            try
            {
                conn.Open();
                var deleteDatesCommand = conn.CreateCommand();
                deleteDatesCommand.CommandText = "DELETE FROM Dates";
                datesDeleted = deleteDatesCommand.ExecuteNonQuery();
                var deleteProcessInfoCommand = conn.CreateCommand();
                deleteProcessInfoCommand.CommandText = "DELETE FROM ProcessInfos";
                processInfoDeleted = deleteProcessInfoCommand.ExecuteNonQuery();
                if ((datesDeleted > 0) && (processInfoDeleted > 0))
                    allDeleted = true;
            }
            finally
            {
                conn.Close();
            }
            File.Delete(conn.DataSource);
            connection = null;
            return allDeleted;
        }

        public static bool ClearAll() {
            SqlCeConnection connection = Connection();
            return ClearAllWithConnection(connection);
        }
    }
}
