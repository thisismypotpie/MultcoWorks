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
    class Situation1409Tests
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
            clients = Pages.Database.ServiceType1409top10SkipHiddenClients();
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

        public bool dateCheckAgainstInputDate(int taskID, string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] inputDate = Pages.Database.getInputDate(taskID.ToString(),client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> InputDueDate = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in inputDate)
                {
                    int.TryParse(toInt, out toAdd);
                    InputDueDate.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(InputDueDate[2], InputDueDate[0], InputDueDate[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public void ensurelastServiceDatesExist(string client)
        {
            string[] lastServiceDateSCAnnualONA = Pages.Database.GetInputLastServiceDate(client, "SC Annual ONA").Split('/');
            string[] lastServiceDateISPStartDate = Pages.Database.GetInputLastServiceDate(client, "ISP Start Date").Split('/');
            string[] lastServiceDateAssessorONA = Pages.Database.GetInputLastServiceDate(client, "Assessor ONA").Split('/');
            string output = Pages.Database.GetClientAge(client);
            int age = 0;
            int.TryParse(output, out age);
            if (lastServiceDateAssessorONA == null || lastServiceDateAssessorONA.Length == 0)
            {
                throw new Exception("The last service date for a client's Assessor ONA task was not found!");
            }
            if(lastServiceDateISPStartDate == null || lastServiceDateISPStartDate.Length == 0)
            {
                throw new Exception("The last service date for a client's ISP Start Date task was not found!");
            }
            if((lastServiceDateSCAnnualONA == null || lastServiceDateSCAnnualONA.Length == 0) &&(age>18))
            {
                throw new Exception("The last service date for a client's SC Annual ONA task was not found!");
            }
        }
        #endregion

        [Test]
        public void D4_1_ConfirmSCAnnualONAReviewTaskExists()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("SC Annual ONA Review Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "SC Annual ONA Review Due"));

            }
        }

        [Test]
        public void D4_2_ConfirmISPDueTasksExists()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("ISP Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "SC Annual ONA Review Due"));
            }
        }

        [Test]
        public void D4_3_ConfirmAssessorONATaskExists()
        {
            foreach (string client in clients)
            {
                string output = Pages.Database.GetClientAge(client);
                int age = 0;
                int.TryParse(output, out age);
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Assessor ONA Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Assessor ONA Due"));
                if (age >= 18)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1487,"Assessor ONA Due", "years", client, 5));
                }
            }
        }

        [Test]
        public void E5_1_ConfirmQuarterlySiteVisitUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Quarterly Site Visit Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "Quarterly Site Visit Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Quarterly Site Visit Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Quarterly Site Visit Due"));
                    }
                }
                Assert.IsTrue(dateCheckAgainstInputDate(1524,"Quarterly Site Visit Due", "months", client, 3));
            }
        }

        [Test]
        public void E5_2_ConfirmMedicalChecklistUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Medical Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "Medical Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Medical Checklist Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Medical Checklist Due"));
                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "Medical Checklist Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"Medical Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("Medical Checklist Due", "years",client,1));
                }
            }
        }

        [Test]
        public void E5_3_ConfirmBehavioralChecklistUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Behavioral Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "Behavioral Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Behavioral Checklist Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Behavioral Checklist Due"));

                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "Behavioral Checklist Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"Behavioral Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("Behavioral Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void E5_4_ConfirmFirstFinancialChecklistUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("1st Financial Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "1st Financial Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("1st Financial Checklist Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "1st Financial Checklist Due"));


                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "1st Financial Checklist Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"1st Financial Checklist Due", "months", client, 6));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("1st Financial Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void E5_5_ConfirmSecondFinancialChecklistUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("2nd Financial Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "2nd Financial Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("2nd Financial Checklist Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "2nd Financial Checklist Due"));
                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "2nd Financial Checklist Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"2nd Financial Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("2nd Financial Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void E5_6_ConfirmISPChecklistUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("ISP Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "ISP Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("ISP Checklist Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "ISP Checklist Due"));
                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "ISP Checklist Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"ISP Checklist Due", "years", client, 1));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("ISP Checklist Due", "years", client, 1));
                }
            }
        }

        [Test]
        public void E5_7_ConfirmCaseManagementContactUnchangedOrNewlyCreated()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Case Management Contact Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "Case Management Contact Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        Pages.Tasks.refresh_page();
                        System.Threading.Thread.Sleep(5000);
                        //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Case Management Contact Due"));
                        Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Case Management Contact Du"));

                    }
                }
                string[] lastServiceDate = Pages.Database.GetInputLastServiceDate(client, "Case Management Contact Due").Split('/');
                if (lastServiceDate == null || lastServiceDate.Length == 0)
                {
                    Assert.IsTrue(dateCheckAgainstInputDate(1524,"Case Management Contact Due", "months", client, 3));
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstLastServeiceDate("Case Management Contact Due", "months", client, 3));
                }
            }
        }

        [Test]
        public void F6_1_ConfirmSNAPUnchangedOrNonExistant()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("SNAP Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "SNAP Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SNAP Due"));
                    }
                }
            }
        }

        [Test]
        public void F6_2_ConfirmVocationISPUnchangedOrNonExistant()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("Vocational ISP Checklist Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "Vocational ISP Checklist Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Vocational ISP Checklist Due"));
                    }
                }
            }
        }

        [Test]
        public void F6_3_ConfirmFirstVocationalDSASiteVisitUnchangedOrNonExistant()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("1st Vocational/DSA Site Visit Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "1st Vocational/DSA Site Visit Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("1st Vocational/DSA Site Visit Due"));
                    }
                }
            }
        }

        [Test]
        public void F6_4_ConfirmSecondVocationalDSASiteVisitUnchangedOrNonExistant()
        {
            foreach (string client in clients)
            {
                foreach (client_task_and_date beforeTasks in clientDataBeforeLivingSitutaionChange)//Checking to see if tasks exists before the change.
                {
                    if (beforeTasks.client.Equals(client) && beforeTasks.taskName.Equals("2nd Vocational/DSA Site Visit Due"))
                    {
                        Assert.IsTrue(DoNothingComparison(client, "2nd Vocational/DSA Site Visit Due"));
                        //Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("2nd Vocational/DSA Site Visit Due"));
                    }
                }
            }
        }
    }
       
}
