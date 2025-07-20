using System.Windows;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class PassengerDetailsWindow : Window
{
    private readonly PassengerDetailsViewModel _viewModel;

    public PassengerDetailsWindow(PassengerDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to ViewModel events
        _viewModel.PassengerDetailsCompleted += OnPassengerDetailsCompleted;
        _viewModel.BookingCancelled += OnBookingCancelled;
    }

    private void OnPassengerDetailsCompleted(List<PassengerInfo> passengers)
    {
        DialogResult = true;
        Close();
    }

    private void OnBookingCancelled()
    {
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        _viewModel.PassengerDetailsCompleted -= OnPassengerDetailsCompleted;
        _viewModel.BookingCancelled -= OnBookingCancelled;
        
        base.OnClosed(e);
    }
}