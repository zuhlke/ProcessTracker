using ProcessTracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProcessTrackerTests
{
    
    
    public class DataTableComparer {

        public static bool AreEqual(DataTable DT1, DataTable DT2)
        {
            if ((DT1 == null) && (DT2 == null))
                return true;
            else if ((DT1 != null) && (DT2 != null))
            {
                if (DT1.Rows.Count == DT2.Rows.Count)
                {
                    if (DT1.Columns.Count == DT2.Columns.Count)
                    {
                        for (int i = 0; i < DT1.Rows.Count; i++)
                        {
                            for (int j = 0; j < DT1.Columns.Count; j++)
                            {
                                if (DT1.Rows[i][j].ToString() != DT2.Rows[i][j].ToString())
                                    return false;
                            }
                        }
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }

    /// <summary>
    ///This is a test class for PTManagerTest and is intended
    ///to contain all PTManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PTManagerTest
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

        /// <summary>
        ///A test for PTManager Constructor
        ///</summary>
        [TestMethod()]
        public void PTManagerConstructorTest()
        {
            PTManager target = new PTManager();
            Assert.AreEqual(string.Empty, target.processName);
            Assert.AreEqual(default(DateTime), target.startTime);
        }

        /// <summary>
        ///A test for MakeRecords
        ///</summary>
        [TestMethod()]
        public void MakeRecordsSetInitialisesWithFirstProcessAndStartDateTimeTest()
        {
            PTManager target = new PTManager();
            string expectedProcessName = "name1";
            DateTime expectedDateTime = DateTime.Now;
            List<PTRecord> expected = new List<PTRecord>();
            List<PTRecord> actual = PTManager.MakeRecords(expectedProcessName, expectedDateTime);
            CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(expectedProcessName, target.processName);
            Assert.AreEqual(expectedDateTime, target.startTime);

        }

        /// <summary>
        ///A test for MakeRecords
        ///</summary>
        [TestMethod()]
        public void MakeRecordsGeneratesOneRecordForThePreviousProcessWhenNextCallIsOnTheSameDateTest()
        {
            PTManager target = new PTManager();
            string previousProcessName = "previous Process";
            DateTime previousProcessStartDateTime = DateTime.Now - new TimeSpan(1,0,0);
            string nextProcessName = "next Process";
            DateTime nextProcessStartDateTime = DateTime.Now;
            List<PTRecord> expected = new List<PTRecord>() 
            {
                new PTRecord(previousProcessName, previousProcessStartDateTime.Date, nextProcessStartDateTime - previousProcessStartDateTime)
            };
            List<PTRecord> actual = PTManager.MakeRecords(previousProcessName, previousProcessStartDateTime);
            actual = PTManager.MakeRecords(nextProcessName, nextProcessStartDateTime);
            CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(nextProcessName, target.processName);
            Assert.AreEqual(nextProcessStartDateTime, target.startTime);
        }

        [TestMethod()]
        public void MakeRecordsGeneratesMoreThanOneRecordForThePreviousProcessWhenNextCallIsOnADifferentDateTest()
        {
            PTManager target = new PTManager();
            string previousProcessName = "previous Process";
            DateTime previousProcessStartDateTime = DateTime.Now - new TimeSpan(3, 1, 0, 0);
            string nextProcessName = "next Process";
            DateTime nextProcessStartDateTime = DateTime.Now;
            List<PTRecord> expected = new List<PTRecord>() 
            {
                new PTRecord(previousProcessName, previousProcessStartDateTime.Date, (previousProcessStartDateTime.Date + new TimeSpan(1, 0, 0, 0)) - previousProcessStartDateTime),
                new PTRecord(previousProcessName, previousProcessStartDateTime.Date + new TimeSpan(1, 0, 0, 0), new TimeSpan(1, 0, 0, 0)),
                new PTRecord(previousProcessName, previousProcessStartDateTime.Date + new TimeSpan(2, 0, 0, 0), new TimeSpan(1, 0, 0, 0)),
                new PTRecord(previousProcessName, previousProcessStartDateTime.Date + new TimeSpan(3, 0, 0, 0), nextProcessStartDateTime - (previousProcessStartDateTime.Date + new TimeSpan(3, 0, 0, 0)))
            };
            List<PTRecord> actual = PTManager.MakeRecords(previousProcessName, previousProcessStartDateTime);
            actual = PTManager.MakeRecords(nextProcessName, nextProcessStartDateTime);
            CollectionAssert.AreEqual(expected, actual);
            Assert.AreEqual(nextProcessName, target.processName);
            Assert.AreEqual(nextProcessStartDateTime, target.startTime);
        }

        /// <summary>
        ///A test for GetDateTable
        ///</summary>
        [TestMethod()]
        public void GetDateTableTest()
        {
            PTManager target = new PTManager();
            DataTable expected = new DataTable("Dates");
            expected.Columns.Add("Date", Type.GetType("System.DateTime"));
            DataRow newRow = expected.NewRow();
            newRow["Date"] = DateTime.Today;
            expected.Rows.Add(newRow);
            string expectedProcessName = "name1";
            DateTime expectedDateTime = DateTime.Today;
            List<PTRecord> recordList = PTManager.MakeRecords(expectedProcessName, expectedDateTime);
            recordList = PTManager.MakeRecords(expectedProcessName, expectedDateTime);
            PTDatabase.HandleRecord(recordList[0]);
            DataTable actual = target.GetDateTable();
            Assert.IsTrue(DataTableComparer.AreEqual(expected, actual));
        }

        /// <summary>
        ///A test for GetProcessInfoTableForDate
        ///</summary>
        [TestMethod()]
        public void GetProcessInfoTableForDateTest()
        {
            string expectedProcessName = "name1";
            PTManager target = new PTManager(); DataTable expected = new DataTable("ProcessInfos");
            expected.Columns.Add("Name", Type.GetType("System.String"));
            expected.Columns.Add("ActiveTime", Type.GetType("System.Int32"));
            DataRow newRow = expected.NewRow();
            newRow["Name"] = expectedProcessName;
            newRow["ActiveTime"] = 0;
            expected.Rows.Add(newRow);
            DateTime expectedDateTime = DateTime.Today;
            List<PTRecord> recordList = PTManager.MakeRecords(expectedProcessName, expectedDateTime);
            recordList = PTManager.MakeRecords(expectedProcessName, expectedDateTime);
            PTDatabase.HandleRecord(recordList[0]);
            DataTable actual = target.GetProcessInfoTableForDate(expectedDateTime);
            Assert.IsTrue(DataTableComparer.AreEqual(expected, actual));

        }
    }
}
