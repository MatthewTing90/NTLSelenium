using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest.Automation
{
    abstract class WebAutomation
    {
        public WebAutomation(IWebDriver driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor) driver;
            action = new Actions(driver);
        }

        public IWebDriver driver { get; set; }
        public IJavaScriptExecutor js { get; set; }
        public Actions action { get; set; }

        public abstract void StartProgram();

        public void EndProgram(string alertMsg) {
            // Complete Program
            js.ExecuteScript($"alert('{alertMsg}');");
        }

        public void CloseProgram() {
            driver.Quit();
        }

        // Typing
        public void SendKeys(IWebElement elem, string msg)
        {
            string[] arr = msg.Split(' ');
            foreach(var str in arr)
            {
                elem.SendKeys(str + " ");
                Task.Delay(500).Wait();
            }
        }
    }
}
