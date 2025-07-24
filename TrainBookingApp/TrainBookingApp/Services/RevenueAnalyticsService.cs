using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class RevenueAnalyticsService : IRevenueAnalyticsService
{
    private readonly Context _context;

    public RevenueAnalyticsService(Context context)
    {
        _context = context;
    }

    public async Task<decimal> GetRevenueForPeriodAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .SumAsync(b => b.TotalPrice);
    }

    public async Task<decimal> GetDailyRevenueAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1).AddTicks(-1);
        return await GetRevenueForPeriodAsync(startDate, endDate);
    }

    public async Task<decimal> GetWeeklyRevenueAsync(DateTime weekStart)
    {
        var endDate = weekStart.AddDays(7).AddTicks(-1);
        return await GetRevenueForPeriodAsync(weekStart, endDate);
    }

    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddTicks(-1);
        return await GetRevenueForPeriodAsync(startDate, endDate);
    }

    public async Task<List<DailyRevenueData>> GetDailyRevenueDataAsync(DateTime startDate, DateTime endDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .GroupBy(b => b.BookingDateTime.Date)
            .Select(g => new DailyRevenueData
            {
                Date = g.Key,
                Revenue = g.Sum(b => b.TotalPrice),
                BookingCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        // Fill in missing dates with zero revenue
        var result = new List<DailyRevenueData>();
        var currentDate = startDate.Date;
        
        while (currentDate <= endDate.Date)
        {
            var existing = bookings.FirstOrDefault(b => b.Date == currentDate);
            if (existing != null)
            {
                result.Add(existing);
            }
            else
            {
                result.Add(new DailyRevenueData
                {
                    Date = currentDate,
                    Revenue = 0,
                    BookingCount = 0
                });
            }
            currentDate = currentDate.AddDays(1);
        }

        return result;
    }

    public async Task<List<MonthlyRevenueData>> GetMonthlyRevenueDataAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);

        var monthlyData = await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .GroupBy(b => b.BookingDateTime.Month)
            .Select(g => new MonthlyRevenueData
            {
                Month = g.Key,
                Revenue = g.Sum(b => b.TotalPrice),
                BookingCount = g.Count()
            })
            .ToListAsync();

        // Fill in missing months with zero revenue
        var result = new List<MonthlyRevenueData>();
        for (int month = 1; month <= 12; month++)
        {
            var existing = monthlyData.FirstOrDefault(m => m.Month == month);
            if (existing != null)
            {
                existing.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                result.Add(existing);
            }
            else
            {
                result.Add(new MonthlyRevenueData
                {
                    Month = month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Revenue = 0,
                    BookingCount = 0
                });
            }
        }

        return result.OrderBy(m => m.Month).ToList();
    }

    public async Task<RevenueStatistics> GetRevenueStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .ToListAsync();

        var refunds = await GetRefundAmountAsync(startDate, endDate);
        var dailyRevenues = await GetDailyRevenueDataAsync(startDate, endDate);

        var totalRevenue = bookings.Sum(b => b.TotalPrice);
        var totalBookings = bookings.Count;
        var averageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;

        var highestDay = dailyRevenues.OrderByDescending(d => d.Revenue).FirstOrDefault();
        var lowestDay = dailyRevenues.Where(d => d.Revenue > 0).OrderBy(d => d.Revenue).FirstOrDefault();

        return new RevenueStatistics
        {
            TotalRevenue = totalRevenue,
            AverageBookingValue = averageBookingValue,
            TotalBookings = totalBookings,
            TotalRefunds = refunds,
            NetRevenue = totalRevenue - refunds,
            HighestDayRevenue = highestDay?.Revenue ?? 0,
            HighestRevenueDate = highestDay?.Date ?? DateTime.MinValue,
            LowestDayRevenue = lowestDay?.Revenue ?? 0,
            LowestRevenueDate = lowestDay?.Date ?? DateTime.MinValue
        };
    }

    public async Task<List<TopRouteRevenue>> GetTopRoutesByRevenueAsync(DateTime startDate, DateTime endDate, int limit = 5)
    {
        return await _context.Tickets
            .Include(t => t.Booking)
            .Include(t => t.Trip)
            .ThenInclude(trip => trip.Route)
            .Where(t => t.Booking.BookingDateTime >= startDate && 
                       t.Booking.BookingDateTime <= endDate &&
                       t.Booking.PaymentStatus == "Paid")
            .GroupBy(t => new { t.Trip.Route.RouteId, t.Trip.Route.RouteName })
            .Select(g => new TopRouteRevenue
            {
                RouteId = g.Key.RouteId,
                RouteName = g.Key.RouteName,
                Revenue = g.Sum(t => t.Price),
                BookingCount = g.Count()
            })
            .OrderByDescending(r => r.Revenue)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageBookingValueAsync(DateTime startDate, DateTime endDate)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .ToListAsync();

        return bookings.Count > 0 ? bookings.Average(b => b.TotalPrice) : 0;
    }

    public async Task<int> GetTotalBookingsCountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Bookings
            .Where(b => b.BookingDateTime >= startDate && 
                       b.BookingDateTime <= endDate &&
                       b.PaymentStatus == "Paid")
            .CountAsync();
    }

    public async Task<decimal> GetRefundAmountAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Refunds
            .Include(r => r.Booking)
            .Where(r => r.Booking.BookingDateTime >= startDate && 
                       r.Booking.BookingDateTime <= endDate)
            .SumAsync(r => r.ActualRefundAmount);
    }
}