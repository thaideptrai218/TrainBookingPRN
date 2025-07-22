using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for CoachTypeService - testing CRUD operations with compartment filtering and usage validation
/// This service includes business logic for compartment types and usage validation
/// </summary>
public class CoachTypeServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly CoachTypeService _coachTypeService;

    public CoachTypeServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestContext(options);
        _coachTypeService = new CoachTypeService(_context);

        // Seed initial test data
        SeedTestData();
    }

    [Fact]
    public void GetAllCoachTypes_WithExistingTypes_ShouldReturnOrderedListWithCoaches()
    {
        // Act
        var coachTypes = _coachTypeService.GetAllCoachTypes().ToList();

        // Assert
        Assert.NotNull(coachTypes);
        Assert.Equal(4, coachTypes.Count);
        
        // Verify ordering by TypeName
        Assert.Equal("Business Class", coachTypes[0].TypeName);
        Assert.Equal("Economy Class", coachTypes[1].TypeName);
        Assert.Equal("First Class", coachTypes[2].TypeName);
        Assert.Equal("Sleeper Class", coachTypes[3].TypeName);
        
        // Verify includes work (Coaches should be loaded)
        Assert.NotNull(coachTypes[0].Coaches);
    }

    [Fact]
    public void GetAllCoachTypes_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Clear all data
        _context.CoachTypes.RemoveRange(_context.CoachTypes);
        _context.SaveChanges();

        // Act
        var coachTypes = _coachTypeService.GetAllCoachTypes().ToList();

        // Assert
        Assert.NotNull(coachTypes);
        Assert.Empty(coachTypes);
    }

    [Fact]
    public void GetCoachTypeById_WithExistingId_ShouldReturnCoachTypeWithCoaches()
    {
        // Act
        var coachType = _coachTypeService.GetCoachTypeById(1);

        // Assert
        Assert.NotNull(coachType);
        Assert.Equal(1, coachType.CoachTypeId);
        Assert.Equal("Economy Class", coachType.TypeName);
        Assert.Equal(1.0m, coachType.PriceMultiplier);
        Assert.False(coachType.IsCompartmented);
        Assert.NotNull(coachType.Coaches); // Should include coaches
    }

    [Fact]
    public void GetCoachTypeById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var coachType = _coachTypeService.GetCoachTypeById(999);

        // Assert
        Assert.Null(coachType);
    }

    [Fact]
    public void GetCoachTypeByName_WithExistingName_ShouldReturnCoachType()
    {
        // Act
        var coachType = _coachTypeService.GetCoachTypeByName("First Class");

        // Assert
        Assert.NotNull(coachType);
        Assert.Equal("First Class", coachType.TypeName);
        Assert.Equal(2.5m, coachType.PriceMultiplier);
        Assert.True(coachType.IsCompartmented);
    }

    [Fact]
    public void GetCoachTypeByName_WithCaseInsensitiveName_ShouldReturnCoachType()
    {
        // Act
        var coachType1 = _coachTypeService.GetCoachTypeByName("first class");
        var coachType2 = _coachTypeService.GetCoachTypeByName("FIRST CLASS");

        // Assert
        Assert.NotNull(coachType1);
        Assert.NotNull(coachType2);
        Assert.Equal("First Class", coachType1.TypeName);
        Assert.Equal("First Class", coachType2.TypeName);
        Assert.Equal(coachType1.CoachTypeId, coachType2.CoachTypeId);
    }

    [Fact]
    public void GetCoachTypeByName_WithNonExistentName_ShouldReturnNull()
    {
        // Act
        var coachType = _coachTypeService.GetCoachTypeByName("Luxury Suite");

        // Assert
        Assert.Null(coachType);
    }

    [Fact]
    public void GetCompartmentedCoachTypes_ShouldReturnOnlyCompartmentedTypes()
    {
        // Act
        var compartmentedTypes = _coachTypeService.GetCompartmentedCoachTypes().ToList();

        // Assert
        Assert.NotNull(compartmentedTypes);
        Assert.Equal(2, compartmentedTypes.Count); // First Class and Sleeper Class
        Assert.All(compartmentedTypes, ct => Assert.True(ct.IsCompartmented));
        
        // Verify ordering
        Assert.Equal("First Class", compartmentedTypes[0].TypeName);
        Assert.Equal("Sleeper Class", compartmentedTypes[1].TypeName);
    }

    [Fact]
    public void GetNonCompartmentedCoachTypes_ShouldReturnOnlyNonCompartmentedTypes()
    {
        // Act
        var nonCompartmentedTypes = _coachTypeService.GetNonCompartmentedCoachTypes().ToList();

        // Assert
        Assert.NotNull(nonCompartmentedTypes);
        Assert.Equal(2, nonCompartmentedTypes.Count); // Economy and Business Class
        Assert.All(nonCompartmentedTypes, ct => Assert.False(ct.IsCompartmented));
        
        // Verify ordering
        Assert.Equal("Business Class", nonCompartmentedTypes[0].TypeName);
        Assert.Equal("Economy Class", nonCompartmentedTypes[1].TypeName);
    }

    [Fact]
    public void GetCompartmentedCoachTypes_WithNoCompartmentedTypes_ShouldReturnEmptyList()
    {
        // Arrange - Remove compartmented types
        var compartmentedTypes = _context.CoachTypes.Where(ct => ct.IsCompartmented).ToList();
        _context.CoachTypes.RemoveRange(compartmentedTypes);
        _context.SaveChanges();

        // Act
        var result = _coachTypeService.GetCompartmentedCoachTypes().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateCoachType_WithValidData_ShouldReturnCreatedCoachType()
    {
        // Arrange
        var newCoachType = new CoachType
        {
            TypeName = "VIP Class",
            PriceMultiplier = 5.0m,
            IsCompartmented = true,
            DefaultCompartmentCapacity = 2,
            Description = "Premium VIP service"
        };

        // Act
        var result = _coachTypeService.CreateCoachType(newCoachType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("VIP Class", result.TypeName);
        Assert.Equal(5.0m, result.PriceMultiplier);
        Assert.True(result.CoachTypeId > 0);
        
        // Verify it was saved to database
        var savedType = _context.CoachTypes.FirstOrDefault(ct => ct.TypeName == "VIP Class");
        Assert.NotNull(savedType);
        Assert.Equal(5.0m, savedType.PriceMultiplier);
    }

    [Fact]
    public void UpdateCoachType_WithExistingType_ShouldReturnUpdatedCoachType()
    {
        // Arrange
        var existingType = _context.CoachTypes.First();
        existingType.TypeName = "Updated Economy";
        existingType.PriceMultiplier = 1.2m;
        existingType.Description = "Updated description";

        // Act
        var result = _coachTypeService.UpdateCoachType(existingType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Economy", result.TypeName);
        Assert.Equal(1.2m, result.PriceMultiplier);
        
        // Verify the update in database
        var updatedType = _context.CoachTypes.Find(existingType.CoachTypeId);
        Assert.NotNull(updatedType);
        Assert.Equal("Updated Economy", updatedType.TypeName);
        Assert.Equal(1.2m, updatedType.PriceMultiplier);
    }

    [Fact]
    public void UpdateCoachType_WithInvalidData_ShouldReturnNull()
    {
        // Arrange - Create coach type with invalid properties
        var invalidType = new CoachType
        {
            CoachTypeId = 999,
            TypeName = null!, // Invalid - required field
            PriceMultiplier = -1.0m
        };

        // Act
        var result = _coachTypeService.UpdateCoachType(invalidType);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteCoachType_WithExistingIdAndNotInUse_ShouldReturnTrue()
    {
        // Arrange
        var initialCount = _context.CoachTypes.Count();
        var typeToDelete = _context.CoachTypes.First(ct => ct.TypeName == "Business Class");
        var idToDelete = typeToDelete.CoachTypeId;

        // Act
        var result = _coachTypeService.DeleteCoachType(idToDelete);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedType = _context.CoachTypes.Find(idToDelete);
        Assert.Null(deletedType);
        
        // Verify count decreased
        var newCount = _context.CoachTypes.Count();
        Assert.Equal(initialCount - 1, newCount);
    }

    [Fact]
    public void DeleteCoachType_WithExistingIdButInUse_ShouldReturnFalse()
    {
        // Arrange - Create a coach that uses a coach type
        var coachType = _context.CoachTypes.First();
        var coach = new Coach
        {
            CoachId = 100,
            TrainId = 1,
            CoachTypeId = coachType.CoachTypeId,
            CoachNumber = 1,
            CoachName = "Test Coach",
            PositionInTrain = 1
        };
        _context.Coaches.Add(coach);
        _context.SaveChanges();

        var initialCount = _context.CoachTypes.Count();

        // Act
        var result = _coachTypeService.DeleteCoachType(coachType.CoachTypeId);

        // Assert
        Assert.False(result);
        
        // Verify coach type still exists
        var stillExists = _context.CoachTypes.Find(coachType.CoachTypeId);
        Assert.NotNull(stillExists);
        
        // Verify count unchanged
        var newCount = _context.CoachTypes.Count();
        Assert.Equal(initialCount, newCount);
    }

    [Fact]
    public void DeleteCoachType_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _coachTypeService.DeleteCoachType(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCoachTypeNameUnique_WithNewUniqueName_ShouldReturnTrue()
    {
        // Act
        var isUnique = _coachTypeService.IsCoachTypeNameUnique("Premium Class");

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsCoachTypeNameUnique_WithExistingName_ShouldReturnFalse()
    {
        // Act
        var isUnique = _coachTypeService.IsCoachTypeNameUnique("Economy Class");

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public void IsCoachTypeNameUnique_WithCaseInsensitiveExistingName_ShouldReturnFalse()
    {
        // Act
        var isUniqueUpper = _coachTypeService.IsCoachTypeNameUnique("ECONOMY CLASS");
        var isUniqueLower = _coachTypeService.IsCoachTypeNameUnique("economy class");

        // Assert
        Assert.False(isUniqueUpper);
        Assert.False(isUniqueLower);
    }

    [Fact]
    public void IsCoachTypeNameUnique_WithExistingNameButExcludingItself_ShouldReturnTrue()
    {
        // Arrange
        var existingType = _context.CoachTypes.First(ct => ct.TypeName == "Economy Class");

        // Act
        var isUnique = _coachTypeService.IsCoachTypeNameUnique("Economy Class", existingType.CoachTypeId);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsCoachTypeInUse_WithUsedCoachType_ShouldReturnTrue()
    {
        // Arrange - Create a coach that uses a coach type
        var coachType = _context.CoachTypes.First();
        var coach = new Coach
        {
            CoachId = 101,
            TrainId = 1,
            CoachTypeId = coachType.CoachTypeId,
            CoachNumber = 2,
            CoachName = "Test Coach 2",
            PositionInTrain = 2
        };
        _context.Coaches.Add(coach);
        _context.SaveChanges();

        // Act
        var isInUse = _coachTypeService.IsCoachTypeInUse(coachType.CoachTypeId);

        // Assert
        Assert.True(isInUse);
    }

    [Fact]
    public void IsCoachTypeInUse_WithUnusedCoachType_ShouldReturnFalse()
    {
        // Arrange - Find a coach type that's not used
        var unusedType = _context.CoachTypes.First(ct => ct.TypeName == "Business Class");

        // Act
        var isInUse = _coachTypeService.IsCoachTypeInUse(unusedType.CoachTypeId);

        // Assert
        Assert.False(isInUse);
    }

    [Fact]
    public void IsCoachTypeInUse_WithNonExistentCoachType_ShouldReturnFalse()
    {
        // Act
        var isInUse = _coachTypeService.IsCoachTypeInUse(999);

        // Assert
        Assert.False(isInUse);
    }

    [Fact]
    public void CoachTypeService_WorkflowTest_CreateUpdateDelete()
    {
        // This test validates the complete CRUD workflow with business logic
        
        // 1. Create
        var newType = new CoachType
        {
            TypeName = "Luxury Suite",
            PriceMultiplier = 10.0m,
            IsCompartmented = true,
            DefaultCompartmentCapacity = 1,
            Description = "Ultimate luxury experience"
        };
        
        var createdType = _coachTypeService.CreateCoachType(newType);
        Assert.NotNull(createdType);
        Assert.True(createdType.CoachTypeId > 0);
        
        // 2. Read and verify uniqueness
        var retrievedType = _coachTypeService.GetCoachTypeById(createdType.CoachTypeId);
        Assert.NotNull(retrievedType);
        Assert.Equal("Luxury Suite", retrievedType.TypeName);
        
        var nameExists = _coachTypeService.IsCoachTypeNameUnique("Luxury Suite");
        Assert.False(nameExists); // Should not be unique now
        
        // 3. Filter by compartment type
        var compartmentedTypes = _coachTypeService.GetCompartmentedCoachTypes().ToList();
        Assert.Contains(compartmentedTypes, ct => ct.TypeName == "Luxury Suite");
        
        // 4. Update
        retrievedType.PriceMultiplier = 15.0m;
        var updatedType = _coachTypeService.UpdateCoachType(retrievedType);
        Assert.NotNull(updatedType);
        Assert.Equal(15.0m, updatedType.PriceMultiplier);
        
        // 5. Verify not in use
        var inUse = _coachTypeService.IsCoachTypeInUse(createdType.CoachTypeId);
        Assert.False(inUse);
        
        // 6. Delete
        var deleteResult = _coachTypeService.DeleteCoachType(createdType.CoachTypeId);
        Assert.True(deleteResult);
        
        // Verify deletion
        var deletedType = _coachTypeService.GetCoachTypeById(createdType.CoachTypeId);
        Assert.Null(deletedType);
    }

    private void SeedTestData()
    {
        _context.CoachTypes.AddRange(
            new CoachType 
            { 
                CoachTypeId = 1, 
                TypeName = "Economy Class", 
                PriceMultiplier = 1.0m,
                IsCompartmented = false,
                DefaultCompartmentCapacity = null,
                Description = "Standard class seating"
            },
            new CoachType 
            { 
                CoachTypeId = 2, 
                TypeName = "Business Class", 
                PriceMultiplier = 1.8m,
                IsCompartmented = false,
                DefaultCompartmentCapacity = null,
                Description = "Enhanced comfort and service"
            },
            new CoachType 
            { 
                CoachTypeId = 3, 
                TypeName = "First Class", 
                PriceMultiplier = 2.5m,
                IsCompartmented = true,
                DefaultCompartmentCapacity = 4,
                Description = "Premium compartmented seating"
            },
            new CoachType 
            { 
                CoachTypeId = 4, 
                TypeName = "Sleeper Class", 
                PriceMultiplier = 2.0m,
                IsCompartmented = true,
                DefaultCompartmentCapacity = 6,
                Description = "Overnight sleeping compartments"
            }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}