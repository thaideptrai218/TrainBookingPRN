using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class StationManagementViewModel : BaseManagerViewModel
{
    private readonly IStationService _stationService;
    
    private ObservableCollection<Station> _stations = new();
    private Station? _selectedStation;
    private string _newStationCode = string.Empty;
    private string _newStationName = string.Empty;
    private string _newStationAddress = string.Empty;
    private string _newStationCity = string.Empty;
    private string _newStationRegion = string.Empty;
    private string _newStationPhone = string.Empty;
    private string _searchTerm = string.Empty;

    public StationManagementViewModel(IStationService stationService)
    {
        _stationService = stationService;
        InitializeCommands();
        RefreshData();
    }

    public override string TabName => "Station Management";

    #region Properties

    public ObservableCollection<Station> Stations
    {
        get => _stations;
        set => SetProperty(ref _stations, value);
    }

    public Station? SelectedStation
    {
        get => _selectedStation;
        set 
        { 
            SetProperty(ref _selectedStation, value);
            PopulateFormFromSelectedStation();
        }
    }

    public string NewStationCode
    {
        get => _newStationCode;
        set => SetProperty(ref _newStationCode, value);
    }

    public string NewStationName
    {
        get => _newStationName;
        set => SetProperty(ref _newStationName, value);
    }

    public string NewStationAddress
    {
        get => _newStationAddress;
        set => SetProperty(ref _newStationAddress, value);
    }

    public string NewStationCity
    {
        get => _newStationCity;
        set => SetProperty(ref _newStationCity, value);
    }

    public string NewStationRegion
    {
        get => _newStationRegion;
        set => SetProperty(ref _newStationRegion, value);
    }

    public string NewStationPhone
    {
        get => _newStationPhone;
        set => SetProperty(ref _newStationPhone, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddStationCommand { get; private set; } = null!;
    public ICommand UpdateStationCommand { get; private set; } = null!;
    public ICommand DeleteStationCommand { get; private set; } = null!;
    public ICommand SearchStationsCommand { get; private set; } = null!;
    public ICommand LoadStationToFormCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddStationCommand = new RelayCommand(_ => AddStation(), _ => CanAddStation());
        UpdateStationCommand = new RelayCommand(_ => UpdateStation(), _ => CanUpdateStation());
        DeleteStationCommand = new RelayCommand(_ => DeleteStation(), _ => CanDeleteStation());
        SearchStationsCommand = new RelayCommand(_ => SearchStations());
        LoadStationToFormCommand = new RelayCommand(_ => LoadStationToForm());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading stations...");
            var stations = _stationService.GetAllStations();
            Stations = new ObservableCollection<Station>(stations);
            SetSuccessMessage("Stations loaded successfully");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading stations: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        NewStationCode = string.Empty;
        NewStationName = string.Empty;
        NewStationAddress = string.Empty;
        NewStationCity = string.Empty;
        NewStationRegion = string.Empty;
        NewStationPhone = string.Empty;
        SelectedStation = null;
        SetStatusMessage("Form cleared");
    }

    public void LoadStationToForm()
    {
        if (SelectedStation != null)
        {
            NewStationCode = SelectedStation.StationCode;
            NewStationName = SelectedStation.StationName;
            NewStationCity = SelectedStation.City ?? string.Empty;
            NewStationRegion = SelectedStation.Region ?? string.Empty;
            NewStationPhone = SelectedStation.PhoneNumber ?? string.Empty;
            NewStationAddress = SelectedStation.Address ?? string.Empty;
        }
    }

    #endregion

    #region Private Methods

    private bool CanAddStation()
    {
        return !string.IsNullOrWhiteSpace(NewStationCode) && 
               !string.IsNullOrWhiteSpace(NewStationName);
    }

    private void AddStation()
    {
        try
        {
            if (_stationService.IsStationCodeExists(NewStationCode))
            {
                SetErrorMessage("Station code already exists!");
                return;
            }

            var station = new Station
            {
                StationCode = NewStationCode,
                StationName = NewStationName,
                Address = string.IsNullOrWhiteSpace(NewStationAddress) ? null : NewStationAddress,
                City = string.IsNullOrWhiteSpace(NewStationCity) ? null : NewStationCity,
                Region = string.IsNullOrWhiteSpace(NewStationRegion) ? null : NewStationRegion,
                PhoneNumber = string.IsNullOrWhiteSpace(NewStationPhone) ? null : NewStationPhone
            };

            var createdStation = _stationService.CreateStation(station);
            Stations.Add(createdStation);
            
            ClearForm();
            SetSuccessMessage("Station added successfully!");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding station: {ex.Message}");
        }
    }

    private bool CanUpdateStation()
    {
        return SelectedStation != null && 
               !string.IsNullOrWhiteSpace(NewStationCode) && 
               !string.IsNullOrWhiteSpace(NewStationName);
    }

    private void UpdateStation()
    {
        if (SelectedStation == null) return;

        try
        {
            SelectedStation.StationCode = NewStationCode;
            SelectedStation.StationName = NewStationName;
            SelectedStation.Address = string.IsNullOrWhiteSpace(NewStationAddress) ? null : NewStationAddress;
            SelectedStation.City = string.IsNullOrWhiteSpace(NewStationCity) ? null : NewStationCity;
            SelectedStation.Region = string.IsNullOrWhiteSpace(NewStationRegion) ? null : NewStationRegion;
            SelectedStation.PhoneNumber = string.IsNullOrWhiteSpace(NewStationPhone) ? null : NewStationPhone;

            var updatedStation = _stationService.UpdateStation(SelectedStation);
            if (updatedStation != null)
            {
                SetSuccessMessage("Station updated successfully!");
                RefreshData();
            }
            else
            {
                SetErrorMessage("Failed to update station!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating station: {ex.Message}");
        }
    }

    private bool CanDeleteStation()
    {
        return SelectedStation != null;
    }

    private void DeleteStation()
    {
        if (SelectedStation == null) return;

        try
        {
            var success = _stationService.DeleteStation(SelectedStation.StationId);
            if (success)
            {
                Stations.Remove(SelectedStation);
                SetSuccessMessage("Station deleted successfully!");
                ClearForm();
            }
            else
            {
                SetErrorMessage("Cannot delete station - it may be used in routes!");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting station: {ex.Message}");
        }
    }

    private void SearchStations()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                RefreshData();
                return;
            }

            var stations = _stationService.SearchStations(SearchTerm);
            Stations = new ObservableCollection<Station>(stations);
            SetSuccessMessage($"Found {stations.Count()} stations");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching stations: {ex.Message}");
        }
    }

    private void PopulateFormFromSelectedStation()
    {
        if (SelectedStation != null)
        {
            NewStationCode = SelectedStation.StationCode;
            NewStationName = SelectedStation.StationName;
            NewStationAddress = SelectedStation.Address ?? string.Empty;
            NewStationCity = SelectedStation.City ?? string.Empty;
            NewStationRegion = SelectedStation.Region ?? string.Empty;
            NewStationPhone = SelectedStation.PhoneNumber ?? string.Empty;
        }
    }

    #endregion
}