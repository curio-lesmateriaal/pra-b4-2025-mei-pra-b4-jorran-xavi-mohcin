using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        public static Home Window { get; set; }
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        public void Start()
        {
            PicturesToDisplay.Clear();

            var now = DateTime.Now;
            int today = (int)now.DayOfWeek;

            // Calculate time bounds: between 2 and 30 minutes ago
            DateTime lowerBound = now.AddMinutes(-30);
            DateTime upperBound = now.AddMinutes(-2);

            string basePath = Path.GetFullPath(@"../../../fotos");
            var allPhotos = new List<(DateTime Time, string Path)>();

            // Collect all photos within time window
            foreach (string dir in Directory.GetDirectories(basePath))
            {
                string folderName = Path.GetFileName(dir);
                string[] parts = folderName.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        string[] fileParts = fileName.Split('_');
                        if (fileParts.Length >= 3 &&
                            int.TryParse(fileParts[0], out int hour) &&
                            int.TryParse(fileParts[1], out int minute) &&
                            int.TryParse(fileParts[2], out int second))
                        {
                            DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
                            if (photoTime >= lowerBound && photoTime <= upperBound)
                            {
                                allPhotos.Add((photoTime, file));
                            }
                        }
                    }
                }
            }

            // Sort by time
            allPhotos = allPhotos.OrderBy(p => p.Time).ToList();

            // Create pairs of photos taken 60 seconds apart
            var processedIndices = new HashSet<int>();
            
            // First pass: Find and add pairs
            for (int i = 0; i < allPhotos.Count; i++)
            {
                if (processedIndices.Contains(i)) continue;

                PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = allPhotos[i].Path });
                processedIndices.Add(i);

                // Look for matching photo taken ~60 seconds later
                for (int j = i + 1; j < allPhotos.Count; j++)
                {
                    if (processedIndices.Contains(j)) continue;

                    var timeDiff = allPhotos[j].Time - allPhotos[i].Time;
                    if (Math.Abs(timeDiff.TotalSeconds - 60) < 1)
                    {
                        // Found matching photo, add it next to the current one
                        PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = allPhotos[j].Path });
                        processedIndices.Add(j);
                        break;
                    }
                }
            }

            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        public void RefreshButtonClick()
        {
            Start();
        }
    }
}
