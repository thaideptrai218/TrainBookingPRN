using System.Windows;
using TrainBookingApp.Models;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class BookingConfirmationWindow : Window
{
    private readonly BookingConfirmationViewModel _viewModel;

    public BookingConfirmationWindow(BookingConfirmationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        // Subscribe to ViewModel events
        _viewModel.BookingCompleted += OnBookingCompleted;
        _viewModel.BookingCancelled += OnBookingCancelled;
    }

    private void OnBookingCompleted(Booking booking)
    {
        MessageBox.Show($"Booking confirmed successfully!\n\nBooking Reference: {booking.BookingCode}\nTotal Amount: {booking.TotalPrice:C}", 
                       "Booking Confirmed", 
                       MessageBoxButton.OK, 
                       MessageBoxImage.Information);
        
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
        _viewModel.BookingCompleted -= OnBookingCompleted;
        _viewModel.BookingCancelled -= OnBookingCancelled;
        
        base.OnClosed(e);
    }
}