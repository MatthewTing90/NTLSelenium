using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest
{
    class mOdooCustomer
    {
        public mOdooCustomer()
        {

        }
        public mOdooCustomer(string customer, List<mOdooProduct> product)
        {
            this.customer = customer;
            this.product = product;
        }

        public string customer { get; set; }
        public List<mOdooProduct> product { get; set; }
    }
}
