using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest
{
    class mOdooProduct
    {
        public mOdooProduct(string sku, int qty, int width, int height)
        {
            this.sku = sku;
            this.qty = qty;
            this.width = width;
            this.height = height;
        }

        public string sku { get; set; }
        public int qty { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
