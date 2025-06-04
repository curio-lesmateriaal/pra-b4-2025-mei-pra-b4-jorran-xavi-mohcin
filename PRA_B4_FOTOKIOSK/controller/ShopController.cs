using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home Window { get; set; }
        private string currentReceipt = "";

        public void Start()
        {
            // Stel de prijslijst in aan de rechter kant.
            ShopManager.SetShopPriceList("Prijzen:\nFoto 10x15: €2.55");

            // Vul de productlijst met producten
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.55m, Description = "Standaard fotoformaat 10x15 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 13x18", Price = 3.25m, Description = "Middelgroot fotoformaat 13x18 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 20x30", Price = 5.95m, Description = "Groot fotoformaat 20x30 cm" });

            // Update prijslijst met alle producten
            UpdatePriceList();

            // Update dropdown met producten
            ShopManager.UpdateDropDownProducts();
        }

        private void UpdatePriceList()
        {
            string pricesText = "Prijslijst:\n\n";
            foreach (var product in ShopManager.Products)
            {
                pricesText += $"{product.Name}: €{product.Price:F2}\n";
            }
            ShopManager.SetShopPriceList(pricesText);
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

                // Zoek het product in de ShopManager.Products lijst
                var product = ShopManager.Products.FirstOrDefault(p => p.Name == selectedProduct);
                if (product == null)
                {
                    MessageBox.Show("Selecteer een geldig product.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal price = product.Price;
                decimal total = price * amount;

                string receiptLine = $"Foto {fotoId} - {amount}x {selectedProduct} - €{total:F2}\n";
                ShopManager.AddShopReceipt(receiptLine);


                UpdateReceiptDisplay();

                // inputs weghalen
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

                // totaal berekenen
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

                // totaalbedrag toevoegen aan bon
                string finalReceipt = receipt + $"\nTotaal: €{total:F2}";
                ShopManager.SetShopReceipt(finalReceipt);
                UpdateReceiptDisplay();

                // maakt unieke bestandsnaam aan en slaat de bon op in de documenten map
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"bon_{timestamp}.txt";
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FotoKiosk", "Bonnen");

                // maak een map voor de bonnen als deze nog niet bestaat
                Directory.CreateDirectory(folderPath);

                // volledige path
                string filePath = Path.Combine(folderPath, fileName);

                // bon opslaan in document
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
