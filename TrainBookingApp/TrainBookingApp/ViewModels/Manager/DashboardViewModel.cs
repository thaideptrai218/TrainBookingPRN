using System.Collections.ObjectModel;
using System.Windows.Input;
using TrainBookingApp.Services;

namespace TrainBookingApp.ViewModels.Manager;

public class DashboardViewModel : BaseManagerViewModel, IManagerTabViewModel
{
    private readonly IRevenueAnalyticsService _revenueService;

    private DateTime _selectedStartDate = DateTime.Today.AddDays(-30);
    private DateTime _selectedEndDate = DateTime.Today;
    private string _selectedTimePeriod = "Last 30 Days";
    private decimal _totalRevenue;
    private decimal _averageBookingValue;
    private int _totalBookings;
    private decimal _netRevenue;
    private decimal _totalRefunds;
    private decimal _dailyRevenue;
    private decimal _weeklyRevenue;
    private decimal _monthlyRevenue;
    private decimal _quarterlyRevenue;
    private decimal _yearlyRevenue;

    public DashboardViewModel(IRevenueAnalyticsService revenueService)
    {
        _revenueService = revenueService;

        DailyRevenueData = new ObservableCollection<DailyRevenueData>();
        MonthlyRevenueData = new ObservableCollection<MonthlyRevenueData>();
        TopRoutes = new ObservableCollection<TopRouteRevenue>();

        TimePeriodOptions = new ObservableCollection<string>
        {
            "Today",
            "Last 7 Days",
            "Last 30 Days",
            "Last 3 Months",
            "Last 12 Months",
            "Custom Range"
        };

        InitializeCommands();
        RefreshData();
    }

    #region Properties

    public override string TabName => "Revenue Dashboard";

    public ObservableCollection<string> TimePeriodOptions { get; }
    public ObservableCollection<DailyRevenueData> DailyRevenueData { get; }
    public ObservableCollection<MonthlyRevenueData> MonthlyRevenueData { get; }
    public ObservableCollection<TopRouteRevenue> TopRoutes { get; }

    public string SelectedTimePeriod
    {
        get => _selectedTimePeriod;
        set
        {
            if (SetProperty(ref _selectedTimePeriod, value))
            {
                OnTimePeriodChanged();
            }
        }
    }

    public DateTime SelectedStartDate
    {
        get => _selectedStartDate;
        set
        {
            if (SetProperty(ref _selectedStartDate, value))
            {
                if (SelectedTimePeriod == "Custom Range")
                {
                    RefreshData();
                }
            }
        }
    }

    public DateTime SelectedEndDate
    {
        get => _selectedEndDate;
        set
        {
            if (SetProperty(ref _selectedEndDate, value))
            {
                if (SelectedTimePeriod == "Custom Range")
                {
                    RefreshData();
                }
            }
        }
    }

    public bool IsCustomRangeVisible => SelectedTimePeriod == "Custom Range";

    public decimal TotalRevenue
    {
        get => _totalRevenue;
        set => SetProperty(ref _totalRevenue, value);
    }

    public decimal AverageBookingValue
    {
        get => _averageBookingValue;
        set => SetProperty(ref _averageBookingValue, value);
    }

    public int TotalBookings
    {
        get => _totalBookings;
        set => SetProperty(ref _totalBookings, value);
    }

    public decimal NetRevenue
    {
        get => _netRevenue;
        set => SetProperty(ref _netRevenue, value);
    }

    public decimal TotalRefunds
    {
        get => _totalRefunds;
        set => SetProperty(ref _totalRefunds, value);
    }

    public decimal DailyRevenue
    {
        get => _dailyRevenue;
        set => SetProperty(ref _dailyRevenue, value);
    }

    public decimal WeeklyRevenue
    {
        get => _weeklyRevenue;
        set => SetProperty(ref _weeklyRevenue, value);
    }

    public decimal MonthlyRevenue
    {
        get => _monthlyRevenue;
        set => SetProperty(ref _monthlyRevenue, value);
    }

    public decimal QuarterlyRevenue
    {
        get => _quarterlyRevenue;
        set => SetProperty(ref _quarterlyRevenue, value);
    }

    public decimal YearlyRevenue
    {
        get => _yearlyRevenue;
        set => SetProperty(ref _yearlyRevenue, value);
    }

    #endregion

