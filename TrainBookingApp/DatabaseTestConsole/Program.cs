using System;
using System.Linq;
using TrainBookingApp.Models;

Console.WriteLine("Checking PassengerTypes table:");

try
{
    using var context = new Context();
    
    // Check if the table exists and has data
    var passengerTypes = context.PassengerTypes.ToList();
    
    if (passengerTypes.Any())
    {
        Console.WriteLine($"Found {passengerTypes.Count} PassengerTypes:");
        foreach (var pt in passengerTypes)
        {
            Console.WriteLine($"  ID: {pt.PassengerTypeId}, Name: {pt.TypeName}, Discount: {pt.DiscountPercentage:P}, RequiresDocument: {pt.RequiresDocument}");
        }
    }
    else
    {
        Console.WriteLine("No PassengerTypes found in database.");
        Console.WriteLine("The table exists but is empty.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error checking PassengerTypes: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();