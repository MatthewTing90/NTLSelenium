using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeShipmentAutomation : WebAutomation
    {
        public ShopeeShipmentAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);
        }

        public override void StartProgram()
        {
            Console.WriteLine("Start Mass Ship all orders....");

            CheckShipmentPage();
        }

        public void CheckShipmentPage()
        {
            // Dynamically Wait for Elements To Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);

            // Statically Wait
            Task.Delay(1000).Wait();

            // Find "To Ship" WebElement
            IWebElement ToShipElem = driver.FindElement(By.CssSelector(".shopee-tabs__nav-tab:nth-child(3)"));

            // Check if "To Ship" has a number or not
            // Solution: Check if "tips-meta" Element Exist or Not
            List<IWebElement> ToShipNoti = ToShipElem.FindElements(By.CssSelector(".tab-badge")).ToList();

            // If it Exist, Select the "Mass Shipment" button
            if(ToShipNoti.Count > 0)
            {
                // Select The "Mass Shipment" button
                IWebElement massShipBtn = driver.FindElement(By.CssSelector(".ship-btn"));
                action.MoveToElement(massShipBtn).Click().Perform();

                // Enter "Mass Shipment" Page
                MassShipmentPage();
            }
        }

        public void MassShipmentPage()
        {
            // Dynamically Wait for Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);

            // Statically Wait for Elements to Load
            Task.Delay(1000).Wait();

            // Get List of Available Carriers
            List<IWebElement> carrierList = driver.FindElements(By.CssSelector(".shopee-radio-button")).ToList();

            // Alternative: Filter Carrier List, Then Loop
            carrierList = carrierList.Where(carrier => {

                string rgx = @".*(\(.*\)\s+)?\(\s(\d+)\s\)?";

                // List Out Carrier Name
                string carrierTxt = carrier.Text;

                // Find The Carrier With Number - Represent Number of Orders to Ship
                return Regex.IsMatch(carrierTxt, rgx);

            }).ToList();

            foreach (var carrier in carrierList)
            {
                // Find The Carrier With Number - Represent Number of Orders to Ship
                // Select the Carrier
                action.MoveToElement(carrier).Click().Perform();

                // Dynamically Wait for Web Element to Load
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                // Statically Wait for Web Element to Load
                Task.Delay(1000).Wait();

                // Select the Top Table Header ( Maybe Check if its disabled? )
                // Alternative: Execute Javascript
                string jsScript = "document.querySelector(`.order-list-table-header input[type='checkbox']`).click();";
                js.ExecuteScript(jsScript);

                js.ExecuteScript("console.log(`Hello World`)");

                // Wait For Mass Shipment Button to be active
                Task.Delay(2000).Wait();

                // Select Mass Shipment Button
                IWebElement massDropBtn = driver.FindElement(By.CssSelector(".shopee-button--outline"));
                action.MoveToElement(massDropBtn).Click().Perform();

                // Complete Shipment Page
                CompleteShipmentPage();
            }
        }

        public void CompleteShipmentPage()
        {
            // Dynamically Wait for Web Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            // Statically Wait for Web Elements to Load
            Task.Delay(1500).Wait();

            // Select "Confirm" Button
            IWebElement confirmBtn = driver.FindElement(By.CssSelector(".mass-action-panel .shopee-button--primary"));
            action.MoveToElement(confirmBtn).Click().Perform();

            // Wait for 3 Seconds
            Task.Delay(3000).Wait();
        }
    }
}
