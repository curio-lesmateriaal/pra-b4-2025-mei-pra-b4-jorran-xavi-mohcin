using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;

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

            // Bepaal de tijdsgrenzen: van 2 minuten 30 seconden geleden tot nu
            DateTime lowerBound = now.AddMinutes(-2).AddSeconds(-30);
            DateTime upperBound = now;

            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                string folderName = Path.GetFileName(dir);
                string[] parts = folderName.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay) && folderDay == today)
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file); // bijv. 10_05_30_id8824
                        string[] fileParts = fileName.Split('_');
                        if (fileParts.Length >= 3 &&
                            int.TryParse(fileParts[0], out int hour) &&
                            int.TryParse(fileParts[1], out int minute) &&
                            int.TryParse(fileParts[2], out int second))
                        {
                            DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);

                            // Alleen foto's van de afgelopen 2 minuten en 30 seconden
                            if (photoTime >= lowerBound && photoTime <= upperBound)
                            {
                                PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                            }
                        }
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