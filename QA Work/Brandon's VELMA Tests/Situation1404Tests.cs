using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using OpenQA.Selenium;
using Velma_TestAutomation.Velma_Pages;

namespace Unit_Tests
{
    class Situation1404Tests
    {
        #region variables
        private List<String> clients = new List<string>();
        private List<client_task_and_date> clientDataBeforeLivingSitutaionChange = new List<client_task_and_date>();
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
        #endregion
        #region Setup functions
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
            //BeforeChangeCheckExistanceForISPDue1314();
            ChangeLivingSituationToChildsFosterCare();
        }
        public void ChangeLivingSituationToChildsFosterCare()
        {
            clients = Pages.Database.ServiceType1404top10SkipHiddenClients();
            Hashtable clientTasks = new Hashtable();
            Pages.Dashboard.GoToSearch();
            Pages.Search.FindClient(clients);
            Console.WriteLine("Found clients");
            foreach (string client in clients)
            {
                PopulateClientDataBeforeChange(client);
                ensurelastServiceDatesExist(client);
            }
            Pages.Tasks.FillOutDueDates(ref clientTasks);
            Pages.LivingSituation.GoTo();
            Console.WriteLine("Went to client menu");
            Pages.LivingSituation.EditLivingSituationToChildFosterCare();
        }
        public void PopulateClientDataBeforeChange(string client_id)
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
        public bool lastServicedateCheck(string task, string timeInterval, string client, int timeAmount)
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
                Assert.Fail("ISP Due Date or ISP Date was null.");
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
        public bool DoNothingComparison(string client_id, string input_task)
        {
            //Checks to see if task exists then will compare data from existing task to data from before living situation change.
            if (Pages.Tasks.ValidateInputTaskCleared(input_task) == false)
            {
                foreach (client_task_and_date client in clientDataBeforeLivingSitutaionChange)
                {
                    if (client.lastServiceDate.Equals(Pages.Database.GetInputLastServiceDate(client_id, input_task)) &&
                        client.serviceDateDue.Equals(Pages.Database.GetInputTaskDueDate(client_id, input_task)) &&
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
        public bool IspDateCheck(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] IspDueDate = Pages.Database.GetIspDueDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> ServiceDateAsInt = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in IspDueDate)
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
                Assert.Fail("ISP Due Date or ISP Date was null.");
            }
            return false;
        }

        public void ensurelastServiceDatesExist(string client)
        {
            string[] lastServiceDateSCAnnualONA = Pages.Database.GetInputLastServiceDate(client, "ASP Due").Split('/');
            string[] lastServiceDateISPStartDate = Pages.Database.GetInputLastServiceDate(client, "Annual IEP Due").Split('/');
            string[] lastServiceDateAssessorONA = Pages.Database.GetInputLastServiceDate(client, "Assessor ONA").Split('/');
            string output = Pages.Database.GetClientAge(client);
            int age = 0;
            int.TryParse(output, out age);
            if (lastServiceDateAssessorONA == null || lastServiceDateAssessorONA.Length == 0)
            {
                throw new Exception("The last service date for a client's Assessor ONA task was not found!");
            }
            if (lastServiceDateISPStartDate == null || lastServiceDateISPStartDate.Length == 0)
            {
                throw new Exception("The last service date for a client's ISP Start Date task was not found!");
            }
            if ((lastServiceDateSCAnnualONA == null || lastServiceDateSCAnnualONA.Length == 0) && (age > 18))
            {
                throw new Exception("The last service date for a client's SC Annual ONA task was not found!");
            }
        }
        #endregion

        [Test]
        public void F6_3_IEPAgeCheck()
        {
            foreach (string client in clients)
            {
                string output = Pages.Database.GetClientAge(client);
                int age = 0;
                int.TryParse(output, out age);
                if (age > 21)//If you are over 21, make sure task does not exist.
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("IEP Attendance Due"));
                }
                else if (Pages.Database.validateInputTaskExists(client,"IEP Attendance Due"))//If under 21 make sure the task is unchanged.
                {
                    Assert.IsTrue(DoNothingComparison(client, "IEP Attendance Due"));
                }
                else
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        public void F6_4_ONAAssessorDateTest()
        {
            foreach (string client in clients)
            {
                if (Pages.Database.validateInputTaskExists(client,"Assessor ONA Due") == true)
                {
                    string output = Pages.Database.GetClientAge(client);
                    int age = 0;
                    int.TryParse(output, out age);
                    if (age >= 18)
                    {
                        Assert.IsTrue(dateCheckAgainstONAAssessorDate("Assessor ONA Due", "years", client, 5));
                    }
                    else
                    {
                        //run batch, not sure.
                    }
                }
                else
                {
                    Assert.Pass();
                }
            }
        }

        [Test]
        public void F6_4_1_ConfirmQuarterlySiteVisitCreationAndDueDate()
        {
            foreach(string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Quarterly Site Visit Due"));
                Assert.IsTrue(dateCheckAgainstISPStartDate("Quarterly Site Visit Due", "months",client,5));
            }
        }

        [Test]
        public void F6_4_2_ConfirmMedicalChecklistCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Medical Checklist Due"));
                if(Pages.Database.GetInputLastServiceDate(client, "Medical Checklist Due") !=null)
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("Medical Checklist Due","years",client,1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Medical Checklist Due", "months", client, 5));
                }
            }
        }

        [Test]
        public void F6_4_3_ConfirmBehavioralChecklistCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Behavioral Checklist Due"));
                if (Pages.Database.GetInputLastServiceDate(client, "Behvaioral Checklist Due") != null)
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("Behavioral Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Behavioral Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void F6_4_4_ConfirmFirstFinancialChecklistCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"1st Financial Checklist Due"));
                if (Pages.Database.GetInputLastServiceDate(client, "1st Financial Checklist Due") != null)
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("1st Financial Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("1st Financial Checklist Due", "months", client, 6));
                }
            }
        }

        [Test]
        public void F6_4_5_ConfirmSecondFinancialChecklistCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"2nd Financial Checklist Due"));
                if (Pages.Database.GetInputLastServiceDate(client, "2nd Financial Checklist Due") != null)
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("2nd Financial Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("2nd Financial Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void F6_4_6_ConfirmISPChecklistCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"ISP Checklist Due"));
                if (Pages.Database.GetInputLastServiceDate(client, "ISP Checklist Due") != null)
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("ISP Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("ISP Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void F6_4_7_ConfirmCaseManagementContactCreationAndDueDate()
        {
            foreach (string client in clients)
            {
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Case Management Contact Due"));
                Assert.IsTrue(dateCheckAgainstLastServeiceDate("Case Management Contact Due", "months", client, 3) || dateCheckAgainstISPStartDate("Case Management Contact Due", "months", client, 3));
            }
        }

        [Test]
        public void G7_1_ConfirmSNAPUnchangedOrRemoved()
        {
            foreach (string client in clients)
            {
                if(Pages.Database.validateInputTaskExists(client,"SNAP Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client,"SNAP Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SNAP Due"));
                }
            }
        }

        [Test]
        public void G7_2_ConfirmVocationalISPChecklistUnchangedOrRemoved()
        {
            foreach (string client in clients)
            {
                if (Pages.Database.validateInputTaskExists(client,"Vocational ISP Checklist Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client, "Vocational ISP Checklist Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Vocational ISP Checklist Due"));
                }
            }
        }

        [Test]
        public void G7_3_ConfirmFirstVocationalDSASiteVisitUnchangedOrRemoved()
        {
            foreach (string client in clients)
            {
                if (Pages.Database.validateInputTaskExists(client,"1st Vocational/DSA Site Visit Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client, "1st Vocational/DSA Site Visit Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("1st Vocational/DSA Site Visit Due"));
                }
            }
        }

        [Test]
        public void G7_3_ConfirmSecondVocationalDSASiteVisitUnchangedOrRemoved()
        {
            foreach (string client in clients)
            {
                if (Pages.Database.validateInputTaskExists(client,"2nd Vocational/DSA Site Visit Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client, "2nd Vocational/DSA Site Visit Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("2nd Vocational/DSA Site Visit Due"));
                }
            }
        }
    }
}
