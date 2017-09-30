using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Aimtec;

namespace EUtility
{
    static class Utility
    {
        static Utility()
        {
            if (!Directory.Exists(ResourcePath))
            {
                Directory.CreateDirectory(ResourcePath);
            }
        }

        internal static string ResourcePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Trackeker");

        public static Dictionary<string, Bitmap> BitMapCache = new Dictionary<string, Bitmap>();

        public static string GetPathToResource(string name)
        {
            return Path.Combine(ResourcePath, $"{name}.bmp");
        }

        public static Bitmap LoadFromFile(string name)
        {
            var path = GetPathToResource(name);

            if (File.Exists(path))
            {
                return new Bitmap(path);
            }

            return null;
        }

        public static Bitmap GetCached(string name)
        {
            if (BitMapCache.ContainsKey(name))
            {
                return BitMapCache[name];
            }

            var bmp = LoadFromFile(name);
            if (bmp != null)
            {
                BitMapCache[name] = bmp;
                return bmp;
            }

            return null;
        }

        public static Bitmap GetBitMap(string name)
        {
            var cached = GetCached(name);

            if (cached != null)
            {
                return cached;
            }

            var bmp = DownloadBitMap(name);
            BitMapCache[name] = bmp;
            bmp.Save(GetPathToResource(name));
            
            return bmp;
        }

        public static Bitmap DownloadBitMap(string name)
        {
            try
            {
                System.Net.WebRequest request =
                    System.Net.WebRequest.Create(
                        $"http://ddragon.leagueoflegends.com/cdn/7.18.1/img/spell/{name}.png");
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream =
                    response.GetResponseStream();

                if (responseStream != null)
                {
                    Bitmap bitmap = new Bitmap(responseStream);
                    return bitmap;
                }
            }

            catch (Exception e)
            {
                Console.Write(e);
                Console.WriteLine($"Caused by {name}");
                return null;
            }

            return null;
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch (Exception e)
            {
                Console.WriteLine("Bitmap could not be resized " + e);
                return imgToResize;
            }
        }


        public static void DrawRectangleOutline(Vector2 pos,
            float width,
            float height,
            float lineWidth,
            Color color)
        {
            // Top left to top right
            Render.Line(pos.X, pos.Y, pos.X + width, pos.Y, lineWidth, true, color);

            // Top right to bottom right
            Render.Line(pos.X + width, pos.Y, pos.X + width, pos.Y + height, lineWidth, true, color);

            // Bottom right to bottom left
            Render.Line(pos.X + width, pos.Y + height, pos.X, pos.Y + height, lineWidth, true, color);

            // Bottom left to top left
            Render.Line(pos.X, pos.Y + height, pos.X, pos.Y, lineWidth, true, color);
        }
    }
}
