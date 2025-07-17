using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class TrainManagementViewModel : BaseManagerViewModel
{
    private readonly ITrainService _trainService;
    private readonly ITrainTypeService _trainTypeService;
    
    private ObservableCollection<Train> _trains = new();
    private Train? _selectedTrain;
    private string _newTrainName = string.Empty;
    private ObservableCollection<TrainType> _trainTypes = new();
    private TrainType? _selectedTrainType;
    private bool _newIsActive = true;
    private string _searchTerm = string.Empty;

    public TrainManagementViewModel(ITrainService trainService, ITrainTypeService trainTypeService)
    {
        _trainService = trainService;
        _trainTypeService = trainTypeService;
        InitializeCommands();
        RefreshData();
        LoadTrainTypes();
    }

    public override string TabName => "Train Management";

    #region Properties

    public ObservableCollection<Train> Trains
    {
        get => _trains;
        set => SetProperty(ref _trains, value);
    }

    public Train? SelectedTrain
    {
        get => _selectedTrain;
        set => SetProperty(ref _selectedTrain, value);
    }

    public string NewTrainName
    {
        get => _newTrainName;
        set => SetProperty(ref _newTrainName, value);
    }

    public ObservableCollection<TrainType> TrainTypes
    {
        get => _trainTypes;
        set => SetProperty(ref _trainTypes, value);
    }

    public TrainType? SelectedTrainType
    {
        get => _selectedTrainType;
        set => SetProperty(ref _selectedTrainType, value);
    }

    public bool NewIsActive
    {
        get => _newIsActive;
        set => SetProperty(ref _newIsActive, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddTrainCommand { get; private set; } = null!;
    public ICommand UpdateTrainCommand { get; private set; } = null!;
    public ICommand DeleteTrainCommand { get; private set; } = null!;
    public ICommand ActivateTrainCommand { get; private set; } = null!;
    public ICommand DeactivateTrainCommand { get; private set; } = null!;
    public ICommand SearchTrainsCommand { get; private set; } = null!;
    public ICommand LoadTrainToFormCommand { get; private set; } = null!;
    public ICommand ShowActiveTrainsCommand { get; private set; } = null!;
    public ICommand ShowAllTrainsCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddTrainCommand = new RelayCommand(_ => AddTrain(), _ => CanAddTrain());
        UpdateTrainCommand = new RelayCommand(_ => UpdateTrain(), _ => CanUpdateTrain());
        DeleteTrainCommand = new RelayCommand(_ => DeleteTrain(), _ => CanDeleteTrain());
        ActivateTrainCommand = new RelayCommand(_ => ActivateTrain(), _ => CanActivateTrain());
        DeactivateTrainCommand = new RelayCommand(_ => DeactivateTrain(), _ => CanDeactivateTrain());
        SearchTrainsCommand = new RelayCommand(_ => SearchTrains());
        LoadTrainToFormCommand = new RelayCommand(_ => LoadTrainToForm());
        ShowActiveTrainsCommand = new RelayCommand(_ => ShowActiveTrains());
        ShowAllTrainsCommand = new RelayCommand(_ => RefreshData());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading trains...");
            var trains = _trainService.GetAllTrains();
            Trains = new ObservableCollection<Train>(trains);
            SetSuccessMessage("Trains loaded successfully");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading trains: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        NewTrainName = string.Empty;
        SelectedTrainType = null;
        NewIsActive = true;
        SelectedTrain = null;
        SetStatusMessage("Form cleared");
    }

    public void LoadTrainToForm()
    {
        if (SelectedTrain != null)
        {
            NewTrainName = SelectedTrain.TrainName;
            SelectedTrainType = TrainTypes.FirstOrDefault(tt => tt.TrainTypeId == SelectedTrain.TrainTypeId);
            NewIsActive = SelectedTrain.IsActive;
        }
    }

    public void ShowActiveTrains()
    {
        try
        {
            SetLoadingState(true, "Loading active trains...");
            var trains = _trainService.GetActiveTrains();
            Trains = new ObservableCollection<Train>(trains);
            SetSuccessMessage($"Active trains loaded ({trains.Count()} trains)");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading active trains: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    #endregion

    #region Private Methods

    private void LoadTrainTypes()
    {
        try
        {
            var trainTypes = _trainTypeService.GetAllTrainTypes();
            TrainTypes = new ObservableCollection<TrainType>(trainTypes);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading train types: {ex.Message}");
        }
    }

    private bool CanAddTrain()
    {
        return !string.IsNullOrWhiteSpace(NewTrainName) && SelectedTrainType != null;
    }

    private void AddTrain()
    {
        try
        {
            if (SelectedTrainType == null) return;

            var train = new Train
            {
                TrainName = NewTrainName,
                TrainTypeId = SelectedTrainType.TrainTypeId,
                IsActive = NewIsActive
            };

            var createdTrain = _trainService.CreateTrain(train);
            Trains.Add(createdTrain);
            
            ClearForm();
            SetSuccessMessage("Train added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding train: {ex.Message}");
        }
    }

    private bool CanUpdateTrain()
    {
        return SelectedTrain != null && 
               !string.IsNullOrWhiteSpace(NewTrainName) && 
               SelectedTrainType != null;
    }

    private void UpdateTrain()
    {
        if (SelectedTrain == null || SelectedTrainType == null) return;

        try
        {
            SelectedTrain.TrainName = NewTrainName;
            SelectedTrain.TrainTypeId = SelectedTrainType.TrainTypeId;
            SelectedTrain.IsActive = NewIsActive;

            var updatedTrain = _trainService.UpdateTrain(SelectedTrain);
            if (updatedTrain != null)
            {
                SetSuccessMessage("Train updated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to update train!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating train: {ex.Message}");
        }
    }

    private bool CanDeleteTrain()
    {
        return SelectedTrain != null;
    }

    private void DeleteTrain()
    {
        if (SelectedTrain == null) return;

        try
        {
            var success = _trainService.DeleteTrain(SelectedTrain.TrainId);
            if (success)
            {
                Trains.Remove(SelectedTrain);
                SetSuccessMessage("Train deleted successfully!");
                ClearForm();
            }
            else
            {
                SetErrorMessage("Cannot delete train - it may be used in trips!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting train: {ex.Message}");
        }
    }

    private bool CanActivateTrain()
    {
        return SelectedTrain != null && !SelectedTrain.IsActive;
    }

    private void ActivateTrain()
    {
        if (SelectedTrain == null) return;

        try
        {
            var success = _trainService.ActivateTrain(SelectedTrain.TrainId);
            if (success)
            {
                SelectedTrain.IsActive = true;
                SetSuccessMessage("Train activated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to activate train!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error activating train: {ex.Message}");
        }
    }

    private bool CanDeactivateTrain()
    {
        return SelectedTrain != null && SelectedTrain.IsActive;
    }

    private void DeactivateTrain()
    {
        if (SelectedTrain == null) return;

        try
        {
            var success = _trainService.DeactivateTrain(SelectedTrain.TrainId);
            if (success)
            {
                SelectedTrain.IsActive = false;
                SetSuccessMessage("Train deactivated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to deactivate train!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deactivating train: {ex.Message}");
        }
    }

    private void SearchTrains()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                RefreshData();
                return;
            }

            var trains = _trainService.GetAllTrains()
                .Where(t => t.TrainName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                           t.TrainType.TypeName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            
            Trains = new ObservableCollection<Train>(trains);
            SetSuccessMessage($"Found {trains.Count()} trains");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching trains: {ex.Message}");
        }
    }

    #endregion
}