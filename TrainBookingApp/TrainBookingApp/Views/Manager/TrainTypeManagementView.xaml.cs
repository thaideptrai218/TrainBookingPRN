using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class TrainTypeManagementView : UserControl
{
    private TrainTypeManagementViewModel ViewModel => (TrainTypeManagementViewModel)DataContext;

    public TrainTypeManagementView()
    {
        InitializeComponent();
    }

    private void TrainTypeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedTrainType != null)
        {
            ViewModel.LoadTrainTypeToForm();
        }
    }
}