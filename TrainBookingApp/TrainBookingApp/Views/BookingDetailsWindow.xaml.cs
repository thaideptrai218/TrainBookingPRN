using System.Windows;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class BookingDetailsWindow : Window
{
    public BookingDetailsWindow(BookingDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribe to close request
        viewModel.RequestClose += () => Close();
    }
}