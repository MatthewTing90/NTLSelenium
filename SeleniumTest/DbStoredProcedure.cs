using SeleniumTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest
{
    public class DbStoredProcedure
    {
        static dbNtlSystemEntities db = new dbNtlSystemEntities();

        public static int GetID(string tbl_name)
        {
            return db.Database.SqlQuery<int>($"SELECT CAST(IDENT_CURRENT('{tbl_name}') AS INT)").FirstOrDefault();
        }

        public static int GetStatusID(string val, string type)
        {
            return db.TNtlStatus.Where(it => it.name.ToLower().Equals(val.ToLower()) && it.type.ToLower().Equals(type.ToLower())).FirstOrDefault().id;
        }

        public static int GetPlatformID(string val)
        {
            return db.TNtlPlatforms.Where(it => it.name.ToLower().Equals(val.ToLower())).FirstOrDefault().id;
        }

        public static int GetUomID(string val)
        {
            return db.TNtlUoms.Where(it => it.name.ToLower().Equals(val.ToLower())).FirstOrDefault().id;
        }

        public static int GetStockWarehouseID(string val)
        {
            return db.TNtlStockWarehouses.Where(it => it.name.ToLower().Equals(val.ToLower())).FirstOrDefault().id;
        }

        public static void DetailInsert(string name, string remark, string created_by, string last_updated_by)
        {
            DateTime currentTime = DateTime.Now;
            db.NSP_TNtlDetail_Insert(name, remark, created_by, currentTime, last_updated_by, currentTime);
            db.SaveChanges();
        }

        public static void DetailUpdate(int id, string name, string remark, string created_by, DateTime? created_date, string last_updated_by)
        {
            db.NSP_TNtlDetail_Update(id, name, remark, created_by, created_date, last_updated_by, DateTime.Now);
            db.SaveChanges();
        }

        public static void DetailDelete(int id)
        {
            db.NSP_TNtlDetail_Delete(id);
            db.SaveChanges();
        }

        public static void PlatformInsert(TNtlPlatform item, string username)
        {
            DetailInsert($"Platform: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlPlatform_Insert(item.name, item.link, item.detail_id);
            db.SaveChanges();
        }

        public static void PlatformUpdate(TNtlPlatform item, string username)
        {
            var modelItem = db.TNtlPlatforms.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Platform: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlPlatform_Update(item.id, item.name, item.link, item.detail_id);
            db.SaveChanges();
        }

        public static void PlatformDelete(int id)
        {
            var modelItem = db.TNtlPlatforms.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlPlatform_Delete(id);
            db.SaveChanges();
        }

        public static void CustomerInsert(TNtlCustomer item, string username)
        {
            DetailInsert($"Customer: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlCustomer_Insert(item.name, item.email_address, item.phone_number, item.address, item.platform_id, item.detail_id);
            db.SaveChanges();
        }

        public static void CustomerUpdate(TNtlCustomer item, string username)
        {
            var modelItem = db.TNtlCustomers.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Customer: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlCustomer_Update(item.id, item.name, item.email_address, item.phone_number, item.address, item.platform_id, item.detail_id);
            db.SaveChanges();
        }

        public static void CustomerDelete(int id)
        {
            var modelItem = db.TNtlCustomers.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlCustomer_Delete(id);
            db.SaveChanges();
        }

        public static void OrderInsert(TNtlOrder item, string username)
        {
            DetailInsert($"Order: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlOrder_Insert(item.name, item.code, item.sub_total_price, item.tax_price, item.discount_fee, item.total_price, item.voucher_no, item.odoo_sales_id, item.odoo_sales_no, item.external_ref_no, item.odoo_status_id, item.status_id, item.customer_id, item.created_by, item.created_date, item.last_updated_by, item.last_updated_date, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void OrderUpdate(TNtlOrder item, string username)
        {
            var modelItem = db.TNtlOrders.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Order: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlOrder_Update(item.id, item.name, item.code, item.sub_total_price, item.tax_price, item.discount_fee, item.total_price, item.voucher_no, item.odoo_sales_id, item.odoo_sales_no, item.external_ref_no, item.odoo_status_id, item.status_id, item.customer_id, item.created_by, item.created_date, item.last_updated_by, item.last_updated_date, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void OrderDelete(int id)
        {
            var modelItem = db.TNtlOrders.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlOrder_Delete(id);
            db.SaveChanges();
        }

        public static void OrderUpdatePrice(int order_id, decimal? total_price, string username)
        {
            var order = db.TNtlOrders.Find(order_id);

            // Update Price Here
            order.sub_total_price += total_price;
            order.total_price = order.sub_total_price - order.discount_fee - order.tax_price;

            OrderUpdate(order, username);
        }

        public static void OrderItemInsert(TNtlOrderItem item, string username)
        {
            DetailInsert($"OrderItem: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlOrderItem_Insert(item.name, item.sku, item.unit_price, item.quantity, item.sub_total_price, item.tax_price, item.discount_fee, item.total_price, item.voucher_no, item.order_id, item.product_id, item.unit_id, item.total_usage, item.uom_id, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void OrderItemUpdate(TNtlOrderItem item, string username)
        {
            var modelItem = db.TNtlOrderItems.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"OrderItem: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlOrderItem_Update(item.id, item.name, item.sku, item.unit_price, item.quantity, item.sub_total_price, item.tax_price, item.discount_fee, item.total_price, item.voucher_no, item.order_id, item.product_id, item.unit_id, item.total_usage, item.uom_id, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void OrderItemDelete(int id)
        {
            var modelItem = db.TNtlOrderItems.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlOrderItem_Delete(id);
            db.SaveChanges();
        }

        public static void InvoiceInsert(TNtlInvoice item, string username)
        {
            DetailInsert($"Invoice: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlInvoice_Insert(item.name, item.code, item.details, item.created_date, item.completed_date, item.shipping_fee, item.customer_address_line_1, item.customer_address_line_2, item.customer_city, item.customer_state, item.customer_zip_code, item.customer_country, item.payment_method_id, item.status_id, item.customer_id, item.order_id, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void InvoiceUpdate(TNtlInvoice item, string username)
        {
            var modelItem = db.TNtlInvoices.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Invoice: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlInvoice_Update(item.id, item.name, item.code, item.details, item.created_date, item.completed_date, item.shipping_fee, item.customer_address_line_1, item.customer_address_line_2, item.customer_city, item.customer_state, item.customer_zip_code, item.customer_country, item.payment_method_id, item.status_id, item.customer_id, item.order_id, item.remark, item.detail_id);
            db.SaveChanges();
        }

        public static void InvoiceDelete(int id)
        {
            var modelItem = db.TNtlInvoices.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlInvoice_Delete(id);
            db.SaveChanges();
        }

        public static void PaymentMethodInsert(TNtlPaymentMethod item, string username)
        {
            DetailInsert($"PaymentMethod: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlPaymentMethod_Insert(item.name, item.detail_id);
            db.SaveChanges();
        }

        public static void PaymentMethodUpdate(TNtlPaymentMethod item, string username)
        {
            var modelItem = db.TNtlPaymentMethods.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"PaymentMethod: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlPaymentMethod_Update(item.id, item.name, item.detail_id);
            db.SaveChanges();
        }

        public static void PaymentMethodDelete(int id)
        {
            var modelItem = db.TNtlPaymentMethods.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlPaymentMethod_Delete(id);
            db.SaveChanges();
        }

        public static void UomInsert(TNtlUom item, string username)
        {
            DetailInsert($"Uom: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlUom_Insert(item.name, item.detail_id);
            db.SaveChanges();
        }

        public static void UomUpdate(TNtlUom item, string username)
        {
            var modelItem = db.TNtlUoms.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Uom: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlUom_Update(item.id, item.name, item.detail_id);
            db.SaveChanges();
        }

        public static void UomDelete(int id)
        {
            var modelItem = db.TNtlUoms.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlUom_Delete(id);
            db.SaveChanges();
        }

        public static void StatusInsert(TNtlStatu item, string username)
        {
            DetailInsert($"Status: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlStatus_Insert(item.name, item.type, item.detail_id);
            db.SaveChanges();
        }

        public static void StatusUpdate(TNtlStatu item, string username)
        {
            var modelItem = db.TNtlStatus.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Status: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlStatus_Update(item.id, item.name, item.type, item.detail_id);
            db.SaveChanges();
        }

        public static void StatusDelete(int id)
        {
            var modelItem = db.TNtlStatus.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlStatus_Delete(id);
            db.SaveChanges();
        }

        public static void ProductComponentInsert(TNtlProductComponent item, string username)
        {
            DetailInsert($"ProductComponent: {item.master_product_id}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlProductComponent_Insert(item.master_product_id, item.sub_product_id, item.quantity, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void ProductComponentUpdate(TNtlProductComponent item, string username)
        {
            var modelItem = db.TNtlProductComponents.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"ProductComponent: {item.master_product_id}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlProductComponent_Update(item.id, item.master_product_id, item.sub_product_id, item.quantity, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void ProductComponentDelete(int id)
        {
            var modelItem = db.TNtlProductComponents.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlProductComponent_Delete(id);
            db.SaveChanges();
        }

        public static void StockWarehouseInsert(TNtlStockWarehouse item, string username)
        {
            DetailInsert($"StockWarehouse: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlStockWarehouse_Insert(item.name, item.email_address, item.phone_number, item.address_line_1, item.address_line_2, item.city, item.state, item.zip_code, item.country, item.detail_id);
            db.SaveChanges();
        }

        public static void StockWarehouseUpdate(TNtlStockWarehouse item, string username)
        {
            var modelItem = db.TNtlStockWarehouses.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"StockWarehouse: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlStockWarehouse_Update(item.id, item.name, item.email_address, item.phone_number, item.address_line_1, item.address_line_2, item.city, item.state, item.zip_code, item.country, item.detail_id);
            db.SaveChanges();
        }

        public static void StockWarehouseDelete(int id)
        {
            var modelItem = db.TNtlStockWarehouses.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlStockWarehouse_Delete(id);
            db.SaveChanges();
        }

        public static void StockItemInsert(TNtlStockItem item, string username)
        {
            DetailInsert($"StockItem: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlStockItem_Insert(item.name, item.description, item.quantity, item.product_id, item.uom_id, item.stock_warehouse_id, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void StockItemUpdate(TNtlStockItem item, string username)
        {
            var modelItem = db.TNtlStockItems.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"StockItem: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlStockItem_Update(item.id, item.name, item.description, item.quantity, item.product_id, item.uom_id, item.stock_warehouse_id, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void StockItemDelete(int id)
        {
            var modelItem = db.TNtlStockItems.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlStockItem_Delete(id);
            db.SaveChanges();
        }

        public static void ProductInsert(TNtlProduct item, string username)
        {
            DetailInsert($"Product: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlProduct_Insert(item.name, item.description, item.SKU, item.SKU2, item.buy_price, item.sell_price, item.product_category_id, item.product_sub_category_id, item.uom_id, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void ProductUpdate(TNtlProduct item, string username)
        {
            var modelItem = db.TNtlProducts.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"Product: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlProduct_Update(item.id, item.name, item.description, item.SKU, item.SKU2, item.buy_price, item.sell_price, item.product_category_id, item.product_sub_category_id, item.uom_id, item.remarks, item.detail_id);
            db.SaveChanges();
        }

        public static void ProductDelete(int id)
        {
            var modelItem = db.TNtlProducts.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            // Delete All 

            db.NSP_TNtlProduct_Delete(id);
            db.SaveChanges();
        }

        public static void JobBatchInsert(TNtlJobBatch item, string username)
        {
            DetailInsert($"JobBatch: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlJobBatch_Insert(item.name, item.batch_no, item.created_date, item.completed_date, item.staff_name, item.status_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobBatchUpdate(TNtlJobBatch item, string username)
        {
            var modelItem = db.TNtlJobBatches.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"JobBatch: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlJobBatch_Update(item.id, item.name, item.batch_no, item.created_date, item.completed_date, item.staff_name, item.status_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobBatchDelete(int id)
        {
            var modelItem = db.TNtlJobBatches.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            // Delete All Job Orders
            db.TNtlJobOrders.Where(it => it.batch_id == id).ToList().ForEach(it => JobOrderDelete(it.id));

            db.NSP_TNtlJobBatch_Delete(id);
            db.SaveChanges();
        }

        public static void JobOrderInsert(TNtlJobOrder item, string username)
        {
            DetailInsert($"JobOrder: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlJobOrder_Insert(item.name, item.code, item.created_date, item.client_order_ref, item.sub_total_price, item.discount_fee, item.tax_price, item.total_price, item.customer_id, item.customer_name, item.customer_street, item.customer_street2, item.customer_zip_code, item.customer_city, item.customer_state, item.customer_country, item.odoo_sales_id, item.odoo_status_id, item.status_id, item.batch_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobOrderUpdate(TNtlJobOrder item, string username)
        {
            var modelItem = db.TNtlJobOrders.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"JobOrder: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlJobOrder_Update(item.id, item.name, item.code, item.created_date, item.client_order_ref, item.sub_total_price, item.discount_fee, item.tax_price, item.total_price, item.customer_id, item.customer_name, item.customer_street, item.customer_street2, item.customer_zip_code, item.customer_city, item.customer_state, item.customer_country, item.odoo_sales_id, item.odoo_status_id, item.status_id, item.batch_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobOrderDelete(int id)
        {
            var modelItem = db.TNtlJobOrders.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            // Delete all Job Order Item
            db.TNtlJobOrderItems.Where(it => it.order_id == modelItem.odoo_sales_id).ToList().ForEach(it => JobOrderItemDelete(it.id));

            db.NSP_TNtlJobOrder_Delete(id);
            db.SaveChanges();
        }

        public static void JobOrderItemInsert(TNtlJobOrderItem item, string username)
        {
            DetailInsert($"JobOrderItem: {item.name}", "", username, username);
            item.detail_id = GetID("TNtlDetail");

            db.NSP_TNtlJobOrderItem_Insert(item.name, item.sku, item.uom, item.unit_price, item.quantity, item.length, item.width, item.height, item.sub_total_price, item.discount_fee, item.tax_price, item.total_price, item.order_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobOrderItemUpdate(TNtlJobOrderItem item, string username)
        {
            var modelItem = db.TNtlJobOrderItems.Find(item.id);

            item.detail_id = (int)modelItem.detail_id;
            TNtlDetail detail = db.TNtlDetails.FirstOrDefault(it => it.id == item.detail_id);
            detail.name = $"JobOrderItem: {item.name}";

            DetailUpdate(detail.id, detail.name, detail.remark, detail.created_by, detail.created_date, username);

            db.NSP_TNtlJobOrderItem_Update(item.id, item.name, item.sku, item.uom, item.unit_price, item.quantity, item.length, item.width, item.height, item.sub_total_price, item.discount_fee, item.tax_price, item.total_price, item.order_id, item.detail_id);
            db.SaveChanges();
        }

        public static void JobOrderItemDelete(int id)
        {
            var modelItem = db.TNtlJobOrderItems.Find(id);

            int detail_id = (int)modelItem.detail_id;
            DetailDelete(detail_id);

            db.NSP_TNtlJobOrderItem_Delete(id);
            db.SaveChanges();
        }

        public static void UserInsert(TNtlUser item)
        {
            db.NSP_TNtlUser_Insert(item.username, item.password, item.email_address);
            db.SaveChanges();
        }

        public static void UserUpdate(TNtlUser item)
        {
            db.NSP_TNtlUser_Update(item.id, item.username, item.password, item.email_address);
            db.SaveChanges();
        }

        public static void UserDelete(int id)
        {
            db.NSP_TNtlUser_Delete(id);
            db.SaveChanges();
        }

        public static void RoleInsert(TNtlRole item)
        {
            db.NSP_TNtlRole_Insert(item.name);
            db.SaveChanges();
        }

        public static void RoleUpdate(TNtlRole item)
        {
            db.NSP_TNtlRole_Update(item.id, item.name);
            db.SaveChanges();
        }

        public static void RoleDelete(int id)
        {
            db.NSP_TNtlRole_Delete(id);
            db.SaveChanges();
        }

        public static void UserRoleInsert(TNtlUserRole item)
        {
            db.NSP_TNtlUserRole_Insert(item.user_id, item.role_id);
            db.SaveChanges();
        }

        public static void UserRoleUpdate(TNtlUserRole item)
        {
            db.NSP_TNtlUserRole_Update(item.id, item.user_id, item.role_id);
            db.SaveChanges();
        }

        public static void UserRoleDelete(int id)
        {
            db.NSP_TNtlUserRole_Delete(id);
            db.SaveChanges();
        }

        public static void CustomerChatInsert(TNtlCustomerChat item)
        {
            db.NSP_TNtlCustomerChat_Insert(item.message, item.created_date, item.msg_type, item.sender_type, item.customer_id, item.user_id, item.status_id, item.platform_id);
            db.SaveChanges();
        }

        public static void CustomerChatUpdate(TNtlCustomerChat item)
        {
            db.NSP_TNtlCustomerChat_Update(item.id, item.message, item.created_date, item.msg_type, item.sender_type, item.customer_id, item.user_id, item.status_id, item.platform_id);
            db.SaveChanges();
        }

        public static void CustomerChatDelete(int id)
        {
            db.NSP_TNtlCustomerChat_Delete(id);
            db.SaveChanges();
        }

        public static void PermissionInsert(TNtlPermission item)
        {
            db.NSP_TNtlPermission_Insert(item.name);
            db.SaveChanges();
        }

        public static void PermissionUpdate(TNtlPermission item)
        {
            db.NSP_TNtlPermission_Update(item.id, item.name);
            db.SaveChanges();
        }

        public static void PermissionDelete(int id)
        {
            db.NSP_TNtlPermission_Delete(id);
            db.SaveChanges();
        }

        public static void RolePermissionInsert(TNtlRolePermission item)
        {
            db.NSP_TNtlRolePermission_Insert(item.role_id, item.permission_id);
            db.SaveChanges();
        }

        public static void RolePermissionUpdate(TNtlRolePermission item)
        {
            db.NSP_TNtlRolePermission_Update(item.id, item.role_id, item.permission_id);
            db.SaveChanges();
        }

        public static void RolePermissionDelete(int id)
        {
            db.NSP_TNtlRolePermission_Delete(id);
            db.SaveChanges();
        }

        public static List<string> GetPermissionFromUser(string uName)
        {
            if (uName.Equals("Admin")) return new List<string>();

            IDictionary<int, string> roleDict = new Dictionary<int, string>();
            db.TNtlRoles.ToList().ForEach(it => roleDict.Add(it.id, it.name));

            IDictionary<int, string> permissionDict = new Dictionary<int, string>();
            db.TNtlPermissions.ToList().ForEach(it => permissionDict.Add(it.id, it.name));

            // Get Current User
            TNtlUser user = db.TNtlUsers.FirstOrDefault(it => it.username.Equals(uName));

            // Get List of Roles
            List<int> roleList = db.TNtlUserRoles.Where(it => it.user_id == user.id).Select(it => (int)it.role_id).ToList();

            List<string> permissionList = new List<string>();

            // From List of Roles, Get List of Permissions
            roleList.ForEach(role =>
            {
                // Get all Permission from selected Role
                List<int> permissionIdList = db.TNtlRolePermissions.Where(it => it.role_id == role).Select(it => (int)it.permission_id).ToList();
                permissionIdList.ForEach(permission => permissionList.Add(permissionDict[permission]));
            });

            return permissionList.ToList();
        }

        public static void SeleniumLogInsert(TNtlSeleniumLog item)
        {
            db.NSP_TNtlSeleniumLog_Insert(item.log_name, item.type, item.start_date, item.end_date, item.platform_id, item.status, item.remarks);
            db.SaveChanges();
        }
        
        public static void SeleniumLogUpdate(TNtlSeleniumLog item)
        {
            db.NSP_TNtlSeleniumLog_Update(item.id, item.log_name, item.type, item.start_date, item.end_date, item.platform_id, item.status, item.remarks);
            db.SaveChanges();
        }
        
        public static void SeleniumLogDelete(int id)
        {
            db.NSP_TNtlSeleniumLog_Delete(id);
            db.SaveChanges();
        }

    }
}

