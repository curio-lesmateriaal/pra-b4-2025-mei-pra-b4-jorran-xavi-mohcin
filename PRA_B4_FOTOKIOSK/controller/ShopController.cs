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
            // Stel de prijslijst in aan de rechter kant.
            ShopManager.SetShopPriceList("Prijzen:\nFoto 10x15: €2.55");

            // Initialize the prices display
            UpdatePricesDisplay();

            // Vul de productlijst met producten
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.55m, Description = "Standaard fotoformaat 10x15 cm" });
            // Placeholders
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 13x18", Price = 3.25m, Description = "Middelgroot fotoformaat 13x18 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 20x30", Price = 5.95m, Description = "Groot fotoformaat 20x30 cm" });

            // Update prijslijst met alle producten
            UpdatePriceList();

            // Update dropdown met producten
            ShopManager.UpdateDropDownProducts();
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