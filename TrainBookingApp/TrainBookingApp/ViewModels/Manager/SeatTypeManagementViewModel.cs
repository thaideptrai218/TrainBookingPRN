using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class SeatTypeManagementViewModel : BaseManagerViewModel, IManagerTabViewModel
{
    private readonly ISeatTypeService _seatTypeService;

    public SeatTypeManagementViewModel(ISeatTypeService seatTypeService)
    {
        _seatTypeService = seatTypeService ?? throw new ArgumentNullException(nameof(seatTypeService));
        
        SeatTypes = new ObservableCollection<SeatType>();
        
        AddCommand = new RelayCommand(_ => Add());
        EditCommand = new RelayCommand(param => Edit(param as SeatType), param => CanEdit(param as SeatType));
        DeleteCommand = new RelayCommand(param => Delete(param as SeatType), param => CanDelete(param as SeatType));
        SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel(), _ => CanCancel());
        // Override the base RefreshCommand to use our specific implementation
        RefreshCommand = new RelayCommand(async _ => await LoadSeatTypesAsync());
        
        CurrentSeatType = new SeatType();
        
        _ = LoadSeatTypesAsync();
    }

    public ObservableCollection<SeatType> SeatTypes { get; }

    private SeatType _currentSeatType = new();
    public SeatType CurrentSeatType
    {
        get => _currentSeatType;
        set
        {
            if (SetProperty(ref _currentSeatType, value))
            {
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAdding));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanCancel));
            }
        }
    }

    private SeatType? _selectedSeatType;
    public SeatType? SelectedSeatType
    {
        get => _selectedSeatType;
        set
        {
            if (SetProperty(ref _selectedSeatType, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
            }
        }
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAdding));
            }
        }
    }

    public bool IsEditing => IsEditMode && CurrentSeatType.SeatTypeId > 0;
    public bool IsAdding => IsEditMode && CurrentSeatType.SeatTypeId == 0;

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterSeatTypes();
            }
        }
    }

    private string _filterBy = "All";
    public string FilterBy
    {
        get => _filterBy;
        set
        {
            if (SetProperty(ref _filterBy, value))
            {
                FilterSeatTypes();
            }
        }
    }

    public List<string> FilterOptions { get; } = new() { "All", "Berth", "Non-Berth", "Upper Berth", "Middle Berth", "Lower Berth" };

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string _successMessage = "";
    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public new ICommand RefreshCommand { get; }

    private async Task LoadSeatTypesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await Task.Run(() =>
            {
                var seatTypes = _seatTypeService.GetAllSeatTypes();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SeatTypes.Clear();
                    foreach (var seatType in seatTypes)
                    {
                        SeatTypes.Add(seatType);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load seat types: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterSeatTypes()
    {
        try
        {
            var allSeatTypes = _seatTypeService.GetAllSeatTypes();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                allSeatTypes = allSeatTypes.Where(st => 
                    st.TypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (st.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Apply category filter
            allSeatTypes = FilterBy switch
            {
                "Berth" => allSeatTypes.Where(st => st.BerthLevel.HasValue),
                "Non-Berth" => allSeatTypes.Where(st => !st.BerthLevel.HasValue),
                "Upper Berth" => allSeatTypes.Where(st => st.BerthLevel == 3),
                "Middle Berth" => allSeatTypes.Where(st => st.BerthLevel == 2),
                "Lower Berth" => allSeatTypes.Where(st => st.BerthLevel == 1),
                _ => allSeatTypes
            };

            SeatTypes.Clear();
            foreach (var seatType in allSeatTypes)
            {
                SeatTypes.Add(seatType);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to filter seat types: {ex.Message}";
        }
    }

    private void Add()
    {
        CurrentSeatType = new SeatType
        {
            PriceMultiplier = 1.0m,
            BerthLevel = null
        };
        IsEditMode = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private bool CanEdit(SeatType? seatType)
    {
        return seatType != null && !IsEditMode;
    }

    private void Edit(SeatType? seatType)
    {
        if (seatType != null)
        {
            CurrentSeatType = new SeatType
            {
                SeatTypeId = seatType.SeatTypeId,
                TypeName = seatType.TypeName,
                PriceMultiplier = seatType.PriceMultiplier,
                Description = seatType.Description,
                BerthLevel = seatType.BerthLevel
            };
            IsEditMode = true;
            ErrorMessage = null;
            SuccessMessage = null;
        }
    }

    private bool CanDelete(SeatType? seatType)
    {
        return seatType != null && !IsEditMode;
    }

    private async void Delete(SeatType? seatType)
    {
        if (seatType == null) return;

        try
        {
            var isInUse = await Task.Run(() => _seatTypeService.IsSeatTypeInUse(seatType.SeatTypeId));
            if (isInUse)
            {
                ErrorMessage = "Cannot delete this seat type because it is currently in use by one or more seats.";
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the seat type '{seatType.TypeName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await Task.Run(() => _seatTypeService.DeleteSeatType(seatType.SeatTypeId));
                if (success)
                {
                    SeatTypes.Remove(seatType);
                    SuccessMessage = "Seat type deleted successfully.";
                    ErrorMessage = null;
                }
                else
                {
                    ErrorMessage = "Failed to delete the seat type.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete seat type: {ex.Message}";
        }
    }

    private bool CanSave()
    {
        return IsEditMode && 
               !string.IsNullOrWhiteSpace(CurrentSeatType.TypeName) &&
               CurrentSeatType.PriceMultiplier > 0 &&
               (!CurrentSeatType.BerthLevel.HasValue || CurrentSeatType.BerthLevel >= 1 && CurrentSeatType.BerthLevel <= 3);
    }

    private async void Save()
    {
        try
        {
            ErrorMessage = null;

            var isNameUnique = await Task.Run(() => 
                _seatTypeService.IsSeatTypeNameUnique(CurrentSeatType.TypeName, CurrentSeatType.SeatTypeId == 0 ? null : CurrentSeatType.SeatTypeId));

            if (!isNameUnique)
            {
                ErrorMessage = "A seat type with this name already exists.";
                return;
            }

            SeatType savedSeatType;
            if (CurrentSeatType.SeatTypeId == 0)
            {
                savedSeatType = await Task.Run(() => _seatTypeService.CreateSeatType(CurrentSeatType));
                SeatTypes.Add(savedSeatType);
                SuccessMessage = "Seat type created successfully.";
            }
            else
            {
                savedSeatType = await Task.Run(() => _seatTypeService.UpdateSeatType(CurrentSeatType));
                if (savedSeatType != null)
                {
                    var existingIndex = SeatTypes.ToList().FindIndex(st => st.SeatTypeId == savedSeatType.SeatTypeId);
                    if (existingIndex >= 0)
                    {
                        SeatTypes[existingIndex] = savedSeatType;
                    }
                    SuccessMessage = "Seat type updated successfully.";
                }
                else
                {
                    ErrorMessage = "Failed to update the seat type.";
                    return;
                }
            }

            IsEditMode = false;
            CurrentSeatType = new SeatType();
            SelectedSeatType = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save seat type: {ex.Message}";
        }
    }

    private bool CanCancel()
    {
        return IsEditMode;
    }

    private void Cancel()
    {
        IsEditMode = false;
        CurrentSeatType = new SeatType();
        ErrorMessage = null;
        SuccessMessage = null;
        SelectedSeatType = null;
    }

    public override string TabName => "Seat Types";

    public override void RefreshData()
    {
        _ = LoadSeatTypesAsync();
    }

    public override void ClearForm()
    {
        if (IsEditMode)
        {
            Cancel();
        }
    }
}