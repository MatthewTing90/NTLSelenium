using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using Serilog;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeChatMsgAutomation : WebAutomation
    {
        public ShopeeChatMsgAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
        }

        dbNtlSystemEntities db = new dbNtlSystemEntities();

        public override void StartProgram()
        {

            // Select "All"
            IWebElement popupElem = driver.FindElement(By.CssSelector("div[class='_1FJHgSaCGF']"));
            action.MoveToElement(popupElem).Click().Perform();

            Task.Delay(500).Wait();

            // Select "All" in Popup Modal
            IWebElement modalElem = driver.FindElement(By.CssSelector("div[class='_3cyrFIvvEl']:nth-child(1)"));
            action.MoveToElement(modalElem).Click().Perform();

            Task.Delay(500).Wait();

            // To Remove
            Log.Information("Now Starting Shopee Chat Msg Automation Program");

            int uns_sta_id = DbStoredProcedure.GetStatusID("unsend", "Platform Message");
            int s_sta_id = DbStoredProcedure.GetStatusID("send", "Platform Message");

            Log.Information("Searching for Unsend messages in database");

            var unsendChatList = db.TNtlCustomerChats.Where(it => it.status_id == uns_sta_id).ToList();

            unsendChatList.ForEach(it =>
            {
                var customer = db.TNtlCustomers.Find(it.customer_id);
                SendChatMsg(customer.name, it.message);

                // Update Send Status To Complete
                it.status_id = s_sta_id;
                DbStoredProcedure.CustomerChatUpdate(it);
            });
            
            // Select "All"
            IWebElement popupElem2 = driver.FindElement(By.CssSelector("div[class='_1FJHgSaCGF']"));
            action.MoveToElement(popupElem2).Click().Perform();

            Task.Delay(500).Wait();

            // Select "UnRead" in Popup Modal
            IWebElement modalElem2 = driver.FindElement(By.CssSelector("div[class='_3cyrFIvvEl']:nth-child(2)"));
            action.MoveToElement(modalElem2).Click().Perform();

            Task.Delay(500).Wait();

            Log.Information("Ended Shopee Chat Msg Automation Program");
        }

        public void SendChatMsg(string username, string msg)
        {
            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(1);

            // Get List of User Elements
            List<IWebElement> userChatDivArr = driver.FindElements(By.CssSelector("._29OqseoAF4")).ToList();

            // Get Elements by username
            Dictionary<string, IWebElement> usrElemDict = new Dictionary<string, IWebElement>();

            // Get List of Chat Elements
            List<IWebElement> usrChatDivArr = driver.FindElements(By.CssSelector("._29OqseoAF4 ")).ToList();

            // Get List of Username
            List<string> usrNameArr = driver.FindElements(By.CssSelector(".i8Vttj4zbA")).Select(elem => elem.Text).ToList();

            for (int i = 0; i < usrChatDivArr.Count; i++)
            {
                IWebElement elem = usrChatDivArr.ElementAt(i);
                string usrName = usrNameArr.ElementAt(i);
                usrElemDict.Add(usrName, elem);
            }

            IWebElement usrChatDiv = usrElemDict[username];

            // Check To see if got Notification
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            List<IWebElement> notiElemList = usrChatDiv.FindElements(By.CssSelector("._2UghmBs3lm")).ToList();

            if (notiElemList.Count > 0) {
                getChatMsg(usrChatDiv);
            }

            // Click on Div
            action.MoveToElement(usrChatDiv).Click().Perform();

            // Wait for 3 Seconds
            Task.Delay(2000).Wait();

            // Scroll Down To Latest Message
            string scrollDownScript = "document.querySelector(`#messageSection .ReactVirtualized__List`).scrollBy({ top: +document.querySelector(`#messageSection .ReactVirtualized__Grid__innerScrollContainer`).style.height.slice(0, -2), left: 0, behavior: 'smooth' }); ";
            js.ExecuteScript(scrollDownScript);

            // Select Text Area
            IWebElement txtArea = driver.FindElement(By.CssSelector("textarea"));

            Log.Information($"Message: {msg} - Date Time: {(new DateTime()).ToString()}");

            // Wait 3 seconds
            SendKeys(txtArea, msg);

            // Wait 3 Seconds
            Task.Delay(2000).Wait();

            // Click Send
            IWebElement sendBtn = driver.FindElement(By.CssSelector("._1nHakyuDrjrbCFMmkRDvd8 ._3kEAcT1Mk5"));
            action.MoveToElement(sendBtn).Click().Perform();

            // Scroll Down Again
            js.ExecuteScript(scrollDownScript);
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

            // To Remove
            Console.WriteLine();

        }
    }
}
