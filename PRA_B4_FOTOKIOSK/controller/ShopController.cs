using PRA_B4_FOTOKIOSK.magie;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home Window { get; set; }
        private string currentReceipt = "";
        private Dictionary<string, decimal> productPrices = new Dictionary<string, decimal>()
        {
            { "10x15 foto", 0.50m },
            { "13x18 foto", 1.00m },
            { "20x30 poster", 5.00m },
            { "30x45 poster", 7.50m }
        };

        public void Start()
        {
            // Initialize the products dropdown
            foreach (var product in productPrices.Keys)
            {
                Window.cbProducts.Items.Add(product);
            }
            if (Window.cbProducts.Items.Count > 0)
                Window.cbProducts.SelectedIndex = 0;

            // Initialize the prices display
            UpdatePricesDisplay();

            // Clear the receipt
            ShopManager.SetShopReceipt("");
            UpdateReceiptDisplay();
        }

        private void UpdatePricesDisplay()
        {
            string pricesText = "Prijslijst:\n\n";
            foreach (var kvp in productPrices)
            {
                pricesText += $"{kvp.Key}: €{kvp.Value:F2}\n";
            }
            Window.lbPrices.Content = pricesText;
        }

        private void UpdateReceiptDisplay()
        {
            Window.lbReceipt.Content = ShopManager.GetShopReceipt();
        }

        public void AddButtonClick()
        {
            try
            {
                string selectedProduct = ShopManager.GetSelectedProduct();
                string fotoId = ShopManager.GetFotoId();
                int amount = ShopManager.GetAmount();

                if (string.IsNullOrEmpty(fotoId))
                {
                    MessageBox.Show("Voer een geldig foto-ID in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (amount <= 0)
                {
                    MessageBox.Show("Voer een geldig aantal in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!productPrices.ContainsKey(selectedProduct))
                {
                    MessageBox.Show("Selecteer een geldig product.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal price = productPrices[selectedProduct];
                decimal total = price * amount;

                string receiptLine = $"Foto {fotoId} - {amount}x {selectedProduct} - €{total:F2}\n";
                ShopManager.AddShopReceipt(receiptLine);

                // Update the display
                UpdateReceiptDisplay();

                // Clear the inputs
                Window.tbFotoId.Text = "";
                Window.tbAmount.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ResetButtonClick()
        {
            ShopManager.SetShopReceipt("");
            UpdateReceiptDisplay();
        }

        public void SaveButtonClick()
        {
            try
            {
                string receipt = ShopManager.GetShopReceipt();
                if (string.IsNullOrEmpty(receipt))
                {
                    MessageBox.Show("Er is geen bon om op te slaan.", "Informatie", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Calculate total
                decimal total = 0;
                string[] lines = receipt.Split('\n');
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        string priceStr = line.Split('€')[1];
                        total += decimal.Parse(priceStr);
                    }
                }

                // Add total to receipt
                string finalReceipt = receipt + $"\nTotaal: €{total:F2}";
                ShopManager.SetShopReceipt(finalReceipt);
                UpdateReceiptDisplay();

                // Create a unique filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"bon_{timestamp}.txt";
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FotoKiosk", "Bonnen");
                
                // Create directory if it doesn't exist
                Directory.CreateDirectory(folderPath);
                
                // Full path for the receipt file
                string filePath = Path.Combine(folderPath, fileName);

                // Save receipt to file
                File.WriteAllText(filePath, finalReceipt);

                MessageBox.Show($"Bon is opgeslagen in:\n{filePath}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden bij het opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}