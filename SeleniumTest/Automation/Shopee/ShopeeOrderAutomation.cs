using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using Serilog;
using System.Net.Http;
using Newtonsoft.Json;

namespace SeleniumTest.Automation.Shopee
{
    class ShopeeOrderAutomation : WebAutomation
    {
        private static readonly HttpClient client = new HttpClient();

        public ShopeeOrderAutomation(IWebDriver driver) : base(driver)
        {
            this.driver = driver;
            js = (IJavaScriptExecutor)driver;
            action = new Actions(driver);

            db = new dbNtlSystemEntities();
            ntlLog = new TNtlSeleniumLog();
        }

        Dictionary<string, string> orderDict;
        dbNtlSystemEntities db;
        TNtlSeleniumLog ntlLog;
        string _latestOrderId;

        // Need to Learn How to Continue From Last Existing Orders
        // Continue Based on Order Name / Code / External Ref No
        public override void StartProgram()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/OrderLog.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            ntlLog.log_name = $"OrderLog{DateTime.Today.ToString("yyyyMMdd")}.txt";
            ntlLog.start_date = DateTime.Today;
            ntlLog.end_date = DateTime.Today;
            ntlLog.status = "Offline";
            ntlLog.type = 2;

            // Get Shopee Platform
            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");
            ntlLog.platform_id = platform_id;

            DbStoredProcedure.SeleniumLogInsert(ntlLog);
            ntlLog.id = DbStoredProcedure.GetID("TNtlSeleniumLog");

            Log.Information("Now Starting Shopee Order Automation Program");

            int numOfActual = ShopeeOrders();

            //// Go Back To Shopee Orders
            //driver.Navigate().GoToUrl("https://seller.shopee.com.my/portal/sale/order");

            //int numOfCheck = CheckShopeeOrders();

            //Log.Information($"Actual: {numOfActual}, Check: {numOfCheck}");

            //if(numOfActual == numOfCheck)
            //{
            //    ntlLog.status = "Online";
            //}

            Log.Information("Ended Shopee Order Automation Program");

            ntlLog.end_date = DateTime.Today;
            DbStoredProcedure.SeleniumLogUpdate(ntlLog);
        }

        public int ShopeeOrders()
        {
            int numOfLines = 0;
            string rgx = "";
            // Maximizes the Browser Window
            // driver.Manage().Window.FullScreen();

            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            Task.Delay(3000).Wait();

            // Wait for Btn Skip
            if (driver.FindElements(By.CssSelector(".btn-skip")).Count > 0)
            {
                Log.Information("Now locating Skip Button in Order Page");

                IWebElement btnSkip = driver.FindElements(By.CssSelector(".btn-skip")).ElementAt(0);
                action.MoveToElement(btnSkip).Click().Perform();

                Task.Delay(3000).Wait();

                Log.Information("Successfully Clicked Skip Button");
            }

            Log.Information("Creating Order Dictionary"); numOfLines += 1;
            Log.Information("Capturing Order ID and Their Respective Links"); numOfLines += 1;
            Log.Information("Storing In Order Dicitonary with Order ID as Key and Link as Value"); numOfLines += 1;

            /// Web Components
            // Get Order IDs and Order Links [Note: Id And Link are different Value]
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            Log.Information("Finding 'To Ship' Navigation Tab"); numOfLines += 1;

            // Select "To Ship"
            IWebElement shipmentTabBtn = driver.FindElement(By.CssSelector("div[class='shopee-tabs__nav-tab']:nth-child(3)"));

            IWebElement shipmentTabLabelElem = shipmentTabBtn.FindElement(By.CssSelector(".tab-label"));

            // Format Text To Remove Inner Whitespace
            string shipmentTabLabel = shipmentTabLabelElem.Text;
            rgx = @"(To Ship)\s*(\d+)";

            string numOfShipment = Regex.Replace(shipmentTabLabel, rgx, "$2");

            if(numOfShipment.Equals(""))
            {
                return numOfLines;
            }

            // Click the Shipment Tab
            action.MoveToElement(shipmentTabBtn).Click().Perform();
            Log.Information("Selecting 'To Ship' Navigation Tab");

            // Wait for 3 Seconds
            Task.Delay(1000).Wait();

            List<IWebElement> orderIdElemArr = driver.FindElements(By.CssSelector(".order-sn span")).ToList();
            List<IWebElement> orderLinkElemArr = driver.FindElements(By.CssSelector("a[href*='portal/sale/order/']")).ToList();

            // Get List of Order Ids
            List<string> orderIdArr = orderIdElemArr.Select(it => it.Text.Split(' ')[2].Trim()).ToList();

            // Reverse Order ID
            orderIdArr.Reverse();

            int orderIntId = DbStoredProcedure.GetID("TNtlOrder");
            var latestOrder = db.TNtlOrders.FirstOrDefault(it => it.id == orderIntId);

            Log.Information("Getting Latest NTL Order ID From Database"); numOfLines += 1;

            string latestOrderId = (latestOrder == null) ? "21100661X6BWB4" : latestOrder.name;
            _latestOrderId = latestOrderId;

            Log.Information($"Matching Latest NTL Order ID {latestOrderId} With Latest Shopee Order ID {orderIdArr[0]}"); numOfLines += 1;

            // Slice Order Id Arr
            if (!latestOrderId.Equals(""))
            {
                int tmpId = orderIdArr.IndexOf(latestOrderId);
                orderIdArr = orderIdArr.GetRange(tmpId + 1, orderIdArr.Count - 1 - tmpId);
            }

            int numOfOrders = orderIdArr.Count;

            // Initialize Dictionary
            orderDict = new Dictionary<string, string>();

            for (int i = 0; i < numOfOrders; i++)
            {
                string order_id = orderIdElemArr.ElementAt(i).Text.Split(' ')[2].Trim();
                string order_link = orderLinkElemArr.ElementAt(i).GetAttribute("href");
                orderDict.Add(order_id, order_link);
            }

            // Loop Through Order Id
            orderIdArr.ForEach(it =>
            {
                numOfLines += CollectOrderDetail(it);
            });

            return numOfLines;
        }

