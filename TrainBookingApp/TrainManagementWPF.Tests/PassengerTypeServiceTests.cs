using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for PassengerTypeService - testing basic CRUD operations with in-memory database
/// This service has minimal business logic and simple database operations
/// </summary>
public class PassengerTypeServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly PassengerTypeService _passengerTypeService;

    public PassengerTypeServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestContext(options);
        _passengerTypeService = new PassengerTypeService(_context);

        // Seed initial test data
        SeedTestData();
    }

    [Fact]
    public void GetAllPassengerTypes_WithExistingTypes_ShouldReturnOrderedList()
    {
        // Act
        var passengerTypes = _passengerTypeService.GetAllPassengerTypes();

        // Assert
        Assert.NotNull(passengerTypes);
        Assert.Equal(3, passengerTypes.Count);
        
        // Verify ordering by TypeName
        Assert.Equal("Adult", passengerTypes[0].TypeName);
        Assert.Equal("Child", passengerTypes[1].TypeName);
        Assert.Equal("Senior", passengerTypes[2].TypeName);
    }

    [Fact]
    public void GetAllPassengerTypes_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Clear all data
        _context.PassengerTypes.RemoveRange(_context.PassengerTypes);
        _context.SaveChanges();

        // Act
        var passengerTypes = _passengerTypeService.GetAllPassengerTypes();

        // Assert
        Assert.NotNull(passengerTypes);
        Assert.Empty(passengerTypes);
    }

    [Fact]
    public void GetPassengerTypeById_WithExistingId_ShouldReturnPassengerType()
    {
        // Act
        var passengerType = _passengerTypeService.GetPassengerTypeById(1);

        // Assert
        Assert.NotNull(passengerType);
        Assert.Equal(1, passengerType.PassengerTypeId);
        Assert.Equal("Adult", passengerType.TypeName);
        Assert.Equal(0.0m, passengerType.DiscountPercentage);
    }

    [Fact]
    public void GetPassengerTypeById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var passengerType = _passengerTypeService.GetPassengerTypeById(999);

        // Assert
        Assert.Null(passengerType);
    }

    [Fact]
    public void CreatePassengerType_WithValidData_ShouldReturnTrueAndSaveToDatabase()
    {
        // Arrange
        var newPassengerType = new PassengerType
        {
            TypeName = "Student",
            DiscountPercentage = 15.0m
        };

        // Act
        var result = _passengerTypeService.CreatePassengerType(newPassengerType);

        // Assert
        Assert.True(result);
        
        // Verify it was saved
        var savedType = _context.PassengerTypes.FirstOrDefault(pt => pt.TypeName == "Student");
        Assert.NotNull(savedType);
        Assert.Equal(15.0m, savedType.DiscountPercentage);
        
        // Verify total count increased
        var totalCount = _context.PassengerTypes.Count();
        Assert.Equal(4, totalCount);
    }

    [Fact]
    public void CreatePassengerType_WithNullData_ShouldThrowException()
    {
        // Arrange
        PassengerType nullPassengerType = null!;

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _passengerTypeService.CreatePassengerType(nullPassengerType));
        
        Assert.Contains("Error creating passenger type", exception.Message);
    }

    [Fact]
    public void UpdatePassengerType_WithExistingType_ShouldReturnTrueAndUpdateDatabase()
    {
        // Arrange
        var existingType = _context.PassengerTypes.First();
        existingType.TypeName = "Updated Adult";
        existingType.DiscountPercentage = 5.0m;

        // Act
        var result = _passengerTypeService.UpdatePassengerType(existingType);

        // Assert
        Assert.True(result);
        
        // Verify the update
        var updatedType = _context.PassengerTypes.Find(existingType.PassengerTypeId);
        Assert.NotNull(updatedType);
        Assert.Equal("Updated Adult", updatedType.TypeName);
        Assert.Equal(5.0m, updatedType.DiscountPercentage);
    }

    [Fact]
    public void UpdatePassengerType_WithNonExistentType_ShouldThrowException()
    {
        // EF Core in-memory database throws exception when trying to update non-existent entity
        // Arrange
        var nonExistentType = new PassengerType
        {
            PassengerTypeId = 999,
            TypeName = "Non-existent",
            DiscountPercentage = 0.0m
        };

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => 
            _passengerTypeService.UpdatePassengerType(nonExistentType));
        
        Assert.Contains("Error updating passenger type", exception.Message);
    }

    [Fact]
    public void DeletePassengerType_WithExistingId_ShouldReturnTrueAndRemoveFromDatabase()
    {
        // Arrange
        var initialCount = _context.PassengerTypes.Count();
        var typeToDelete = _context.PassengerTypes.First();
        var idToDelete = typeToDelete.PassengerTypeId;

        // Act
        var result = _passengerTypeService.DeletePassengerType(idToDelete);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedType = _context.PassengerTypes.Find(idToDelete);
        Assert.Null(deletedType);
        
        // Verify count decreased
        var newCount = _context.PassengerTypes.Count();
        Assert.Equal(initialCount - 1, newCount);
    }

    [Fact]
    public void DeletePassengerType_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _passengerTypeService.DeletePassengerType(999);

        // Assert
        Assert.False(result);
        
        // Verify count unchanged
        var count = _context.PassengerTypes.Count();
        Assert.Equal(3, count);
    }

    [Fact]
    public void IsPassengerTypeNameUnique_WithNewUniqueName_ShouldReturnTrue()
    {
        // Act
        var isUnique = _passengerTypeService.IsPassengerTypeNameUnique("Infant");

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsPassengerTypeNameUnique_WithExistingName_ShouldReturnFalse()
    {
        // Act
        var isUnique = _passengerTypeService.IsPassengerTypeNameUnique("Adult");

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public void IsPassengerTypeNameUnique_WithExistingNameButExcludingItself_ShouldReturnTrue()
    {
        // Arrange - Get existing type
        var existingType = _context.PassengerTypes.First(pt => pt.TypeName == "Adult");

        // Act - Check uniqueness excluding the existing type's ID
        var isUnique = _passengerTypeService.IsPassengerTypeNameUnique("Adult", existingType.PassengerTypeId);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsPassengerTypeNameUnique_WithExistingNameAndDifferentExcludeId_ShouldReturnFalse()
    {
        // Act - Check uniqueness excluding a different ID
        var isUnique = _passengerTypeService.IsPassengerTypeNameUnique("Adult", 999);

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public void IsPassengerTypeNameUnique_WithCaseSensitiveName_ShouldBeCaseSensitive()
    {
        // Act
        var isUniqueUpperCase = _passengerTypeService.IsPassengerTypeNameUnique("ADULT");
        var isUniqueLowerCase = _passengerTypeService.IsPassengerTypeNameUnique("adult");

        // Assert - Should be case sensitive (these should be considered unique)
        Assert.True(isUniqueUpperCase);
        Assert.True(isUniqueLowerCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsPassengerTypeNameUnique_WithEmptyOrWhitespaceName_ShouldReturnTrue(string typeName)
    {
        // Act
        var isUnique = _passengerTypeService.IsPassengerTypeNameUnique(typeName);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void CreatePassengerType_WithDuplicateName_ShouldSucceedInInMemoryDb()
    {
        // In-memory database doesn't enforce unique constraints like SQL Server would
        // Arrange
        var duplicateType = new PassengerType
        {
            TypeName = "Adult", // This already exists
            DiscountPercentage = 10.0m
        };

        // Act
        var result = _passengerTypeService.CreatePassengerType(duplicateType);

        // Assert - In-memory DB allows duplicates, but real DB would fail
        Assert.True(result);
        
        // Verify both entries exist
        var adultTypes = _context.PassengerTypes.Where(pt => pt.TypeName == "Adult").ToList();
        Assert.Equal(2, adultTypes.Count);
    }

    [Fact]
    public void PassengerTypeService_WorkflowTest_CreateUpdateDelete()
    {
        // This test validates the complete CRUD workflow
        
        // 1. Create
        var newType = new PassengerType
        {
            TypeName = "VIP",
            DiscountPercentage = 0.0m
        };
        
        var createResult = _passengerTypeService.CreatePassengerType(newType);
        Assert.True(createResult);
        
        // 2. Read
        var createdType = _context.PassengerTypes.FirstOrDefault(pt => pt.TypeName == "VIP");
        Assert.NotNull(createdType);
        var createdId = createdType.PassengerTypeId;
        
        // 3. Update
        createdType.DiscountPercentage = 20.0m;
        var updateResult = _passengerTypeService.UpdatePassengerType(createdType);
        Assert.True(updateResult);
        
        // Verify update
        var updatedType = _passengerTypeService.GetPassengerTypeById(createdId);
        Assert.NotNull(updatedType);
        Assert.Equal(20.0m, updatedType.DiscountPercentage);
        
        // 4. Delete
        var deleteResult = _passengerTypeService.DeletePassengerType(createdId);
        Assert.True(deleteResult);
        
        // Verify deletion
        var deletedType = _passengerTypeService.GetPassengerTypeById(createdId);
        Assert.Null(deletedType);
    }

    [Fact]
    public void GetAllPassengerTypes_MultipleCalls_ShouldReturnConsistentResults()
    {
        // Act
        var result1 = _passengerTypeService.GetAllPassengerTypes();
        var result2 = _passengerTypeService.GetAllPassengerTypes();

        // Assert
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(result1.First().TypeName, result2.First().TypeName);
        Assert.Equal(result1.Last().TypeName, result2.Last().TypeName);
    }

    private void SeedTestData()
    {
        _context.PassengerTypes.AddRange(
            new PassengerType { PassengerTypeId = 1, TypeName = "Adult", DiscountPercentage = 0.0m },
            new PassengerType { PassengerTypeId = 2, TypeName = "Child", DiscountPercentage = 50.0m },
            new PassengerType { PassengerTypeId = 3, TypeName = "Senior", DiscountPercentage = 25.0m }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}