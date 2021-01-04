using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using Velma_TestAutomation.Velma_Pages;

namespace Unit_Tests
{
    [TestFixture]
    public class Situation1407Tests
    {
        //Setup for testing in Velma, puts in email of user to login and goes to the dashboard page.

        //This list of clients is to compare clients before their change to 1407, this is used for checking if certains tasks were created.
        private List<String> clients = new List<string>();

        //There are some tests that need cusotmer task dates before being altered to child foster care living situation, so I need a list of these to compare
        //a before and after change of tasks and confirm that nothing had changed.
        private struct client_task_and_date
        {
            public string client;
            public string taskName;
            public string lastServiceDate;
            public string serviceDateDue;

            public client_task_and_date(string _client, string _taskName, string _lastServiceDate, string _serviceateDue)
            {
                client = _client;
                taskName = _taskName;
                lastServiceDate = _lastServiceDate;
                serviceDateDue = _serviceateDue;
            }
        }
        


        private List<client_task_and_date> clientDataBeforeLivingSitutaionChange = new List<client_task_and_date>();

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                Pages.Home.GoToHome();
                //Pages.Home.LogIn(); Commented out because I get auto signed in currently
            }
            catch (Exception e)
            {
                Console.WriteLine("Setup failed with error: " + e);
                throw;
            }

