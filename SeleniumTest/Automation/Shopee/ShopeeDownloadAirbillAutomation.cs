using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeDownloadAirbillAutomation : WebAutomation
    {
        public ShopeeDownloadAirbillAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
        }

        public override void StartProgram()
        {
            Console.WriteLine("Start Download All Airbill PDF....");

            DownloadAirbill();
        }

        public void DownloadAirbill()
        {
            // Initialize List of Orders
            List<string> orderList = new List<string>()
            {
                "2208240Y0N6UMN", "2208241F9YTQ5T"
            };

            // Dynamically Wait for Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);

            // Statically Wait for Elements to Load
            Task.Delay(1500).Wait();

            // Wait for Btn Skip
            if (driver.FindElements(By.CssSelector(".btn-skip")).Count > 0)
            {
                IWebElement btnSkip = driver.FindElements(By.CssSelector(".btn-skip")).ElementAt(0);
                action.MoveToElement(btnSkip).Click().Perform();

                Task.Delay(3000).Wait();
            }

            // Get All Order Elem
            List<IWebElement> orderElemList = driver.FindElements(By.CssSelector("a[class='order-item']")).ToList();

            // Initialize List of Empty Order Css
            List<string> orderCssList = new List<string>();

            for(int i = 1; i < orderElemList.Count + 1; i++)
            {
                string cssClass = $"a[class='order-item']:nth-child({i})";

                IWebElement orderIdElem = driver.FindElement(By.CssSelector($"{cssClass} .orderid"));
                string orderId = orderIdElem.Text.Trim();

                if(orderList.Contains(orderId))
                {
                    orderCssList.Add(cssClass);
                }
            }

            orderCssList.ForEach(css =>
            {
                DownloadAirbillDetail(css);

                // Wait For 3 Seconds
                Task.Delay(1500).Wait();
            });

        }

        public void DownloadAirbillDetail(string order_css)
        {

        }
    }
}
