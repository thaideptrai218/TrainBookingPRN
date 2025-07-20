using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class PricingRuleManagementViewModel : BaseManagerViewModel
{
    private readonly IPricingRuleService _pricingRuleService;
    private readonly ITrainTypeService _trainTypeService;
    private readonly IRouteService _routeService;
    
    private ObservableCollection<PricingRule> _pricingRules = new();
    private ObservableCollection<TrainType> _trainTypes = new();
    private ObservableCollection<Route> _routes = new();
    private PricingRule? _selectedPricingRule;
    private TrainType? _selectedTrainTypeFilter;
    private Route? _selectedRouteFilter;
    
    private string _newRuleName = string.Empty;
    private string _newDescription = string.Empty;
    private decimal _newBasePricePerKm = 0;
    private int? _newTrainTypeId = null;
    private int? _newRouteId = null;
    private bool _newIsForRoundTrip = false;
    private DateTime? _newApplicableDateStart;
    private DateTime? _newApplicableDateEnd;
    private int _newPriority = 1;
    private bool _newIsActive = true;
    private DateTime _newEffectiveFromDate = DateTime.Today;
    private DateTime? _newEffectiveToDate;
    private string _searchTerm = string.Empty;

    public PricingRuleManagementViewModel(
        IPricingRuleService pricingRuleService,
        ITrainTypeService trainTypeService,
        IRouteService routeService)
    {
        _pricingRuleService = pricingRuleService;
        _trainTypeService = trainTypeService;
        _routeService = routeService;
        
        InitializeCommands();
        RefreshData();
    }

    public override string TabName => "Pricing Rules";

    #region Properties

    public ObservableCollection<PricingRule> PricingRules
    {
        get => _pricingRules;
        set => SetProperty(ref _pricingRules, value);
    }

    public ObservableCollection<TrainType> TrainTypes
    {
        get => _trainTypes;
        set => SetProperty(ref _trainTypes, value);
    }

    public ObservableCollection<Route> Routes
    {
        get => _routes;
        set => SetProperty(ref _routes, value);
    }

    public PricingRule? SelectedPricingRule
    {
        get => _selectedPricingRule;
        set => SetProperty(ref _selectedPricingRule, value);
    }

    public TrainType? SelectedTrainTypeFilter
    {
        get => _selectedTrainTypeFilter;
        set => SetProperty(ref _selectedTrainTypeFilter, value);
    }

    public Route? SelectedRouteFilter
    {
        get => _selectedRouteFilter;
        set => SetProperty(ref _selectedRouteFilter, value);
    }

    public string NewRuleName
    {
        get => _newRuleName;
        set => SetProperty(ref _newRuleName, value);
    }

    public string NewDescription
    {
        get => _newDescription;
        set => SetProperty(ref _newDescription, value);
    }

    public decimal NewBasePricePerKm
    {
        get => _newBasePricePerKm;
        set => SetProperty(ref _newBasePricePerKm, value);
    }

    public int? NewTrainTypeId
    {
        get => _newTrainTypeId;
        set => SetProperty(ref _newTrainTypeId, value);
    }

    public int? NewRouteId
    {
        get => _newRouteId;
        set => SetProperty(ref _newRouteId, value);
    }

    public bool NewIsForRoundTrip
    {
        get => _newIsForRoundTrip;
        set => SetProperty(ref _newIsForRoundTrip, value);
    }

    public DateTime? NewApplicableDateStart
    {
        get => _newApplicableDateStart;
        set => SetProperty(ref _newApplicableDateStart, value);
    }

    public DateTime? NewApplicableDateEnd
    {
        get => _newApplicableDateEnd;
        set => SetProperty(ref _newApplicableDateEnd, value);
    }

    public int NewPriority
    {
        get => _newPriority;
        set => SetProperty(ref _newPriority, value);
    }

    public bool NewIsActive
    {
        get => _newIsActive;
        set => SetProperty(ref _newIsActive, value);
    }

    public DateTime NewEffectiveFromDate
    {
        get => _newEffectiveFromDate;
        set => SetProperty(ref _newEffectiveFromDate, value);
    }

    public DateTime? NewEffectiveToDate
    {
        get => _newEffectiveToDate;
        set => SetProperty(ref _newEffectiveToDate, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    #endregion

    #region Commands

    public ICommand AddPricingRuleCommand { get; private set; } = null!;
    public ICommand UpdatePricingRuleCommand { get; private set; } = null!;
    public ICommand DeletePricingRuleCommand { get; private set; } = null!;
    public ICommand ToggleActiveCommand { get; private set; } = null!;
    public ICommand SearchPricingRulesCommand { get; private set; } = null!;
    public ICommand LoadPricingRuleToFormCommand { get; private set; } = null!;
    public ICommand TestPriceCalculationCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        AddPricingRuleCommand = new RelayCommand(_ => AddPricingRule(), _ => CanAddPricingRule());
        UpdatePricingRuleCommand = new RelayCommand(_ => UpdatePricingRule(), _ => CanUpdatePricingRule());
        DeletePricingRuleCommand = new RelayCommand(_ => DeletePricingRule(), _ => CanDeletePricingRule());
        ToggleActiveCommand = new RelayCommand(_ => ToggleActive(), _ => CanToggleActive());
        SearchPricingRulesCommand = new RelayCommand(_ => SearchPricingRules());
        LoadPricingRuleToFormCommand = new RelayCommand(_ => LoadPricingRuleToForm());
        TestPriceCalculationCommand = new RelayCommand(_ => TestPriceCalculation(), _ => CanTestPriceCalculation());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        try
        {
            SetLoadingState(true, "Loading pricing rules...");
            
            var pricingRules = _pricingRuleService.GetAllPricingRules();
            var trainTypes = _trainTypeService.GetAllTrainTypes();
            var routes = _routeService.GetAllRoutes();

            PricingRules = new ObservableCollection<PricingRule>(pricingRules);
            
            TrainTypes = new ObservableCollection<TrainType>(trainTypes);
            Routes = new ObservableCollection<Route>(routes);

            SetSuccessMessage($"Loaded {PricingRules.Count} pricing rules");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error loading pricing rules: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    public override void ClearForm()
    {
        NewRuleName = string.Empty;
        NewDescription = string.Empty;
        NewBasePricePerKm = 0;
        NewTrainTypeId = null;
        NewRouteId = null;
        NewIsForRoundTrip = false;
        NewApplicableDateStart = null;
        NewApplicableDateEnd = null;
        NewPriority = 1;
        NewIsActive = true;
        NewEffectiveFromDate = DateTime.Today;
        NewEffectiveToDate = null;
        SelectedPricingRule = null;
        SetStatusMessage("Form cleared");
    }

    public void LoadPricingRuleToForm()
    {
        if (SelectedPricingRule != null)
        {
            NewRuleName = SelectedPricingRule.RuleName;
            NewDescription = SelectedPricingRule.Description ?? string.Empty;
            NewBasePricePerKm = SelectedPricingRule.BasePricePerKm;
            NewTrainTypeId = SelectedPricingRule.TrainTypeId;
            NewRouteId = SelectedPricingRule.RouteId;
            NewIsForRoundTrip = SelectedPricingRule.IsForRoundTrip ?? false;
            NewApplicableDateStart = SelectedPricingRule.ApplicableDateStart?.ToDateTime(TimeOnly.MinValue);
            NewApplicableDateEnd = SelectedPricingRule.ApplicableDateEnd?.ToDateTime(TimeOnly.MinValue);
            NewPriority = SelectedPricingRule.Priority;
            NewIsActive = SelectedPricingRule.IsActive;
            NewEffectiveFromDate = SelectedPricingRule.EffectiveFromDate.ToDateTime(TimeOnly.MinValue);
            NewEffectiveToDate = SelectedPricingRule.EffectiveToDate?.ToDateTime(TimeOnly.MinValue);
        }
    }

    #endregion

    #region Private Methods

    private bool CanAddPricingRule()
    {
        return !string.IsNullOrWhiteSpace(NewRuleName) && 
               NewBasePricePerKm > 0 && 
               NewPriority > 0 &&
               !IsLoading;
    }

    private void AddPricingRule()
    {
        try
        {
            var newRule = new PricingRule
            {
                RuleName = NewRuleName,
                Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription,
                BasePricePerKm = NewBasePricePerKm,
                TrainTypeId = NewTrainTypeId,
                RouteId = NewRouteId,
                IsForRoundTrip = NewIsForRoundTrip,
                ApplicableDateStart = NewApplicableDateStart.HasValue ? DateOnly.FromDateTime(NewApplicableDateStart.Value) : null,
                ApplicableDateEnd = NewApplicableDateEnd.HasValue ? DateOnly.FromDateTime(NewApplicableDateEnd.Value) : null,
                Priority = NewPriority,
                IsActive = NewIsActive,
                EffectiveFromDate = DateOnly.FromDateTime(NewEffectiveFromDate),
                EffectiveToDate = NewEffectiveToDate.HasValue ? DateOnly.FromDateTime(NewEffectiveToDate.Value) : null
            };

            if (_pricingRuleService.AddPricingRule(newRule))
            {
                RefreshData();
                ClearForm();
                SetSuccessMessage("Pricing rule added successfully");
            }
            else
            {
                SetErrorMessage("Failed to add pricing rule");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error adding pricing rule: {ex.Message}");
        }
    }

    private bool CanUpdatePricingRule()
    {
        return SelectedPricingRule != null && 
               !string.IsNullOrWhiteSpace(NewRuleName) && 
               NewBasePricePerKm > 0 && 
               NewPriority > 0 &&
               !IsLoading;
    }

    private void UpdatePricingRule()
    {
        if (SelectedPricingRule == null) return;

        try
        {
            SelectedPricingRule.RuleName = NewRuleName;
            SelectedPricingRule.Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription;
            SelectedPricingRule.BasePricePerKm = NewBasePricePerKm;
            SelectedPricingRule.TrainTypeId = NewTrainTypeId;
            SelectedPricingRule.RouteId = NewRouteId;
            SelectedPricingRule.IsForRoundTrip = NewIsForRoundTrip;
            SelectedPricingRule.ApplicableDateStart = NewApplicableDateStart.HasValue ? DateOnly.FromDateTime(NewApplicableDateStart.Value) : null;
            SelectedPricingRule.ApplicableDateEnd = NewApplicableDateEnd.HasValue ? DateOnly.FromDateTime(NewApplicableDateEnd.Value) : null;
            SelectedPricingRule.Priority = NewPriority;
            SelectedPricingRule.IsActive = NewIsActive;
            SelectedPricingRule.EffectiveFromDate = DateOnly.FromDateTime(NewEffectiveFromDate);
            SelectedPricingRule.EffectiveToDate = NewEffectiveToDate.HasValue ? DateOnly.FromDateTime(NewEffectiveToDate.Value) : null;

            if (_pricingRuleService.UpdatePricingRule(SelectedPricingRule))
            {
                RefreshData();
                SetSuccessMessage("Pricing rule updated successfully");
            }
            else
            {
                SetErrorMessage("Failed to update pricing rule");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error updating pricing rule: {ex.Message}");
        }
    }

    private bool CanDeletePricingRule()
    {
        return SelectedPricingRule != null && !IsLoading;
    }

    private void DeletePricingRule()
    {
        if (SelectedPricingRule == null) return;

        try
        {
            if (_pricingRuleService.DeletePricingRule(SelectedPricingRule.RuleId))
            {
                RefreshData();
                ClearForm();
                SetSuccessMessage("Pricing rule deleted successfully");
            }
            else
            {
                SetErrorMessage("Failed to delete pricing rule");
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error deleting pricing rule: {ex.Message}");
        }
    }

    private bool CanToggleActive()
    {
        return SelectedPricingRule != null && !IsLoading;
    }

    private void ToggleActive()
    {
        if (SelectedPricingRule == null) return;

        try
        {
            if (SelectedPricingRule.IsActive)
            {
                if (_pricingRuleService.DeactivatePricingRule(SelectedPricingRule.RuleId))
                {
                    RefreshData();
                    SetSuccessMessage("Pricing rule deactivated successfully");
                }
                else
                {
                    SetErrorMessage("Failed to deactivate pricing rule");
                }
            }
            else
            {
                if (_pricingRuleService.ActivatePricingRule(SelectedPricingRule.RuleId))
                {
                    RefreshData();
                    SetSuccessMessage("Pricing rule activated successfully");
                }
                else
                {
                    SetErrorMessage("Failed to activate pricing rule");
                }
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error toggling pricing rule: {ex.Message}");
        }
    }

    private void SearchPricingRules()
    {
        try
        {
            SetLoadingState(true, "Searching pricing rules...");
            
            var allRules = _pricingRuleService.GetAllPricingRules();
            var filteredRules = allRules.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredRules = filteredRules.Where(r => 
                    r.RuleName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedTrainTypeFilter != null)
            {
                filteredRules = filteredRules.Where(r => r.TrainTypeId == SelectedTrainTypeFilter.TrainTypeId);
            }

            if (SelectedRouteFilter != null)
            {
                filteredRules = filteredRules.Where(r => r.RouteId == SelectedRouteFilter.RouteId);
            }

            PricingRules = new ObservableCollection<PricingRule>(filteredRules);
            SetSuccessMessage($"Found {PricingRules.Count} pricing rules");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error searching pricing rules: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool CanTestPriceCalculation()
    {
        return SelectedRouteFilter != null && SelectedTrainTypeFilter != null && !IsLoading;
    }

    private void TestPriceCalculation()
    {
        if (SelectedRouteFilter == null || SelectedTrainTypeFilter == null) return;

        try
        {
            var testDate = DateTime.Today;
            var oneWayPrice = _pricingRuleService.CalculatePrice(SelectedRouteFilter.RouteId, SelectedTrainTypeFilter.TrainTypeId, false, testDate);
            var roundTripPrice = _pricingRuleService.CalculatePrice(SelectedRouteFilter.RouteId, SelectedTrainTypeFilter.TrainTypeId, true, testDate);

            SetSuccessMessage($"Test Price - One Way: {oneWayPrice:C}, Round Trip: {roundTripPrice:C}");
        }
        catch (Exception ex)
        {
            SetErrorMessage($"Error calculating test price: {ex.Message}");
        }
    }

    #endregion
}