        public int CollectOrderDetail(string order_id_str)
        {
            int numOfLines = 0;
            Log.Information($"Navigating To Order {order_id_str}, Link: {orderDict[order_id_str]}"); numOfLines += 1;

            // For Regex Purpose
            string rgx = "";
            string _width, _height;

            // Navigate To Link
            string link = orderDict[order_id_str];
            driver.Navigate().GoToUrl(link);

            Log.Information($"Successfully Enter Order {order_id_str}, Link: {orderDict[order_id_str]}"); numOfLines += 1;

            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);

            // Get Created Date
            IWebElement createdDateElem = driver.FindElement(By.CssSelector(".od-log:last-child .time"));
            DateTime createdDt = Convert.ToDateTime(createdDateElem.Text); // Convert Date String to DateTime

            // Get Platform ID
            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");

            // Get Customer Name
            List<IWebElement> custElemArr = driver.FindElements(By.CssSelector(".card-style:nth-child(3) .section:nth-child(2) .body div")).ToList();
            string custName = custElemArr[0].Text.Split(',')[0].Trim();
            string custPhoneNum = custElemArr[0].Text.Split(',')[1].Trim();
            string custAddress = custElemArr[1].Text.Trim();

            var customer = db.TNtlCustomers.FirstOrDefault(it => it.name.Equals(custName));
            if (customer == null)
            {
                Log.Information($"Customer {custName} is not found in NTL Database! Now creating new Customer in NTL Database....");
                customer = new TNtlCustomer();
                customer.name = custName;
                customer.phone_number = custPhoneNum;
                customer.email_address = "";
                customer.address = custAddress;
                customer.platform_id = platform_id;
                DbStoredProcedure.CustomerInsert(customer, "Selenium");

                customer.id = DbStoredProcedure.GetID("TNtlCustomer");
            }

            int inc_sta_id = DbStoredProcedure.GetStatusID("draft", "Order");
            int c_sta_id = DbStoredProcedure.GetStatusID("sale", "Order");

            string username = "Admin";

            TNtlOrder order = new TNtlOrder();

            order.name = order_id_str;
            order.code = link.Split('/')[6];
            order.created_date = createdDt;
            order.customer_id = customer.id;
            order.tax_price = 0;
            order.discount_fee = 0;
            order.sub_total_price = 0;
            order.total_price = 0;

            order.odoo_status_id = inc_sta_id;
            order.status_id = inc_sta_id;

