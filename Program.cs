using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

namespace MsEdgeLauncher
{
    class Program
    {
		///-------------------------------------------------------------------------------------------------------------///
         /* method to launch "Google Chrome / Chromium" executable
         * iterate through every "chrome.exe" available in the current launcher directory & sub-directories
         * execute one each time and waiting for the process to exit before executing the next one
         */
        static void LauncheEdge(string currentDir, string commandLineArguments, string cacheDir)
        {
			string cacheDirCommandLineArgument = $" --disk-cache-dir={cacheDir}";
            Process msedge = new Process();
            msedge.StartInfo.Arguments = commandLineArguments + cacheDirCommandLineArgument;

            DirectoryInfo dir = new DirectoryInfo(currentDir);
            try
            {
                foreach (FileInfo file in dir.GetFiles("msedge.exe", SearchOption.AllDirectories))
                    {
                        msedge.StartInfo.FileName = file.FullName;
                        msedge.Start();
                        msedge.WaitForExit();
                        break;
                    }
            }
            catch{}
        }
		///-------------------------------------------------------------------------------------------------------------///
        /*
         * method that create a temporary directory in the %TEMP% directory
         * and return the path of the created directory
         * 
         * - isFolderNameRandom : toggle whether the temporary folder created is randomely named
         */
		private static string GetTemporaryDirectory(bool isFolderNameRandom)
        {
            string randomFileName = "";
            if (isFolderNameRandom)
            {
                randomFileName = Path.GetRandomFileName();
            }
            string path = Path.Combine(Path.GetTempPath(), "XHCH_MSEDGE.Cache." + randomFileName + "\\"); 
            try
			{
				Directory.CreateDirectory(path);
				return path;
			}
			catch
			{
				return null;
			}
        }
		///-------------------------------------------------------------------------------------------------------------///
        private static void CleanupCache(string profileDir, string cacheDir)
        {
            try
            {
                Directory.Delete(cacheDir, true);
                /*
                Directory.Delete(profileDir + $@"\ShaderCache", true);
                Directory.Delete(profileDir + $@"\BrowserMetrics", true);
                Directory.Delete(profileDir + $@"\Crashpad", true);
                Directory.Delete(profileDir + $@"\GrShaderCache", true);
                Directory.Delete(profileDir + $@"\OnDeviceHeadSuggestModel", true);
                Directory.Delete(profileDir + $@"\ZxcvbnData", true);
                */
                DirectoryInfo dirInfo = new DirectoryInfo(profileDir);
                foreach (DirectoryInfo dir in dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if(dir.Name != "Default")
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                }
                Directory.Delete(profileDir + $@"\Default\blob_storage", true);
                Directory.Delete(profileDir + $@"\Default\Cache", true);
                Directory.Delete(profileDir + $@"\Default\Code Cache", true);
                Directory.Delete(profileDir + $@"\Default\GPUCache", true);
                File.Delete(profileDir + @"\CrashpadMetrics-active.pma");
                File.Delete(profileDir + @"\Module Info Cache");

            }
            catch { }
        }
		///-------------------------------------------------------------------------------------------------------------///
        static void Cleanup(string profileDir)
        {
            try
            {
                Directory.Delete(profileDir, true);
            }
            catch { }
        }
        ///-------------------------------------------------------------------------------------------------------------///
        static void Main(string[] args)
        {
			string tempDir = null;
            string currentDir = Directory.GetCurrentDirectory() + @"\";
            string profileDir = currentDir + "MsEdgeProfile";
            string cacheDir = currentDir + "MsEdgeProfileCache";
            //profileDir += "Incognito";
			bool keepCache = false;
            bool tempCache = false;

            string exeArguments = $"--user-data-dir=\"{profileDir}\" --disable-smooth-scrolling --disable-notifications --enable-quic --no-default-browser-check --disable-crash-reporter --disable-plugins --allow-insecure-localhost --enable-parallel-downloading";

            /// Disable Extensions
            //exeArguments += "--disable-extensions";

            ///------------------------------------------------------------------////
            
            if (args.Length != 0)
            {
                foreach(string arg in args)
                {
					if(arg != "--tempcache" || arg != "--keepcache")
                    {
						exeArguments += " " + arg;
					}
					if(arg == "--tempcache")
					{
                        tempCache = true;
					}  
					if(arg == "--keepcache")
					{
						keepCache = true;
					}
                }
            }
            if(tempCache)
            {
                tempDir = GetTemporaryDirectory(!keepCache);
                if (tempDir != null)
                {
                    cacheDir = tempDir + "MsEdgeProfileCache";
                }
            }
			///------------------------------------------------------------------////
            if (!Directory.Exists(profileDir))
            {
                try
                {
                    Directory.CreateDirectory(profileDir);
                }
                catch { }
            }

			if(keepCache == false)
            { 
                CleanupCache(profileDir, cacheDir);
            }

            CleanupCache(profileDir, cacheDir);
            LauncheEdge(currentDir, exeArguments, cacheDir);
            CleanupCache(profileDir, cacheDir);

            if (keepCache == false)
            {
                CleanupCache(profileDir, cacheDir);
				Cleanup(tempDir);
			}
            //Cleanup(profileDir);

        }
    }
}
