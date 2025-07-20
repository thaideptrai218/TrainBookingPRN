using System.Windows;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class SeatSelectionWindow : Window
{
    private readonly SeatSelectionViewModel _viewModel;

    public SeatSelectionWindow(SeatSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to ViewModel events
        _viewModel.SeatSelectionCompleted += OnSeatSelectionCompleted;
        _viewModel.SelectionCancelled += OnSelectionCancelled;
    }

    private void OnSeatSelectionCompleted(List<TrainBookingApp.Models.Seat> selectedSeats)
    {
        DialogResult = true;
        Close();
    }

    private void OnSelectionCancelled()
    {
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        _viewModel.SeatSelectionCompleted -= OnSeatSelectionCompleted;
        _viewModel.SelectionCancelled -= OnSelectionCancelled;
        
        base.OnClosed(e);
    }
}