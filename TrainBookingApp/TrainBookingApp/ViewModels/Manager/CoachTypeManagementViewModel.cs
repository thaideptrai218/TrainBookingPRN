using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class CoachTypeManagementViewModel : BaseManagerViewModel, IManagerTabViewModel
{
    private readonly ICoachTypeService _coachTypeService;

    public CoachTypeManagementViewModel(ICoachTypeService coachTypeService)
    {
        _coachTypeService = coachTypeService ?? throw new ArgumentNullException(nameof(coachTypeService));
        
        CoachTypes = new ObservableCollection<CoachType>();
        
        AddCommand = new RelayCommand(_ => Add());
        EditCommand = new RelayCommand(param => Edit(param as CoachType), param => CanEdit(param as CoachType));
        DeleteCommand = new RelayCommand(param => Delete(param as CoachType), param => CanDelete(param as CoachType));
        SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel(), _ => CanCancel());
        // Override the base RefreshCommand to use our specific implementation
        RefreshCommand = new RelayCommand(async _ => await LoadCoachTypesAsync());
        
        CurrentCoachType = new CoachType();
        
        _ = LoadCoachTypesAsync();
    }

    public ObservableCollection<CoachType> CoachTypes { get; }

    private CoachType _currentCoachType = new();
    public CoachType CurrentCoachType
    {
        get => _currentCoachType;
        set
        {
            if (SetProperty(ref _currentCoachType, value))
            {
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAdding));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanCancel));
            }
        }
    }

    private CoachType? _selectedCoachType;
    public CoachType? SelectedCoachType
    {
        get => _selectedCoachType;
        set
        {
            if (SetProperty(ref _selectedCoachType, value))
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

    public bool IsEditing => IsEditMode && CurrentCoachType.CoachTypeId > 0;
    public bool IsAdding => IsEditMode && CurrentCoachType.CoachTypeId == 0;

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterCoachTypes();
            }
        }
    }

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

    private async Task LoadCoachTypesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await Task.Run(() =>
            {
                var coachTypes = _coachTypeService.GetAllCoachTypes();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CoachTypes.Clear();
                    foreach (var coachType in coachTypes)
                    {
                        CoachTypes.Add(coachType);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load coach types: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterCoachTypes()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _ = LoadCoachTypesAsync();
            return;
        }

        try
        {
            var allCoachTypes = _coachTypeService.GetAllCoachTypes();
            var filtered = allCoachTypes.Where(ct => 
                ct.TypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (ct.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

            CoachTypes.Clear();
            foreach (var coachType in filtered)
            {
                CoachTypes.Add(coachType);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to filter coach types: {ex.Message}";
        }
    }

    private void Add()
    {
        CurrentCoachType = new CoachType
        {
            PriceMultiplier = 1.0m,
            IsCompartmented = false
        };
        IsEditMode = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private bool CanEdit(CoachType? coachType)
    {
        return coachType != null && !IsEditMode;
    }

    private void Edit(CoachType? coachType)
    {
        if (coachType != null)
        {
            CurrentCoachType = new CoachType
            {
                CoachTypeId = coachType.CoachTypeId,
                TypeName = coachType.TypeName,
                PriceMultiplier = coachType.PriceMultiplier,
                IsCompartmented = coachType.IsCompartmented,
                DefaultCompartmentCapacity = coachType.DefaultCompartmentCapacity,
                Description = coachType.Description
            };
            IsEditMode = true;
            ErrorMessage = null;
            SuccessMessage = null;
        }
    }

    private bool CanDelete(CoachType? coachType)
    {
        return coachType != null && !IsEditMode;
    }

    private async void Delete(CoachType? coachType)
    {
        if (coachType == null) return;

        try
        {
            var isInUse = await Task.Run(() => _coachTypeService.IsCoachTypeInUse(coachType.CoachTypeId));
            if (isInUse)
            {
                ErrorMessage = "Cannot delete this coach type because it is currently in use by one or more coaches.";
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the coach type '{coachType.TypeName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await Task.Run(() => _coachTypeService.DeleteCoachType(coachType.CoachTypeId));
                if (success)
                {
                    CoachTypes.Remove(coachType);
                    SuccessMessage = "Coach type deleted successfully.";
                    ErrorMessage = null;
                }
                else
                {
                    ErrorMessage = "Failed to delete the coach type.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete coach type: {ex.Message}";
        }
    }

    private bool CanSave()
    {
        return IsEditMode && 
               !string.IsNullOrWhiteSpace(CurrentCoachType.TypeName) &&
               CurrentCoachType.PriceMultiplier > 0 &&
               (!CurrentCoachType.IsCompartmented || CurrentCoachType.DefaultCompartmentCapacity > 0);
    }

    private async void Save()
    {
        try
        {
            ErrorMessage = null;

            var isNameUnique = await Task.Run(() => 
                _coachTypeService.IsCoachTypeNameUnique(CurrentCoachType.TypeName, CurrentCoachType.CoachTypeId == 0 ? null : CurrentCoachType.CoachTypeId));

            if (!isNameUnique)
            {
                ErrorMessage = "A coach type with this name already exists.";
                return;
            }

            CoachType savedCoachType;
            if (CurrentCoachType.CoachTypeId == 0)
            {
                savedCoachType = await Task.Run(() => _coachTypeService.CreateCoachType(CurrentCoachType));
                CoachTypes.Add(savedCoachType);
                SuccessMessage = "Coach type created successfully.";
            }
            else
            {
                savedCoachType = await Task.Run(() => _coachTypeService.UpdateCoachType(CurrentCoachType));
                if (savedCoachType != null)
                {
                    var existingIndex = CoachTypes.ToList().FindIndex(ct => ct.CoachTypeId == savedCoachType.CoachTypeId);
                    if (existingIndex >= 0)
                    {
                        CoachTypes[existingIndex] = savedCoachType;
                    }
                    SuccessMessage = "Coach type updated successfully.";
                }
                else
                {
                    ErrorMessage = "Failed to update the coach type.";
                    return;
                }
            }

            IsEditMode = false;
            CurrentCoachType = new CoachType();
            SelectedCoachType = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save coach type: {ex.Message}";
        }
    }

    private bool CanCancel()
    {
        return IsEditMode;
    }

    private void Cancel()
    {
        IsEditMode = false;
        CurrentCoachType = new CoachType();
        ErrorMessage = null;
        SuccessMessage = null;
        SelectedCoachType = null;
    }

    public override string TabName => "Coach Types";

    public override void RefreshData()
    {
        _ = LoadCoachTypesAsync();
    }

    public override void ClearForm()
    {
        if (IsEditMode)
        {
            Cancel();
        }
    }
}