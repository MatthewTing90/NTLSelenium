using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumTest.Automation;
using SeleniumTest.Automation.Shopee;
using System.Threading;
using System.Net.Http;
using IronXL;
using Serilog;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SeleniumTest
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static dbNtlSystemEntities db = new dbNtlSystemEntities();

        static void Main(string[] args)
        {

            //ExecTimer(180);

            // Logging
            WebAutomation automation;
            IWebDriver driver;

            // [Firefox Version]
            FirefoxOptions options = new FirefoxOptions();
            // options.AddArguments("--headless");

            driver = new FirefoxDriver(options);
            Console.Clear();

            // Navigate To Site
            driver.Navigate().GoToUrl("https://seller.shopee.com.my/account/signin");

            automation = new ShopeeLoginAutomation(driver);
            automation.StartProgram();

            // Collect Orders
            driver.Navigate().GoToUrl("https://seller.shopee.com.my/portal/sale/order");

            automation = new ShopeeOrderAutomation(driver);
            automation.StartProgram();

            // while (true)
            // {
            //    // Navigate To Website
            //    driver.Navigate().GoToUrl("https://seller.shopee.com.my/webchat/conversations");

            //    automation = new ShopeeChatAutomation(driver);
            //    automation.StartProgram();
            //    Console.WriteLine();

            //    automation = new ShopeeChatMsgAutomation(driver);
            //    automation.StartProgram();
            //    Console.WriteLine();

            //    Task.Delay(3000).Wait();
            // }

            driver.Close();
            Console.WriteLine("Completed Program!");
        }

        static void ExecTimer(int resetTimer)
        {
            while (resetTimer >= 0)
            {
                Console.WriteLine(TimeHelper.GetTime(resetTimer));
                Task.Delay(1000).Wait();
                resetTimer -= 1;
            }
        }

        static void InsertProduct(WorkBook wb, string name)
        {
            WorkSheet ws = wb.GetWorkSheet(name);

            int uom_id = DbStoredProcedure.GetUomID("cm");

            string rgx = @"ST-(.*)";
            string pName = Regex.Replace(name, rgx, "$1");

            Random rand = new Random();

            foreach (var row in ws.Rows.ToList().GetRange(1, ws.RowCount - 1))
            {
                int qty = pName.Equals("Pre-Cut") ? rand.Next(1, 11) : 10;

                var tmp_arr = row.ToArray();

                string description = (string)tmp_arr[1].Value;
                double width = (double)tmp_arr[2].Value;
                double length = (double)tmp_arr[3].Value;
                string sku = (string)tmp_arr[4].Value;

                Console.WriteLine($"[{pName}] {description} - Width: {width}, Length: {length} - SKU: {sku}");

                TNtlProduct product = new TNtlProduct();
                product.name = $"[{pName}] {description}";
                product.description = $"Width: {width}, Length: {length}";
                product.SKU = sku;
                product.SKU2 = sku;
                product.buy_price = 1;
                product.sell_price = 1;
                product.uom_id = uom_id;
                DbStoredProcedure.ProductInsert(product, "Admin");

                int product_id = DbStoredProcedure.GetID("TNtlProduct");
                int stock_warehouse_id = DbStoredProcedure.GetStockWarehouseID("Shopee");

                TNtlStockItem stockItem = new TNtlStockItem();
                stockItem.name = $"[{pName}] {description}";
                stockItem.description = $"Width: {width}, Length: {length}";
                stockItem.quantity = qty;
                stockItem.product_id = product_id;
                stockItem.stock_warehouse_id = stock_warehouse_id;
                stockItem.uom_id = uom_id;
                DbStoredProcedure.StockItemInsert(stockItem, "Admin");
            }
        }
    }
}
