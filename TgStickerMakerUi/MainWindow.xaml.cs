using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TgStickerMaker;
using TgStickerMakerUi.ViewModels;

namespace TgStickerMakerUi
{
    public partial class MainWindow : Window
    {
        private bool _isDragging;
        private Point _startPoint;
        private TextBlock _selectedTextBlock;
        private TextBox _currentTextBox;
        private DateTime _lastClickTime;
        private Rect _mediaElementBounds;

        public MainWindow()
        {
            InitializeComponent();
            ServiceConfiguration.ConfigureServices();
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.RegisterMediaElement(MediaElement);

            // Подписка на событие KeyDown для обработки нажатия клавиш
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Снять фокус с текстового поля при нажатии Esc
                if (_currentTextBox != null)
                {
                    _currentTextBox.Visibility = Visibility.Collapsed;
                    var parent = (Grid)_currentTextBox.Parent;
                    var textBlock = parent?.FindName("TextBlockControl") as TextBlock;

                    if (textBlock != null)
                    {
                        textBlock.Text = _currentTextBox.Text;
                        textBlock.Visibility = Visibility.Visible;
                    }

                    _currentTextBox = null;
                    _isDragging = false;
                    e.Handled = true; // Помечаем событие как обработанное
                }
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                var currentTime = DateTime.Now;
                if ((currentTime - _lastClickTime).TotalMilliseconds < 500)
                {
                    // Handle double-click
                    var parent = (Grid)textBlock.Parent;
                    _currentTextBox = parent?.FindName("EditingTextBox") as TextBox;

                    if (_currentTextBox != null)
                    {
                        _currentTextBox.Text = textBlock.Text;
                        _currentTextBox.Visibility = Visibility.Visible;
                        _currentTextBox.Focus();
                        _currentTextBox.RenderTransform = textBlock.RenderTransform.Clone();
                        textBlock.Visibility = Visibility.Collapsed;
                        _selectedTextBlock = textBlock;
                    }
                }
                _lastClickTime = currentTime;

                _isDragging = true;
                _startPoint = e.GetPosition(OverlayCanvas);
                _selectedTextBlock = textBlock;
                _selectedTextBlock.CaptureMouse();
            }
        }

        private void MediaElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Получаем координаты MediaElement относительно родительского Grid
            Point position = MediaElement.TransformToVisual(MainGrid).Transform(new Point(0, 0));
            _mediaElementBounds = new Rect(position.X, position.Y, MediaElement.ActualWidth, MediaElement.ActualHeight);
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                // Получаем размер видео
                double width = mediaElement.NaturalVideoWidth;
                double height = mediaElement.NaturalVideoHeight;

                OverlayCanvas.Width = mediaElement.NaturalVideoWidth;
                OverlayCanvas.Height = mediaElement.NaturalVideoHeight;
                // Устанавливаем размер MediaElement
                mediaElement.Width = width;
                mediaElement.Height = height;
            }
        }

        private void TextBlock_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                var newSize = textBlock.FontSize + (e.Delta > 0 ? 2 : -2); // Увеличиваем или уменьшаем размер на 2
                textBlock.FontSize = Math.Max(8, newSize); // Устанавливаем минимальный размер шрифта

                var parent = (Grid)textBlock.Parent;
                var textBox = parent?.FindName("EditingTextBox") as TextBox;

                if (textBox != null)
                {
                    textBox.FontSize = textBlock.FontSize; // Синхронизируем размер шрифта в TextBox
                    var textOverlay = (TextOverlay)textBox.DataContext;
                    textOverlay.FontSize = textBox.FontSize;
                }
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectedTextBlock != null)
            {
                _isDragging = false;
                _selectedTextBlock.ReleaseMouseCapture();
                _selectedTextBlock = null;
            }
        }

        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedTextBlock != null)
            {
                var position = e.GetPosition(OverlayCanvas); // Получаем позицию относительно OverlayCanvas
                var globW = (OverlayCanvas.ActualWidth - MediaElement.ActualWidth) / 2;
                var globH = (OverlayCanvas.ActualHeight - MediaElement.ActualHeight) / 2;
                Logs.Text = $"{e.GetPosition(_selectedTextBlock).Y}";
                var textOverlay = (TextOverlay)_selectedTextBlock.DataContext;
                // Корректировка координат с учетом границ MediaElement
                if (position.X < 0)
                {
                    position.X = 0;
                }
                if (position.X > OverlayCanvas.ActualWidth - _selectedTextBlock.ActualWidth)
                {
                    position.X = OverlayCanvas.ActualWidth - _selectedTextBlock.ActualWidth;
                }
                if (position.Y < 0)
                {
                    position.Y = 0;
                }
                if (position.Y > OverlayCanvas.ActualHeight - _selectedTextBlock.ActualHeight)
                {
                    position.Y = OverlayCanvas.ActualHeight - _selectedTextBlock.ActualHeight;
                }

                double x = position.X;// Math.Max(_mediaElementBounds.Left, Math.Min(_mediaElementBounds.Right - _selectedTextBlock.ActualWidth, position.X));
                double y = position.Y;// Math.Max(_mediaElementBounds.Top, Math.Min(_mediaElementBounds.Bottom - _selectedTextBlock.ActualHeight, position.Y));

                // Обновляем координаты TextOverlay
                textOverlay.X = x;
                textOverlay.Y = y;
                _startPoint = position;
            }
        }

        private void ConstrainTextPosition(TextBlock textBlock)
        {
            if (textBlock == null) return;

            var transform = (TranslateTransform)textBlock.RenderTransform;
            double minX = _mediaElementBounds.Left;
            double minY = _mediaElementBounds.Top;
            double maxX = _mediaElementBounds.Right - textBlock.ActualWidth;
            double maxY = _mediaElementBounds.Bottom - textBlock.ActualHeight;

            transform.X = Math.Max(minX, Math.Min(maxX, transform.X));
            transform.Y = Math.Max(minY, Math.Min(maxY, transform.Y));
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_currentTextBox != null)
            {
                var parent = (Grid)_currentTextBox.Parent;
                var textBlock = parent?.FindName("TextBlockControl") as TextBlock;

                if (textBlock != null)
                {
                    textBlock.Text = _currentTextBox.Text;
                    textBlock.Visibility = Visibility.Visible;
                }

                _currentTextBox.Visibility = Visibility.Collapsed;
                _currentTextBox = null;
            }
        }
    }
}
