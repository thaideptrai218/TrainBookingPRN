namespace TrainBookingApp.ViewModels.Manager;

public interface IManagerTabViewModel
{
    string TabName { get; }
    void RefreshData();
    void ClearForm();
    string StatusMessage { get; set; }
    bool IsLoading { get; set; }
}