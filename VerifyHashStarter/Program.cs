using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VerifyHashStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "VerifyFiles v0.3 Beta";
            Console.WriteLine("VerifyFiles v0.3 Beta");
            string copyrightText = "";
            if (DateTime.Now.Year != 2019)
            {
                copyrightText = "2019-" + DateTime.Now.Year;
            }
            else
            {
                copyrightText = "2019";
            }

            Console.WriteLine("Copyright " + copyrightText + " - WorldUnited");
            Console.WriteLine("Author: MeTonaTOR & ReenduX");
            Console.WriteLine("------------------------------------------------------------");

            if (!File.Exists("nfsw.exe"))
            {
                Console.WriteLine("Cannot find nfsw.exe, please place VerifyHash.exe directly to Need for Speed: World installation folder.");
                Console.WriteLine("------------------------------------------------------------");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine("This tool will verify your NFSW installation, if you agree \non that, press anything on keyboard.");
            Console.WriteLine("Otherwise, shut down this app by clicking on X or ALT+F4.");
            Console.ReadKey();
            Console.WriteLine("------------------------------------------------------------");
            bool taskDone = false;
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;

            do
            {
                while (!Console.KeyAvailable)
                {
                    if (taskDone != true)
                    {
                        Console.WriteLine("[INFO] Loading hashes");
                        VerifyHash.VerifyHash vh = new VerifyHash.VerifyHash();
                        TaskbarProgress.SetState(handle, TaskbarProgress.TaskbarStates.Indeterminate);

                        Task.Run(() => {
                            while (vh.totalFilesScanned != vh.filesToScan)
                            {
                                TaskbarProgress.SetValue(handle, (int)(100 * vh.totalFilesScanned / vh.filesToScan), 100);

                                Console.Title = "[" + vh.totalFilesScanned + "/" + vh.filesToScan + "] VerifyFiles v0.2 Beta";

                                Task.Delay(100);
                            }
                        });

                        vh.ScanLines((output) => Console.WriteLine(output));

                        if (vh.invalidFileList.Count != 0)
                        {
                            Console.WriteLine("------------------------------------------------------------");
                            Console.Title = "VerifyFiles v0.2 Beta";
                            Console.WriteLine("  Redownloading " + vh.invalidFileList.Count + " files: ");

                            Task.Run(() => {
                                while (vh.redownloadedCount != vh.badFiles)
                                {
                                    TaskbarProgress.SetValue(handle, (int)(100 * vh.redownloadedCount / vh.badFiles), 100);

                                    Console.Title = "[" + vh.redownloadedCount + "/" + vh.badFiles + "] VerifyFiles v0.2 Beta";

                                    Task.Delay(100);
                                }
                            });

                            vh.DownloadFiles((output) => Console.WriteLine(output));
                        }

                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine("  All done! Results: ");
                        Console.WriteLine("    Total Files Scanned: " + vh.totalFilesScanned.ToString());
                        Console.WriteLine("    Total Files To Scan: " + vh.filesToScan.ToString());
                        Console.WriteLine("    Mismatched Hash: " + vh.badFiles.ToString());
                        Console.WriteLine("    Redownloaded Files: " + vh.redownloadedCount.ToString());
                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine("Press ESC to quit.");
                        taskDone = true;
                    }
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
