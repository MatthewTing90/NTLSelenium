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
            string rgx = "";

            // Dynamically Wait for Elements To Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            Task.Delay(3000).Wait();

            // Wait for Btn Skip
            if (driver.FindElements(By.CssSelector(".btn-skip")).Count > 0)
            {
                IWebElement btnSkip = driver.FindElements(By.CssSelector(".btn-skip")).ElementAt(0);
                action.MoveToElement(btnSkip).Click().Perform();

                Task.Delay(3000).Wait();
            }

            // Statically Wait
            Task.Delay(1000).Wait();

            Console.WriteLine("After Btn Skip...");

            IWebElement shipmentTabBtn = driver.FindElement(By.CssSelector("div[class='shopee-tabs__nav-tab']:nth-child(3)"));

            IWebElement shipmentTabLabelElem = shipmentTabBtn.FindElement(By.CssSelector(".tab-label"));

            // Format Text To Remove Inner Whitespace
            string shipmentTabLabel = shipmentTabLabelElem.Text;
            rgx = @"(To ship)\s*(\d+)";

            string numOfShipmentStr = Regex.Replace(shipmentTabLabel, rgx, "$2");

            int numOfShip = (numOfShipmentStr.Equals("")) ? 0 : Convert.ToInt32(numOfShipmentStr);

            Console.WriteLine($"Number of Shipment: {numOfShip}");

            // Click Shipment Tab Button
            action.MoveToElement(shipmentTabBtn).Click().Perform();

            // Statically Wait For Web Elements to Load
            Task.Delay(1500).Wait();

            // If it Exist, Select the "Mass Shipment" button
            if (numOfShip > 0)
            {
                // Select The "Mass Shipment" button
                IWebElement massShipBtn = driver.FindElement(By.CssSelector(".ship-btn"));
                action.MoveToElement(massShipBtn).Click().Perform();

                // Click Skip in Mass Shipment Page
                Task.Delay(1500).Wait();

                // Wait for Btn Skip
                if (driver.FindElements(By.CssSelector(".btn-skip")).Count > 0)
                {
                    IWebElement btnSkip = driver.FindElements(By.CssSelector(".btn-skip")).ElementAt(0);
                    action.MoveToElement(btnSkip).Click().Perform();

                    Task.Delay(3000).Wait();
                }

                // Enter "Mass Shipment" Page
                MassShipmentPage();
            }
        }

        public void MassShipmentPage()
        {
            // Dynamically Wait for Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);

            // Statically Wait for Elements to Load
            Task.Delay(1500).Wait();

            // Get List of Available Carriers
            List<IWebElement> carrierList = driver.FindElements(By.CssSelector(".channels-selector .shopee-radio-button")).ToList();

            Console.WriteLine("Getting List of Carrier With Order");

            // Initialize List Of Empty Available Carrier CSS
            List<string> carrierCssList = new List<string>();

            for (int i = 2; i < carrierList.Count + 2; i++)
            {
                string cssClass = $".channels-selector .shopee-radio-button:nth-child({i})";
                string rgx = @".*(\(.*\)\s+)?\(\s(\d+)\s\)?";

                // List Out Carrier Name
                IWebElement carrierLabel = driver.FindElement(By.CssSelector($"{cssClass} .shopee-radio-button__label"));
                string carrierTxt = carrierLabel.Text;

                // Find The Carrier With Number - Represent Number of Orders to Ship
                if (Regex.IsMatch(carrierTxt, rgx))
                {
                    carrierCssList.Add(cssClass);
                }
            }

            carrierCssList.ForEach(carrierCss =>
            {
                MassShipmentPageDetail(carrierCss);

                // Wait For 3 Seconds
                Task.Delay(1500).Wait();
            });
        }

        public void MassShipmentPageDetail(string cssClass)
        {
            // Navigate To Order Page
            driver.Navigate().GoToUrl("http://localhost:5500/Shopee/order/portal/sale/mass/ship/");

            // Wait For 3 Seconds
            Task.Delay(1500).Wait();

            // Dynamically Wait for Web Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            // Get Web Element For Carrier Label
            IWebElement carrierElem = driver.FindElement(By.CssSelector(cssClass));

            // Find The Carrier With Number - Represent Number of Orders to Ship
            // Select the Carrier
            action.MoveToElement(carrierElem).Click().Perform();

            // Dynamically Wait for Web Element to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            // Statically Wait for Web Element to Load
            Task.Delay(1000).Wait();

            // Select the Top Table Header ( Maybe Check if its disabled? )
            // Alternative: Execute Javascript
            string jsScript = "document.querySelector(`.order-list-table-header input[type='checkbox']`).click();";
            js.ExecuteScript(jsScript);

            // Wait For Mass Shipment Button to be active
            Task.Delay(2000).Wait();

            // Select Mass Shipment Button
            IWebElement massDropBtn = driver.FindElement(By.CssSelector(".shopee-button--outline"));
            action.MoveToElement(massDropBtn).Click().Perform();

            // Complete Shipment Page
            CompleteShipmentPage();

            // Wait For 3 Seconds
            Task.Delay(1500).Wait();
        }

        public void CompleteShipmentPage()
        {
            // Dynamically Wait for Web Elements to Load
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            // Statically Wait for Web Elements to Load
            Task.Delay(1500).Wait();

            // Select "Confirm" Button
            IWebElement confirmBtn = driver.FindElement(By.CssSelector(".mass-action-panel .shopee-button--primary"));

            Console.WriteLine("Now Confirming Shipment Order...");

            action.MoveToElement(confirmBtn).Click().Perform();

            // Statically Wait for Web Elements to Load
            Task.Delay(1500).Wait();

            // Select "Collapse" Div
            IWebElement collapseDiv = driver.FindElement(By.CssSelector(".collapse"));

            Console.WriteLine("Closed Shipment Popup Modal");

            action.MoveToElement(collapseDiv).Click().Perform();
        }
    }
}
