//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SeleniumTest
{
    using System;
    using System.Collections.Generic;
    
    public partial class TNtlOrder
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public Nullable<decimal> sub_total_price { get; set; }
        public Nullable<decimal> tax_price { get; set; }
        public Nullable<decimal> discount_fee { get; set; }
        public Nullable<decimal> total_price { get; set; }
        public string voucher_no { get; set; }
        public Nullable<int> odoo_sales_id { get; set; }
        public string odoo_sales_no { get; set; }
        public string external_ref_no { get; set; }
        public Nullable<int> odoo_status_id { get; set; }
        public Nullable<int> status_id { get; set; }
        public Nullable<int> customer_id { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string last_updated_by { get; set; }
        public Nullable<System.DateTime> last_updated_date { get; set; }
        public string remark { get; set; }
        public Nullable<int> detail_id { get; set; }
    }
}
