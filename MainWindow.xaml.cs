using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AirlyATC.Models;
using AirlyATC.Services;

namespace AirlyATC;

public partial class MainWindow : Window
{
    private readonly AtcPositionService _positionService = new();
    private readonly ObservableCollection<AtcPosition> _visiblePositions = new();

    // Temporary development identity. This will come from Airly authentication.
    private const string CurrentController = "AIRLY-DEV-0001";

    public MainWindow()
    {
        InitializeComponent();

        foreach (var position in _positionService.GetAll())
            _visiblePositions.Add(position);

        PositionList.ItemsSource = _visiblePositions;
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _visiblePositions.Clear();

        foreach (var position in _positionService.Search(SearchBox.Text))
            _visiblePositions.Add(position);

        RefreshSelection();
    }

    private void PositionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (PositionList.SelectedItem is not AtcPosition position)
        {
            SelectedPositionText.Text = "Select an ATC position";
            SelectedAirportText.Text = "Choose an airport position from the left.";
            ControlStatusText.Text = "AIRLY AI";
            FrequencyText.Text = "—";
            QueueText.Text = "Queue empty";
            ControlButton.IsEnabled = false;
            QueueButton.IsEnabled = false;
            return;
        }

        SelectedPositionText.Text = $"{position.AirportIcao} {position.PositionName.ToUpperInvariant()}";
        SelectedAirportText.Text = $"{position.AirportName} • {position.Frequency} MHz";
        ControlStatusText.Text = position.IsHumanControlled
            ? $"HUMAN • {position.HumanController}"
            : "AIRLY AI";
        FrequencyText.Text = $"Frequency {position.Frequency} MHz";
        QueueText.Text = position.ControllerQueue.Count == 0
            ? "Queue empty"
            : $"{position.ControllerQueue.Count} controller(s) waiting";

        ControlButton.IsEnabled = !position.IsHumanControlled;
        QueueButton.IsEnabled = position.IsHumanControlled &&
                                !position.ControllerQueue.Contains(CurrentController);
    }

    private void ControlButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (PositionList.SelectedItem is not AtcPosition position)
            return;

        if (!_positionService.TakePosition(position, CurrentController))
        {
            MessageBox.Show(
                "This position is already controlled. Join the queue instead.",
                "Airly ATC",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            RefreshSelection();
            return;
        }

        RefreshPositionList();
        RefreshSelection();
    }

    private void QueueButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (PositionList.SelectedItem is not AtcPosition position)
            return;

        if (!_positionService.JoinQueue(position, CurrentController))
        {
            MessageBox.Show(
                "Unable to join this position's queue.",
                "Airly ATC",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        RefreshPositionList();
        RefreshSelection();
    }

    private void RefreshPositionList()
    {
        var selectedId = (PositionList.SelectedItem as AtcPosition)?.Id;

        PositionList.Items.Refresh();

        if (selectedId is null)
            return;

        var selected = _visiblePositions.FirstOrDefault(x => x.Id == selectedId);
        if (selected is not null)
            PositionList.SelectedItem = selected;
    }
}
