using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IRevenueAnalyticsService
{
    Task<decimal> GetRevenueForPeriodAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetDailyRevenueAsync(DateTime date);
    Task<decimal> GetWeeklyRevenueAsync(DateTime weekStart);
    Task<decimal> GetMonthlyRevenueAsync(int year, int month);
    Task<List<DailyRevenueData>> GetDailyRevenueDataAsync(DateTime startDate, DateTime endDate);
    Task<List<MonthlyRevenueData>> GetMonthlyRevenueDataAsync(int year);
    Task<RevenueStatistics> GetRevenueStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<List<TopRouteRevenue>> GetTopRoutesByRevenueAsync(DateTime startDate, DateTime endDate, int limit = 5);
    Task<decimal> GetAverageBookingValueAsync(DateTime startDate, DateTime endDate);
    Task<int> GetTotalBookingsCountAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetRefundAmountAsync(DateTime startDate, DateTime endDate);
}

public class DailyRevenueData
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class MonthlyRevenueData
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class RevenueStatistics
{
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal HighestDayRevenue { get; set; }
    public DateTime HighestRevenueDate { get; set; }
    public decimal LowestDayRevenue { get; set; }
    public DateTime LowestRevenueDate { get; set; }
}

public class TopRouteRevenue
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}