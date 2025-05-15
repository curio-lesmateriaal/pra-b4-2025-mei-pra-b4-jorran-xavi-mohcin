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
            PicturesToDisplay.Clear(); // Clear previous photos

            var now = DateTime.Now;
            int today = (int)now.DayOfWeek; // 0 = Sunday, 1 = Monday, etc.

            foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
            {
                // Get the folder name, e.g., "2_Dinsdag"
                string folderName = Path.GetFileName(dir);

                // Split on '_' and parse the first part as the day number
                string[] parts = folderName.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out int folderDay))
                {
                    if (folderDay == today)
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
                        }
                    }
                }
            }

            // Update the photos in the UI
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        public void RefreshButtonClick()
        {
            Start();
        }
    }
}