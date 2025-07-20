using System.Windows;
using TrainBookingApp.ViewModels;

namespace TrainBookingApp.Views;

public partial class CustomerWindow : Window
{
    public CustomerWindow()
    {
        InitializeComponent();
    }

    public CustomerWindow(CustomerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}