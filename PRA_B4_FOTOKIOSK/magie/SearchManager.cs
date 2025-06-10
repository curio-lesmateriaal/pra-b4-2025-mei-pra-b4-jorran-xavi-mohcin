using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace PRA_B4_FOTOKIOSK.magie
{
    public class SearchManager
    {
        public static Home Instance { get; set; }

        public static void SetPicture(string path)
        {
            if (Instance?.imgBig == null)
            {
                throw new InvalidOperationException("SearchManager Instance or imgBig is null");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Photo file not found: {path}");
            }

            Instance.imgBig.Source = pathToImage(path);
        }

        public static BitmapImage pathToImage(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Image file not found: {path}");
            }

            try
            {
                var stream = new System.IO.MemoryStream(File.ReadAllBytes(path));
                var img = new BitmapImage();

                img.BeginInit();
                img.StreamSource = stream;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze(); // Important for thread safety

                return img;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load image from {path}: {ex.Message}", ex);
            }
        }

        public static string GetSearchInput()
        {
            if (Instance?.tbZoeken == null)
            {
                return string.Empty;
            }
            return Instance.tbZoeken.Text?.Trim() ?? string.Empty;
        }

        public static void SetSearchImageInfo(string info)
        {
            if (Instance?.lbSearchInfo == null)
            {
                throw new InvalidOperationException("SearchManager Instance or lbSearchInfo is null");
            }
            Instance.lbSearchInfo.Content = info ?? string.Empty;
        }

        public static void ClearSearchResults()
        {
            if (Instance?.imgBig != null)
            {
                Instance.imgBig.Source = null;
            }
            if (Instance?.lbSearchInfo != null)
            {
                Instance.lbSearchInfo.Content = string.Empty;
            }
        }
    }
}
