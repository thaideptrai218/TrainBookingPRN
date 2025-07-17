using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class RouteManagementView : UserControl
{
    private RouteManagementViewModel ViewModel => (RouteManagementViewModel)DataContext;

    public RouteManagementView()
    {
        InitializeComponent();
    }

    private void RouteDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedRoute != null)
        {
            ViewModel.LoadRouteToForm();
        }
    }
}