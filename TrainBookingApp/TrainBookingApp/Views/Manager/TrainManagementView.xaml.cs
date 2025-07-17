using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class TrainManagementView : UserControl
{
    private TrainManagementViewModel ViewModel => (TrainManagementViewModel)DataContext;

    public TrainManagementView()
    {
        InitializeComponent();
    }

    private void TrainDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedTrain != null)
        {
            ViewModel.LoadTrainToForm();
        }
    }
}