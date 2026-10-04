using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AirlyATC.Models;
using AirlyATC.Services;

namespace AirlyATC;

public partial class MainWindow : Window
{
    private readonly AtcPositionService _positionService = new();
    private readonly TrafficService _trafficService = new();
    private readonly ObservableCollection<AtcPosition> _visiblePositions = new();
    private readonly ObservableCollection<FlightStrip> _strips = new();
    private readonly DispatcherTimer _timer;
    private AtcPosition? _selectedPosition;
    private double _zoom = 1.0;

    private const string CurrentController = "AIRLY-DEV-0001";

    public MainWindow()
    {
        InitializeComponent();

        foreach (var position in _positionService.GetAll())
            _visiblePositions.Add(position);

        PositionList.ItemsSource = _visiblePositions;
        StripList.ItemsSource = _strips;

        _selectedPosition = _visiblePositions.FirstOrDefault();
        PositionList.SelectedItem = _selectedPosition;
        RefreshSelection();
        RefreshTraffic();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        _timer.Tick += (_, _) =>
        {
            _trafficService.Tick();
            ClockText.Text = DateTime.UtcNow.ToString("HH:mm:ss") + "Z";
            RefreshTraffic();
        };
        _timer.Start();
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _visiblePositions.Clear();
        foreach (var position in _positionService.Search(SearchBox.Text))
            _visiblePositions.Add(position);

        if (_selectedPosition is null || !_visiblePositions.Contains(_selectedPosition))
            _selectedPosition = _visiblePositions.FirstOrDefault();

        PositionList.SelectedItem = _selectedPosition;
        RefreshSelection();
    }

    private void PositionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedPosition = PositionList.SelectedItem as AtcPosition;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (_selectedPosition is null)
            return;

        SelectedPositionText.Text = $"{_selectedPosition.AirportIcao} {_selectedPosition.PositionName}";
        SelectedAirportText.Text = $"{_selectedPosition.AirportName} • {_selectedPosition.Frequency} MHz";
        ScopeTitle.Text = $"{_selectedPosition.AirportIcao} {_selectedPosition.PositionName} • {_selectedPosition.Frequency}";
        Com1Text.Text = _selectedPosition.Frequency;
        ControlStatusText.Text = _selectedPosition.IsHumanControlled
            ? $"HUMAN • {_selectedPosition.HumanController}"
            : "AIRLY AI";
        QueueText.Text = _selectedPosition.QueueLabel;

        ControlButton.IsEnabled = !_selectedPosition.IsHumanControlled;
        QueueButton.IsEnabled = _selectedPosition.IsHumanControlled &&
                                !_selectedPosition.ControllerQueue.Contains(CurrentController);
        ReleaseButton.IsEnabled = string.Equals(_selectedPosition.HumanController, CurrentController, StringComparison.OrdinalIgnoreCase);

        PositionList.Items.Refresh();
    }

    private void ControlButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedPosition is null)
            return;

        if (_positionService.TakePosition(_selectedPosition, CurrentController))
            RefreshSelection();
        else
            System.Windows.MessageBox.Show("This position is already human-controlled. Join the queue.", "Airly ATC", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void QueueButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedPosition is null)
            return;

        if (_positionService.JoinQueue(_selectedPosition, CurrentController))
            RefreshSelection();
        else
            System.Windows.MessageBox.Show("A human controller must currently own this position before you can queue.", "Airly ATC", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReleaseButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedPosition is null ||
            !string.Equals(_selectedPosition.HumanController, CurrentController, StringComparison.OrdinalIgnoreCase))
            return;

        _positionService.ReleasePosition(_selectedPosition);
        RefreshSelection();
    }

    private void RefreshTraffic()
    {
        _strips.Clear();

        foreach (var aircraft in _trafficService.Aircraft)
        {
            _strips.Add(new FlightStrip
            {
                Callsign = aircraft.Callsign,
                AircraftType = aircraft.AircraftType,
                From = aircraft.From,
                To = aircraft.To,
                Altitude = $"FL{aircraft.Altitude / 100:000}",
                Squawk = aircraft.Squawk,
                Route = aircraft.Route
            });
        }

        TrafficCountText.Text = $"{_trafficService.Aircraft.Count} aircraft";
        DrawRadar();
    }

    private void DrawRadar()
    {
        RadarCanvas.Children.Clear();

        var width = Math.Max(1, RadarCanvas.ActualWidth);
        var height = Math.Max(1, RadarCanvas.ActualHeight);
        var cx = width / 2;
        var cy = height / 2;
        var radius = Math.Min(width, height) * .39 * _zoom;

        for (var ring = .25; ring <= 1.0; ring += .25)
        {
            var size = radius * 2 * ring;
            RadarCanvas.Children.Add(new Ellipse
            {
                Width = size,
                Height = size,
                Stroke = new SolidColorBrush(Color.FromRgb(28, 62, 64)),
                StrokeThickness = 1,
                Opacity = .8
            });
            Canvas.SetLeft(RadarCanvas.Children[^1], cx - size / 2);
            Canvas.SetTop(RadarCanvas.Children[^1], cy - size / 2);
        }

        foreach (var aircraft in _trafficService.Aircraft)
        {
            var x = aircraft.X * width;
            var y = aircraft.Y * height;

            var tag = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(210, 7, 18, 20)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(55, 92, 96)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 2, 4, 2),
                Child = new TextBlock
                {
                    Text = $"{aircraft.Callsign}  {aircraft.Altitude / 100:000}",
                    FontSize = 10,
                    Foreground = Brushes.White
                }
            };

            RadarCanvas.Children.Add(tag);
            Canvas.SetLeft(tag, Math.Clamp(x, 4, width - 100));
            Canvas.SetTop(tag, Math.Clamp(y, 4, height - 25));

            var vector = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x + Math.Sin(aircraft.Heading * Math.PI / 180) * 20,
                Y2 = y - Math.Cos(aircraft.Heading * Math.PI / 180) * 20,
                Stroke = new SolidColorBrush(Color.FromRgb(78, 192, 199)),
                StrokeThickness = 1
            };
            RadarCanvas.Children.Add(vector);
        }
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        _zoom = Math.Min(1.8, _zoom + .1);
        DrawRadar();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        _zoom = Math.Max(.7, _zoom - .1);
        DrawRadar();
    }

    private void WorldButton_Click(object sender, RoutedEventArgs e) => SearchBox.Text = "";
    private void AirportButton_Click(object sender, RoutedEventArgs e) => SearchBox.Focus();
    private void RadarButton_Click(object sender, RoutedEventArgs e) => RadarCanvas.Focus();
    private void StripsButton_Click(object sender, RoutedEventArgs e) => StripList.Focus();
    private void AtisButton_Click(object sender, RoutedEventArgs e) =>
        System.Windows.MessageBox.Show("ATIS service is connected to this controller position. Live network ATIS will be supplied by the Airly server.", "Airly ATIS");
    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        System.Windows.MessageBox.Show("Controller settings will be persisted by the Airly account service.", "Airly Settings");

    private void SendMessage_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show($"COM1 {_selectedPosition?.Frequency ?? "—"}\n\n{RadioMessageBox.Text}", "Airly Radio");
        RadioMessageBox.Text = "";
    }
}
