using ProcessTracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.IO;

namespace ProcessTrackerTests
{
    
    /// <summary>
    ///This is a test class for PTDatabaseTest and is intended
    ///to contain all PTDatabaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PTDatabaseTest
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestCleanup()]
        public void FinalCleanup() {
            
            PTDatabase.ClearAll();
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        private void SqlCeConnectionEqual(SqlCeConnection expected, SqlCeConnection actual) {
            Assert.AreEqual<string>(expected.ConnectionString, actual.ConnectionString);
            Assert.AreEqual<string>(expected.Database, actual.Database);
            Assert.AreEqual<string>(expected.DataSource, actual.DataSource);
        }

        /// <summary>
        ///A test for CreateConnection
        ///</summary>
        [TestMethod()]
        public void CreateConnectionTest()
        {
            SqlCeConnection expected = new SqlCeConnection("Data Source = PTDatabase.sdf");
            SqlCeConnection actual = PTDatabase.Connection();
            SqlCeConnectionEqual(expected, actual);
        }

        [TestMethod()]
        public void CreateConnectionWithFilePathTest()
        {
            var sdfPath = "PTDatabase.sdf";
            SqlCeConnection expectedConnection = new SqlCeConnection("Data Source = " + sdfPath);
            bool expected = true;
            bool created = PTDatabase.CreateConnectionWithSDFFile(sdfPath);
            SqlCeConnection actualConnction = PTDatabase.Connection();
            Assert.AreEqual(expected, created);
            Assert.AreEqual(expected, File.Exists(sdfPath));
            SqlCeConnectionEqual(expectedConnection, actualConnction);
        }

        /// <summary>
        ///A test for insertDate
        ///</summary>
        [TestMethod()]
        public void insertDateTest()
        {
            PTDate date;
            date.index= 1;
            date.date = DateTime.Now.Date;
            bool expected = true;
            bool actual = PTDatabase.InsertDate(date);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for deleteDate
        ///</summary>
        [TestMethod()]
        public void deleteDateTest()
        {
            PTDate date;
            date.index = 1;
            date.date = DateTime.Now.Date;
            bool expected = true;
            bool inserted = PTDatabase.InsertDate(date);
            bool actual = PTDatabase.DeleteDate(date);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for insertProcessInfo
        ///</summary>
        [TestMethod()]
        public void insertProcessInfoTest()
        {
            PTProcessInfo info;
            info.index = 1;
            info.name = "name";
            info.activeTime = new TimeSpan(2,3,30);
            PTDate date;
            date.index = 1;
            date.date = DateTime.Now.Date;
            bool expected = true;
            bool actual = PTDatabase.InsertProcessInfo(info, date);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for deleteProcessInfo
        ///</summary>
        [TestMethod()]
        public void deleteProcessInfoTest()
        {
            PTProcessInfo info;
            info.index = 1;
            info.name = "name";
            info.activeTime = new TimeSpan(2, 3, 30);
            PTDate date;
            date.index = 1;
            date.date = DateTime.Now.Date;
            bool expected = true;
            bool inserted = PTDatabase.InsertProcessInfo(info, date);
            bool actual = PTDatabase.DeleteProcessInfo(info, date);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for updateProcessTime
        ///</summary>
        [TestMethod()]
        public void updateProcessTimeSuccessfulTest()
        {
            string name = "name";
            TimeSpan time = new TimeSpan(0,20,0);
            PTProcessInfo info;
            info.index = 1;
            info.name = name;
            info.activeTime = time;
            PTDate date;
            date.index = 1;
            date.date = DateTime.Now.Date;
            bool expected = true;
            bool inserted = PTDatabase.InsertProcessInfo(info, date);
            bool actual = PTDatabase.UpdateProcessTime(name, date, time);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void updateProcessTimeFailedTest()
        {
            string name = "name";
            TimeSpan time = new TimeSpan(0, 20, 0);
            PTDate date = new PTDate(1, DateTime.Today);
            bool expected = false;
            bool actual = PTDatabase.UpdateProcessTime(name, date, time);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CreateSDFFile
        ///</summary>
        [TestMethod()]
        public void CreateSDFFileTest()
        {
            string filePath = "PTDatabase.sdf";
            bool expected = true;
            bool actual = PTDatabase.CreateSDFFile(filePath);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, File.Exists(filePath));
            File.Delete(filePath);
        }

        /// <summary>
        ///A test for processInfoForDate
        ///</summary>
        [TestMethod()]
        public void processInfoForDateTest()
        {
            PTDate date = new PTDate(1, DateTime.Now.Date);
            PTDate otherDate = new PTDate(2, DateTime.Today + new TimeSpan(1,0,0,0));
            List<PTProcessInfo> expected = new List<PTProcessInfo>() 
            {
                new PTProcessInfo(1, "name1", new TimeSpan(0, 5, 0)),
                new PTProcessInfo(2, "name2", new TimeSpan(0, 75, 0)),
                new PTProcessInfo(3, "name3", new TimeSpan(1, 5, 0))
            };
            PTProcessInfo otherInfo = new PTProcessInfo(4, "name4", new TimeSpan(1, 25, 0));
            PTDatabase.InsertProcessInfo(expected[0], date);
            PTDatabase.InsertProcessInfo(expected[1], date);
            PTDatabase.InsertProcessInfo(expected[2], date);
            PTDatabase.InsertProcessInfo(otherInfo, otherDate);
            List<PTProcessInfo> actual = PTDatabase.ProcessInfoForDate(date);
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for dates
        ///</summary>
        [TestMethod()]
        public void datesTest()
        {
            List<PTDate> expected = new List<PTDate>()
            {
                new PTDate(1, new DateTime(2015,05,01)),
                new PTDate(2, new DateTime(2015,05,02)),
                new PTDate(3, new DateTime(2015,05,03)),
            };
            PTDatabase.InsertDate(expected[0]);
            PTDatabase.InsertDate(expected[1]);
            PTDatabase.InsertDate(expected[2]);
            List<PTDate> actual = PTDatabase.dates;
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetDateForDateTime
        ///</summary>
        [TestMethod()]
        public void GetDateForDateTimeWhenTheDateExistsInDBTest()
        {
            DateTime datetime = DateTime.Now;
            PTDate expected = new PTDate(1,datetime.Date);
            PTDatabase.InsertDate(expected);
            PTDate actual = PTDatabase.GetDateForDateTime(datetime);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetDateForDateTimeWhenTheDateDoesNotExistInDBTest()
        {
            DateTime datetime = DateTime.Now;
            PTDate expected = new PTDate(1, datetime.Date);
            PTDate actual = PTDatabase.GetDateForDateTime(datetime);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InsertDate
        ///</summary>
        [TestMethod()]
        public void InsertDateTest()
        {
            DateTime datetime = DateTime.Now;
            bool expected = true;
            bool actual = PTDatabase.InsertDate(datetime);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InsertProcessInfo
        ///</summary>
        [TestMethod()]
        public void InsertProcessInfoTest()
        {
            string name = "name";
            TimeSpan time = new TimeSpan(0, 10, 0);
            PTDate date = new PTDate(1, DateTime.Today);
            bool expected = true;
            bool actual = PTDatabase.InsertProcessInfo(name, time, date);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for HandleRecord
        ///</summary>
        [TestMethod()]
        public void HandleRecordInsertNewRecordToDBTest()
        {
            PTRecord record = new PTRecord("name", DateTime.Today, new TimeSpan(0,10,0));
            PTDatabase.HandleRecord(record);
            PTDate expectedDate = new PTDate(1, DateTime.Today);
            PTProcessInfo expectedInfo = new PTProcessInfo(1, "name", new TimeSpan(0, 10, 0));
            PTDate actualDate = PTDatabase.GetDateForDateTime(DateTime.Today);
            PTProcessInfo actaulInfo = PTDatabase.GetInfoForProcessOnDate(record.name, actualDate);
            Assert.AreEqual(expectedDate, actualDate);
            Assert.AreEqual(expectedInfo, actaulInfo);
        }

        [TestMethod()]
        public void HandleRecordUpdatesExistingEntryInDBTest()
        {
            PTRecord record1 = new PTRecord("name", DateTime.Today, new TimeSpan(0, 10, 0));
            PTRecord record2 = new PTRecord("name", DateTime.Today, new TimeSpan(0, 20, 0));
            PTDate expectedDate = new PTDate(1, DateTime.Today);
            PTProcessInfo expectedInfo1 = new PTProcessInfo(1, "name", new TimeSpan(0, 10, 0));
            PTProcessInfo expectedInfo2 = new PTProcessInfo(1, "name", new TimeSpan(0, 30, 0));
            PTDatabase.HandleRecord(record1);
            PTDate actualDate = PTDatabase.GetDateForDateTime(DateTime.Today);
            PTProcessInfo actaulInfo1 = PTDatabase.GetInfoForProcessOnDate(record1.name, actualDate);
            Assert.AreEqual(expectedDate, actualDate);
            Assert.AreEqual(expectedInfo1, actaulInfo1);
            PTDatabase.HandleRecord(record2);
            PTProcessInfo actaulInfo2 = PTDatabase.GetInfoForProcessOnDate(record1.name, actualDate);
            Assert.AreEqual(expectedInfo2, actaulInfo2);
        }

        [TestMethod()]
        public void HandleRecordInsertsSeparateRecordforDifferentProcessInDBTest()
        {
            PTRecord record1 = new PTRecord("name1", DateTime.Today, new TimeSpan(0, 10, 0));
            PTRecord record2 = new PTRecord("name2", DateTime.Today, new TimeSpan(0, 20, 0));
            PTDate expectedDate = new PTDate(1, DateTime.Today);
            PTProcessInfo expectedInfo1 = new PTProcessInfo(1, record1.name, new TimeSpan(0, 10, 0));
            PTProcessInfo expectedInfo2 = new PTProcessInfo(2, record2.name, new TimeSpan(0, 20, 0));
            PTDatabase.HandleRecord(record1);
            PTDate actualDate = PTDatabase.GetDateForDateTime(DateTime.Today);
            PTProcessInfo actaulInfo1 = PTDatabase.GetInfoForProcessOnDate(record1.name, actualDate);
            Assert.AreEqual(expectedDate, actualDate);
            Assert.AreEqual(expectedInfo1, actaulInfo1);
            PTDatabase.HandleRecord(record2);
            PTProcessInfo actaulInfo2 = PTDatabase.GetInfoForProcessOnDate(record2.name, actualDate);
            Assert.AreEqual(expectedInfo2, actaulInfo2);
        }

        [TestMethod()]
        public void HandleRecordInsertsNewEntryForTheSameProcessIfTheDateIsDifferentDBTest()
        {
            PTRecord record1 = new PTRecord("name", DateTime.Today - new TimeSpan(1,0,0,0), new TimeSpan(0, 10, 0));
            PTRecord record2 = new PTRecord("name", DateTime.Today, new TimeSpan(0, 20, 0));
            PTDate expectedDate1 = new PTDate(1, record1.datetime);
            PTDate expectedDate2 = new PTDate(2, record2.datetime);
            PTProcessInfo expectedInfo1 = new PTProcessInfo(1, "name", new TimeSpan(0, 10, 0));
            PTProcessInfo expectedInfo2 = new PTProcessInfo(2, "name", new TimeSpan(0, 20, 0));
            PTDatabase.HandleRecord(record1);
            PTDate actualDate1 = PTDatabase.GetDateForDateTime(DateTime.Today - new TimeSpan(1, 0, 0, 0));
            PTDate actualDate2 = PTDatabase.GetDateForDateTime(DateTime.Today);
            PTProcessInfo actaulInfo1 = PTDatabase.GetInfoForProcessOnDate(record1.name, actualDate1);
            Assert.AreEqual(expectedDate1, actualDate1);
            Assert.AreEqual(expectedInfo1, actaulInfo1);
            PTDatabase.HandleRecord(record2);
            PTProcessInfo actaulInfo2 = PTDatabase.GetInfoForProcessOnDate(record1.name, actualDate2);
            Assert.AreEqual(expectedDate2, actualDate2);
            Assert.AreEqual(expectedInfo2, actaulInfo2);
        }

        /// <summary>
        ///A test for GetInfoForProcessOnDate
        ///</summary>
        [TestMethod()]
        public void GetInfoForProcessOnDateTest()
        {
            string processName = "name";
            PTDate date = new PTDate(1, DateTime.Today);
            string otherProcessName = "otherName";
            PTProcessInfo expected = new PTProcessInfo(1, processName, new TimeSpan(0,10,0));
            PTProcessInfo otherInfo = new PTProcessInfo(2, otherProcessName, new TimeSpan(0, 10, 0));
            PTDatabase.InsertDate(date);
            PTDatabase.InsertProcessInfo(expected, date);
            PTDatabase.InsertProcessInfo(otherInfo, date);
            PTProcessInfo actual = PTDatabase.GetInfoForProcessOnDate(processName, date);
            Assert.AreEqual(expected, actual);
        }
    }
}