            order.created_by = customer.name;

            order.last_updated_by = "Selenium";
            order.last_updated_date = createdDt;

            DbStoredProcedure.OrderInsert(order, username);

            // Latest Order Id
            int order_id = DbStoredProcedure.GetID("TNtlOrder");

            Log.Information($"Successfully created Order {order.name} in NTL Database"); numOfLines += 1;

            // To Remove
            Log.Information($"Order: {order.name} - Created Date: {order.created_date} - Customer: {custName} - Total Price: {order.total_price}"); numOfLines += 1;

            // Collect Order Detail
            // Get By Row Rather than Individual Elements

            // Get List of Products
            List<IWebElement> productElemArr = driver.FindElements(By.CssSelector(".product-item")).ToList();

            productElemArr = productElemArr.GetRange(1, productElemArr.Count - 1); // Remove 1st Product item

            Log.Information("Order Items: "); numOfLines += 1;

            for(int i = 2; i < productElemArr.Count + 2; i++)
            {
                string cssClass = $".product-list > div:nth-child({i})";

                // Name
                IWebElement nameElem = driver.FindElement(By.CssSelector($"{cssClass} .product-name"));
                string productName = nameElem.Text;

                // Unit Price
                IWebElement priceElem = driver.FindElement(By.CssSelector($"{cssClass} .price"));
                decimal unitPrice = Convert.ToDecimal(priceElem.Text.Trim());

                // Quantity
                IWebElement qtyElem = driver.FindElement(By.CssSelector($"{cssClass} .qty"));
                decimal qty = Convert.ToDecimal(qtyElem.Text.Trim());

                // SKU
                IWebElement skuElem = driver.FindElement(By.CssSelector($"{cssClass} .product-meta"));
                string sku = skuElem.Text.Trim();

                rgx = @"(.|\n)*?SKU:\s+(.*)";
                sku = Regex.Replace(sku, rgx, "$2");

                var product = db.TNtlProducts.FirstOrDefault(it => it.SKU.Equals(sku));

                if (product == null)
                {
                    product = db.TNtlProducts.FirstOrDefault(it => it.SKU.Equals("default"));
                }

                TNtlOrderItem orderItem = new TNtlOrderItem();

                orderItem.order_id = order_id;
                orderItem.product_id = product.id;

                orderItem.name = productName;

                // From ProductSKU, get Width and Height
                rgx = @"[A-Za-z]+(\d{3})(\d{3})?";

                _width = Regex.Replace(sku, rgx, "$1");
                _height = Regex.Replace(sku, rgx, "$2");

                decimal width = (_width.Equals("")) ? 100 : Convert.ToDecimal(_width);
                decimal height = (_height.Equals("")) ? 100 : Convert.ToDecimal(_height);

                orderItem.unit_price = unitPrice;
                orderItem.quantity = qty;
                orderItem.sub_total_price = unitPrice * qty;
                orderItem.discount_fee = 0;
                orderItem.tax_price = 0;
                orderItem.total_price = orderItem.sub_total_price - orderItem.discount_fee - orderItem.tax_price;
                orderItem.uom_id = product.uom_id;

                sku = (sku.Equals("")) ? "defaultxx" : sku;
                orderItem.sku = sku;

                // Set Remark
                orderItem.remark = $"[{sku}] {width}cm|{height}cm|{qty}";

                // Set Total Usage
                orderItem.total_usage = height * width * qty / 100 / 100;

                DbStoredProcedure.OrderItemInsert(orderItem, username);

                // To Remove
                Log.Information($"Product {productName} - SKU {sku} - Quantity {qty} - Unit Price {unitPrice} - Sub Total Price {orderItem.sub_total_price}"); numOfLines += 1;
            }

            Log.Information($"Successfully Added All Order Items with Order ID {order_id_str} into NTL Database"); numOfLines += 1;

            // Update Order Total Price
            var totalPriceList = db.TNtlOrderItems.Where(it => it.order_id == order_id).Select(it => it.total_price).ToList();

            order = db.TNtlOrders.FirstOrDefault(it => it.id == order_id);

            order.status_id = c_sta_id;
            order.sub_total_price = totalPriceList.Sum();
            order.total_price = order.sub_total_price - order.tax_price - order.discount_fee;

