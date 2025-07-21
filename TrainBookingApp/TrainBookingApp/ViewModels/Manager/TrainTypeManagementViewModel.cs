using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class TrainTypeManagementViewModel : BaseManagerViewModel
{
    private readonly ITrainTypeService _trainTypeService;
    
    private ObservableCollection<TrainType> _trainTypes = new();
    private TrainType? _selectedTrainType;
    private string _newTypeName = string.Empty;
    private string _newDescription = string.Empty;
    private decimal? _newAverageVelocity;
    private string _searchTerm = string.Empty;

    public TrainTypeManagementViewModel(ITrainTypeService trainTypeService)
    {
        _trainTypeService = trainTypeService;
        InitializeCommands();
        RefreshData();
    }

    public override string TabName => "Train Type Management";

    #region Properties

    public ObservableCollection<TrainType> TrainTypes
    {
        get => _trainTypes;
        set => SetProperty(ref _trainTypes, value);
    }

    public TrainType? SelectedTrainType
    {
        get => _selectedTrainType;
        set 
        { 
            SetProperty(ref _selectedTrainType, value);
            PopulateFormFromSelectedTrainType();
        }
    }

    public string NewTypeName
    {
        get => _newTypeName;
        set => SetProperty(ref _newTypeName, value);
    }

    public string NewDescription
    {
        get => _newDescription;
        set => SetProperty(ref _newDescription, value);
    }

    public decimal? NewAverageVelocity
    {
        get => _newAverageVelocity;
        set => SetProperty(ref _newAverageVelocity, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddTrainTypeCommand { get; private set; } = null!;
    public ICommand UpdateTrainTypeCommand { get; private set; } = null!;
    public ICommand DeleteTrainTypeCommand { get; private set; } = null!;
    public ICommand SearchTrainTypesCommand { get; private set; } = null!;
    public ICommand LoadTrainTypeToFormCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddTrainTypeCommand = new RelayCommand(_ => AddTrainType(), _ => CanAddTrainType());
        UpdateTrainTypeCommand = new RelayCommand(_ => UpdateTrainType(), _ => CanUpdateTrainType());
        DeleteTrainTypeCommand = new RelayCommand(_ => DeleteTrainType(), _ => CanDeleteTrainType());
        SearchTrainTypesCommand = new RelayCommand(_ => SearchTrainTypes());
        LoadTrainTypeToFormCommand = new RelayCommand(_ => LoadTrainTypeToForm());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading train types...");
            var trainTypes = _trainTypeService.GetAllTrainTypes();
            TrainTypes = new ObservableCollection<TrainType>(trainTypes);
            SetSuccessMessage("Train types loaded successfully");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading train types: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        NewTypeName = string.Empty;
        NewDescription = string.Empty;
        NewAverageVelocity = null;
        SelectedTrainType = null;
        SetStatusMessage("Form cleared");
    }

    public void LoadTrainTypeToForm()
    {
        if (SelectedTrainType != null)
        {
            NewTypeName = SelectedTrainType.TypeName;
            NewDescription = SelectedTrainType.Description ?? string.Empty;
            NewAverageVelocity = SelectedTrainType.AverageVelocity;
        }
    }

    #endregion

    #region Private Methods

    private bool CanAddTrainType()
    {
        return !string.IsNullOrWhiteSpace(NewTypeName);
    }

    private void AddTrainType()
    {
        try
        {
            if (_trainTypeService.IsTrainTypeNameExists(NewTypeName))
            {
                SetErrorMessage("Train type name already exists!");
                return;
            }

            var trainType = new TrainType
            {
                TypeName = NewTypeName,
                Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription,
                AverageVelocity = NewAverageVelocity
            };

            var createdTrainType = _trainTypeService.CreateTrainType(trainType);
            TrainTypes.Add(createdTrainType);
            
            ClearForm();
            SetSuccessMessage("Train type added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding train type: {ex.Message}");
        }
    }

    private bool CanUpdateTrainType()
    {
        return SelectedTrainType != null && !string.IsNullOrWhiteSpace(NewTypeName);
    }

    private void UpdateTrainType()
    {
        if (SelectedTrainType == null) return;

        try
        {
            SelectedTrainType.TypeName = NewTypeName;
            SelectedTrainType.Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription;
            SelectedTrainType.AverageVelocity = NewAverageVelocity;

            var updatedTrainType = _trainTypeService.UpdateTrainType(SelectedTrainType);
            if (updatedTrainType != null)
            {
                SetSuccessMessage("Train type updated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to update train type!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating train type: {ex.Message}");
        }
    }

    private bool CanDeleteTrainType()
    {
        return SelectedTrainType != null;
    }

    private void DeleteTrainType()
    {
        if (SelectedTrainType == null) return;

        try
        {
            var success = _trainTypeService.DeleteTrainType(SelectedTrainType.TrainTypeId);
            if (success)
            {
                TrainTypes.Remove(SelectedTrainType);
                SetSuccessMessage("Train type deleted successfully!");
                ClearForm();
            }
            else
            {
                SetErrorMessage("Cannot delete train type - it may be used by trains!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting train type: {ex.Message}");
        }
    }

    private void SearchTrainTypes()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                RefreshData();
                return;
            }

            var trainTypes = _trainTypeService.SearchTrainTypes(SearchTerm);
            TrainTypes = new ObservableCollection<TrainType>(trainTypes);
            SetSuccessMessage($"Found {trainTypes.Count()} train types");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching train types: {ex.Message}");
        }
    }

    private void PopulateFormFromSelectedTrainType()
    {
        if (SelectedTrainType != null)
        {
            NewTypeName = SelectedTrainType.TypeName;
            NewDescription = SelectedTrainType.Description ?? string.Empty;
            NewAverageVelocity = SelectedTrainType.AverageVelocity;
        }
    }

    #endregion
}