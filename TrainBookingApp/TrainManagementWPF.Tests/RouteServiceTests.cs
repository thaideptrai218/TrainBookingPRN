using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for RouteService - testing complex relationship management and station sequencing
/// This is the most complex service with intricate business logic for route-station relationships
/// </summary>
public class RouteServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly RouteService _routeService;

    public RouteServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestContext(options);
        _routeService = new RouteService(_context);

        // Seed initial test data
        SeedTestData();
    }

    [Fact]
    public void GetAllRoutes_WithExistingRoutes_ShouldReturnOrderedListWithStations()
    {
        // Act
        var routes = _routeService.GetAllRoutes().ToList();

        // Assert
        Assert.NotNull(routes);
        Assert.Equal(2, routes.Count);
        
        // Verify ordering by RouteName
        Assert.Equal("North-South Express", routes[0].RouteName);
        Assert.Equal("West-East Local", routes[1].RouteName);
        
        // Verify route stations are loaded
        Assert.NotEmpty(routes[0].RouteStations);
        Assert.All(routes[0].RouteStations, rs => Assert.NotNull(rs.Station));
    }

    [Fact]
    public void GetAllRoutes_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Clear all data
        _context.RouteStations.RemoveRange(_context.RouteStations);
        _context.Routes.RemoveRange(_context.Routes);
        _context.SaveChanges();

        // Act
        var routes = _routeService.GetAllRoutes().ToList();

        // Assert
        Assert.NotNull(routes);
        Assert.Empty(routes);
    }

    [Fact]
    public void GetRouteById_WithExistingId_ShouldReturnRouteWithStations()
    {
        // Act
        var route = _routeService.GetRouteById(1);

        // Assert
        Assert.NotNull(route);
        Assert.Equal(1, route.RouteId);
        Assert.Equal("North-South Express", route.RouteName);
        Assert.NotEmpty(route.RouteStations);
        Assert.Equal(3, route.RouteStations.Count);
        
        // Verify stations are loaded
        Assert.All(route.RouteStations, rs => Assert.NotNull(rs.Station));
    }

    [Fact]
    public void GetRouteById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var route = _routeService.GetRouteById(999);

        // Assert
        Assert.Null(route);
    }

    [Fact]
    public void GetRoutesByStation_WithExistingStation_ShouldReturnRoutesContainingStation()
    {
        // Act - Get routes that contain Central Station (ID: 1)
        var routes = _routeService.GetRoutesByStation(1).ToList();

        // Assert
        Assert.NotNull(routes);
        Assert.Single(routes);
        Assert.Equal("North-South Express", routes[0].RouteName);
        
        // Verify the route actually contains the station
        Assert.Contains(routes[0].RouteStations, rs => rs.StationId == 1);
    }

    [Fact]
    public void GetRoutesByStation_WithNonExistentStation_ShouldReturnEmptyList()
    {
        // Act
        var routes = _routeService.GetRoutesByStation(999).ToList();

        // Assert
        Assert.NotNull(routes);
        Assert.Empty(routes);
    }

    [Fact]
    public void CreateRoute_WithValidData_ShouldReturnCreatedRoute()
    {
        // Arrange
        var newRoute = new Route
        {
            RouteName = "East-West Express",
            Description = "High-speed cross-country route"
        };

        // Act
        var result = _routeService.CreateRoute(newRoute);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("East-West Express", result.RouteName);
        Assert.True(result.RouteId > 0);
        
        // Verify it was saved to database
        var savedRoute = _context.Routes.FirstOrDefault(r => r.RouteName == "East-West Express");
        Assert.NotNull(savedRoute);
        Assert.Equal("High-speed cross-country route", savedRoute.Description);
    }

    [Fact]
    public void CreateRoute_WithNullData_ShouldThrowException()
    {
        // Arrange
        Route nullRoute = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _routeService.CreateRoute(nullRoute));
    }

    [Fact]
    public void UpdateRoute_WithExistingRoute_ShouldReturnUpdatedRoute()
    {
        // Arrange
        var existingRoute = _context.Routes.First();
        existingRoute.RouteName = "Updated Express Route";
        existingRoute.Description = "Updated description";

        // Act
        var result = _routeService.UpdateRoute(existingRoute);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Express Route", result.RouteName);
        
        // Verify the update in database
        var updatedRoute = _context.Routes.Find(existingRoute.RouteId);
        Assert.NotNull(updatedRoute);
        Assert.Equal("Updated Express Route", updatedRoute.RouteName);
        Assert.Equal("Updated description", updatedRoute.Description);
    }

    [Fact]
    public void UpdateRoute_WithInvalidData_ShouldReturnNull()
    {
        // Arrange - Create a route with invalid properties
        var invalidRoute = new Route
        {
            RouteId = 999, // Non-existent ID
            RouteName = null!, // Invalid - required field
            Description = "Test"
        };

        // Act
        var result = _routeService.UpdateRoute(invalidRoute);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteRoute_WithExistingRoute_ShouldReturnTrueAndRemoveFromDatabase()
    {
        // Arrange
        var routeToDelete = _context.Routes.Skip(1).First(); // Get second route to avoid FK constraints
        var routeId = routeToDelete.RouteId;
        var initialCount = _context.Routes.Count();

        // Act
        var result = _routeService.DeleteRoute(routeId);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedRoute = _context.Routes.Find(routeId);
        Assert.Null(deletedRoute);
        
        // Verify count decreased
        var newCount = _context.Routes.Count();
        Assert.Equal(initialCount - 1, newCount);
    }

    [Fact]
    public void DeleteRoute_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _routeService.DeleteRoute(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAllStations_ShouldReturnOrderedStations()
    {
        // Act
        var stations = _routeService.GetAllStations().ToList();

        // Assert
        Assert.NotNull(stations);
        Assert.Equal(4, stations.Count);
        
        // Verify ordering by StationName
        Assert.Equal("Central Station", stations[0].StationName);
        Assert.Equal("East Terminal", stations[1].StationName);
        Assert.Equal("North Terminal", stations[2].StationName);
        Assert.Equal("South Terminal", stations[3].StationName);
    }

    [Fact]
    public void AddStationToRoute_WithValidData_ShouldReturnTrueAndCreateRouteStation()
    {
        // Arrange
        var routeId = 1;
        var stationId = 4; // East Terminal - not yet in route 1
        var sequenceNumber = 4;
        var distanceFromStart = 200.0m;
        var defaultStopTime = 3;

        // Act
        var result = _routeService.AddStationToRoute(routeId, stationId, sequenceNumber, distanceFromStart, defaultStopTime);

        // Assert
        Assert.True(result);
        
        // Verify route station was created
        var routeStation = _context.RouteStations
            .FirstOrDefault(rs => rs.RouteId == routeId && rs.StationId == stationId);
        Assert.NotNull(routeStation);
        Assert.Equal(sequenceNumber, routeStation.SequenceNumber);
        Assert.Equal(distanceFromStart, routeStation.DistanceFromStart);
        Assert.Equal(defaultStopTime, routeStation.DefaultStopTime);
    }

    [Fact]
    public void AddStationToRoute_WithNonExistentRoute_ShouldReturnFalse()
    {
        // Act
        var result = _routeService.AddStationToRoute(999, 1, 1, 0.0m, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AddStationToRoute_WithNonExistentStation_ShouldReturnFalse()
    {
        // Act
        var result = _routeService.AddStationToRoute(1, 999, 1, 0.0m, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AddStationToRoute_WithExistingStationInRoute_ShouldReturnFalse()
    {
        // Act - Try to add Central Station (ID: 1) to Route 1 again
        var result = _routeService.AddStationToRoute(1, 1, 4, 0.0m, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveStationFromRoute_WithExistingRouteStation_ShouldReturnTrueAndRemove()
    {
        // Arrange - Add a station first
        _routeService.AddStationToRoute(1, 4, 4, 200.0m, 3);
        
        // Verify it was added
        var routeStationBefore = _context.RouteStations
            .FirstOrDefault(rs => rs.RouteId == 1 && rs.StationId == 4);
        Assert.NotNull(routeStationBefore);

        // Act
        var result = _routeService.RemoveStationFromRoute(1, 4);

        // Assert
        Assert.True(result);
        
        // Verify removal
        var routeStationAfter = _context.RouteStations
            .FirstOrDefault(rs => rs.RouteId == 1 && rs.StationId == 4);
        Assert.Null(routeStationAfter);
    }

    [Fact]
    public void RemoveStationFromRoute_WithNonExistentRouteStation_ShouldReturnFalse()
    {
        // Act
        var result = _routeService.RemoveStationFromRoute(999, 999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateRouteStationSequence_WithExistingRouteStation_ShouldReturnTrueAndUpdate()
    {
        // Arrange - Get existing route station
        var existingRouteStation = _context.RouteStations.First();
        var routeId = existingRouteStation.RouteId;
        var stationId = existingRouteStation.StationId;
        var originalSequence = existingRouteStation.SequenceNumber;
        var newSequence = 99;

        // Act
        var result = _routeService.UpdateRouteStationSequence(routeId, stationId, newSequence);

        // Assert
        Assert.True(result);
        
        // Verify update
        var updatedRouteStation = _context.RouteStations
            .FirstOrDefault(rs => rs.RouteId == routeId && rs.StationId == stationId);
        Assert.NotNull(updatedRouteStation);
        Assert.Equal(newSequence, updatedRouteStation.SequenceNumber);
        Assert.NotEqual(originalSequence, updatedRouteStation.SequenceNumber);
    }

    [Fact]
    public void UpdateRouteStationSequence_WithNonExistentRouteStation_ShouldReturnFalse()
    {
        // Act
        var result = _routeService.UpdateRouteStationSequence(999, 999, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetRouteStations_WithExistingRoute_ShouldReturnOrderedStations()
    {
        // Act
        var routeStations = _routeService.GetRouteStations(1).ToList();

        // Assert
        Assert.NotNull(routeStations);
        Assert.Equal(3, routeStations.Count);
        
        // Verify ordering by SequenceNumber
        Assert.Equal(1, routeStations[0].SequenceNumber);
        Assert.Equal(2, routeStations[1].SequenceNumber);
        Assert.Equal(3, routeStations[2].SequenceNumber);
        
        // Verify stations are loaded
        Assert.All(routeStations, rs => Assert.NotNull(rs.Station));
        
        // Verify specific station sequence
        Assert.Equal("Central Station", routeStations[0].Station.StationName);
        Assert.Equal("North Terminal", routeStations[1].Station.StationName);
        Assert.Equal("South Terminal", routeStations[2].Station.StationName);
    }

    [Fact]
    public void GetRouteStations_WithNonExistentRoute_ShouldReturnEmptyList()
    {
        // Act
        var routeStations = _routeService.GetRouteStations(999).ToList();

        // Assert
        Assert.NotNull(routeStations);
        Assert.Empty(routeStations);
    }

    [Fact]
    public void IsRouteNameExists_WithExistingName_ShouldReturnTrue()
    {
        // Act
        var exists = _routeService.IsRouteNameExists("North-South Express");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void IsRouteNameExists_WithNonExistentName_ShouldReturnFalse()
    {
        // Act
        var exists = _routeService.IsRouteNameExists("Hyperloop Route");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void IsRouteNameExists_WithCaseSensitiveName_ShouldBeCaseSensitive()
    {
        // Act
        var existsUpperCase = _routeService.IsRouteNameExists("NORTH-SOUTH EXPRESS");
        var existsLowerCase = _routeService.IsRouteNameExists("north-south express");

        // Assert - Should be case sensitive
        Assert.False(existsUpperCase);
        Assert.False(existsLowerCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsRouteNameExists_WithEmptyOrWhitespaceName_ShouldReturnFalse(string routeName)
    {
        // Act
        var exists = _routeService.IsRouteNameExists(routeName);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void RouteService_ComplexWorkflowTest_CreateRouteWithStations()
    {
        // This test validates the complete route creation workflow with stations
        
        // 1. Create Route
        var newRoute = new Route
        {
            RouteName = "Coastal Express",
            Description = "Scenic coastal route"
        };
        
        var createdRoute = _routeService.CreateRoute(newRoute);
        Assert.NotNull(createdRoute);
        Assert.True(createdRoute.RouteId > 0);
        
        var routeId = createdRoute.RouteId;
        
        // 2. Add Stations to Route (in sequence)
        var addStation1 = _routeService.AddStationToRoute(routeId, 1, 1, 0.0m, 5);    // Central Station
        var addStation2 = _routeService.AddStationToRoute(routeId, 2, 2, 50.0m, 3);   // North Terminal  
        var addStation3 = _routeService.AddStationToRoute(routeId, 4, 3, 120.0m, 2);  // East Terminal
        
        Assert.True(addStation1);
        Assert.True(addStation2);
        Assert.True(addStation3);
        
        // 3. Verify Route Stations
        var routeStations = _routeService.GetRouteStations(routeId).ToList();
        Assert.Equal(3, routeStations.Count);
        Assert.Equal(1, routeStations[0].SequenceNumber);
        Assert.Equal(2, routeStations[1].SequenceNumber);
        Assert.Equal(3, routeStations[2].SequenceNumber);
        
        // 4. Update Station Sequence
        var updateSequence = _routeService.UpdateRouteStationSequence(routeId, 4, 4);
        Assert.True(updateSequence);
        
        // 5. Remove a Station
        var removeStation = _routeService.RemoveStationFromRoute(routeId, 2);
        Assert.True(removeStation);
        
        // 6. Verify Final State
        var finalStations = _routeService.GetRouteStations(routeId).ToList();
        Assert.Equal(2, finalStations.Count);
        
        // 7. Verify Route is Found by Station
        var routesByStation = _routeService.GetRoutesByStation(1).ToList();
        Assert.Contains(routesByStation, r => r.RouteId == routeId);
        
        // 8. Delete Route
        var deleteResult = _routeService.DeleteRoute(routeId);
        Assert.True(deleteResult);
        
        // 9. Verify Deletion
        var deletedRoute = _routeService.GetRouteById(routeId);
        Assert.Null(deletedRoute);
    }

    [Fact]
    public void RouteService_StationSequenceIntegrityTest()
    {
        // This test validates station sequence management
        
        var routeId = 1;
        
        // Get current stations
        var originalStations = _routeService.GetRouteStations(routeId).ToList();
        Assert.Equal(3, originalStations.Count);
        
        // Add a new station with sequence 2 (should fit between existing stations)
        var addResult = _routeService.AddStationToRoute(routeId, 4, 2, 75.0m, 3);
        Assert.True(addResult);
        
        // Verify the new station is inserted correctly
        var updatedStations = _routeService.GetRouteStations(routeId).ToList();
        Assert.Equal(4, updatedStations.Count);
        
        // Note: The service doesn't automatically resequence, so we should have:
        // Sequence 1: Central Station, Sequence 2: East Terminal (new), 
        // Sequence 2: North Terminal (duplicate sequence), Sequence 3: South Terminal
        var stationsWithSeq2 = updatedStations.Where(rs => rs.SequenceNumber == 2).ToList();
        Assert.Equal(2, stationsWithSeq2.Count); // Two stations with sequence 2
        
        // Update sequences to fix conflicts
        var updateResult1 = _routeService.UpdateRouteStationSequence(routeId, 2, 3); // North Terminal to 3
        var updateResult2 = _routeService.UpdateRouteStationSequence(routeId, 3, 4); // South Terminal to 4
        
        Assert.True(updateResult1);
        Assert.True(updateResult2);
        
        // Verify final sequence is correct
        var finalStations = _routeService.GetRouteStations(routeId).ToList();
        Assert.Equal(4, finalStations.Count);
        Assert.Equal(1, finalStations[0].SequenceNumber);
        Assert.Equal(2, finalStations[1].SequenceNumber);
        Assert.Equal(3, finalStations[2].SequenceNumber);
        Assert.Equal(4, finalStations[3].SequenceNumber);
    }

    [Fact]
    public void RouteService_ConcurrencyTest_MultipleOperations()
    {
        // Test multiple operations on the same route
        var routeId = 1;
        
        // Perform multiple operations
        var getAllRoutes1 = _routeService.GetAllRoutes().ToList();
        var getRouteStations1 = _routeService.GetRouteStations(routeId).ToList();
        var getAllRoutes2 = _routeService.GetAllRoutes().ToList();
        var getRouteStations2 = _routeService.GetRouteStations(routeId).ToList();
        
        // Results should be consistent
        Assert.Equal(getAllRoutes1.Count, getAllRoutes2.Count);
        Assert.Equal(getRouteStations1.Count, getRouteStations2.Count);
        
        for (int i = 0; i < getRouteStations1.Count; i++)
        {
            Assert.Equal(getRouteStations1[i].RouteStationId, getRouteStations2[i].RouteStationId);
            Assert.Equal(getRouteStations1[i].SequenceNumber, getRouteStations2[i].SequenceNumber);
        }
    }

    private void SeedTestData()
    {
        // Seed Stations
        _context.Stations.AddRange(
            new Station { StationId = 1, StationName = "Central Station", StationCode = "CS001", City = "Metro City" },
            new Station { StationId = 2, StationName = "North Terminal", StationCode = "NT002", City = "North City" },
            new Station { StationId = 3, StationName = "South Terminal", StationCode = "ST003", City = "South City" },
            new Station { StationId = 4, StationName = "East Terminal", StationCode = "ET004", City = "East City" }
        );

        // Seed Routes
        _context.Routes.AddRange(
            new Route { RouteId = 1, RouteName = "North-South Express", Description = "High-speed north-south corridor" },
            new Route { RouteId = 2, RouteName = "West-East Local", Description = "Local service across the region" }
        );

        // Seed RouteStations for Route 1 (North-South Express)
        _context.RouteStations.AddRange(
            new RouteStation { RouteStationId = 1, RouteId = 1, StationId = 1, SequenceNumber = 1, DistanceFromStart = 0.0m, DefaultStopTime = 5 },
            new RouteStation { RouteStationId = 2, RouteId = 1, StationId = 2, SequenceNumber = 2, DistanceFromStart = 50.0m, DefaultStopTime = 3 },
            new RouteStation { RouteStationId = 3, RouteId = 1, StationId = 3, SequenceNumber = 3, DistanceFromStart = 100.0m, DefaultStopTime = 2 }
        );

        // Seed RouteStations for Route 2 (West-East Local) - minimal for testing
        _context.RouteStations.AddRange(
            new RouteStation { RouteStationId = 4, RouteId = 2, StationId = 4, SequenceNumber = 1, DistanceFromStart = 0.0m, DefaultStopTime = 5 }
        );

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}