            DbStoredProcedure.OrderUpdate(order, username);
            Log.Information("Successfully updated Order Price"); numOfLines += 1;

            Log.Information("Now Sending Order to Odoo using POST API"); numOfLines += 1;

            mOdooCustomer model = new mOdooCustomer();
            model.customer = "Shopee";

            List<mOdooProduct> productList = new List<mOdooProduct>();
            db.TNtlOrderItems.Where(it => it.order_id == order.id).ToList().ForEach(it =>
            {

                string sku = it.sku;

                sku = (sku.Equals("defaultxx")) ? "" : sku;

                rgx = @"[A-Za-z]+(\d{3})(\d{3})?";

                _width = Regex.Replace(sku, rgx, "$1");
                _height = Regex.Replace(sku, rgx, "$2");

                int iWidth = (_width.Equals("")) ? 100 : Convert.ToInt32(_width);
                int iHeight = (_height.Equals("")) ? 100 : Convert.ToInt32(_height);

                mOdooProduct _model = new mOdooProduct(it.sku, (int)it.quantity, iWidth, iHeight);
                productList.Add(_model);
            });

            model.product = productList;

            // Generate Quotation
            string json = JsonConvert.SerializeObject(model);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = client.PostAsync("http://localhost:8069/odoo_controller/addSO", content).Result;

            Log.Information($"Successfully send order data to Odoo! Result: {json}"); numOfLines += 1;

            // Wait for 5 Seconds
            Task.Delay(5000).Wait();

