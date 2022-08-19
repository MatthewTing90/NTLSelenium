using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SeleniumTest.Automation
{
    class ExampleAutomation : WebAutomation
    {
        public ExampleAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
        }

        public override void StartProgram()
        {
            // Navigate To Link
            driver.Navigate().GoToUrl("https://phptravels.com/demo/");

            // Maximizes the browser window
            driver.Manage().Window.FullScreen();

            // Set Profile Info
            string first_name = "Justin";
            string last_name = "Tan";
            string business_name = "Ivend DB";
            string email_address = "bahici6475@shbiso.com";

            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Web Components
            IWebElement fName_input = driver.FindElement(By.CssSelector(".form input[name='first_name']"));
            IWebElement lName_input = driver.FindElement(By.CssSelector(".form input[name='last_name']"));
            IWebElement bName_input = driver.FindElement(By.CssSelector(".form input[name='business_name']"));
            IWebElement email_input = driver.FindElement(By.CssSelector(".form input[name='email']"));

            // Fill In Form
            Task.Delay(1000).Wait();
            fName_input.SendKeys(first_name);

            Task.Delay(1000).Wait();
            lName_input.SendKeys(last_name);

            Task.Delay(1000).Wait();
            bName_input.SendKeys(business_name);

            Task.Delay(1000).Wait();
            email_input.SendKeys(email_address);

            // Select Submit Button
            IWebElement submit_btn = driver.FindElement(By.CssSelector(".form button"));

            Task.Delay(1000).Wait();
            action.MoveToElement(submit_btn).Click().Perform();

            // Wait for 10 Seconds
            // Static Wait
            Task.Delay(5000).Wait();

            IWebElement completed_div = driver.FindElement(By.CssSelector(".completed"));

            if (completed_div.GetCssValue("display").Equals("block"))
            {
                // Complete Program
                EndProgram("Automation has completed!");

                // Wait for 3 Seconds
                Task.Delay(3000).Wait();
            }
        }
    }
}
