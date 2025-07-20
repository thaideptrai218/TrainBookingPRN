using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public class PricingRuleService : IPricingRuleService
{
    private readonly Context _context;

    public PricingRuleService(Context context)
    {
        _context = context;
    }

    public List<PricingRule> GetAllPricingRules()
    {
        return _context.PricingRules
            .Include(pr => pr.TrainType)
            .Include(pr => pr.Route)
            .OrderBy(pr => pr.Priority)
            .ThenBy(pr => pr.RuleName)
            .ToList();
    }

    public PricingRule? GetPricingRuleById(int ruleId)
    {
        return _context.PricingRules
            .Include(pr => pr.TrainType)
            .Include(pr => pr.Route)
            .FirstOrDefault(pr => pr.RuleId == ruleId);
    }

    public List<PricingRule> GetActivePricingRules()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _context.PricingRules
            .Include(pr => pr.TrainType)
            .Include(pr => pr.Route)
            .Where(pr => pr.IsActive && 
                         pr.EffectiveFromDate <= today && 
                         (pr.EffectiveToDate == null || pr.EffectiveToDate >= today))
            .OrderBy(pr => pr.Priority)
            .ToList();
    }

    public List<PricingRule> GetPricingRulesByTrainType(int trainTypeId)
    {
        return _context.PricingRules
            .Include(pr => pr.TrainType)
            .Include(pr => pr.Route)
            .Where(pr => pr.TrainTypeId == trainTypeId)
            .OrderBy(pr => pr.Priority)
            .ToList();
    }

    public List<PricingRule> GetPricingRulesByRoute(int routeId)
    {
        return _context.PricingRules
            .Include(pr => pr.TrainType)
            .Include(pr => pr.Route)
            .Where(pr => pr.RouteId == routeId)
            .OrderBy(pr => pr.Priority)
            .ToList();
    }

    public bool AddPricingRule(PricingRule rule)
    {
        try
        {
            _context.PricingRules.Add(rule);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool UpdatePricingRule(PricingRule rule)
    {
        try
        {
            _context.PricingRules.Update(rule);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeletePricingRule(int ruleId)
    {
        try
        {
            var rule = _context.PricingRules.FirstOrDefault(pr => pr.RuleId == ruleId);
            if (rule != null)
            {
                _context.PricingRules.Remove(rule);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool DeactivatePricingRule(int ruleId)
    {
        try
        {
            var rule = _context.PricingRules.FirstOrDefault(pr => pr.RuleId == ruleId);
            if (rule != null)
            {
                rule.IsActive = false;
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool ActivatePricingRule(int ruleId)
    {
        try
        {
            var rule = _context.PricingRules.FirstOrDefault(pr => pr.RuleId == ruleId);
            if (rule != null)
            {
                rule.IsActive = true;
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public decimal CalculatePrice(int routeId, int trainTypeId, bool isRoundTrip, DateTime travelDate)
    {
        var travelDateOnly = DateOnly.FromDateTime(travelDate);
        
        // Get the route to calculate distance
        var route = _context.Routes
            .Include(r => r.RouteStations)
            .FirstOrDefault(r => r.RouteId == routeId);
        
        if (route == null) return 0;
        
        // Calculate total distance
        var totalDistance = route.RouteStations
            .OrderBy(rs => rs.SequenceNumber)
            .LastOrDefault()?.DistanceFromStart ?? 0;
        
        // Find applicable pricing rules
        var applicableRules = _context.PricingRules
            .Where(pr => pr.IsActive &&
                         pr.EffectiveFromDate <= travelDateOnly &&
                         (pr.EffectiveToDate == null || pr.EffectiveToDate >= travelDateOnly) &&
                         (pr.RouteId == null || pr.RouteId == routeId) &&
                         (pr.TrainTypeId == null || pr.TrainTypeId == trainTypeId) &&
                         (pr.IsForRoundTrip == null || pr.IsForRoundTrip == isRoundTrip) &&
                         (pr.ApplicableDateStart == null || pr.ApplicableDateStart <= travelDateOnly) &&
                         (pr.ApplicableDateEnd == null || pr.ApplicableDateEnd >= travelDateOnly))
            .OrderBy(pr => pr.Priority)
            .ToList();
        
        // Use the highest priority rule (lowest Priority number)
        var selectedRule = applicableRules.FirstOrDefault();
        
        if (selectedRule != null)
        {
            var basePrice = selectedRule.BasePricePerKm * totalDistance;
            return isRoundTrip ? basePrice * 2 : basePrice;
        }
        
        // Default fallback price if no rules match
        return totalDistance * 0.5m; // 0.5 per km as default
    }
}