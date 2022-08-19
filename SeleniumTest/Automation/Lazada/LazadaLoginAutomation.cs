using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SeleniumTest.Automation.Lazada
{
    class LazadaLoginAutomation : WebAutomation
    {
        public LazadaLoginAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor) driver;
            action = new Actions(driver);
        }

        public override void StartProgram()
        {
            // Set up Username & Password
            string username = "0166489466";
            string password = "Arf11234";

            // Dynamically Wait for Website To Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            // Statically Wait for Website To Load
            Task.Delay(3000).Wait();

            // Get Username div
            IWebElement userInput = driver.FindElement(By.CssSelector("input[name='account']"));
            userInput.SendKeys(username);

            // Wait for 1 Second
            Task.Delay(1000).Wait();

            // Get Password div
            IWebElement pswdInput = driver.FindElement(By.CssSelector("input[name='password']"));
            pswdInput.SendKeys(password);

            // Wait for 3 Seconds
            Task.Delay(3000).Wait();

            // Select Button 
            IWebElement loginBtn = driver.FindElement(By.CssSelector(".login-page .login-content button"));
            action.MoveToElement(loginBtn).Click().Perform();
                 
        }
    }
}
