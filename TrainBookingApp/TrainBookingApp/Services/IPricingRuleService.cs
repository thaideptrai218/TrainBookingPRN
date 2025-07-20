using TrainBookingApp.Models;

namespace TrainBookingApp.Services;

public interface IPricingRuleService
{
    List<PricingRule> GetAllPricingRules();
    PricingRule? GetPricingRuleById(int ruleId);
    List<PricingRule> GetActivePricingRules();
    List<PricingRule> GetPricingRulesByTrainType(int trainTypeId);
    List<PricingRule> GetPricingRulesByRoute(int routeId);
    bool AddPricingRule(PricingRule rule);
    bool UpdatePricingRule(PricingRule rule);
    bool DeletePricingRule(int ruleId);
    bool DeactivatePricingRule(int ruleId);
    bool ActivatePricingRule(int ruleId);
    decimal CalculatePrice(int routeId, int trainTypeId, bool isRoundTrip, DateTime travelDate);
}