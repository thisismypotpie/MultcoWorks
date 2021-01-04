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
    class Situation1403Tests
    {
        #region variables
        private List<String> clients = new List<string>();
        private List<client_task_and_date> clientDataBeforeLivingSitutaionChange = new List<client_task_and_date>();
        private IWebDriver driver = Pages.Tasks.getBrowser();
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
        public bool dateCheckAgainstAnnualIEPDate(string task, string timeInterval, string client, int timeAmount)
        {
            try
            {
                string[] AnnualIEPDate = Pages.Database.GetAnnualIEPDate(client).Split('/');
                string[] DueDate = Pages.Database.GetInputTaskDueDate(client, task).Split('/');
                List<int> AnnualIEPDateAsInt = new List<int>();
                List<int> DueDateAsInt = new List<int>();
                int toAdd = 0;
                foreach (string toInt in AnnualIEPDate)
                {
                    int.TryParse(toInt, out toAdd);
                    AnnualIEPDateAsInt.Add(toAdd);
                }
                foreach (string toInt in DueDate)
                {
                    int.TryParse(toInt, out toAdd);
                    DueDateAsInt.Add(toAdd);
                }

                return Pages.Tasks.ValidateDate(timeInterval, new DateTime(AnnualIEPDateAsInt[2], AnnualIEPDateAsInt[0], AnnualIEPDateAsInt[1]), new DateTime(DueDateAsInt[2], DueDateAsInt[0], DueDateAsInt[1]), timeAmount);
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
            string[] lastServiceDateSCAnnualONA = Pages.Database.GetInputLastServiceDate(client, "SC Annual ONA Review Date").Split('/');
            string[] lastServiceDateISPStartDate = Pages.Database.GetInputLastServiceDate(client, "ISP Start Date").Split('/');
            string[] lastServiceDateONA = Pages.Database.GetInputLastServiceDate(client, "Last ONA Date").Split('/');
            string[] lastServiceDateAnnualIEP = Pages.Database.GetInputLastServiceDate(client, "Last Annual IEP Date").Split('/');
            string output = Pages.Database.GetClientAge(client);
            int age = 0;
            int.TryParse(output, out age);
            if (lastServiceDateONA == null || lastServiceDateONA.Length == 0)
            {
                throw new Exception("The last service date for a client's ONA task was not found!");
            }
            if (lastServiceDateISPStartDate == null || lastServiceDateISPStartDate.Length == 0)
            {
                throw new Exception("The last service date for a client's ISP Start Date task was not found!");
            }
            if ((lastServiceDateSCAnnualONA == null || lastServiceDateSCAnnualONA.Length == 0) && (age > 18))
            {
                throw new Exception("The last service date for a client's SC Annual ONA task was not found!");
            }
            if ((lastServiceDateSCAnnualONA == null || lastServiceDateSCAnnualONA.Length == 0) && (age > 18))
            {
                throw new Exception("The last service date for a client's SC Annual ONA task was not found!");
            }
        }
        #endregion

        [Test]
        public void C3_1_ConfirmAnnualOnaReviewExists()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("SC Annual ONE Review Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "SC Annual ONE Review Due"));
            }
        }

        [Test]
        public void C3_2_ConfirmISPExists()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("ISP Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "ISP Due"));

            }
        }

        [Test]
        public void C3_3_ConfirmCorrectIEPPlacement()
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
        public void C3_4_ConfirmCorrectONAAssessorPlacement()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Assessor ONA Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Assessor ONA Due"));
                string output = Pages.Database.GetClientAge(client);
                int age = 0;
                int.TryParse(output, out age);
                if (age > 17)
                {
                    Assert.IsTrue(dateCheckAgainstONAAssessorDate("Assessor ONA Due", "years", client, 5));
                }
            }
        }

        [Test]
        public void C3_5_1_CheckStateOfQuarterlySiteVisit()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Quarterly Site Visit"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Quarterly Site Visit"));
                if (DoNothingComparison(client, "Quarterly Site Visit") == false)
                {
                    Assert.IsTrue(dateCheckAgainstISPStartDate("Quarterly Site Visit", "months", client, 3));
                }
            }
        }

        [Test]
        public void C3_5_2_CheckStateOfMedicalChecklist()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Medical Checklist"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Medical Checklist"));
                if (DoNothingComparison(client, "Medical Checklist") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("Medical Checklist", "years", client, 1))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("Medical Checklist", "years", client, 1));
                    }
                }
            }
        }

        [Test]
        public void C3_5_3_CheckStateOfBehavioralChecklist()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Behavioral Checklist"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Behavioral Checklist"));
                if (DoNothingComparison(client, "Behavioral Checklist") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("Behavioral Checklist", "years", client, 1))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("Behavioral Checklist", "years", client, 1));
                    }
                }
            }
        }
        [Test]
        public void C3_5_4_CheckStateOfFirstFinancialChecklist()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("1st Financial Checklist"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "1st Financial Checklist"));
                if (DoNothingComparison(client, "1st Financial Checklist") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("1st Financial Checklist", "years", client, 1))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("1st Financial Checklist", "months", client, 6));
                    }
                }
            }
        }

        [Test]
        public void C3_5_5_CheckStateOfSecondFinancialChecklist()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("ISP Checklist"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "ISP Checklist"));
                if (DoNothingComparison(client, "ISP Checklist") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("ISP Checklist", "years", client, 1))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("ISP Checklist", "years", client, 1));
                    }
                }
            }
        }

        [Test]
        public void C3_5_6_CheckStateOfISPChecklist()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("ISP Checklist"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "ISP Checklist"));
                if (DoNothingComparison(client, "ISP Checklist") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("ISP Checklist", "years", client, 1))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("ISP Checklist", "years", client, 1));
                    }
                }
            }
        }

        [Test]
        public void C3_5_7_CheckStateOfCaseManagementContact()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Case Management Contact"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Case Management Contact"));
                if (DoNothingComparison(client, "Case Management Contact") == false)
                {
                    if (dateCheckAgainstLastServeiceDate("Case Management Contact", "months", client, 3))
                    {
                        Assert.Pass();
                    }
                    else
                    {
                        Assert.IsTrue(dateCheckAgainstISPStartDate("Case Management Contact", "months", client, 3));
                    }
                }
            }
        }

        [Test]
        public void C3_6_1_ConfirmSNAPErased()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client, "SNAP Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client, "SNAP Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("SNAP Due"));
                }
            }
        }

        [Test]
        public void C3_6_2_ConfirmVocationalISPChecklistErased()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client, "Vocational ISP Checklist Due"))
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
        public void C3_6_3_ConfirmFirstVocationalDSASiteVisitErased()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client, "1st Vocational/DSA Site Visit Due"))
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
        public void C3_6_4_ConfirmVocationalDSASiteVisitErased()
        {
            foreach (string client in clients)
            {
                Pages.Dashboard.GoToSearch();
                Pages.Search.ClientSearch(client);
                if (Pages.Database.validateInputTaskExists(client, "2nd Vocational/DSA Site Visit Due"))
                {
                    Assert.IsTrue(DoNothingComparison(client, "2nd Vocational/DSA Site Visit Due"));
                }
                else
                {
                    Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("2nd Vocational/DSA Site Visit Due"));
                }
            }
        }

        [Test]
        public void D4_AddAndCheckANADate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("AnaDate"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "ANA Due"));
            }
        }

        [Test]
        public void I9_AddAndCheckCaseManagementDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("CaseManagementContact"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("Case Management Contact Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "Case Management Contact Due"));

            }
        }

        [Test]
        public void J10_AddAndCheckAnnualIEPDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("AnnualIepDate"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                //Assert.IsTrue(Pages.Tasks.ValidateInputTaskExists("IEP Attendance Due"));
                Assert.IsTrue(Pages.Database.validateInputTaskExists(client, "IEP Attendance Due"));
                Assert.IsTrue(dateCheckAgainstAnnualIEPDate("IEP ATTendance Due","years",client,1));
            }
        }

        [Test]
        public void K11_AddAndCheckHealthCareRepDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("HealthCareRepAssignmentDate"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Health Care Rep Assignment Due"));
            }
        }

        [Test]
        public void L12_AddAndCheckVocationalISPChecklistDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("VocationalIspChecklist"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Vocational ISP Checklist Due"));
                Assert.IsTrue(dateCheckAgainstLastServeiceDate("Vocational ISP Checklist Due", "years", client, 1));
            }
        }

        [Test]
        public void M13_AddandCheckFirstVocationalDSASiteVisitDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("VocationalDsaAnnualSiteVisit"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("1st Vocational/DSA Site Visit Due"));
                Assert.IsTrue(dateCheckAgainstLastServeiceDate("1st Vocational/DSA Site Visit Due", "years", client, 1));
            }
        }

        [Test]
        public void N14_AddandCheckSecondVocationalDSASiteVisitDate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("SecondVocationalDSAAnnualSiteVisitDate"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("2nd Vocational/DSA Site Visit Due"));
                Assert.IsTrue(dateCheckAgainstLastServeiceDate("2nd Vocational/DSA Site Visit Due", "years", client, 1));
            }
        }

        [Test]
        public void O15_AddAndCheckAssessorONADate()
        {
            foreach (string client in clients)
            {
                Pages.Services.GoToServices();
                IWebElement DateElement = driver.FindElement(By.Id("OnaDate"));
                IWebElement SaveButton = driver.FindElement(By.Id("saveButton"));
                DateElement.SendKeys("01-01-2030");
                SaveButton.Click();
                System.Threading.Thread.Sleep(5000);
                Pages.Tasks.GoToTasks();
                Assert.IsTrue(Pages.Tasks.ValidateInputTaskCleared("Assessor ONA Due"));
                int age = 0;
                string output = Pages.Database.GetClientAge(client);
                int.TryParse(output, out age);
                if (age > 17)
                {
                    Assert.IsTrue(dateCheckAgainstONAAssessorDate("Assessor ONA Due", "years", client, 5));
                }
            }
        }
    }
}
