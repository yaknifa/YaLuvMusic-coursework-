using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Win32;
// ДЛЯ КОНВЕРТАЦИИ
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using NAudio.Lame;
// для аудиофайлов формата WAV
using NAudio.Wave;
// для аудиофайлов формата ААС
using MediaToolkit;
using MediaToolkit.Model;





namespace YaMusicLuv

    {
        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                formatComboBox.SelectedIndex = 0;
        }
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string youtubeUrl = urlTextBox.Text;
            string selectedFormat = (formatComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            statusTextBlock.Text = "Обработка...";

            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(youtubeUrl);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (audioStreamInfo != null)
                {
                    string tempAudioPath = Path.Combine(Path.GetTempPath(), $"{video.Title}.mp4");
                    string outputExtension = selectedFormat.ToLower();

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = video.Title,
                        DefaultExt = outputExtension,
                        Filter = $"{selectedFormat.ToUpper()} files|*.{outputExtension}|All files|*.*"
                    };

                    bool? result = saveFileDialog.ShowDialog();

                    if (result != true)
                    {
                        statusTextBlock.Text = "Сохранение аудиофайла отменено пользователем.";
                        return;
                    }

                    string outputPath = saveFileDialog.FileName;

                    statusTextBlock.Text = "Скачивание аудиопотока из видео...";
                    await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempAudioPath);

                    statusTextBlock.Text = $"Преобразование в {selectedFormat.ToUpper()}...";
                    ConvertAudio(tempAudioPath, outputPath, selectedFormat);

                    File.Delete(tempAudioPath);
                    statusTextBlock.Text = $"Файл сохранён в следующем месте:\n{outputPath}";
                }
                else
                {
                    statusTextBlock.Text = "Не удалось найти аудиопоток.";
                }
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = $"Ошибка: дана неверная ссылка.\nПожалуйста, попробуйте ещё раз.";
            }
        }

        static void ConvertAudio(string inputPath, string outputPath, string format)
        {
            switch (format.ToLower())
            {
                case "mp3":
                    using (var reader = new MediaFoundationReader(inputPath))
                    using (var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, 320))
                    {
                        reader.CopyTo(writer);
                    }
                    break;

                case "wav":
                    using (var reader = new MediaFoundationReader(inputPath))
                    using (var writer = new WaveFileWriter(outputPath, reader.WaveFormat))
                    {
                        reader.CopyTo(writer);
                    }
                    break;

                case "aac":
                    var inputFile = new MediaFile { Filename = inputPath };
                    var outputFile = new MediaFile { Filename = outputPath };

                    using (var engine = new Engine())
                    {
                        engine.Convert(inputFile, outputFile);
                    }
                    break;


                default:
                    throw new NotSupportedException($"Формат {format} не поддерживается.");
            }
        }

        private void GoToYouTubeButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.youtube.com/")
            {
                UseShellExecute = true
            });
        }
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            string textToShow = "Данная программа работает по простейшему принципу:\n1) Нажимаете кнопку \"Перейти в YouTube\"; \n2) Находите интересующий Вас трек на сайте\n3) Копируете URL-ссылку из браузера;\n4) Вставляете ссылку в соответствующее текстовое поле; \n5) Выбираете нужный аудиоформат для аудиофайла через контекстное меню (доступны .mp3, .wav, .aac), по умолчанию это будет .mp3\n6) Нажимаете на кнопку \"Скачать аудиофайл\"; \n7) Если ссылка является переходом к видео на YouTube, через некоторое время, программа предложит место для сохранения будущего файла; \n8) Файл скачается на Ваш компьютер, об этом будет соответствующее оповещение.";
            MessageBox.Show(textToShow, "Как работает данная программа?", MessageBoxButton.OK);
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

    }
}



