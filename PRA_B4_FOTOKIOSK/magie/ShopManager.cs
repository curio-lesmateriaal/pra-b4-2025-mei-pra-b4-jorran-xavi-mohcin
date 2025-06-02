using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PRA_B4_FOTOKIOSK.magie
{
    public static class ShopManager
    {
        public static List<KioskProduct> Products = new List<KioskProduct>();
        public static Home Instance { get; set; }
        private static string currentReceipt = "";

        public static void SetShopPriceList(string text)
        {
            Instance.lbPrices.Content = text;
        }

        public static void AddShopPriceList(string text)
        {
            Instance.lbPrices.Content = Instance.lbPrices.Content + text;
        }

        public static void SetShopReceipt(string receipt)
        {
            currentReceipt = receipt;
            Instance.lbReceipt.Content = receipt;
        }

        public static string GetShopReceipt()
        {
            return currentReceipt;
        }

        public static void AddShopReceipt(string receiptLine)
        {
            currentReceipt += receiptLine;
            Instance.lbReceipt.Content = currentReceipt;
        }

        public static void UpdateDropDownProducts()
        {
            Instance.cbProducts.Items.Clear();
            foreach (KioskProduct item in Products)
            {
                Instance.cbProducts.Items.Add(item.Name);
            }
        }

        public static string GetSelectedProduct()
        {
            return Instance.cbProducts.SelectedItem as string;
        }

        public static string GetFotoId()
        {
            return Instance.tbFotoId.Text.Trim();
        }

        public static int GetAmount()
        {
            if (int.TryParse(Instance.tbAmount.Text, out int amount))
            {
                return amount;
            }
            return 0;
        }
    }
}
