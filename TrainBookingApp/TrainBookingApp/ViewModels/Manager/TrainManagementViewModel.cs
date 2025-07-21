using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class TrainManagementViewModel : BaseManagerViewModel
{
    private readonly ITrainService _trainService;
    private readonly ITrainTypeService _trainTypeService;
    private readonly ICoachService _coachService;
    private readonly ISeatService _seatService;
    private readonly ICoachTypeService _coachTypeService;
    private readonly ISeatTypeService _seatTypeService;
    
    private ObservableCollection<Train> _trains = new();
    private Train? _selectedTrain;
    private string _newTrainName = string.Empty;
    private ObservableCollection<TrainType> _trainTypes = new();
    private TrainType? _selectedTrainType;
    private bool _newIsActive = true;
    private string _searchTerm = string.Empty;
    
    // Coach-related properties
    private ObservableCollection<Coach> _coaches = new();
    private Coach? _selectedCoach;
    private string _newCoachNumber = string.Empty;
    private string _newCoachName = string.Empty;
    private string _newCoachCapacity = string.Empty;
    private ObservableCollection<CoachType> _coachTypes = new();
    private CoachType? _selectedCoachType;
    
    // Seat-related properties
    private ObservableCollection<Seat> _seats = new();
    private Seat? _selectedSeat;
    private string _newSeatNumber = string.Empty;
    private string _newSeatName = string.Empty;
    private ObservableCollection<SeatType> _seatTypes = new();
    private SeatType? _selectedSeatType;
    
    // UI state properties
    private int _selectedDetailsTab = 0;
    private string _seatManagementTitle = "Select a coach to manage seats";

    public TrainManagementViewModel(ITrainService trainService, ITrainTypeService trainTypeService, 
        ICoachService coachService, ISeatService seatService, ICoachTypeService coachTypeService, ISeatTypeService seatTypeService)
    {
        _trainService = trainService;
        _trainTypeService = trainTypeService;
        _coachService = coachService;
        _seatService = seatService;
        _coachTypeService = coachTypeService;
        _seatTypeService = seatTypeService;
        InitializeCommands();
        RefreshData();
        LoadTrainTypes();
        LoadCoachTypes();
        LoadSeatTypes();
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
        set
        {
            if (SetProperty(ref _selectedTrain, value))
            {
                LoadTrainToForm();
                LoadCoachesForSelectedTrain();
            }
        }
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

    // Coach Properties
    public ObservableCollection<Coach> Coaches
    {
        get => _coaches;
        set => SetProperty(ref _coaches, value);
    }

    public Coach? SelectedCoach
    {
        get => _selectedCoach;
        set
        {
            if (SetProperty(ref _selectedCoach, value))
            {
                LoadSeatsForSelectedCoach();
                LoadCoachToForm();
            }
        }
    }

    public string NewCoachNumber
    {
        get => _newCoachNumber;
        set => SetProperty(ref _newCoachNumber, value);
    }

    public string NewCoachName
    {
        get => _newCoachName;
        set => SetProperty(ref _newCoachName, value);
    }

    public string NewCoachCapacity
    {
        get => _newCoachCapacity;
        set => SetProperty(ref _newCoachCapacity, value);
    }

    public ObservableCollection<CoachType> CoachTypes
    {
        get => _coachTypes;
        set => SetProperty(ref _coachTypes, value);
    }

    public CoachType? SelectedCoachType
    {
        get => _selectedCoachType;
        set => SetProperty(ref _selectedCoachType, value);
    }

    // Seat Properties
    public ObservableCollection<Seat> Seats
    {
        get => _seats;
        set => SetProperty(ref _seats, value);
    }

    public Seat? SelectedSeat
    {
        get => _selectedSeat;
        set
        {
            if (SetProperty(ref _selectedSeat, value))
            {
                LoadSeatToForm();
            }
        }
    }

    public string NewSeatNumber
    {
        get => _newSeatNumber;
        set => SetProperty(ref _newSeatNumber, value);
    }

    public string NewSeatName
    {
        get => _newSeatName;
        set => SetProperty(ref _newSeatName, value);
    }

    public ObservableCollection<SeatType> SeatTypes
    {
        get => _seatTypes;
        set => SetProperty(ref _seatTypes, value);
    }

    public SeatType? SelectedSeatType
    {
        get => _selectedSeatType;
        set => SetProperty(ref _selectedSeatType, value);
    }

    // UI State Properties
    public int SelectedDetailsTab
    {
        get => _selectedDetailsTab;
        set => SetProperty(ref _selectedDetailsTab, value);
    }

    public string SeatManagementTitle
    {
        get => _seatManagementTitle;
        set => SetProperty(ref _seatManagementTitle, value);
    }

    #endregion

    #region Commands

    // Train Commands
    public ICommand AddTrainCommand { get; private set; } = null!;
    public ICommand UpdateTrainCommand { get; private set; } = null!;
    public ICommand DeleteTrainCommand { get; private set; } = null!;
    public ICommand ActivateTrainCommand { get; private set; } = null!;
    public ICommand DeactivateTrainCommand { get; private set; } = null!;
    public ICommand SearchTrainsCommand { get; private set; } = null!;
    public ICommand LoadTrainToFormCommand { get; private set; } = null!;
    public ICommand ShowActiveTrainsCommand { get; private set; } = null!;
    public ICommand ShowAllTrainsCommand { get; private set; } = null!;
    public ICommand ShowAddTrainFormCommand { get; private set; } = null!;
    public ICommand ShowEditTrainFormCommand { get; private set; } = null!;

    // Coach Commands
    public ICommand AddCoachCommand { get; private set; } = null!;
    public ICommand UpdateCoachCommand { get; private set; } = null!;
    public ICommand DeleteCoachCommand { get; private set; } = null!;
    public ICommand ShowAddCoachFormCommand { get; private set; } = null!;
    public ICommand ShowEditCoachFormCommand { get; private set; } = null!;
    public ICommand ClearCoachFormCommand { get; private set; } = null!;

    // Seat Commands
    public ICommand AddSeatCommand { get; private set; } = null!;
    public ICommand UpdateSeatCommand { get; private set; } = null!;
    public ICommand DeleteSeatCommand { get; private set; } = null!;
    public ICommand ShowSeatManagementCommand { get; private set; } = null!;

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
        ShowAddTrainFormCommand = new RelayCommand(_ => ShowAddTrainForm());
        ShowEditTrainFormCommand = new RelayCommand(_ => ShowEditTrainForm());
        
        // Coach Commands
        AddCoachCommand = new RelayCommand(_ => AddCoach(), _ => CanAddCoach());
        UpdateCoachCommand = new RelayCommand(_ => UpdateCoach(), _ => CanUpdateCoach());
        DeleteCoachCommand = new RelayCommand(_ => DeleteCoach(), _ => CanDeleteCoach());
        ShowAddCoachFormCommand = new RelayCommand(_ => ShowAddCoachForm());
        ShowEditCoachFormCommand = new RelayCommand(_ => ShowEditCoachForm());
        ClearCoachFormCommand = new RelayCommand(_ => ClearCoachForm());
        
        // Seat Commands
        AddSeatCommand = new RelayCommand(_ => AddSeat(), _ => CanAddSeat());
        UpdateSeatCommand = new RelayCommand(_ => UpdateSeat(), _ => CanUpdateSeat());
        DeleteSeatCommand = new RelayCommand(_ => DeleteSeat(), _ => CanDeleteSeat());
        ShowSeatManagementCommand = new RelayCommand(_ => ShowSeatManagement());
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
        
        // Clear coach form
        NewCoachNumber = string.Empty;
        NewCoachName = string.Empty;
        NewCoachCapacity = string.Empty;
        SelectedCoachType = null;
        SelectedCoach = null;
        
        // Clear seat form
        NewSeatNumber = string.Empty;
        NewSeatName = string.Empty;
        SelectedSeatType = null;
        SelectedSeat = null;
        
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

    private void ShowAddTrainForm()
    {
        ClearForm();
        SelectedDetailsTab = 0; // Train Details tab
    }

    private void ShowEditTrainForm()
    {
        if (SelectedTrain != null)
        {
            LoadTrainToForm();
            SelectedDetailsTab = 0; // Train Details tab
        }
    }

    private void ShowAddCoachForm()
    {
        ClearCoachForm();
        SelectedDetailsTab = 1; // Coach Details tab
    }

    private void ShowEditCoachForm()
    {
        if (SelectedCoach != null)
        {
            LoadCoachToForm();
            SelectedDetailsTab = 1; // Coach Details tab
        }
    }

    private void ClearCoachForm()
    {
        NewCoachNumber = string.Empty;
        NewCoachName = string.Empty;
        NewCoachCapacity = string.Empty;
        SelectedCoachType = null;
    }

    private void ShowSeatManagement()
    {
        SelectedDetailsTab = 2; // Seat Management tab
    }

    private void LoadCoachTypes()
    {
        try
        {
            var coachTypes = _coachTypeService.GetAllCoachTypes();
            CoachTypes = new ObservableCollection<CoachType>(coachTypes);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading coach types: {ex.Message}");
        }
    }

    private void LoadSeatTypes()
    {
        try
        {
            var seatTypes = _seatTypeService.GetAllSeatTypes();
            SeatTypes = new ObservableCollection<SeatType>(seatTypes);
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading seat types: {ex.Message}");
        }
    }

    private void LoadCoachesForSelectedTrain()
    {
        try
        {
            if (SelectedTrain == null)
            {
                Coaches.Clear();
                SeatManagementTitle = "Select a train to view coaches";
                return;
            }

            SetLoadingState(true, "Loading coaches...");
            var coaches = _coachService.GetCoachesByTrainId(SelectedTrain.TrainId);
            Coaches = new ObservableCollection<Coach>(coaches);
            
            SeatManagementTitle = $"Coaches for {SelectedTrain.TrainName}";
            SetSuccessMessage($"Loaded {coaches.Count()} coaches for {SelectedTrain.TrainName}");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading coaches: {ex.Message}");
            Coaches.Clear();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void LoadSeatsForSelectedCoach()
    {
        try
        {
            if (SelectedCoach == null)
            {
                Seats.Clear();
                SeatManagementTitle = "Select a coach to manage seats";
                return;
            }

            SetLoadingState(true, "Loading seats...");
            var seats = _seatService.GetSeatsByCoachId(SelectedCoach.CoachId);
            Seats = new ObservableCollection<Seat>(seats);
            
            SeatManagementTitle = $"Seats in Coach {SelectedCoach.CoachName} ({SelectedCoach.CoachNumber})";
            SetSuccessMessage($"Loaded {seats.Count()} seats for coach {SelectedCoach.CoachName}");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading seats: {ex.Message}");
            Seats.Clear();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void LoadCoachToForm()
    {
        if (SelectedCoach != null)
        {
            NewCoachNumber = SelectedCoach.CoachNumber.ToString();
            NewCoachName = SelectedCoach.CoachName;
            NewCoachCapacity = SelectedCoach.Capacity.ToString();
            SelectedCoachType = CoachTypes.FirstOrDefault(ct => ct.CoachTypeId == SelectedCoach.CoachTypeId);
        }
    }

    private void LoadSeatToForm()
    {
        if (SelectedSeat != null)
        {
            NewSeatNumber = SelectedSeat.SeatNumber.ToString();
            NewSeatName = SelectedSeat.SeatName;
            SelectedSeatType = SeatTypes.FirstOrDefault(st => st.SeatTypeId == SelectedSeat.SeatTypeId);
        }
    }

    private bool TryParseCoachCapacity(out int capacity)
    {
        return int.TryParse(NewCoachCapacity, out capacity) && capacity > 0;
    }

    private bool ValidateCoachForm()
    {
        if (string.IsNullOrWhiteSpace(NewCoachNumber))
        {
            SetErrorMessage("Coach number is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewCoachName))
        {
            SetErrorMessage("Coach name is required");
            return false;
        }

        if (!TryParseCoachCapacity(out _))
        {
            SetErrorMessage("Coach capacity must be a valid positive number");
            return false;
        }

        if (SelectedCoachType == null)
        {
            SetErrorMessage("Coach type is required");
            return false;
        }

        return true;
    }

    // Coach CRUD Operations
    private bool CanAddCoach()
    {
        return SelectedTrain != null;
    }

    private void AddCoach()
    {
        if (SelectedTrain == null || !ValidateCoachForm()) return;

        try
        {
            if (!TryParseCoachCapacity(out int capacity)) return;
            if (!int.TryParse(NewCoachNumber, out int coachNumber))
            {
                SetErrorMessage("Coach number must be a valid number");
                return;
            }

            // Check if coach number already exists for this train
            if (!_coachService.IsCoachNumberUniqueInTrain(SelectedTrain.TrainId, coachNumber))
            {
                SetErrorMessage("Coach number already exists for this train!");
                return;
            }

            var coach = new Coach
            {
                TrainId = SelectedTrain.TrainId,
                CoachNumber = coachNumber,
                CoachName = NewCoachName,
                CoachTypeId = SelectedCoachType!.CoachTypeId,
                Capacity = capacity,
                PositionInTrain = _coachService.GetNextAvailablePosition(SelectedTrain.TrainId),
                IsActive = true
            };

            var createdCoach = _coachService.CreateCoach(coach);
            Coaches.Add(createdCoach);
            
            ClearCoachForm();
            SetSuccessMessage("Coach added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding coach: {ex.Message}");
        }
    }

    private bool CanUpdateCoach()
    {
        return SelectedCoach != null;
    }

    private void UpdateCoach()
    {
        if (SelectedCoach == null || !ValidateCoachForm()) return;

        try
        {
            if (!TryParseCoachCapacity(out int capacity)) return;
            if (!int.TryParse(NewCoachNumber, out int coachNumber))
            {
                SetErrorMessage("Coach number must be a valid number");
                return;
            }

            SelectedCoach.CoachNumber = coachNumber;
            SelectedCoach.CoachName = NewCoachName;
            SelectedCoach.CoachTypeId = SelectedCoachType!.CoachTypeId;
            SelectedCoach.Capacity = capacity;

            var updatedCoach = _coachService.UpdateCoach(SelectedCoach);
            if (updatedCoach != null)
            {
                SetSuccessMessage("Coach updated successfully!");
                LoadCoachesForSelectedTrain(); // Refresh the list
            }
            else
            {
                SetErrorMessage("Failed to update coach!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating coach: {ex.Message}");
        }
    }

    private bool CanDeleteCoach()
    {
        return SelectedCoach != null;
    }

    private void DeleteCoach()
    {
        if (SelectedCoach == null) return;

        try
        {
            var success = _coachService.DeleteCoach(SelectedCoach.CoachId);
            if (success)
            {
                Coaches.Remove(SelectedCoach);
                SetSuccessMessage("Coach deleted successfully!");
                ClearCoachForm();
            }
            else
            {
                SetErrorMessage("Cannot delete coach - it may contain seats or be used in bookings!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting coach: {ex.Message}");
        }
    }

    // Seat CRUD Operations
    private bool CanAddSeat()
    {
        return SelectedCoach != null;
    }

    private void AddSeat()
    {
        if (SelectedCoach == null || !ValidateSeatForm()) return;

        try
        {
            if (!int.TryParse(NewSeatNumber, out int seatNumber))
            {
                SetErrorMessage("Seat number must be a valid number");
                return;
            }

            // Check if seat number already exists for this coach
            if (!_seatService.IsSeatNumberUniqueInCoach(SelectedCoach.CoachId, seatNumber))
            {
                SetErrorMessage("Seat number already exists for this coach!");
                return;
            }

            var seat = new Seat
            {
                CoachId = SelectedCoach.CoachId,
                SeatNumber = seatNumber,
                SeatName = NewSeatName,
                SeatTypeId = SelectedSeatType!.SeatTypeId,
                IsEnabled = true
            };

            var createdSeat = _seatService.CreateSeat(seat);
            Seats.Add(createdSeat);
            
            ClearSeatForm();
            SetSuccessMessage("Seat added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding seat: {ex.Message}");
        }
    }

    private bool CanUpdateSeat()
    {
        return SelectedSeat != null;
    }

    private void UpdateSeat()
    {
        if (SelectedSeat == null || !ValidateSeatForm()) return;

        try
        {
            if (!int.TryParse(NewSeatNumber, out int seatNumber))
            {
                SetErrorMessage("Seat number must be a valid number");
                return;
            }

            // Check if seat number already exists for this coach (excluding current seat)
            if (!_seatService.IsSeatNumberUniqueInCoach(SelectedSeat.CoachId, seatNumber, SelectedSeat.SeatId))
            {
                SetErrorMessage("Seat number already exists for this coach!");
                return;
            }

            SelectedSeat.SeatNumber = seatNumber;
            SelectedSeat.SeatName = NewSeatName;
            SelectedSeat.SeatTypeId = SelectedSeatType!.SeatTypeId;

            var updatedSeat = _seatService.UpdateSeat(SelectedSeat);
            if (updatedSeat != null)
            {
                SetSuccessMessage("Seat updated successfully!");
                LoadSeatsForSelectedCoach(); // Refresh the list
            }
            else
            {
                SetErrorMessage("Failed to update seat!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating seat: {ex.Message}");
        }
    }

    private bool CanDeleteSeat()
    {
        return SelectedSeat != null;
    }

    private void DeleteSeat()
    {
        if (SelectedSeat == null) return;

        try
        {
            var success = _seatService.DeleteSeat(SelectedSeat.SeatId);
            if (success)
            {
                Seats.Remove(SelectedSeat);
                SetSuccessMessage("Seat deleted successfully!");
                ClearSeatForm();
            }
            else
            {
                SetErrorMessage("Cannot delete seat - it may be booked or have pending reservations!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting seat: {ex.Message}");
        }
    }

    private bool ValidateSeatForm()
    {
        if (string.IsNullOrWhiteSpace(NewSeatNumber))
        {
            SetErrorMessage("Seat number is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewSeatName))
        {
            SetErrorMessage("Seat name is required");
            return false;
        }

        if (!int.TryParse(NewSeatNumber, out int seatNumber) || seatNumber <= 0)
        {
            SetErrorMessage("Seat number must be a valid positive number");
            return false;
        }

        if (SelectedSeatType == null)
        {
            SetErrorMessage("Seat type is required");
            return false;
        }

        return true;
    }

    private bool CanValidateSeatForm()
    {
        return !string.IsNullOrWhiteSpace(NewSeatNumber) &&
               !string.IsNullOrWhiteSpace(NewSeatName) &&
               int.TryParse(NewSeatNumber, out int seatNumber) && seatNumber > 0 &&
               SelectedSeatType != null;
    }

    private void ClearSeatForm()
    {
        NewSeatNumber = string.Empty;
        NewSeatName = string.Empty;
        SelectedSeatType = null;
    }

    #endregion
}