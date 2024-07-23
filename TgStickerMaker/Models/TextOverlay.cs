using System.ComponentModel;

public class TextOverlay : INotifyPropertyChanged
{
    public static int Count;

    private double _x;
    private double _y;
    private string _text;
    private double _fontSize;

    public double YOffset = Count * 50;

    public double X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged(nameof(X));
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged(nameof(Y));
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            OnPropertyChanged(nameof(FontSize));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
