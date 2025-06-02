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

            // Alleen foto's van de laatste 2,5 minuut
            DateTime lowerBound = now.AddMinutes(-2).AddSeconds(-30);
            DateTime upperBound = now;

            string basePath = Path.GetFullPath(@"../../../fotos");
            var allPhotos = new List<(DateTime Time, string Path)>();

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

            // Sorteer op tijd
            allPhotos = allPhotos.OrderBy(p => p.Time).ToList();

            foreach (var photo in allPhotos)
            {
                PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = photo.Path });
            }

            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        public void RefreshButtonClick()
        {
            Start();
        }
    }
}
