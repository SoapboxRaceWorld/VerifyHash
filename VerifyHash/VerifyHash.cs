using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace VerifyHash
{
    public class VerifyHash
    {
        string[][] scannedHashes;
        public int filesToScan;
        public int badFiles;
        public int totalFilesScanned;
        public int redownloadedCount;
        public List<string> invalidFileList = new List<string>();

        public VerifyHash()
        {
            string[] hashcontent = ExtractResource("VerifyHash.sha1_nfswfiles.txt").Split(new[] { '\n' });

            scannedHashes = new string[hashcontent.Length][];
            for (var i = 0; i < hashcontent.Length; i++)
            {
                scannedHashes[i] = hashcontent[i].Split(' ');
            }

            filesToScan = scannedHashes.Length;
            badFiles = 0;
            totalFilesScanned = 0;
            redownloadedCount = 0;
        }

        public void ScanLines(Action<string> print)
        {
            Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories).AsParallel().ForAll((file) =>
            {
                for (var i = 0; i < scannedHashes.Length; i++)
                {
                    if (scannedHashes[i][1].Trim() == file.Replace(Directory.GetCurrentDirectory(), string.Empty).Trim())
                    {
                        if (scannedHashes[i][0].Trim() != Calculate(file).Trim())
                        {
                            badFiles++;
                            print("[SCAN] Invalid hash \t | " + file.Replace(Directory.GetCurrentDirectory(), string.Empty));
                            invalidFileList.Add(file.Replace(Directory.GetCurrentDirectory(), string.Empty).Trim());
                        }
                    }
                }
                totalFilesScanned++;
            });
        }

        public void DownloadFiles(Action<string> print)
        {
            invalidFileList.AsParallel().ForAll((filename) => {
                try
                {
                    string currentfile = Directory.GetCurrentDirectory() + filename;
                    string downloadurl = "http://cdn.worldunited.gg/verify/unpacked" + filename.Replace("\\", "/");

                    if (File.Exists(currentfile + ".vhbak")) File.Delete(currentfile + ".vhbak");
                    File.Move(currentfile, currentfile + ".vhbak");

                    WebClient downloader = new WebClient();
                    downloader.DownloadFile(downloadurl, currentfile);

                    redownloadedCount++;
                }
                catch
                {
                    print("[FILE] Status: Failed\t | " + filename);
                }
            });
        }

        #region helpers

        private static string Calculate(string filename)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retVal = sha1.ComputeHash(File.OpenRead(filename));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString().ToUpper();
        }

        private static string ExtractResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(filename))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        #endregion
    }
}
