using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    /// <summary>
    /// Program
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            int core = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                core = int.Parse(item["NumberOfCores"].ToString());
            }
            
            Console.WriteLine($"CPU 核心 {core}");
            Console.WriteLine($"CPU 邏輯處理器 {Environment.ProcessorCount}");

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();
            var oriMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"同步 花費時間: {oriMs} ms");

            imageProcess.Clean(destinationPath);

            sw.Restart();
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            sw.Stop();
            var taskMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"非同步 花費時間: {taskMs} ms");

            Console.WriteLine($"提升百分比: {((oriMs - taskMs) / (double)oriMs * 100).ToString("#.#")} %");

            Console.ReadKey();
        }
    }
}
