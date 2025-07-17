using System.Windows.Input;

namespace TrainBookingApp.ViewModels.Manager;

public abstract class BaseManagerViewModel : BaseViewModel, IManagerTabViewModel
{
    private bool _isLoading = false;
    private string _statusMessage = string.Empty;

    public abstract string TabName { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand RefreshCommand { get; protected set; } = null!;
    public ICommand ClearFormCommand { get; protected set; } = null!;

    protected BaseManagerViewModel()
    {
        RefreshCommand = new RelayCommand(_ => RefreshData());
        ClearFormCommand = new RelayCommand(_ => ClearForm());
    }

    public abstract void RefreshData();
    public abstract void ClearForm();

    protected void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    protected void SetSuccessMessage(string message)
    {
        StatusMessage = $"✓ {message}";
    }

    protected void SetErrorMessage(string message)
    {
        StatusMessage = $"✗ {message}";
    }

    protected void SetLoadingState(bool isLoading, string message = "")
    {
        IsLoading = isLoading;
        if (!string.IsNullOrEmpty(message))
        {
            StatusMessage = message;
        }
    }
}