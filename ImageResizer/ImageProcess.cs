using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ImageResizer
{
    /// <summary>
    /// ImageProcess
    /// </summary>
    public class ImageProcess
    {
        #region Private Methods

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        private Bitmap ProcessBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedbitmap))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(img,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(0, 0, srcWidth, srcHeight),
                    GraphicsUnit.Pixel);
                return resizedbitmap;
            }

        }

        /// <summary>
        /// 針對指定圖片進行縮放作業 (非同步)
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        private async Task<Bitmap> ProcessBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            return await Task.Run(() =>
            {
                Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(resizedbitmap))
                {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.Clear(Color.Transparent);

                    g.DrawImage(img,
                        new Rectangle(0, 0, newWidth, newHeight),
                        new Rectangle(0, 0, srcWidth, srcHeight),
                        GraphicsUnit.Pixel);
                    return resizedbitmap;
                }

            });
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <param name="patterns">搜尋條件規則</param>
        /// <returns></returns>
        private List<string> FindFiles(string srcPath, params string[] patterns)
        {
            List<string> files = new List<string>();
            foreach (var pattern in patterns)
            {
                files.AddRange(Directory.GetFiles(srcPath, pattern, SearchOption.AllDirectories));
            }
            return files;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindFiles(sourcePath, "*.png", "*.jpg", "*.jpeg");
            foreach (var filePath in allFiles)
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                Bitmap processedImage = ProcessBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                string destFile = Path.Combine(destPath, imgName + ".jpg");
                processedImage.Save(destFile, ImageFormat.Jpeg);
            }
        }
        
        /// <summary>
        /// 進行圖片的縮放作業 (非同步)
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        /// <returns></returns>
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindFiles(sourcePath, "*.png", "*.jpg", "*.jpeg");

            var tasks = new List<Task>();
            foreach (var path in allFiles)
            {
                Image imgPhoto = Image.FromFile(path);
                string imgName = Path.GetFileNameWithoutExtension(path);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);

                tasks.Add(Task.Run(async () =>
                {
                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    Bitmap processedImage = await ProcessBitmapAsync((Bitmap)imgPhoto, sourceHeight, sourceHeight, destionatonWidth, destionatonHeight);
                    string destFile = Path.Combine(destPath, imgName + ".jpg");
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                    //sw.Stop();
                    //Console.WriteLine($"ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}, Ms={sw.ElapsedMilliseconds}");
                }));
            }

            //var tasks = new ConcurrentStack<Task>();
            //Parallel.ForEach(allFiles, (path) =>
            //{
            //    Image imgPhoto = Image.FromFile(path);
            //    string imgName = Path.GetFileNameWithoutExtension(path);

            //    int sourceWidth = imgPhoto.Width;
            //    int sourceHeight = imgPhoto.Height;

            //    int destionatonWidth = (int)(sourceWidth * scale);
            //    int destionatonHeight = (int)(sourceHeight * scale);

            //    tasks.Push(Task.Run(async () =>
            //    {
            //        //Stopwatch sw = new Stopwatch();
            //        //sw.Start();
            //        Bitmap processedImage = await processBitmapAsync((Bitmap)imgPhoto, sourceHeight, sourceHeight, destionatonWidth, destionatonHeight);
            //        string destFile = Path.Combine(destPath, imgName + ".jpg");
            //        processedImage.Save(destFile, ImageFormat.Jpeg);
            //        //sw.Stop();
            //        //Console.WriteLine($"ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}, Ms={sw.ElapsedMilliseconds}");
            //    }));
            //});

            await Task.WhenAll(tasks);
        }

        #endregion
    }
}
