using System.Windows.Controls;
using TrainBookingApp.ViewModels.Manager;

namespace TrainBookingApp.Views.Manager;

public partial class PricingRuleManagementView : UserControl
{
    private PricingRuleManagementViewModel ViewModel => (PricingRuleManagementViewModel)DataContext;

    public PricingRuleManagementView()
    {
        InitializeComponent();
    }

    private void PricingRuleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedPricingRule != null)
        {
            ViewModel.LoadPricingRuleToForm();
        }
    }
}