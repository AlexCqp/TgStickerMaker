using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using TgStickerMaker;
using TgStickerMakerUi.Commands;
using System.Windows.Controls;
using System.IO;

namespace TgStickerMakerUi.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _filePath;
        private Uri _videoSource;
        private MediaElement _mediaElement;
        private bool _loopVideo;
        private TextOverlay _selectedTextOverlay;
        private string _logOutput;

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public Uri VideoSource
        {
            get => _videoSource;
            set
            {
                _videoSource = value;
                OnPropertyChanged(nameof(VideoSource));
            }
        }

        public ObservableCollection<TextOverlay> TextOverlays { get; set; } = new ObservableCollection<TextOverlay>();

        public bool LoopVideo
        {
            get => _loopVideo;
            set
            {
                _loopVideo = value;
                OnPropertyChanged(nameof(LoopVideo));
            }
        }

        public string LogOutput
        {
            get => _logOutput;
            set
            {
                _logOutput = value;
                OnPropertyChanged(nameof(LogOutput));
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand AddTextCommand { get; }
        public ICommand SelectTextCommand { get; }
        public ICommand DeleteTextCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel()
        {
            SelectFileCommand = new RelayCommand(SelectFile);
            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            AddTextCommand = new RelayCommand(AddText);
            SelectTextCommand = new RelayCommand<TextOverlay>(SelectText);
            DeleteTextCommand = new RelayCommand<TextOverlay>(DeleteText);
            SaveCommand = new RelayCommand(Save);
        }

        private void Log(string message)
        {
            LogOutput += $"{message}\n";
        }

        private void SelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                if (FilePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    var gifName = Path.GetFileName(openFileDialog.FileName);
                    var mp4FilePath = TgStickerMaker.Helpers.WriteFilesHelper.GetUniqueFileName(Path.ChangeExtension(TgStickerMaker.ServiceConfiguration.Settings.TempFiltes, $"\\{gifName}.mp4"));
                    TgStickerMaker.Helpers.Converters.ConvertGifToMp4(FilePath, mp4FilePath);
                    VideoSource = new Uri(mp4FilePath);
                    Log($"Конвертировано GIF в MP4: {mp4FilePath}");
                }
                else
                {
                    VideoSource = new Uri(FilePath);
                    Log($"Выбран файл: {FilePath}");
                }

                _mediaElement.Play();
            }
        }

        private void Play()
        {
            if (_mediaElement != null)
            {
                _mediaElement.Play();
                Log("Видео воспроизводится.");
            }
        }

        private void Pause()
        {
            if (_mediaElement != null)
            {
                _mediaElement.Pause();
                Log("Видео приостановлено.");
            }
        }

        private void Stop()
        {
            if (_mediaElement != null)
            {
                _mediaElement.Stop();
                _mediaElement.Position = TimeSpan.Zero;
                Log("Видео остановлено.");
            }
        }

        private void AddText()
        {
            TextOverlay.Count++;
            TextOverlays.Add(new TextOverlay { Text = "Новый текст" });
            Log("Добавлен новый текст.");
        }

        private void SelectText(TextOverlay textOverlay)
        {
            _selectedTextOverlay = textOverlay;
            Log($"Выбран текст: {textOverlay.Text}");
        }

        private void DeleteText(TextOverlay textOverlay)
        {
            if (textOverlay != null)
            {
                TextOverlays.Remove(textOverlay);
                Log($"Удален текст: {textOverlay.Text}");
            }
        }

        private async void Save()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                Log("Ошибка: Файл не выбран.");
                return;
            }

            try
            {
                // Обновляем координаты для всех текстовых наложений
                foreach (var textOverlay in TextOverlays)
                {
                    // Ваша логика обновления координат, если нужно
                    // Например:
                    textOverlay.X = Math.Max(0, textOverlay.X); // Убедитесь, что координаты не отрицательны
                    textOverlay.Y = Math.Max(0, textOverlay.Y);
                }

                // Сборка данных о тексте и координатах
                var textOverlays = TextOverlays.ToArray();
                var duration = _mediaElement.NaturalDuration.HasTimeSpan ? _mediaElement.NaturalDuration.TimeSpan.TotalSeconds : 0;

                // Запуск процесса обработки видео
                var outputFilePath = await TgStickerMaker.StickerMaker.ProcessVideo(FilePath, textOverlays, duration);
                Log($"Видео сохранено: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Log($"Ошибка сохранения видео: {ex.Message}");
            }
        }


        public void RegisterMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.LoadedBehavior = MediaState.Manual;
            _mediaElement.MediaEnded += (sender, e) =>
            {
                if (LoopVideo)
                {
                    _mediaElement.Position = TimeSpan.Zero;
                    _mediaElement.Play();
                    Log("Видео зациклено.");
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
