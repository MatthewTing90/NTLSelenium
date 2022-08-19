using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Serilog;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeLoginAutomation : WebAutomation
    {
        public ShopeeLoginAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
            ntlLog = new TNtlSeleniumLog();
        }

        TNtlSeleniumLog ntlLog;

        public override void StartProgram()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/LoginLog.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            ntlLog.log_name = $"LoginLog{DateTime.Today.ToString("yyyyMMdd")}.txt";
            ntlLog.start_date = DateTime.Today;
            ntlLog.end_date = DateTime.Today;
            ntlLog.status = "Online";
            ntlLog.type = 1;

            // Get Shopee Platform
            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");
            ntlLog.platform_id = platform_id;

            DbStoredProcedure.SeleniumLogInsert(ntlLog);

            // Dynamically Wait For Web Page to Load for 5 Minutes
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);

            Log.Information("Now Starting Shopee Login Automation!");
            
            // Set Username, Password
            string username = "01110261628";
            string password = "NTLasia123$";

            // Fill In Login Form
            IWebElement usernameInput = driver.FindElement(By.CssSelector("input[placeholder='Email/Phone/Username']"));
            Task.Delay(1000).Wait();
            usernameInput.SendKeys(username);

            IWebElement passwordInput = driver.FindElement(By.CssSelector("input[placeholder='Password']"));
            Task.Delay(1000).Wait();
            passwordInput.SendKeys(password);

            Log.Information($"Successfully enter Username: {username} and Password: {password}!");

            // Wait for 10 Seconds
            Task.Delay(5000).Wait();

            // Select Login Button
            IWebElement loginBtn = driver.FindElement(By.CssSelector(".shopee-button--block"));
            action.MoveToElement(loginBtn).Click().Perform();

            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);

            // Wait for Chat Button Loaded
            IWebElement chatBtn = driver.FindElement(By.CssSelector("div[id='shopee-mini-chat-embedded']"));

            Log.Information("Successfully login to seller.shopee.com!");

            Log.Information("Ended Shopee Login Automation!");

            DbStoredProcedure.SeleniumLogUpdate(ntlLog);

            // Wait for 10 Seconds
            Task.Delay(5000).Wait();
        }
    }
}
