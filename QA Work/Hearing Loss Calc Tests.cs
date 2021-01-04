using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace HearingLossCalculatorTests
{
    public class Tests
    {
        IWebDriver driver;
        IWebElement OneThousandHzLeft;
        IWebElement OneThousandHzRight;
        IWebElement TwoThousandHzLeft;
        IWebElement TwoThousandHzRight;
        IWebElement ThreeThousandHzLeft;
        IWebElement ThreeThousandHzRight;
        IWebElement FourThousandHzLeft;
        IWebElement FourThousandHzRight;
        IWebElement SpeechDiscLeft;
        IWebElement SpeechDiscRight;
        IWebElement CalculateButton;
        IWebElement ResetButton;
        IWebElement SummaryBox;
        List<IWebDriver> instances = new List<IWebDriver>();

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver(@"");//Removed for security reasons.
            driver.Url = "";//removed for security reasons.
            OneThousandHzLeft = driver.FindElement(By.Id("HearingThresholds_0__SoundLevelInDbForLeftEar"));
            OneThousandHzRight = driver.FindElement(By.Id("HearingThresholds_0__SoundLevelInDbForRightEar"));
            TwoThousandHzLeft = driver.FindElement(By.Id("HearingThresholds_1__SoundLevelInDbForLeftEar"));
            TwoThousandHzRight = driver.FindElement(By.Id("HearingThresholds_1__SoundLevelInDbForRightEar"));
            ThreeThousandHzLeft = driver.FindElement(By.Id("HearingThresholds_2__SoundLevelInDbForLeftEar"));
            ThreeThousandHzRight = driver.FindElement(By.Id("HearingThresholds_2__SoundLevelInDbForRightEar"));
            FourThousandHzLeft = driver.FindElement(By.Id("HearingThresholds_3__SoundLevelInDbForLeftEar"));
            FourThousandHzRight = driver.FindElement(By.Id("HearingThresholds_3__SoundLevelInDbForRightEar"));
            SpeechDiscLeft = driver.FindElement(By.Id("SpeechDiscriminationScoreForLeftEar"));
            SpeechDiscRight = driver.FindElement(By.Id("SpeechDiscriminationScoreForRightEar"));
            CalculateButton = driver.FindElement(By.ClassName("btn-primary"));
            ResetButton = driver.FindElement(By.ClassName("btn-secondary"));

            driver.Navigate();

            //Add driver to instances of open windows then close the oldest if there are too many.
            instances.Add(driver);
            checkWindows();
        }


        public void clearInputs()
        {
            OneThousandHzLeft.Clear();
            OneThousandHzRight.Clear();
            TwoThousandHzLeft.Clear();
            TwoThousandHzRight.Clear();
            ThreeThousandHzLeft.Clear();
            ThreeThousandHzRight.Clear();
            FourThousandHzLeft.Clear();
            FourThousandHzRight.Clear();
            SpeechDiscLeft.Clear();
            SpeechDiscRight.Clear();
        }

        public void sendInputs(int oneLeft, int oneRight, int twoLeft, int twoRight, int threeLeft
            , int threeRight, int fourLeft, int fourRight, int discLeft, int discRight)
        {
            OneThousandHzLeft.SendKeys(oneLeft.ToString());
            OneThousandHzRight.SendKeys(oneRight.ToString());
            TwoThousandHzLeft.SendKeys(twoLeft.ToString());
            TwoThousandHzRight.SendKeys(twoRight.ToString());
            ThreeThousandHzLeft.SendKeys(threeLeft.ToString());
            ThreeThousandHzRight.SendKeys(threeRight.ToString());
            FourThousandHzLeft.SendKeys(fourLeft.ToString());
            FourThousandHzRight.SendKeys(fourRight.ToString());
            if(discLeft != -1)
            {
                SpeechDiscLeft.SendKeys(discLeft.ToString());
            }
            if(discRight != -1)
            {
                SpeechDiscRight.SendKeys(discRight.ToString());
            }
        }

        public int parseResultCount()
        {
            /*List<String> results = new List<string>();
            foreach(IWebElement current in driver.FindElements(By.ClassName("badge")))
            {
                if(!current.GetAttribute("class").Contains("badge-warning"))
                {
                    results.Add(current.Text);
                }
            }*/
            int results = 0;
            foreach (IWebElement current in driver.FindElements(By.ClassName("badge")))
            {
                if (!current.GetAttribute("class").Contains("badge-warning"))
                {
                    results++;
                }
            }
            return results;
        }

        public List<string> parseWarnings()
        {
            List<string> warnings = new List<string>();
            foreach (IWebElement current in driver.FindElements(By.ClassName("bg-warning")))
            {
                    warnings.Add(current.Text);
            }
            foreach (IWebElement current in driver.FindElements(By.ClassName("badge-warning")))
            {
                warnings.Add(current.Text);
            }
            return warnings;
        }

        public string getTableVIIResults()
        {
            try
            {
                SummaryBox = driver.FindElement(By.ClassName("bg-info"));
                string wholeText = SummaryBox.Text;
                string Line = wholeText.Split("\r\n")[3];
                return Line.Split('=')[1];
            }
            catch(Exception e)
            {
                return "none";
            }
        }

        //If there are more than five windows open, delete the oldest one.
        public void checkWindows()
        {
            if(instances.Count >= 3)//<-- you can change this number to have as many instances as you want.
            {
                instances[0].Close();
                instances.RemoveAt(0);
            }
        }

        [Test]
        public void TestClient1()
        {
            //set inputs for calculator
            clearInputs();
            sendInputs(5,10,10,15,15,10,20,15,96,100);
            CalculateButton.Click();
            //get results and warnings
            int resultCount = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(resultCount == 0);
            Assert.IsTrue(warnings.Count == 1);
            //warning text
            Assert.IsTrue(warnings[0].Equals("Sorry...Neither ear is service connected"));
            Assert.IsTrue(getTableVIIResults().Equals("none"));
        }

        [Test]
        public void TestClient2()
        {
            //set inputs for calculator
            clearInputs();
            sendInputs(10,20,15,15,15,20,25,40,96,96);
            CalculateButton.Click();
            //get results and warnings
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 1);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text== "Avg. Db Loss 16");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 24");
            //warning text
            Assert.IsTrue(warnings[0].Equals("NOT s/c"));
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0 (LEFT ear is NOT s/c)"));
           
        }
        [Test]
        public void TestClient3()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(30,30,45,40,60,60,70,70,92,92);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 51");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 50");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0"));

        }
        
        [Test]
        public void TestClient4()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(55, 55, 65, 60, 100, 70, 90, 80, 72, 84);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # VII");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 78");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # V");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 66");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 30"));

        }

        [Test]
        public void TestClient5()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(70,65,85,80,100,95,100,100,28,48);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # XI");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 89");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # IX");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 85");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 80"));
        }

        [Test]
        public void TestClient6()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(25,25,70,60,90,95,100,100,72,68);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # VII");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 71");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # VI");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 70");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 30"));
        }

        [Test]
        public void TestClient7()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(5,5,10,15,15,15,20,20,96,96);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  0);
            Assert.IsTrue(warnings.Count == 1);
            Assert.IsTrue(warnings[0].Equals("Sorry...Neither ear is service connected"));
            Assert.IsTrue(getTableVIIResults().Equals("none"));
        }

        [Test]
        public void TestClient8()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(35,30,55,50,75,65,80,70,96,92);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # II");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 61");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 54");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0"));
        }

        [Test]
        public void TestClient9()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(20,20,30,35,45,45,55,55,100,96);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 38");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 39");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0"));
        }

        [Test]
        public void TestClient10()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(45,55,65,50,85,80,110,100,72,72);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results ==  6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # VI");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 76");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # VI");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 71");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 30"));
        }

        [Test]
        public void TestClient11()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(26,25,10,25,26,25,26,25,96,94);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 1);
            Assert.Fail();//There is an error on examples for customer 11. 
        }

        [Test]
        public void TestClient12()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(27,25,52,25,43,25,39,25,93,62);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 40");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # V");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 25");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0"));
        }

        [Test]
        public void TestClient13()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(39,40,26,22,23,32,22,19,98,100);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 1);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 28");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # I");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 28");
            //warning text
            Assert.IsTrue(warnings[0].Equals("NOT s/c"));
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 0 (LEFT ear is NOT s/c)"));
        }

        [Test]
        public void TestClient14()
        {
            //set inputs for calculator.
            clearInputs();
            sendInputs(92,25,98,60,100,95,100,100,-1,-1);
            CalculateButton.Click();
            //get results and warnings.
            int results = parseResultCount();
            List<string> warnings = parseWarnings();
            //count check
            Assert.IsTrue(results == 6);
            Assert.IsTrue(warnings.Count == 0);
            //left ear
            Assert.IsTrue(driver.FindElement(By.Id("IsLeftBetterOrPoorerBadge")).Text == "Poorer");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarRomanNumberBadge")).Text == "Roman # X");
            Assert.IsTrue(driver.FindElement(By.Id("LeftEarAverageDbLossBadge")).Text == "Avg. Db Loss 98");
            //right ear
            Assert.IsTrue(driver.FindElement(By.Id("IsRightBetterOrPoorerBadge")).Text == "Better");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarRomanNumberBadge")).Text == "Roman # VI");
            Assert.IsTrue(driver.FindElement(By.Id("RightEarAverageDbLossBadge")).Text == "Avg. Db Loss 70");
            //table VII results
            Assert.IsTrue(getTableVIIResults().Equals(" 50"));
        }
    }
}