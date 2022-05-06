﻿using System.Windows;
using System;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using System.Windows.Threading;
using System.Diagnostics;



namespace RedTech
{

    public partial class MainWindow : Window
    {
        string dir = null;
        private BackgroundWorker backgroundWorkerUnpacker = null;
        string _username = null;

        int MaxName = 4;
        public MainWindow()
        {
            InitializeComponent();
            dir = AppDomain.CurrentDomain.BaseDirectory;
            Debug("main program started");
            // Debug(dir);
        }
        void Debug(string text)
        {  
            var file_path_debug = dir + "/log";
            try
            {
                if (!Directory.Exists(file_path_debug))
                {
                    Directory.CreateDirectory(file_path_debug);
                    //запись в текст
                    if (!File.Exists(file_path_debug + "/debug.log"))
                    {
                        File.Create(file_path_debug + "/debug.log").Close();
                        File.AppendAllText(file_path_debug + "/debug.log", "[ " + DateTime.Now + " ]: " + text + Environment.NewLine);
                    }
                    else
                    {
                        File.AppendAllText(file_path_debug + "/debug.log", "[ " + DateTime.Now + " ]: " + text + Environment.NewLine);
                    }
                }
                else
                {
                    //запись в текст
                    File.AppendAllText(file_path_debug + "/debug.log", "[ " + DateTime.Now + " ]: " + text + Environment.NewLine);
                }
            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //покажим ошибку если что
            }
        }
        private void DownloadGame(Uri Url) //загрузка игры
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    web.DownloadFileAsync(Url, FileData.NameArchiveGame);
                    web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(web_DownloadProgressChanged);
                    web.DownloadFileCompleted += new AsyncCompletedEventHandler(web_DownloadFileCompleted);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                //то что будет после скачивания файла
                Debug("Download File " + FileData.NameArchiveGame + " Completed");
                Debug("Start unzip " + FileData.NameArchiveGame + "archive");
                if (File.Exists(dir + FileData.NameArchiveGame))
                {
                    if (!Directory.Exists(dir + "/bin"))
                    {
                        Directory.CreateDirectory(dir + "/bin");
                    }
                    else
                    {
                        StartUnpack();
                    }
                }
                else Debug("Not File " + FileData.NameArchiveGame);
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = e.BytesReceived;
            double totalBytes = e.TotalBytesToReceive;
            double percentage = bytesIn / totalBytes * 100;
            progress.Value = (int)percentage;
        }
        private bool IsInstall()
        {
            string[] game_info =
            {
                "gta_sa.exe",
                "samp.exe",
            };

            for(int i = 0; i < game_info.Length; i++)
            {
                if (!File.Exists(dir + "/bin/" + game_info[i]))
                    return false;
            }
            return true;
        }
        private void StartUnpack()
        {
            try
            {
                backgroundWorkerUnpacker = new BackgroundWorker();
                backgroundWorkerUnpacker.DoWork += BackgroundWorkerUnpacker_DoWork;
                backgroundWorkerUnpacker.WorkerReportsProgress = true;
                backgroundWorkerUnpacker.WorkerSupportsCancellation = true;
                backgroundWorkerUnpacker.RunWorkerAsync();
            }catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void BackgroundWorkerUnpacker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {     
                Dispatcher.BeginInvoke(new Action(() =>
                    ZipFile.ExtractToDirectory(FileData.NameArchiveGame, dir + "/bin/")
                ));

            }
            catch(Exception ex) {  MessageBox.Show(ex.Message); }
        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e) {
            if (NickName.Text.Length < MaxName || NickName.Text.Length == 0)
            {
                MessageBox.Show("Ник не указан!");
            }
            else
            {
                _username = NickName.Text;
                if (IsInstall())
                {
                    GameInit(); //парамс
                }
                else
                {
                    DownloadGame(new Uri(DownloadData.UrlGameDownload)); Debug("Download: " + DownloadData.UrlGameDownload);
                }
            }
        }
        private void GameInit()
        {
            var _file_name = "samp.exe";

            string arguments_start_game = "-n " + _username + "-h " + GameData.server_ip + "-p " + GameData.server_port;
                
            if (IsInstall())
            {
                //запуск
                Process.Start(dir + "/bin/" + _file_name, arguments_start_game);
            }
            else
            {
                MessageBox.Show("Установите игру или запустите проверку файлов!");
            }
            
        }
    }
}
