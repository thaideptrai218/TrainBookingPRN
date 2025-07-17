using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class TripManagementView : UserControl
{
    private TripManagementViewModel ViewModel => (TripManagementViewModel)DataContext;

    public TripManagementView()
    {
        InitializeComponent();
    }

    private void TripDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedTrip != null)
        {
            ViewModel.LoadTripToForm();
        }
    }
}