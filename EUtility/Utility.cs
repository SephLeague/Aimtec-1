using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Aimtec;
using Newtonsoft.Json;

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

            if (!Directory.Exists(JsonFolder))
            {
                Directory.CreateDirectory(JsonFolder);
            }
        }

        internal static string ResourcePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EUtility");

        public static Dictionary<string, Bitmap> BitMapCache = new Dictionary<string, Bitmap>();

        internal static string JsonFolder => Path.Combine(ResourcePath, "ChampionData");

        public static Dictionary<string, ChampionData> ChampionDatas = new Dictionary<string, ChampionData>();

        public static string GetPathToResource(string name)
        {
            return Path.Combine(ResourcePath, $"{name}.bmp");
        }

        public static string GetPathToJson(string name)
        {
            return Path.Combine(JsonFolder, $"{name}.json");
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

        public static Bitmap GetSkillBitMap(string name)
        {
            var cached = GetCached(name);

            if (cached != null)
            {
                return cached;
            }

            var bmp = DlSkillBmp(name);
            if (bmp != null)
            {
                BitMapCache[name] = bmp;
                bmp.Save(GetPathToResource(name));
                return bmp;
            }

            return null;
        }


        public static Bitmap GetHeroBitMap(string name)
        {
            var cached = GetCached(name);

            if (cached != null)
            {
                return cached;
            }

            var bmp = DlHeroBmp(name);
            if (bmp != null)
            {
                BitMapCache[name] = bmp;
                bmp.Save(GetPathToResource(name));
                return bmp;
            }

            return null;
        }

        public static ChampionData GetChampionData(string name)
        {
            if (ChampionDatas.ContainsKey(name))
            {
                Console.WriteLine($"Loading local {name}");
                return ChampionDatas[name];
            }

            var pth = GetPathToJson(name);

            if (!File.Exists(pth))
            {
                if (!DownloadChampionData(name))
                {
                    Console.WriteLine($"Downloading data failed for {name}");
                    return null;
                }
            }

            var js = File.ReadAllText(pth);
            var jsobject = ChampionJson.FromJson(js, name);
            var cdata = jsobject.Data.Hero;
            ChampionDatas[name] = cdata;
            return cdata;

        }

        public static bool DownloadChampionData(string name)
        {
            try
            {
                WebClient wb = new WebClient();
                wb.DownloadFile($"http://ddragon.leagueoflegends.com/cdn/7.18.1/data/en_US/champion/{name}.json",
                    GetPathToJson(name));
                return true;
            }

            catch (Exception e)
            {
                Console.WriteLine($"Error downloading champion data for {name}");
                Console.Write(e);
                return false;
            }
        }


        public static Bitmap DlSkillBmp(string name)
        {
            return DownloadBitMap($"http://ddragon.leagueoflegends.com/cdn/7.18.1/img/spell/{name}.png");
        }

        public static Bitmap DlHeroBmp(string name)
        {
            return DownloadBitMap($"http://ddragon.leagueoflegends.com/cdn/7.18.1/img/champion/{name}.png");
        }

        public static Bitmap DownloadBitMap(string link)
        {
            try
            {
                System.Net.WebRequest request =
                    System.Net.WebRequest.Create(link);
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
                Console.WriteLine($"Caused by {link}");
                return null;
            }

            return null;
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage(b))
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

        /* https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale */
        public static Bitmap MakeGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        /* Converted to C# from VB https://stackoverflow.com/questions/5734710/c-sharp-crop-circle-in-a-image-or-bitmap */
        public static Bitmap CropImageToCircle(Bitmap bmp, int circleUpperLeftX, int circleUpperLeftY, int circleDiameter)
        {
            //'//Load our source image
            using (Bitmap SourceImage = bmp)
            {
                //'//Create a rectangle that crops a square of our image
                System.Drawing.Rectangle CropRect = new System.Drawing.Rectangle(circleUpperLeftX, circleUpperLeftY, circleDiameter, circleDiameter);
                //'//Crop the image to that square
                using (Bitmap CroppedImage = SourceImage.Clone(CropRect, SourceImage.PixelFormat))
                {
                    //'//Create a texturebrush to draw our circle with
                    using (TextureBrush TB = new TextureBrush(CroppedImage))
                    {
                        //'//Create our output image
                        Bitmap FinalImage = new Bitmap(circleDiameter, circleDiameter);
                        //'//Create a graphics object to draw with
                        using (Graphics G = Graphics.FromImage(FinalImage))
                        {
                            //'//Draw our cropped image onto the output image as an ellipse with the same width/height (circle)
                            G.FillEllipse(TB, 0, 0, circleDiameter, circleDiameter);
                            return FinalImage;
                        }
                    }
                }
            }
        }

        public static Bitmap AddCircleOutline(Bitmap source, Color color, int width)
        {
            using (Graphics g = Graphics.FromImage(source))
            {
                var p = new Pen(color, width);
                p.Alignment = PenAlignment.Outset;
                g.DrawEllipse(p, 0, 0, source.Width, source.Width);
                return source;
            }
        }

        public static Bitmap ClipToCircle(Bitmap original, PointF center, float radius)
        {
            Bitmap copy = new Bitmap(original);
            using (Graphics g = Graphics.FromImage(copy))
            {
                RectangleF r = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(r);
                g.Clip = new Region(path);
                g.DrawImage(original, 0, 0);
                return copy;
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
