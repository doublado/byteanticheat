using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iCEBYTESetup
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

    }
    public partial class MainWindow : Window
    {
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        public static string[] config = {
            @"C:\Program Files (x86)\BYTE", //0 - BYTE Install Path
            @"C:\Program Files (x86)\BYTE\bin\byte.zip", //1 - BYTE Download path
            @"https://raw.githubusercontent.com/doublado/icebyte/main/icebyte-zipurl.ice", //2 - BYTE Download URL
            @"" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FiveM\FiveM.app" , //3 - FiveM default Path
        };
        public static string action = "Install";
        public MainWindow()
        {
            InitializeComponent();
            ActionButton.Content = "Please wait";
            action = "wait";
            if (!Directory.Exists(config[0]))
            {
                action = "install";
                ActionButton.Content = "Install BYTE Anti-cheat";

            }
            else
            {
                action = "uninstall";
                ActionButton.Content = "Uninstall BYTE Anti-cheat";
            }
        }
        public void KillProcess(int pid)
        {
            Process[] process = Process.GetProcesses();

            foreach (Process prs in process)
            {
                if (prs.Id == pid)
                {
                    prs.Kill();
                    break;
                }
            }
        }
        async void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            if (e.ProgressPercentage == 100)
            {
                iceMessage.Text = "Unzipping..";
                await Task.Delay(2000);
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(config[1]))
                    {
                        int totalEntries = archive.Entries.Count;
                        int extractedEntries = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            extractedEntries++;
                            progressBar.Value = (double)extractedEntries / totalEntries * 1000;
                            await Task.Delay(10);

                            string filePath = System.IO.Path.Combine(config[0], entry.FullName);
                            string directoryPath = System.IO.Path.GetDirectoryName(filePath);

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                                await Task.Delay(100);
                            }
                            else
                            {
                                entry.ExtractToFile(filePath, false);
                            }
                        }
                    }

                    iceMessage.Text = "Completed.";
                    ActionButton.IsEnabled = true;
                    action = "uninstall";
                    ActionButton.Content = "Uninstall BYTE Anti-cheat";
                }
                catch (Exception ex)
                {
                    throwError(ex.Message);
                }
            }
        }
        private void PrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://raw.githubusercontent.com/doublado/icebyte/main/privacy.ice");
        }
        private async void throwError(string error)
        {
            iceMessage.Text = error;
            SystemSounds.Hand.Play();
        }
        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (String.Equals(action, "install"))
            {
                if (Directory.Exists(config[3]))
                {
                    string keyName = @"HKEY_CURRENT_USER\Software\BYTE";
                    string valueName = "FiveM.app";
                    if (Registry.GetValue(keyName, valueName, null) == null)
                    {
                        Microsoft.Win32.RegistryKey key;
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                        key.SetValue("FiveM.app", config[3]);
                        key.Close();
                    }
                    else
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                        key.SetValue("FiveM.app", config[3]);
                        key.Close();
                    }
                    string keyNameB = @"HKEY_CURRENT_USER\Software\BYTE";
                    string valueNameB = "Path";
                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                    {
                        Microsoft.Win32.RegistryKey key;
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                        key.SetValue("Path", config[0]);
                        key.Close();
                    }
                    else
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                        key.SetValue("Path", config[0]);
                        key.Close();
                    }

                    string keyNameC = @"HKEY_CURRENT_USER\Software\BYTE";
                    string valueNameC = "ACExecutable";
                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                    {
                        Microsoft.Win32.RegistryKey key;
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                        key.SetValue("ACExecutable", "BYTE.exe");
                        key.Close();
                    }
                    else
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                        key.SetValue("ACExecutable", "BYTE.exe");
                        key.Close();
                    }

                    string keyNameD = @"HKEY_CURRENT_USER\Software\BYTE";
                    string valueNameD = "Watcher";
                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                    {
                        Microsoft.Win32.RegistryKey key;
                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                        key.SetValue("Watcher", "BYTEHelper.exe");
                        key.Close();
                    }
                    else
                    {
                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                        key.SetValue("Watcher", "BYTEHelper.exe");
                        key.Close();
                    }

                    action = "wait";
                    ActionButton.Content = "Please wait";
                    iceMessage.Text = "Downloading required files..";
                    ActionButton.IsEnabled = false; Directory.CreateDirectory(config[0]);
                    Process[] process = Process.GetProcesses();

                    foreach (Process prs in process)
                    {
                        if (prs.ProcessName.Contains("FiveM"))
                        {
                            KillProcess(prs.Id);
                        }
                        if (prs.ProcessName.Contains("BYTE") && !prs.ProcessName.Contains("Setup"))
                        {
                            KillProcess(prs.Id);
                        }
                    }
                    await Task.Delay(1000);

                    if (Directory.Exists(config[0]))
                    {
                        Directory.Delete(config[0], true);
                    }
                    Directory.CreateDirectory(config[0] + @"\bin");
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                        using (HttpClient client = new HttpClient())
                        {
                            string s = await client.GetStringAsync(config[2]);
                            wc.DownloadFileAsync(
                                new System.Uri(s.RemoveWhitespace()),
                                config[1]
                            ); ;
                        }
                    }
                }
                else
                {
                    action = "wait";
                    ActionButton.Content = "Please wait";
                    iceMessage.Text = "Select your FiveM Application Data folder and FiveM executable";
                    ActionButton.IsEnabled = false;

                    await Task.Delay(2000);

                    var dlg = new FolderPicker();
                    dlg.InputPath = @"C:\";
                    if (dlg.ShowDialog() == true)
                    {
                        if (dlg.ResultPath.Contains("FiveM.app"))
                        {
                            OpenFileDialog openFileDialog1 = new OpenFileDialog();

                            openFileDialog1.InitialDirectory = "c:\\";
                            openFileDialog1.Filter = "Executable files (*.exe)|*.exe";
                            openFileDialog1.FilterIndex = 0;
                            openFileDialog1.RestoreDirectory = true;

                            if (openFileDialog1.ShowDialog() == true)
                            {
                                string selectedFileName = openFileDialog1.FileName;
                                if (selectedFileName.Contains("FiveM"))
                                {
                                    string raisen = @"HKEY_CURRENT_USER\Software\BYTE";
                                    string raisenValue = "FiveM.exe";
                                    if (Registry.GetValue(raisen, raisenValue, null) == null)
                                    {
                                        Microsoft.Win32.RegistryKey key;
                                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                                        key.SetValue("FiveM.exe", selectedFileName);
                                        key.Close();
                                    }

                                    string keyName = @"HKEY_CURRENT_USER\Software\BYTE";
                                    string valueName = "FiveM.app";
                                    if (Registry.GetValue(keyName, valueName, null) == null)
                                    {
                                        Microsoft.Win32.RegistryKey key;
                                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                                        key.SetValue("FiveM.app", dlg.ResultPath);
                                        key.Close();
                                    }
                                    else
                                    {
                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                                        key.SetValue("FiveM.app", dlg.ResultPath);
                                        key.Close();
                                    }

                                    string keyNameB = @"HKEY_CURRENT_USER\Software\BYTE";
                                    string valueNameB = "Path";
                                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                                    {
                                        Microsoft.Win32.RegistryKey key;
                                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                                        key.SetValue("Path", config[0]);
                                        key.Close();
                                    }
                                    else
                                    {
                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                                        key.SetValue("Path", config[0]);
                                        key.Close();
                                    }

                                    string keyNameC = @"HKEY_CURRENT_USER\Software\BYTE";
                                    string valueNameC = "ACExecutable";
                                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                                    {
                                        Microsoft.Win32.RegistryKey key;
                                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                                        key.SetValue("ACExecutable", "BYTE.exe");
                                        key.Close();
                                    }
                                    else
                                    {
                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                                        key.SetValue("ACExecutable", "BYTE.exe");
                                        key.Close();
                                    }

                                    string keyNameD = @"HKEY_CURRENT_USER\Software\BYTE";
                                    string valueNameD = "Watcher";
                                    if (Registry.GetValue(keyNameB, valueNameB, null) == null)
                                    {
                                        Microsoft.Win32.RegistryKey key;
                                        key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\BYTE");
                                        key.SetValue("Watcher", "BYTEHelper.exe");
                                        key.Close();
                                    }
                                    else
                                    {
                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\BYTE", true);
                                        key.SetValue("Watcher", "BYTEHelper.exe");
                                        key.Close();
                                    }

                                    action = "wait";
                                    ActionButton.Content = "Please wait";
                                    iceMessage.Text = "Downloading required files..";
                                    ActionButton.IsEnabled = false; Directory.CreateDirectory(config[0]);
                                    Process[] process = Process.GetProcesses();

                                    foreach (Process prs in process)
                                    {
                                        if (prs.ProcessName.Contains("FiveM"))
                                        {
                                            KillProcess(prs.Id);
                                        }
                                        if (prs.ProcessName.Contains("BYTE") && !prs.ProcessName.Contains("Setup"))
                                        {
                                            KillProcess(prs.Id);
                                        }
                                    }
                                    await Task.Delay(1000);

                                    if (Directory.Exists(config[0]))
                                    {
                                        Directory.Delete(config[0], true);
                                    }
                                    Directory.CreateDirectory(config[0] + @"\bin");
                                    using (WebClient wc = new WebClient())
                                    {
                                        wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                                        using (HttpClient client = new HttpClient())
                                        {
                                            string s = await client.GetStringAsync(config[2]);
                                            wc.DownloadFileAsync(
                                                new System.Uri(s.RemoveWhitespace()),
                                                config[1]
                                            ); ;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            action = "install";
                            ActionButton.Content = "Install BYTE Anti-cheat";
                            iceMessage.Text = "Idle";
                            ActionButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        action = "install";
                        ActionButton.Content = "Install BYTE Anti-cheat";
                        iceMessage.Text = "Idle";
                        ActionButton.IsEnabled = true;
                    }
                 
                }
            }
            if(String.Equals(action, "uninstall"))
            {
                action = "wait";

                ActionButton.Content = "Please wait";
                ActionButton.IsEnabled = false;
                iceMessage.Text = "Uninstalling BYTE from your system..";
                Process[] process = Process.GetProcesses();

                foreach (Process prs in process)
                {
                    if (prs.ProcessName.Contains("FiveM"))
                    {
                        KillProcess(prs.Id);
                    }
                    if (prs.ProcessName.Contains("BYTE") && !prs.ProcessName.Contains("Setup"))
                    {
                        KillProcess(prs.Id);
                    }
                }
                await Task.Delay(1000);
                if (Directory.Exists(config[0]))
                {
                    Directory.Delete(config[0], true);
                }

                ActionButton.IsEnabled = true;
                action = "install";
                ActionButton.Content = "Install BYTE Anti-cheat";
                iceMessage.Text = "Completed.";
            }
        }
    }
}
