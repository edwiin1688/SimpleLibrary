using Autofac;
using ICSharpCode.SharpZipLib.Zip;
using SimpleLibrary.Logger;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLibrary.Zip
{
    public class Zip : PrintLogger
    {
        public Zip(ContainerBuilder builder = null)
        {
            InitLogger(builder);
        }

        public void ZipTo(string outputZipPath, string inputDirectory)
        {
            ZipTo(outputZipPath, inputDirectory, null);
        }

        public void ZipTo(string outputZipPath, string inputDirectory, IProgress<int> progress)
        {
            CheckFilePath(outputZipPath);

            ZipOutputStream zipStream = new ZipOutputStream(File.Create(outputZipPath));
            zipStream.SetLevel(9);

            string[] files = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
            int totalFiles = files.Length;
            int processedFiles = 0;

            foreach (string file in files)
            {
                string relativePath = GetRelativePath(inputDirectory, file);
                AddFileToZip(zipStream, relativePath, file);
                processedFiles++;
                progress?.Report((int)((double)processedFiles / totalFiles * 100));
            }

            zipStream.Finish();
            zipStream.Close();
        }

        public async Task ZipToAsync(string outputZipPath, string inputDirectory, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => ZipTo(outputZipPath, inputDirectory), cancellationToken);
        }

        public async Task ZipToAsync(string outputZipPath, string inputDirectory, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => ZipTo(outputZipPath, inputDirectory, progress), cancellationToken);
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;
            return fullPath.Substring(basePath.Length).Replace('\\', '/');
        }

        private static void AddFileToZip(ZipOutputStream zipStream, string relativePath, string file)
        {
            byte[] buffer = new byte[4096];
            ZipEntry entry = new ZipEntry(relativePath)
            {
                DateTime = DateTime.Now
            };
            zipStream.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(file))
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        public void UnzipTo(string zipFilePath, string outputDirectory)
        {
            UnzipTo(zipFilePath, outputDirectory, null);
        }

        public void UnzipTo(string zipFilePath, string outputDirectory, IProgress<int> progress)
        {
            if (!File.Exists(zipFilePath))
            {
                Print($"Zip file not found: {zipFilePath}", Color.Red);
                return;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (ZipFile zipFile = new ZipFile(zipFilePath))
            {
                int totalEntries = (int)zipFile.Count;
                int processedEntries = 0;

                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsDirectory)
                    {
                        string dirPath = Path.Combine(outputDirectory, entry.Name);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                    }
                    else
                    {
                        string filePath = Path.Combine(outputDirectory, entry.Name);
                        string directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using (var inputStream = zipFile.GetInputStream(entry))
                        using (var outputStream = new FileStream(filePath, FileMode.Create))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outputStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                    processedEntries++;
                    progress?.Report((int)((double)processedEntries / totalEntries * 100));
                }
                Print($"Extracted {totalEntries} files to {outputDirectory}", Color.Green);
            }
        }

        public async Task UnzipToAsync(string zipFilePath, string outputDirectory, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => UnzipTo(zipFilePath, outputDirectory), cancellationToken);
        }

        public async Task UnzipToAsync(string zipFilePath, string outputDirectory, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => UnzipTo(zipFilePath, outputDirectory, progress), cancellationToken);
        }

        private void CheckFilePath(string filePath)
        {
            if (filePath.Contains("'"))
            {
                Print($"File path contains invalid character: {filePath}", Color.OrangeRed);
            }
        }
    }
}
