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
            try
            {
                // Clear any previous search results
                SearchManager.ClearSearchResults();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchController.Start(): {ex.Message}");
            }
        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            try
            {
                // Validate SearchManager instance
                if (SearchManager.Instance == null)
                {
                    MessageBox.Show("Zoekfunctie is niet correct geïnitialiseerd.", "Systeemfout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get and validate search input
                string searchInput = SearchManager.GetSearchInput();
                if (string.IsNullOrWhiteSpace(searchInput))
                {
                    MessageBox.Show("Voer een foto-ID of tijd in om te zoeken.\n\nVoorbeelden:\n- 14_30 (tijd)\n- ABC123 (ID)\n- 09 (uur)", "Invoer vereist", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate and get base path
                string basePath = Path.GetFullPath(@"../../../fotos");
                if (!Directory.Exists(basePath))
                {
                    MessageBox.Show($"Foto map niet gevonden:\n{basePath}\n\nControleer of de map bestaat.", "Map niet gevonden", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Search for photos
                var foundPhotos = SearchPhotos(basePath, searchInput);

                if (foundPhotos.Count == 0)
                {
                    SearchManager.ClearSearchResults();
                    MessageBox.Show($"Geen foto's gevonden met '{searchInput}'.\n\nProbeer:\n- Een ander ID\n- Een tijd (bijv. 14_30)\n- Een gedeelte van de bestandsnaam", "Geen resultaten", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Process and display results
                ProcessSearchResults(foundPhotos, searchInput);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een onverwachte fout opgetreden bij het zoeken:\n\n{ex.Message}\n\nProbeer het opnieuw of neem contact op met de beheerder.", "Zoekfout", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"SearchController Error: {ex}");
            }
        }

        private List<string> SearchPhotos(string basePath, string searchInput)
        {
            var foundPhotos = new List<string>();

            try
            {
                // Search through all directories
                foreach (string dir in Directory.GetDirectories(basePath))
                {
                    if (!Directory.Exists(dir))
                        continue;

                    try
                    {
                        foreach (string file in Directory.GetFiles(dir, "*.jpg").Concat(Directory.GetFiles(dir, "*.jpeg")).Concat(Directory.GetFiles(dir, "*.png")))
                        {
                            if (!File.Exists(file))
                                continue;

                            string fileName = Path.GetFileNameWithoutExtension(file);

                            // Check if the filename contains the search input (case-insensitive)
                            if (fileName.IndexOf(searchInput, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                foundPhotos.Add(file);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Access denied to directory: {dir}");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading directory {dir}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching photos in {basePath}: {ex.Message}", ex);
            }

            return foundPhotos.OrderBy(f => f).ToList();
        }

        private void ProcessSearchResults(List<string> foundPhotos, string searchInput)
        {
            try
            {
                // Group photos that are taken within 60 seconds of each other (B4 A3 requirement)
                var photoGroups = GroupPhotosByTime(foundPhotos);

                // Display the first photo from the first group
                if (photoGroups.Count > 0 && photoGroups[0].Count > 0)
                {
                    SearchManager.SetPicture(photoGroups[0][0]);
                }

                // Build comprehensive information display (C3 requirement)
                var infoBuilder = new StringBuilder();
                infoBuilder.AppendLine("=== ZOEKRESULTATEN ===");
                infoBuilder.AppendLine($"Zoekterm: {searchInput}");
                infoBuilder.AppendLine($"Totaal gevonden: {foundPhotos.Count} foto(s)");
                infoBuilder.AppendLine($"Foto groepen: {photoGroups.Count}");
                infoBuilder.AppendLine();

                int groupNumber = 1;
                foreach (var group in photoGroups)
                {
                    infoBuilder.AppendLine($"--- GROEP {groupNumber} ---");

                    foreach (string photoPath in group)
                    {
                        var details = GetPhotoDetails(photoPath);
                        infoBuilder.AppendLine($" {Path.GetFileName(photoPath)}");
                        infoBuilder.AppendLine($"    Datum: {details.Date}");
                        infoBuilder.AppendLine($"    Tijd: {details.Time}");
                        infoBuilder.AppendLine($"    Map: {details.Folder}");
                        infoBuilder.AppendLine($"    ID: {details.PhotoId}");
                        infoBuilder.AppendLine();
                    }

                    // B4 A3: Indicate if photo pairs are found
                    if (group.Count == 2)
                    {
                        var time1 = GetPhotoDateTime(group[0]);
                        var time2 = GetPhotoDateTime(group[1]);
                        if (time1.HasValue && time2.HasValue)
                        {
                            var timeDiff = Math.Abs((time2.Value - time1.Value).TotalSeconds);
                            infoBuilder.AppendLine($" Foto paar gevonden! (interval: {timeDiff:F0} seconden)");
                        }
                        else
                        {
                            infoBuilder.AppendLine(" Foto paar gevonden!");
                        }
                    }
                    else if (group.Count == 1)
                    {
                        infoBuilder.AppendLine("⚠ Enkele foto - mogelijk ontbreekt de tweede foto");
                    }
                    else
                    {
                        infoBuilder.AppendLine($" {group.Count} foto's in deze groep");
                    }

                    infoBuilder.AppendLine();
                    groupNumber++;
                }

                SearchManager.SetSearchImageInfo(infoBuilder.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing search results: {ex.Message}", ex);
            }
        }

        // B4 A3: Group photos that are taken within 60 seconds of each other
        private List<List<string>> GroupPhotosByTime(List<string> photos)
        {
            var groups = new List<List<string>>();

            foreach (string photo in photos)
            {
                var photoTime = GetPhotoDateTime(photo);
                if (!photoTime.HasValue)
                {
                    // If we can't parse the time, create a separate group
                    groups.Add(new List<string> { photo });
                    continue;
                }

                bool addedToGroup = false;

                // Try to add to existing group (within 60 seconds)
                foreach (var group in groups)
                {
                    var groupTime = GetPhotoDateTime(group[0]);
                    if (groupTime.HasValue)
                    {
                        var timeDifference = Math.Abs((photoTime.Value - groupTime.Value).TotalSeconds);

                        // If within 60 seconds, add to this group
                        if (timeDifference <= 60)
                        {
                            group.Add(photo);
                            addedToGroup = true;
                            break;
                        }
                    }
                }

                // If not added to any group, create new group
                if (!addedToGroup)
                {
                    groups.Add(new List<string> { photo });
                }
            }

            // Sort groups by time
            foreach (var group in groups)
            {
                group.Sort((a, b) =>
                {
                    var timeA = GetPhotoDateTime(a);
                    var timeB = GetPhotoDateTime(b);
                    if (!timeA.HasValue && !timeB.HasValue) return 0;
                    if (!timeA.HasValue) return 1;
                    if (!timeB.HasValue) return -1;
                    return timeA.Value.CompareTo(timeB.Value);
                });
            }

            return groups.OrderBy(g =>
            {
                var firstTime = GetPhotoDateTime(g[0]);
                return firstTime ?? DateTime.MaxValue;
            }).ToList();
        }

        // Extract DateTime from photo filename
        private DateTime? GetPhotoDateTime(string photoPath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(photoPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(photoPath));

                // Parse folder name for day (format: "1_maandag" -> day 1)
                string[] folderParts = folderName.Split('_');
                if (folderParts.Length == 0 || !int.TryParse(folderParts[0], out int dayOfWeek))
                {
                    return null;
                }

                // Parse filename for time (format: "HH_MM_SS_ID" or "HH_MM_SS")
                string[] fileParts = fileName.Split('_');
                if (fileParts.Length < 3)
                {
                    return null;
                }

                if (!int.TryParse(fileParts[0], out int hour) ||
                    !int.TryParse(fileParts[1], out int minute) ||
                    !int.TryParse(fileParts[2], out int second))
                {
                    return null;
                }

                // Validate time values
                if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
                {
                    return null;
                }

                // Create DateTime (using current week for day calculation)
                var now = DateTime.Now;
                int daysToAdd = dayOfWeek - (int)now.DayOfWeek;
                var photoDate = now.Date.AddDays(daysToAdd);

                return new DateTime(photoDate.Year, photoDate.Month, photoDate.Day, hour, minute, second);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing photo datetime from {photoPath}: {ex.Message}");
                return null;
            }
        }

        // C3: Get detailed photo information - FIXED VERSION
        private (string Date, string Time, string Folder, string PhotoId) GetPhotoDetails(string photoPath)
        {
            try
            {
                var dateTime = GetPhotoDateTime(photoPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(photoPath));
                string fileName = Path.GetFileNameWithoutExtension(photoPath);

                // Extract photo ID from filename (last part after splitting by '_')
                string[] parts = fileName.Split('_');
                string photoId = parts.Length > 3 ? parts[3] : "Onbekend";

                if (dateTime.HasValue)
                {
                    return (
                        Date: dateTime.Value.ToShortDateString(),
                        Time: dateTime.Value.ToShortTimeString(),
                        Folder: folderName,
                        PhotoId: photoId
                    );
                }
                else
                {
                    return (
                        Date: "Onbekend",
                        Time: "Onbekend",
                        Folder: folderName,
                        PhotoId: photoId
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting photo details for {photoPath}: {ex.Message}");
                return (
                    Date: "Fout bij lezen",
                    Time: "Fout bij lezen",
                    Folder: "Onbekend",
                    PhotoId: "Onbekend"
                );
            }
        }
    }
}