            return numOfLines;
        }

        public int CheckShopeeOrders()
        {
            int numOfLines = 0;
            string rgx = "";
            // Maximizes the Browser Window
            // driver.Manage().Window.FullScreen();
        
            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        
            Task.Delay(3000).Wait();
        
            // Wait for Btn Skip
            if (driver.FindElements(By.CssSelector(".btn-skip")).Count > 0)
            {
                Console.WriteLine("Now locating Skip Button in Order Page");
        
                IWebElement btnSkip = driver.FindElements(By.CssSelector(".btn-skip")).ElementAt(0);
                action.MoveToElement(btnSkip).Click().Perform();
        
                Task.Delay(3000).Wait();
        
                Console.WriteLine("Successfully Clicked Skip Button");
            }
        
            Console.WriteLine("Creating Order Dictionary"); numOfLines+=1;
            Console.WriteLine("Capturing Order ID and Their Respective Links"); numOfLines+=1;
            Console.WriteLine("Storing In Order Dicitonary with Order ID as Key and Link as Value"); numOfLines+=1;
            
            /// Web Components
            // Get Order IDs and Order Links [Note: Id And Link are different Value]
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            Log.Information("Finding 'To Ship' Navigation Tab"); numOfLines += 1;

            // Select "To Ship"
            IWebElement shipmentTabBtn = driver.FindElement(By.CssSelector("div[class='shopee-tabs__nav-tab']:nth-child(3)"));

            IWebElement shipmentTabLabelElem = shipmentTabBtn.FindElement(By.CssSelector(".tab-label"));

            // Format Text To Remove Inner Whitespace
            string shipmentTabLabel = shipmentTabLabelElem.Text;
            rgx = @"(To Ship)\s*(\d+)";

            string numOfShipment = Regex.Replace(shipmentTabLabel, rgx, "$2");

            if(numOfShipment.Equals(""))
            {
                return numOfLines;
            }

            // Click the Shipment Tab
            action.MoveToElement(shipmentTabBtn).Click().Perform();
            Log.Information("Selecting 'To Ship' Navigation Tab");
        
            List<IWebElement> orderIdElemArr = driver.FindElements(By.CssSelector(".orderid")).ToList();
            List<IWebElement> orderLinkElemArr = driver.FindElements(By.CssSelector("a[href*='portal/sale/order/']")).ToList();
        
            // Get List of Order Ids
            List<string> orderIdArr = orderIdElemArr.Select(it => it.Text.Split(' ')[2].Trim()).ToList();
        
            // Reverse Order ID
            orderIdArr.Reverse();
        
            int orderIntId = DbStoredProcedure.GetID("TNtlOrder");
        
            Console.WriteLine("Getting Latest NTL Order ID From Database"); numOfLines+=1;
            Console.WriteLine("Matching Latest NTL Order ID With Latest Shopee Order ID"); numOfLines+=1;
        
            string latestOrderId = _latestOrderId;
        
            // Slice Order Id Arr
            if (!latestOrderId.Equals(""))
            {
                int tmpId = orderIdArr.IndexOf(latestOrderId);
                orderIdArr = orderIdArr.GetRange(tmpId + 1, orderIdArr.Count - 1 - tmpId);
            }
        
            int numOfOrders = orderIdArr.Count;
        
            // Initialize Dictionary
            orderDict = new Dictionary<string, string>();
        
            for (int i = 0; i < numOfOrders; i++)
            {
                string order_id = orderIdElemArr.ElementAt(i).Text.Split(' ')[2].Trim();
                string order_link = orderLinkElemArr.ElementAt(i).GetAttribute("href");
                orderDict.Add(order_id, order_link);
            }
        
            // Loop Through Order Id
            orderIdArr.ForEach(it => {
                numOfLines += CheckCollectOrderDetail(it);
            });
        
            return numOfLines;
        }
        
        public int CheckCollectOrderDetail(string order_id_str)
        {
            int numOfLines = 0;
            Console.WriteLine($"Navigating To Order {order_id_str}, Link: {orderDict[order_id_str]}"); numOfLines+=1;
        
            // For Regex Purpose
            string rgx = "";
            string _width, _height;
        
            // Navigate To Link
            string link = orderDict[order_id_str];
            driver.Navigate().GoToUrl(link);
        
            Console.WriteLine($"Successfully Enter Order {order_id_str}, Link: {orderDict[order_id_str]}"); numOfLines+=1;
            
            // Dynamic Wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);
        
            // Get Created Date
            IWebElement createdDateElem = driver.FindElement(By.CssSelector(".od-log:last-child .time"));
            DateTime createdDt = Convert.ToDateTime(createdDateElem.Text); // Convert Date String to DateTime
        
            // Get Platform ID
            int platform_id = DbStoredProcedure.GetPlatformID("Shopee");
        
            // Get Customer Name
            List<IWebElement> custElemArr = driver.FindElements(By.CssSelector(".card-style:nth-child(3) .section:nth-child(2) .body div")).ToList();
            string custName = custElemArr[0].Text.Split(',')[0].Trim();
            string custPhoneNum = custElemArr[0].Text.Split(',')[1].Trim();
            string custAddress = custElemArr[1].Text.Trim();
        
            var customer = db.TNtlCustomers.FirstOrDefault(it => it.name.Equals(custName));
        
            int inc_sta_id = DbStoredProcedure.GetStatusID("draft", "Order");
            int c_sta_id = DbStoredProcedure.GetStatusID("sale", "Order");
        
            string username = "Admin";
        
            TNtlOrder order = new TNtlOrder();
        
            order.name = order_id_str;
            order.code = link.Split('/')[6];
            order.created_date = createdDt;
            order.customer_id = customer.id;
            order.tax_price = 0;
            order.discount_fee = 0;
            order.sub_total_price = 0;
            order.total_price = 0;
        
            order.odoo_status_id = inc_sta_id;
            order.status_id = inc_sta_id;
        
            order.created_by = customer.name;
        
            order.last_updated_by = "Selenium";
            order.last_updated_date = createdDt;
        
            Console.WriteLine($"Successfully created Order {order.name} in NTL Database"); numOfLines+=1;
        
            // To Remove
            Console.WriteLine($"Order: {order.name} - Created Date: {order.created_date} - Customer: {custName} - Total Price: {order.total_price}"); numOfLines+=1;
        
            // Latest Order Id
            int order_id = DbStoredProcedure.GetID("TNtlOrder");
        
            // Collect Order Detail
            // [Return List of Products]
            List<IWebElement> productNameElemArr = driver.FindElements(By.CssSelector(".product-item .product-detail .product-name")).ToList();
            List<IWebElement> unitPriceElemArr = driver.FindElements(By.CssSelector(".product-item~.price")).ToList();
            List<IWebElement> quantityElemArr = driver.FindElements(By.CssSelector(".product-item~.qty")).ToList();
            List<IWebElement> skuElemArr = driver.FindElements(By.CssSelector(".product-meta div:nth-child(2)")).ToList();
        
            int num_of_products = driver.FindElements(By.CssSelector(".product-item")).Count;
        
            Console.WriteLine("Order Items: "); numOfLines+=1;
            
        
            for (int i = 0; i < num_of_products; i++)
            {
                TNtlOrderItem orderItem = new TNtlOrderItem();
        
                string productSku = skuElemArr.ElementAt(i).Text.Split(' ')[2];
        
                string productName = productNameElemArr.ElementAt(i).Text.Trim();
                decimal unitPrice = Convert.ToDecimal(unitPriceElemArr.ElementAt(i).Text.Trim());
                decimal quantity = Convert.ToDecimal(quantityElemArr.ElementAt(i).Text.Trim());
        
                var product = db.TNtlProducts.FirstOrDefault(it => it.SKU.Equals(productSku));
        
                if (product == null)
                {
                    product = db.TNtlProducts.FirstOrDefault(it => it.SKU.Equals("default"));
                }
        
                orderItem.order_id = order_id;
                orderItem.product_id = product.id;
        
                orderItem.name = productName;
                orderItem.sku = productSku;
        
                // From ProductSKU, get Width and Height
                rgx = @"[A-Za-z]+(\d{3})(\d{3})?";
        
                _width = Regex.Replace(productSku, rgx, "$1");
                _height = Regex.Replace(productSku, rgx, "$2");
        
                decimal width = (_width.Equals("")) ? 100 : Convert.ToDecimal(_width);
                decimal height = (_height.Equals("")) ? 100 : Convert.ToDecimal(_height);
        
                orderItem.unit_price = unitPrice;
                orderItem.quantity = quantity;
                orderItem.sub_total_price = unitPrice * quantity;
                orderItem.discount_fee = 0;
                orderItem.tax_price = 0;
                orderItem.total_price = orderItem.sub_total_price - orderItem.discount_fee - orderItem.tax_price;
                orderItem.uom_id = product.uom_id;
        
                // Set Remark
                orderItem.remark = $"[{productSku}] {width}cm|{height}cm|{quantity}";
        
                // Set Total Usage
                orderItem.total_usage = height * width * quantity / 100 / 100;
        
                // To Remove
                Console.WriteLine($"Product {productName} - SKU {productSku} - Quantity {quantity} - Unit Price {unitPrice} - Sub Total Price {orderItem.sub_total_price}"); numOfLines+=1;
                
            }
            Console.WriteLine($"Successfully Added All Order Items with Order ID {order_id_str} into NTL Database"); numOfLines+=1;
        
            // Update Order Total Price
            var totalPriceList = db.TNtlOrderItems.Where(it => it.order_id == order_id).Select(it => it.total_price).ToList();
        
            order = db.TNtlOrders.FirstOrDefault(it => it.id == order_id);
        
            order.status_id = c_sta_id;
            order.sub_total_price = totalPriceList.Sum();
            order.total_price = order.sub_total_price - order.tax_price - order.discount_fee;
        
            Console.WriteLine("Successfully updated Order Price"); numOfLines+=1;
        
            Console.WriteLine("Now Sending Order to Odoo using POST API"); numOfLines+=1;
        
            mOdooCustomer model = new mOdooCustomer();
            model.customer = "Shopee";
        
            List<mOdooProduct> productList = new List<mOdooProduct>();
            db.TNtlOrderItems.Where(it => it.order_id == order.id).ToList().ForEach(it =>
            {
                rgx = @"[A-Za-z]+(\d{3})(\d{3})?";
        
                _width = Regex.Replace(it.sku, rgx, "$1");
                _height = Regex.Replace(it.sku, rgx, "$2");
        
                int iWidth = (_width.Equals("")) ? 100 : Convert.ToInt32(_width);
                int iHeight = (_height.Equals("")) ? 100 : Convert.ToInt32(_height);
        
                mOdooProduct _model = new mOdooProduct(it.sku.Substring(0, 9), (int)it.quantity, iWidth, iHeight);
                productList.Add(_model);
            });
        
            model.product = productList;
        
            // Generate Quotation
            string json = JsonConvert.SerializeObject(model);
        
            Console.WriteLine($"Successfully send order data to Odoo! Result: {json}"); numOfLines+=1;
        
            // Wait for 5 Seconds
            Task.Delay(5000).Wait();
        
            return numOfLines; 
        }

    }
}
