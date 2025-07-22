using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for TrainTypeService - testing CRUD operations and search functionality
/// This service includes basic business logic for name existence checking and search
/// </summary>
public class TrainTypeServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly TrainTypeService _trainTypeService;

    public TrainTypeServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestContext(options);
        _trainTypeService = new TrainTypeService(_context);

        // Seed initial test data
        SeedTestData();
    }

    [Fact]
    public void GetAllTrainTypes_WithExistingTypes_ShouldReturnOrderedList()
    {
        // Act
        var trainTypes = _trainTypeService.GetAllTrainTypes().ToList();

        // Assert
        Assert.NotNull(trainTypes);
        Assert.Equal(3, trainTypes.Count);
        
        // Verify ordering by TypeName
        Assert.Equal("Express", trainTypes[0].TypeName);
        Assert.Equal("Local", trainTypes[1].TypeName);
        Assert.Equal("Super Express", trainTypes[2].TypeName);
    }

    [Fact]
    public void GetAllTrainTypes_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Clear all data
        _context.TrainTypes.RemoveRange(_context.TrainTypes);
        _context.SaveChanges();

        // Act
        var trainTypes = _trainTypeService.GetAllTrainTypes().ToList();

        // Assert
        Assert.NotNull(trainTypes);
        Assert.Empty(trainTypes);
    }

    [Fact]
    public void GetTrainTypeById_WithExistingId_ShouldReturnTrainType()
    {
        // Act
        var trainType = _trainTypeService.GetTrainTypeById(1);

        // Assert
        Assert.NotNull(trainType);
        Assert.Equal(1, trainType.TrainTypeId);
        Assert.Equal("Express", trainType.TypeName);
        Assert.Equal(120.0m, trainType.AverageVelocity);
    }

    [Fact]
    public void GetTrainTypeById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var trainType = _trainTypeService.GetTrainTypeById(999);

        // Assert
        Assert.Null(trainType);
    }

    [Fact]
    public void CreateTrainType_WithValidData_ShouldReturnCreatedTrainType()
    {
        // Arrange
        var newTrainType = new TrainType
        {
            TypeName = "Bullet Train",
            AverageVelocity = 300.0m,
            Description = "High-speed rail transport"
        };

        // Act
        var result = _trainTypeService.CreateTrainType(newTrainType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bullet Train", result.TypeName);
        Assert.Equal(300.0m, result.AverageVelocity);
        Assert.True(result.TrainTypeId > 0); // Should have generated ID
        
        // Verify it was saved to database
        var savedType = _context.TrainTypes.FirstOrDefault(tt => tt.TypeName == "Bullet Train");
        Assert.NotNull(savedType);
        Assert.Equal(300.0m, savedType.AverageVelocity);
    }

    [Fact]
    public void CreateTrainType_WithNullData_ShouldThrowException()
    {
        // Arrange
        TrainType nullTrainType = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _trainTypeService.CreateTrainType(nullTrainType));
    }

    [Fact]
    public void UpdateTrainType_WithExistingType_ShouldReturnUpdatedTrainType()
    {
        // Arrange
        var existingType = _context.TrainTypes.First();
        existingType.TypeName = "Updated Express";
        existingType.AverageVelocity = 150.0m;
        existingType.Description = "Updated description";

        // Act
        var result = _trainTypeService.UpdateTrainType(existingType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Express", result.TypeName);
        Assert.Equal(150.0m, result.AverageVelocity);
        
        // Verify the update in database
        var updatedType = _context.TrainTypes.Find(existingType.TrainTypeId);
        Assert.NotNull(updatedType);
        Assert.Equal("Updated Express", updatedType.TypeName);
        Assert.Equal(150.0m, updatedType.AverageVelocity);
    }

    [Fact]
    public void UpdateTrainType_WithInvalidData_ShouldReturnNull()
    {
        // Arrange - Create a train type with invalid properties that might cause save to fail
        var invalidType = new TrainType
        {
            TrainTypeId = 999, // Non-existent ID
            TypeName = null!, // Invalid - required field
            AverageVelocity = -1 // Invalid velocity
        };

        // Act
        var result = _trainTypeService.UpdateTrainType(invalidType);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteTrainType_WithExistingIdAndNoTrains_ShouldReturnTrue()
    {
        // Arrange
        var initialCount = _context.TrainTypes.Count();
        var typeToDelete = _context.TrainTypes.First();
        var idToDelete = typeToDelete.TrainTypeId;

        // Act
        var result = _trainTypeService.DeleteTrainType(idToDelete);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedType = _context.TrainTypes.Find(idToDelete);
        Assert.Null(deletedType);
        
        // Verify count decreased
        var newCount = _context.TrainTypes.Count();
        Assert.Equal(initialCount - 1, newCount);
    }

    [Fact]
    public void DeleteTrainType_WithExistingIdButHasTrains_ShouldReturnFalse()
    {
        // Arrange - Create a train that uses a train type
        var trainType = _context.TrainTypes.First();
        var train = new Train
        {
            TrainId = 100,
            TrainName = "Test Train",
            TrainTypeId = trainType.TrainTypeId
        };
        _context.Trains.Add(train);
        _context.SaveChanges();

        var initialCount = _context.TrainTypes.Count();

        // Act
        var result = _trainTypeService.DeleteTrainType(trainType.TrainTypeId);

        // Assert
        Assert.False(result);
        
        // Verify train type still exists
        var stillExists = _context.TrainTypes.Find(trainType.TrainTypeId);
        Assert.NotNull(stillExists);
        
        // Verify count unchanged
        var newCount = _context.TrainTypes.Count();
        Assert.Equal(initialCount, newCount);
    }

    [Fact]
    public void DeleteTrainType_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _trainTypeService.DeleteTrainType(999);

        // Assert
        Assert.False(result);
        
        // Verify count unchanged
        var count = _context.TrainTypes.Count();
        Assert.Equal(3, count);
    }

    [Fact]
    public void IsTrainTypeNameExists_WithExistingName_ShouldReturnTrue()
    {
        // Act
        var exists = _trainTypeService.IsTrainTypeNameExists("Express");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void IsTrainTypeNameExists_WithNonExistentName_ShouldReturnFalse()
    {
        // Act
        var exists = _trainTypeService.IsTrainTypeNameExists("Hyperloop");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void IsTrainTypeNameExists_WithCaseSensitiveName_ShouldBeCaseSensitive()
    {
        // Act
        var existsUpperCase = _trainTypeService.IsTrainTypeNameExists("EXPRESS");
        var existsLowerCase = _trainTypeService.IsTrainTypeNameExists("express");

        // Assert - Should be case sensitive
        Assert.False(existsUpperCase);
        Assert.False(existsLowerCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsTrainTypeNameExists_WithEmptyOrWhitespaceName_ShouldReturnFalse(string typeName)
    {
        // Act
        var exists = _trainTypeService.IsTrainTypeNameExists(typeName);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void SearchTrainTypes_WithMatchingTypeName_ShouldReturnMatchingTypes()
    {
        // Act
        var results = _trainTypeService.SearchTrainTypes("Express").ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count); // "Express" and "Super Express"
        Assert.All(results, r => Assert.Contains("Express", r.TypeName));
        
        // Verify ordering
        Assert.Equal("Express", results[0].TypeName);
        Assert.Equal("Super Express", results[1].TypeName);
    }

    [Fact]
    public void SearchTrainTypes_WithMatchingDescription_ShouldReturnMatchingTypes()
    {
        // Act
        var results = _trainTypeService.SearchTrainTypes("high-speed").ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Super Express", results[0].TypeName);
    }

    [Fact]
    public void SearchTrainTypes_WithNoMatches_ShouldReturnEmptyList()
    {
        // Act
        var results = _trainTypeService.SearchTrainTypes("Spaceship").ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public void SearchTrainTypes_WithEmptySearchTerm_ShouldReturnAllTypes()
    {
        // Act
        var results = _trainTypeService.SearchTrainTypes("").ToList();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SearchTrainTypes_WithPartialMatch_ShouldReturnMatchingTypes()
    {
        // Act
        var results = _trainTypeService.SearchTrainTypes("cal").ToList(); // Should match "Local"

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Local", results[0].TypeName);
    }

    [Fact]
    public void TrainTypeService_WorkflowTest_CreateUpdateDelete()
    {
        // This test validates the complete CRUD workflow
        
        // 1. Create
        var newType = new TrainType
        {
            TypeName = "Maglev",
            AverageVelocity = 400.0m,
            Description = "Magnetic levitation train"
        };
        
        var createdType = _trainTypeService.CreateTrainType(newType);
        Assert.NotNull(createdType);
        Assert.True(createdType.TrainTypeId > 0);
        
        // 2. Read
        var retrievedType = _trainTypeService.GetTrainTypeById(createdType.TrainTypeId);
        Assert.NotNull(retrievedType);
        Assert.Equal("Maglev", retrievedType.TypeName);
        
        // 3. Update
        retrievedType.AverageVelocity = 450.0m;
        var updatedType = _trainTypeService.UpdateTrainType(retrievedType);
        Assert.NotNull(updatedType);
        Assert.Equal(450.0m, updatedType.AverageVelocity);
        
        // 4. Search
        var searchResults = _trainTypeService.SearchTrainTypes("Maglev").ToList();
        Assert.Single(searchResults);
        Assert.Equal("Maglev", searchResults[0].TypeName);
        
        // 5. Delete
        var deleteResult = _trainTypeService.DeleteTrainType(createdType.TrainTypeId);
        Assert.True(deleteResult);
        
        // Verify deletion
        var deletedType = _trainTypeService.GetTrainTypeById(createdType.TrainTypeId);
        Assert.Null(deletedType);
    }

    [Fact]
    public void GetAllTrainTypes_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Act
        var result1 = _trainTypeService.GetAllTrainTypes().ToList();
        var result2 = _trainTypeService.GetAllTrainTypes().ToList();

        // Assert
        Assert.Equal(result1.Count, result2.Count);
        for (int i = 0; i < result1.Count; i++)
        {
            Assert.Equal(result1[i].TypeName, result2[i].TypeName);
            Assert.Equal(result1[i].TrainTypeId, result2[i].TrainTypeId);
        }
    }

    private void SeedTestData()
    {
        _context.TrainTypes.AddRange(
            new TrainType 
            { 
                TrainTypeId = 1, 
                TypeName = "Express", 
                AverageVelocity = 120.0m,
                Description = "Fast passenger service"
            },
            new TrainType 
            { 
                TrainTypeId = 2, 
                TypeName = "Local", 
                AverageVelocity = 60.0m,
                Description = "Regular passenger service with frequent stops"
            },
            new TrainType 
            { 
                TrainTypeId = 3, 
                TypeName = "Super Express", 
                AverageVelocity = 200.0m,
                Description = "Premium high-speed service"
            }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}