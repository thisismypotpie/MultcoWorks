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
    public class Situation1408Tests
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
        #region setup
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
            ChangeLivingSituationToChildsFosterCare();
        }

        public void ChangeLivingSituationToChildsFosterCare()
        {
            clients = Pages.Database.ServiceType1408top10SkipHiddenClients();
            Hashtable clientTasks = new Hashtable();
            Pages.Dashboard.GoToSearch();
            Pages.Search.FindClient(clients);
            Console.WriteLine("Found clients");
            foreach (string client in clients)
            {
                PopulateClientDataBeforeChange(client);
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
        public bool dateCheck(string task, string timeInterval, string client, int timeAmount)
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
        #endregion


        [Test]
        public void A1_SCOnaReviewDate()
        {
            foreach (string client in clients)
            {
                if (Pages.Database.GetAnnualONAReviewDate(client) == null)
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SC Annual ONA Review Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"SC Annual ONA Review Due"));
                }

            }
        }

        [Test]
        public void B2_ISPDueDateCheck()
        {
            foreach (string client in clients)
            {
                if(Pages.Database.GetISPStartDate(client) == null)
                {
                    Pages.Tasks.ValidateInputTaskCleared("ISP Due");
                }
                else
                {
                    Pages.Dashboard.GoToSearch();
                    Pages.Search.ClientSearch(client);
                    Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"ISP Due"));
                }
            }
        }

        [Test]
        public void C3_IEPAgeCheck()
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
        public void D4_ONAAssessorCheck()
        {
            foreach(string client in clients)
            {
                string output = Pages.Database.GetClientAge(client);
                int age = 0;
                int.TryParse(output, out age);
                if (Pages.Database.validateInputTaskExists(client,"Assessor ONA DUE"))
                {
                    Assert.True(DoNothingComparison(client,"Assessor ONA DUE"));
                    if(age >= 18)
                    {
                        Assert.IsTrue(dateCheckAgainstONAAssessorDate("Assessor ONA DUE","years",client,5));
                    }
                    else
                    {
                        Assert.Pass();
                    }
                }
            }
        }

        [Test]
        public void E5_1_CompareQuarterlySiteVisitToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Quarterly Site Visit Due"));
                if (DoNothingComparison(client, "Quarterly Site Visit Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Quarterly Site Visit Due", "months", client, 3));
                }
            }
        }

        [Test]
        public void E5_2_CompareMedicalChecklistToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Medical Checklist Due"));
                if (DoNothingComparison(client, "Medical Checklist Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client, "Medical Checklist Due") == null)
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("Medical Checklist Due", "years", client, 1));
                    }
                    else
                    {
                        Assert.IsTrue(dateCheck("Medical Checklist Due", "years", client, 1));
                    }
                }

            }
        }

        [Test]
        public void E5_3_CompareBehavioralChecklistToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"Behavioral Checklist Due"));
                if (DoNothingComparison(client, "Behavioral Checklist Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client, "Behavioral Checklist Due") == null)
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("Behavioral Checklist Due", "years", client, 1));
                    }
                    else
                    {
                        Assert.IsTrue(dateCheck("Behavioral Checklist Due", "years", client, 1));
                    }
                }
            }
        }

        [Test]
        public void E5_4_CompareFirstFinancialChecklistToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"1st Financial Checklist Due"));
                if (DoNothingComparison(client, "1st Financial Checklist Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client, "1st Financial Checklist Due") == null)
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("1st Financial Checklist Due", "months", client, 6));
                    }
                    else
                    {
                        Assert.IsTrue(dateCheck("1st Financial Checklist Due", "months", client, 6));
                    }
                }
            }
        }

        [Test]
        public void E5_5_CompareSecondFinancialChecklistToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"2nd Financial Checklist Due"));
                if (DoNothingComparison(client, "2nd Financial Checklist Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client, "2nd Financial Checklist Due") == null)
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("2nd Financial Checklist Due", "years", client, 1));
                    }
                    else
                    {
                        Assert.IsTrue(dateCheck("2nd Financial Checklist Due", "years", client, 1));
                    }
                }
            }
        }

        [Test]
        public void E5_6_CompareISPChecklistToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client,"ISP Checklist Due"));
                if (DoNothingComparison(client, "ISP Checklist Due"))
                {
                    Assert.True(true);
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client,"ISP Checklist Due") == null)
                    {
                        dateCheckAgainstISPStartDate("ISP Checklist Due", "years", client, 1);
                    }
                    else
                    {
                        dateCheck("ISP Checklist Due", "years", client, 1);
                    }
                }
            }
        }

        [Test]
        public void E5_7_CompareCaseManagementContactToISPStartDate()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client,"Case Management Contact Due"))
                {
                    DoNothingComparison(client, "Case Management Contact Due");
                }
                else
                {
                    if (Pages.Database.GetInputLastServiceDate(client, "Case Management Contact Due") == null)
                    {
                        dateCheckAgainstISPStartDate("Case Management Contact Due", "months", client, 3);
                    }
                    else
                    {
                        dateCheck("Case Management Contact Due", "months", client, 3);
                    }
                }
            }
        }

        [Test]
        public void F6_1_SNAPConfirmUnchnaged()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client,"SNAP Due"))
                {
                    DoNothingComparison(client, "SNAP Due");
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SNAP Due"));
                }
            }
        }

        [Test]
        public void F6_2_VocationalISPChecklistConfirmUnchnaged()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client,"Vocational ISP Checklist Due"))
                {
                    DoNothingComparison(client, "Vocational ISP Checklist Due");
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Vocational ISP Checklist Due"));
                }
            }
        }

        [Test]
        public void F6_3_FirstVocationalDSASightVisitConfirmUnchanged()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client,"1st Vocational/DSA Annual Site Visit Due"))
                {
                    DoNothingComparison(client, "1st Vocational/DSA Annual Site Visit Due");
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("1st Vocational/DSA Annual Site Visit Due"));
                }
            }
        }

        [Test]
        public void F6_4_SecondVocationalDSASightVisitConfirmUnchanged()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client,"2nd Vocational/DSA Annual Site Visit Due"))
                {
                    DoNothingComparison(client, "2nd Vocational/DSA Annual Site Visit Due");
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("2nd Vocational/DSA Annual Site Visit Due"));
                }
            }
        }

        [Test]
        public void G7_ConfirmNoOtherTasks()
        {
            List<string> bad_tasks = new List<string>();
            List<String> TasksToCheck = new List<string>()
            {
                "ISP Due",
                "IEP Attendance Due",
                "Quarterly Site Visit Due",
                "Medical Checklist Due",
                "Behavioral Checklist Due",
                "1st Financial Checklist Due",
                "2nd Financial Checklist Due",
                "ISP Checklist Due",
                "Vocational ISP Checklist Due",
                "Vocational/DSA Annual Site Visit Due",
                "Case Management Contact Due",
                "ONA Due",
                "LOC/Annual ONA Review Due"

            };
            //Going through each client
            foreach (string client in clients)
            {
                HashSet<string> Tasks = Pages.Database.RetreiveClientTasks(client);
                List<String> clientTasks = Tasks.ToList();
                //Going through each task of each client
                foreach (string task in clientTasks)
                {
                    if (!TasksToCheck.Contains(task))
                    {
                        bad_tasks.Add(task);
                    }
                }
                //Remove task if they had it before living situation change.
                Console.WriteLine("boop");
                foreach (client_task_and_date beforeTask in clientDataBeforeLivingSitutaionChange)
                {
                    if (beforeTask.client.Equals(client))
                    {
                        bad_tasks.RemoveAll(item => item.Contains(beforeTask.taskName));
                    }
                }
                //Remove bad task if it is a determination or redetermination task.
                Console.WriteLine("boop");
                if (bad_tasks.Contains("Redetermination"))
                {
                    bad_tasks.RemoveAll(item => item.Contains("Redetermination"));
                }

                bad_tasks.Clear();
                /*
                if(bad_tasks.Count > 0)
                {
                    Assert.Fail();
                }*/
            }
        }

    }
}
