using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home Window { get; set; }
        private List<OrderedProduct> orderedProducts = new List<OrderedProduct>();
        private decimal totalAmount = 0.00m;

        public void Start()
        {
            // Stel de prijslijst in aan de rechter kant.
            ShopManager.SetShopPriceList("Prijzen:\nFoto 10x15: €2.55");

            // Stel de bon in onderaan het scherm
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");

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

        private void UpdatePriceList()
        {
            // Begin met een header voor de prijslijst
            StringBuilder priceList = new StringBuilder("Prijzen:\n");

            // Loop door alle producten en voeg ze toe aan de prijslijst
            foreach (KioskProduct product in ShopManager.Products)
            {
                priceList.AppendLine($"{product.Name}: €{product.Price:F2}");
            }

            // Stel de prijslijst in
            ShopManager.SetShopPriceList(priceList.ToString());
        }

        // Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
        public void AddButtonClick()
        {
            try
            {
                // Haal de geselecteerde product op
                KioskProduct selectedProduct = Window.cbProducts.SelectedItem as KioskProduct;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Selecteer eerst een product.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal het foto ID op
                string photoId = Window.tbFotoId.Text.Trim();
                if (string.IsNullOrEmpty(photoId))
                {
                    MessageBox.Show("Vul een foto ID in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal het aantal op
                if (!int.TryParse(Window.tbAmount.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Vul een geldig aantal in (groter dan 0).", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Bereken de totaalprijs voor dit product
                decimal productTotal = selectedProduct.Price * quantity;

                // Maak een nieuw OrderedProduct object
                OrderedProduct orderedProduct = new OrderedProduct
                {
                    PhotoId = photoId,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    TotalPrice = productTotal
                };

                // Voeg het toe aan de lijst
                orderedProducts.Add(orderedProduct);

                // Update het totaalbedrag
                totalAmount += productTotal;

                // Update de kassabon
                UpdateReceipt();

                // Reset de invoervelden
                Window.tbFotoId.Text = "";
                Window.tbAmount.Text = "1";
                Window.cbProducts.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReceipt()
        {
            StringBuilder receipt = new StringBuilder();

            // Voeg het eindbedrag toe
            receipt.AppendLine($"Eindbedrag\n€{totalAmount:F2}\n");

            // Voeg een scheidingslijn toe
            receipt.AppendLine("------------------------");

            // Voeg alle bestelde producten toe
            receipt.AppendLine("Kassabon:");
            foreach (OrderedProduct product in orderedProducts)
            {
                receipt.AppendLine(product.ToString());
            }

            // Update de kassabon in de UI
            ShopManager.SetShopReceipt(receipt.ToString());
        }

        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            // Reset de lijst met bestelde producten
            orderedProducts.Clear();

            // Reset het totaalbedrag
            totalAmount = 0.00m;

            // Reset de bon
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");

            // Reset de invoervelden
            if (Window != null)
            {
                Window.tbFotoId.Text = "";
                Window.tbAmount.Text = "1";
                Window.cbProducts.SelectedIndex = 0;
            }
        }

        // Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {
            // =opslaan van bon kan hier komen
        }
    }
}
