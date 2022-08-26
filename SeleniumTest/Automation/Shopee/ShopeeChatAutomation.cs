using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Serilog;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeChatAutomation : WebAutomation
    {
        public ShopeeChatAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
        }

        dbNtlSystemEntities db = new dbNtlSystemEntities();

        TNtlSeleniumLog ntlLog;
        public override void StartProgram()
        {
            // Author: Matthew Ting
            // Date: 2022-08-26
            // Description: For Chat Downtime Report
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.File("logs/OrderLog.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();

            ntlLog.log_name = $"ChatLog{DateTime.Today.ToString("yyyyMMdd")}.txt";
            ntlLog.start_date = DateTime.Today;
            ntlLog.end_date = DateTime.Today;
            ntlLog.status = "Offline";
            ntlLog.type = 2;

            // Get Shopee Platform
            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");
            ntlLog.platform_id = platform_id;

            DbStoredProcedure.SeleniumChatDowntimeInsert(ntlLog);
            ntlLog.id = DbStoredProcedure.GetID("TNtlSeleniumLog");


            // To Remove
            Log.Information("Now Starting Shopee Chat Automation Program");

            // Maximize Window
            driver.Manage().Window.Maximize();

            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Select "All"
            IWebElement popupElem = driver.FindElement(By.CssSelector("div[class='_1FJHgSaCGF']"));
            action.MoveToElement(popupElem).Click().Perform();

            Task.Delay(500).Wait();

            // Select "All" in Popup Modal
            IWebElement modalElem = driver.FindElement(By.CssSelector("div[class='_3cyrFIvvEl']:nth-child(1)"));
            action.MoveToElement(modalElem).Click().Perform();

            Task.Delay(500).Wait();

            Log.Information("Getting List of Chat Message with Notification");

            // Get List of Chat Elements with Notification
            if (driver.FindElements(By.XPath("//div[@class='_2UghmBs3lm']/../../..")).Count > 0)
            {
                List<IWebElement> userChatDivArr = driver.FindElements(By.XPath("//div[@class='_2UghmBs3lm']/../../..")).ToList();
                userChatDivArr.ForEach(elem => getChatMsg(elem));
            }

            Log.Information("Ended Shopee Chat Automation Program");


            ntlLog.end_date = DateTime.Today;
            DbStoredProcedure.SeleniumChatDowntimeUpdate(ntlLog);
        }

        public void getChatMsg(IWebElement elem)
        {
            string username = "admin";

            // Get Number of Notification
            IWebElement notiDiv = elem.FindElement(By.XPath(".//.//.//div[@class='_2UghmBs3lm']"));
            int number_of_notification = Convert.ToInt32(notiDiv.Text);

            // Get Customer Name
            IWebElement customerNameDiv = elem.FindElement(By.XPath(".//.//.//.//div[@class='i8Vttj4zbA']"));
            string customer_name = customerNameDiv.Text;

            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");

            // Get Customer
            var customer = db.TNtlCustomers.FirstOrDefault(it => it.name.Equals(customer_name));
            if (customer == null)
            {
                customer = new TNtlCustomer();
                customer.name = customer_name;
                customer.platform_id = platform_id;
                customer.email_address = "";
                customer.phone_number = "";
                customer.address = "";

                DbStoredProcedure.CustomerInsert(customer, username);

                customer.id = DbStoredProcedure.GetID("TNtlCustomer");
            }

            // To Remove
            Log.Information($"Customer {customer_name} - Platform Shopee - Notifications {number_of_notification}");

            int customer_id = customer.id;

            // Click on Div
            action.MoveToElement(elem).Click().Perform();

            // Wait for 3 Seconds
            Task.Delay(1500).Wait();

            // Wait for 5 Seconds
            Task.Delay(1500).Wait();

            // Get List of Chat Messages
            List<IWebElement> msgArr = driver.FindElements(By.CssSelector(".wdzNs0Fsxa4AgiJ7EztJg")).ToList();

            // Slice List to Get Last 5
            msgArr = msgArr.GetRange(msgArr.Count - number_of_notification, number_of_notification);

            int unr_sta_id = DbStoredProcedure.GetStatusID("unread", "Platform Message");

            string msgType = "";

            int ind = 1;
            foreach (var msgElem in msgArr)
            {
                // Get Message
                IWebElement msgChildElem = msgElem.FindElement(By.CssSelector("._32ZAsOEAJDT5HNehjDfHgo"));

                string scrollDownScript = $"document.querySelector(\".wdzNs0Fsxa4AgiJ7EztJg:nth-child({ind})\").scrollIntoView()";
                js.ExecuteScript(scrollDownScript);

                string html = msgChildElem.GetAttribute("innerHTML");

                string msg = "";

                bool flag = false;

                if (html.Contains("img"))
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                    msgChildElem = msgChildElem.FindElement(By.XPath(".//div[contains(@class, '_4uQ7ljL1sog-SxKNsGgva')]//img"));

                    msgType = "Image";
                    msg = msgChildElem.GetAttribute("src");

                    flag = true;
                }
                else if (html.Contains("pre"))
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                    msgChildElem = msgChildElem.FindElement(By.XPath(".//pre"));

                    msgType = "Text";
                    msg = msgChildElem.Text;

                    flag = true;
                }

                if (flag)
                {
                    // Get Time & Date
                    IWebElement dateTimeElem = msgElem.FindElement(By.CssSelector("._2MF9IAvjD0A2hQZ7AZQpQ4"));
                    string dateTimeStr = dateTimeElem.Text;
                    dateTimeStr = dateTimeStr.Replace("Today", DateTime.Today.ToString("d") + ",");
                    DateTime dt = Convert.ToDateTime(dateTimeStr);

                    TNtlCustomerChat chatItem = new TNtlCustomerChat();
                    chatItem.customer_id = customer_id;
                    chatItem.message = msg;
                    chatItem.msg_type = msgType;
                    chatItem.sender_type = "client";
                    chatItem.created_date = dt;
                    chatItem.status_id = unr_sta_id;
                    chatItem.platform_id = platform_id;

                    DbStoredProcedure.CustomerChatInsert(chatItem);

                    // To Remove
                    Log.Information($"Message: {msg} - Type: {msgType} - Date: {dt}");
                }

                Task.Delay(1000).Wait();
                ind++;
            }

            // Select "All"
            IWebElement popupElem = driver.FindElement(By.CssSelector("div[class='_1FJHgSaCGF']"));
            action.MoveToElement(popupElem).Click().Perform();

            Task.Delay(500).Wait();

            // Select "UnRead" in Popup Modal
            IWebElement modalElem = driver.FindElement(By.CssSelector("div[class='_3cyrFIvvEl']:nth-child(2)"));
            action.MoveToElement(modalElem).Click().Perform();

            Task.Delay(500).Wait();

            // To Remove
            Console.WriteLine();

        }
    }
}
