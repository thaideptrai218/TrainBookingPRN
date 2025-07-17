using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class StationManagementView : UserControl
{
    private StationManagementViewModel ViewModel => (StationManagementViewModel)DataContext;

    public StationManagementView()
    {
        InitializeComponent();
    }

    private void StationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedStation != null)
        {
            ViewModel.LoadStationToForm();
        }
    }
}