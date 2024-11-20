using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;

using System.Linq;
namespace ImageDownloader
{
    public class ImageDownloader
    {
        public void SaveImage(string imageURL)
        {
            try
            {
                
                // Ensure the directory exists
                 string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string parentDirectory = System.IO.Path.GetDirectoryName(currentDirectory);
                string[] subdirectories = Directory.GetDirectories(currentDirectory);
                string selectedDirectory = subdirectories.FirstOrDefault(d => d.EndsWith("Image"));
                if (!Directory.Exists(selectedDirectory))
                {
                    Directory.CreateDirectory(selectedDirectory);
                }
                string savePath = selectedDirectory;
                // Write the image data to the file
                File.WriteAllBytes(savePath, imageData);

                Console.WriteLine($"Image saved to {savePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
            }
        }

    }