    #region Commands

    public ICommand RefreshDataCommand { get; private set; } = null!;
    public ICommand ExportDataCommand { get; private set; } = null!;

    #endregion

    #region Command Initialization

    private void InitializeCommands()
    {
        RefreshDataCommand = new RelayCommand(_ => RefreshData());
        ExportDataCommand = new RelayCommand(_ => ExportData());
    }

    #endregion

    #region Public Methods

    public override void RefreshData()
    {
        _ = LoadRevenueDataAsync();
    }

    public override void ClearForm()
    {
        SelectedTimePeriod = "Last 30 Days";
        OnTimePeriodChanged();
    }

    #endregion

    #region Private Methods

    private void OnTimePeriodChanged()
    {
        var today = DateTime.Today;

        switch (SelectedTimePeriod)
        {
            case "Today":
                SelectedStartDate = today;
                SelectedEndDate = today.AddDays(1).AddTicks(-1);
                break;
            case "Last 7 Days":
                SelectedStartDate = today.AddDays(-7);
                SelectedEndDate = today.AddDays(1).AddTicks(-1);
                break;
            case "Last 30 Days":
                SelectedStartDate = today.AddDays(-30);
                SelectedEndDate = today.AddDays(1).AddTicks(-1);
                break;
            case "Last 3 Months":
                SelectedStartDate = today.AddMonths(-3);
                SelectedEndDate = today.AddDays(1).AddTicks(-1);
                break;
            case "Last 12 Months":
                SelectedStartDate = today.AddYears(-1);
                SelectedEndDate = today.AddDays(1).AddTicks(-1);
                break;
            case "Custom Range":
                OnPropertyChanged(nameof(IsCustomRangeVisible));
                return;
        }

        OnPropertyChanged(nameof(IsCustomRangeVisible));
        RefreshData();
    }

    private async Task LoadRevenueDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading revenue data...";

            // Load summary statistics
            var statistics = await _revenueService.GetRevenueStatisticsAsync(SelectedStartDate, SelectedEndDate);

            TotalRevenue = statistics.TotalRevenue;
            AverageBookingValue = statistics.AverageBookingValue;
            TotalBookings = statistics.TotalBookings;
            TotalRefunds = statistics.TotalRefunds;
            NetRevenue = statistics.NetRevenue;

            // Load quick revenue snapshots
            DailyRevenue = await _revenueService.GetDailyRevenueAsync(DateTime.Today);
            WeeklyRevenue = await _revenueService.GetWeeklyRevenueAsync(DateTime.Today.AddDays(-7));
            MonthlyRevenue = await _revenueService.GetMonthlyRevenueAsync(DateTime.Today.Year, DateTime.Today.Month);
            QuarterlyRevenue = await _revenueService.GetRevenueForPeriodAsync(
                new DateTime(DateTime.Today.Year, ((DateTime.Today.Month - 1) / 3) * 3 + 1, 1),
                DateTime.Today);
            YearlyRevenue = await _revenueService.GetRevenueForPeriodAsync(
                new DateTime(DateTime.Today.Year, 1, 1),
                DateTime.Today);

            // Load chart data
            var dailyData = await _revenueService.GetDailyRevenueDataAsync(SelectedStartDate, SelectedEndDate);
            DailyRevenueData.Clear();
            foreach (var item in dailyData)
            {
                DailyRevenueData.Add(item);
            }

            // Load monthly data if date range is large enough
            if ((SelectedEndDate - SelectedStartDate).TotalDays > 30)
            {
                var monthlyData = await _revenueService.GetMonthlyRevenueDataAsync(SelectedEndDate.Year);
                MonthlyRevenueData.Clear();
                foreach (var item in monthlyData)
                {
                    MonthlyRevenueData.Add(item);
                }
            }

            // Load top routes
            var topRoutes = await _revenueService.GetTopRoutesByRevenueAsync(SelectedStartDate, SelectedEndDate, 5);
            TopRoutes.Clear();
            foreach (var route in topRoutes)
            {
                TopRoutes.Add(route);
            }

            StatusMessage = $"Revenue data loaded successfully. Total: {TotalRevenue:C}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading revenue data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExportData()
    {
        try
        {
            StatusMessage = "Export functionality will be implemented in a future update";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting data: {ex.Message}";
        }
    }

    #endregion
}