            //Changes ten clients to living situation 1407 to test.
            //clients = Pages.Database.ServiceType1407top10SkipHiddenClients();
            clients = Pages.Database.ServiceTypeNot1407();
            ChangeLivingSituationTo1407();
        }
        [Test]
        public void Z26_DatabaseTest()
        {
            try
            {
                List<string> output = new List<string>();
                output = Pages.Database.ServiceType1407();
                foreach (string number in output)
                { Console.WriteLine(number); }
            }
            catch (Exception e)
            {
                Console.WriteLine("Database Test Failed with exception: " + e);
                throw;
            }
        }

        public void ChangeLivingSituationTo1407()
        {
            //try
            //{
            //Grab the list of the clients that pertain to this test
            clients = Pages.Database.ServiceType1407top10SkipHiddenClients();
            Hashtable clientTasks = new Hashtable();
            //create a new hashtable to keep track of the clients tasks and their respective due dates
            Pages.Dashboard.GoToSearch();
            Pages.Search.FindClient(clients);
            Console.WriteLine("Found clients"); // REMEMBER TO DELETE
                                                //Stores task data from each client in a struct to be compared to when the client data is changed due to a living situation change.
            foreach (string client in clients)
            {
                populateClientDataBeforeChange(client);
            }
            Pages.Tasks.FillOutDueDates(ref clientTasks);
            //Console.WriteLine("Due dates filled"); // REMEMBER TO DELETE
            Pages.LivingSituation.GoTo();
            Console.WriteLine("Went to client menu"); // REMEMBER TO DELETE
            Pages.LivingSituation.EditLivingSituationToChildFosterCare();
            //}
            /*catch (Exception e)
            {
                Console.WriteLine("Trigger Changing Living Situation To 1407 failed with exception: " + e);
                throw;
            }*/
        }

        public void populateClientDataBeforeChange(string client_id)
        {
            HashSet<string> Tasks = Pages.Database.RetreiveClientTasks(client_id);
            List<String> clientTasks = Tasks.ToList();
            List<String> dueDates = new List<string>();
            List<String> LastServiceDates = new List<string>();
            foreach (string task in clientTasks)
            {
                dueDates.Add(Pages.Database.GetInputTaskDueDate(client_id, task));
                string lastServiceDate = Pages.Database.GetInputLastServiceDate(client_id, task);
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    LastServiceDates.Add("NULL");
                }
                else
                {
                    LastServiceDates.Add(lastServiceDate);
                }
            }
            for (int i = 0; i < clientTasks.Count; i++)
            {
                clientDataBeforeLivingSitutaionChange.Add(new client_task_and_date(client_id, clientTasks[i], LastServiceDates[i], dueDates[i]));
            }
        }

        //In some acceptance criteria I am instructed to do nothing, this funciton will test the customer data against its data from before the living situation
        //was changed and confirm that the information has not been changed since the living stituiation change.
        public bool DoNothingComparison(string client_id, string input_task)
        {
            //Checks to see if task exists then will compare data from existing task to data from before living situation change.
            if (Pages.Tasks.ValidateInputTaskCleared(input_task) == false)
            {
                foreach (client_task_and_date client in clientDataBeforeLivingSitutaionChange)
                {
                    string input_last_service_date = Pages.Database.GetInputLastServiceDate(client_id, input_task);
                    string input_task_due_date = Pages.Database.GetInputTaskDueDate(client_id, input_task);
                    if (client.lastServiceDate.Equals(input_last_service_date) &&
                        client.serviceDateDue.Equals(input_task_due_date) &&
                        client.client.Equals(client_id) && client.taskName.Equals(input_task))
                    {
                        return true;
                    }
                }
                //If the task if not found, return false;
                return false;
            }
            //task does not exist, make sure that the task also didnt exist before living situation was changed.
            else
            {
                foreach (client_task_and_date client in clientDataBeforeLivingSitutaionChange)
                {
                    //return false if you find the task from before the living situation was changed.
                    if (client.taskName.Equals(input_task) && client.client.Equals(client_id))
                    {
                        return false;
                    }
                }
                //If that task if not found, return false;
                return true;
            }
        }

        public bool dateCheckAgainstLastServeiceDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, task).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> ServiceDateAsInt = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in lastServiceDate)
                {
                    int.TryParse(toInt, out toAdd);
                    ServiceDateAsInt.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(ServiceDateAsInt[2], ServiceDateAsInt[0], ServiceDateAsInt[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public bool dateCheckAgainstISPStartDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] ISPStartDate = Pages.Database.GetISPStartDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> ISPStartDateAsInt = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in ISPStartDate)
                {
                    int.TryParse(toInt, out toAdd);
                    ISPStartDateAsInt.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(ISPStartDateAsInt[2], ISPStartDateAsInt[0], ISPStartDateAsInt[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception)
            {
                return false;
                //Assert.Fail("ISP start date did not come back in expected format.");
            }
            return false;
        }

        public bool dateCheckAgainstISPMeetingDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] ISPMeetingDate = Pages.Database.GetISPMeetingDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, "Case Management Contact").Split('/');
                List<int> ISPMeetingDateAsInt = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in ISPMeetingDate)
                {
                    int.TryParse(toInt, out toAdd);
                    ISPMeetingDateAsInt.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(ISPMeetingDateAsInt[2], ISPMeetingDateAsInt[0], ISPMeetingDateAsInt[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception)
            {
                return false;
                //Assert.Fail("ISP start date did not come back in expected format.");
            }
            return false;
        }

        public bool dateCheckAgainstONAAnnualReviewDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] ONAReviewDate = Pages.Database.GetAnnualONAReviewDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> ONADueDate = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in ONAReviewDate)
                {
                    int.TryParse(toInt, out toAdd);
                    ONADueDate.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(ONADueDate[2], ONADueDate[0], ONADueDate[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception)
            {
                return false;
                //Assert.Fail("ISP start date did not come back in expected format.");
            }
            return false;
        }

        public bool dateCheckAgainstONAAssessorDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] ONAAssessorDate = Pages.Database.GetAnnualONAReviewDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> ONADueDate = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in ONAAssessorDate)
                {
                    int.TryParse(toInt, out toAdd);
                    ONADueDate.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(ONADueDate[2], ONADueDate[0], ONADueDate[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception)
            {
                return false;
                //Assert.Fail("ISP start date did not come back in expected format.");
            }
            return false;
        }

        public bool confirmISPDateSameAsInputTask(string task, string client)
        {
            string taskDue = Pages.Database.GetInputLastServiceDate(client, task);
            string ispDue = Pages.Database.GetInputTaskDueDate(client, "ISP Due");
            if (taskDue.Equals(ispDue))
                return true;
            else
                return false;
        }

        public bool taskHasLastServiceDate(string task, string client)
        {
            foreach(client_task_and_date info in clientDataBeforeLivingSitutaionChange)
            {
                if(info.client.Equals(client)&&info.taskName.Equals(task)&&info.lastServiceDate!=null)
                {
                    return true;
                }
            }
            return false;
        }

        [Test]
        public void A1_ScOnaReviewTaskDateCheck1337()//Verified Test #1
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("SC Annual ONA Review Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "SC Annual ONA Review Due"));
                        Assert.Pass();
                    }
                }
                Assert.IsTrue(dateCheckAgainstONAAnnualReviewDate("SC Annual ONA Review Due", "years", client, 1) == true);
            }
        }

        [Test]
        public void B2_ISPDateCheck1314() //Verified Test #2  Keeps sending the same client.
        {
            bool task_already_existed = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("ISP Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "ISP Due"));
                        task_already_existed = true;
                    }
                }
                if (task_already_existed == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("ISP Due", "months", client, 10));
                }
                task_already_existed = false;
            }
        }

        [Test]
        public void C3_InitialASPTaskRemoval1443()//Verified Test #3
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an Initial ASP task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("ASP(Initial)"));
            }
        }

        [Test]
        public void D4_ASPTaskRemoval1434()//Verified Test #4
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an ASP task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("ASP Due"));
                
            }
        }

        [Test]
        public void E5_SISTaskREmoval1354()//Verfied Test #5
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SIS"));
            }
        }

        [Test]
        public void F6_SNAPTaskRemoval1355()//Verified Test #6
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SNAP task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SNAP"));
            }
        }

        [Test]
        public void G7_VocationalDiscoveryTaskRemoval1356() // Verified Test #7
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an Vocational Discovery task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Vocational Discovery"));
            }
        }

        [Test]
        public void H8_PlanOfCareInHomeSupportTaskRemoval1345()//Verified Test #8
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an Initial ANA task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Plan of Care/In-Home Services Due"));
            }

        }

        [Test]
        public void I9_ChildrensInHomeSupportGenPlanTaskRemoval1344() //Verified Test #9
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Children's In-Home Support Gen-Plan"));
            }
        }

        [Test]
        public void J10_SPPCTaskRemoval1359()//Verifed Test #10
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SPPC"));
            }
        }



        [Test]
        public void K11_IEPAgeCheck1358()//Verified Test #11
        {

            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an Initial ASP task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);

                if (Pages.Database.validateInputTaskExists(client,"IEP Attendance Due") == true)
                {
                    string output = Pages.Database.GetClientAge(client);
                    int age = 0;
                    int.TryParse(output, out age);
                    if (age > 21)
                    {
                        Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("IEP Attendance Due"));
                    }
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("IEP Attendance Due"));
                }
            }

        }

        [Test]
        public void L12_HealthCareRepAssignmentTaskRemoval1436() //Verified Test #12
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Health Care Rep Assignment"));
            }
        }

        [Test]
        public void M13_QuarterlySiteVisitDateCheck1339()//Verified Test #13
        {
            //Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Quarterly Site Visit Due"));//Make sure that the task exists to begin with.
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {

                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Quarterly Site Visit Due"))
                    {
                        return;//Tasks already existed, do nothing.
                    }
                }
                Assert.IsTrue(dateCheckAgainstISPStartDate("Quarter Site Visit Due", "months", client, 3));
            }
        }

        [Test]
        public void N14_CaseManagementContactDateCheck1462()//Verified Test #14
        {

            //Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Quarterly Site Visit Due"));//Make sure that the task exists to begin with.
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {

                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Case Management Contact"))
                    {
                        return;//Tasks already existed, do nothing.
                    }
                }
                //Need to do ISP meeting.
                Assert.IsTrue(dateCheckAgainstISPMeetingDate("Case Management Contact", "months", client, 3));
            }
        }

        [Test]
        public void O15_QuarterlyMonitoringTaskRemoval1421()//Verified Test #15
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Quarterly Monitoring"));
            }
        }

        [Test]
        public void P16_AnnualSiteVisitTaskRemoval1420() //Verfied Test #16
        {
            foreach (string client in clients)
            {
                //==========================Verifying that the clients do not have an SIS task===========================================
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Annual Site Visit"));
            }
        }

        [Test]
        public void Q17_MedicalChecklistDAteCheck1346()//Verified Test #17
        {
            bool task_already_existed = false;
            bool date_check_for_due_date = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Medical Checklist Due"))
                    {
                        task_already_existed = true;
                    }
                    else
                    {
                        date_check_for_due_date = dateCheckAgainstLastServeiceDate("Medical Checklist Due", "years", client, 1);
                    }
                }
                if (task_already_existed == false && date_check_for_due_date == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Medical Checklist Due", "years", client, 1));
                }
                task_already_existed = false;
            }

        }
        [Test]
        public void R18_BehavioralChecklistDateCheck1347()//Verified Test #18
        {
            bool task_already_existed = false;
            bool date_check_for_due_date = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Behavioral Checklist Due"))
                    {
                        task_already_existed = true;
                    }
                    else
                    {
                        date_check_for_due_date = dateCheckAgainstLastServeiceDate("Behavioral Checklist Due", "years", client, 1);
                    }
                }
                if(task_already_existed == false && date_check_for_due_date == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Behavioral Checklist Due", "months", client, 10));
                }
                task_already_existed = false;
            }

        }

        [Test]
        public void S19_FirstFinancialChecklistDateCheck1351()//Verified Test #19
        {
            bool task_already_existed = false;
            bool date_check_for_due_date = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("1st Financial Checklist Due"))
                    {
                        task_already_existed = true;
                    }
                    else
                    {
                        date_check_for_due_date = dateCheckAgainstLastServeiceDate("1st Financial Checklist Due", "years", client, 1);
                    }
                }
                if (task_already_existed == false && date_check_for_due_date == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("1st Financial Checklist Due", "months", client, 6));
                }
                task_already_existed = false;
            }

        }

        [Test]
        public void T20_SecondFinancialChecklistDateCheck1350()//Verified Test #20
        {
            bool task_already_existed = false;
            bool date_check_for_due_date = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("2nd Financial Checklist Due"))
                    {
                        task_already_existed = true;
                    }
                    else
                    {
                        date_check_for_due_date = dateCheckAgainstLastServeiceDate("2nd Financial Checklist Due", "years", client, 1);
                    }
                }
                if (task_already_existed == false && date_check_for_due_date == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("2nd Financial Checklist Due", "years", client, 1));
                }
                task_already_existed = false;
            }

        }

        [Test]
        public void U21_ISPChecklistDateCheck1349()//Verified Test #21
        {
            bool task_already_existed = false;
            bool date_check_for_due_date = false;
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("ISP Checklist Due"))
                    {
                        task_already_existed = true;
                    }
                    else
                    {
                        date_check_for_due_date = dateCheckAgainstLastServeiceDate("ISP Checklist Due", "years", client, 1);
                    }
                }
                if (task_already_existed == false && date_check_for_due_date == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("ISP Checklist Due", "years", client, 1));
                }
                task_already_existed = false;
            }

        }

        [Test]
        public void V22_VocationalIspCheclistNoChangeCheck1352() //Verified Test #22
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(DoNothingComparison(client, "Vocational ISP Checklist Due"));
            }
        }

        [Test]
        public void W23_FirstVocationalDsaSiteVisitNoChangeCheck1353() //Verified Test #23
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(DoNothingComparison(client, "1st Vocational/DSA Site Visit Due"));
            }
        }

        [Test]
        public void X24_SecondVocationalDsaSiteVisitNoChangeCheck1540() //Verified Test #24
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(DoNothingComparison(client, "2nd Vocational/DSA Site Visit Due"));
            }
        }

        [Test]
        public void Y25_AssessorONAAgeCheck1487()//Verified Test #25
        {
            bool foundONAInBefore = false;
            foreach(string client in clients)
            {
                string output = Pages.Database.GetClientAge(client);
                int age = 0;
                int.TryParse(output, out age);
                foreach (client_task_and_date beforeClient in clientDataBeforeLivingSitutaionChange)
                {
                    if(beforeClient.taskName.Equals("Assessor ONA DUE"))
                    {
                        Assert.Pass();
                    }
                }
                if(age < 18)
                {
                   /* Not sure what to do here.*/
                }
                else
                {
                    dateCheckAgainstONAAssessorDate("Assessor ONA DUE", "years", client, 5);
                }
            }
        }


    }
}
