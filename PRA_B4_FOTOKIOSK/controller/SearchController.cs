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
    public class SearchController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }
        

        // Start methode die wordt aangeroepen wanneer de zoek pagina opent.
        public void Start()
        {

        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            try
            {
                string searchInput = SearchManager.GetSearchInput();
                if (string.IsNullOrEmpty(searchInput))
                {
                    MessageBox.Show("Voer een foto-ID in om te zoeken.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string basePath = Path.GetFullPath(@"../../../fotos");
                var now = DateTime.Now;
                int today = (int)now.DayOfWeek;

                // Search in today's folders
                foreach (string dir in Directory.GetDirectories(basePath))
                {
                    string folderName = Path.GetFileName(dir);
                    string[] parts = folderName.Split('_');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            if (fileName.Contains(searchInput))
                            {
                                // Found the photo, display it
                                SearchManager.SetPicture(file);

                                // Get photo information
                                string[] fileParts = fileName.Split('_');
                                if (fileParts.Length >= 3)
                                {
                                    int hour = int.Parse(fileParts[0]);
                                    int minute = int.Parse(fileParts[1]);
                                    int second = int.Parse(fileParts[2]);

                                    DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
                                    
                                    // Display photo information
                                    string info = $"Foto informatie:\n";
                                    info += $"ID: {searchInput}\n";
                                    info += $"Datum: {photoTime.ToShortDateString()}\n";
                                    info += $"Tijd: {photoTime.ToShortTimeString()}\n";
                                    info += $"Map: {folderName}\n";
                                    info += $"Bestandsnaam: {fileName}";

                                    SearchManager.SetSearchImageInfo(info);
                                    return;
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("Foto niet gevonden.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
