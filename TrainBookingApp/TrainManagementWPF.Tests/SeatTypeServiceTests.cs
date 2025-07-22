using Microsoft.EntityFrameworkCore;
using TrainBookingApp.Models;
using TrainBookingApp.Services;

namespace TrainManagementWPF.Tests;

/// <summary>
/// Unit tests for SeatTypeService - testing CRUD operations with berth level filtering and usage validation
/// This service includes business logic for berth levels and seat usage validation
/// </summary>
public class SeatTypeServiceTests : IDisposable
{
    private readonly TestContext _context;
    private readonly SeatTypeService _seatTypeService;

    public SeatTypeServiceTests()
    {
        // Setup in-memory database with unique name for each test instance
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestContext(options);
        _seatTypeService = new SeatTypeService(_context);

        // Seed initial test data
        SeedTestData();
    }

    [Fact]
    public void GetAllSeatTypes_WithExistingTypes_ShouldReturnOrderedListWithSeats()
    {
        // Act
        var seatTypes = _seatTypeService.GetAllSeatTypes().ToList();

        // Assert
        Assert.NotNull(seatTypes);
        Assert.Equal(5, seatTypes.Count);
        
        // Verify ordering by TypeName
        Assert.Equal("Lower Berth", seatTypes[0].TypeName);
        Assert.Equal("Middle Berth", seatTypes[1].TypeName);
        Assert.Equal("Standard Seat", seatTypes[2].TypeName);
        Assert.Equal("Upper Berth", seatTypes[3].TypeName);
        Assert.Equal("Window Seat", seatTypes[4].TypeName);
        
        // Verify includes work (Seats should be loaded)
        Assert.NotNull(seatTypes[0].Seats);
    }

    [Fact]
    public void GetAllSeatTypes_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Clear all data
        _context.SeatTypes.RemoveRange(_context.SeatTypes);
        _context.SaveChanges();

        // Act
        var seatTypes = _seatTypeService.GetAllSeatTypes().ToList();

        // Assert
        Assert.NotNull(seatTypes);
        Assert.Empty(seatTypes);
    }

    [Fact]
    public void GetSeatTypeById_WithExistingId_ShouldReturnSeatTypeWithSeats()
    {
        // Act
        var seatType = _seatTypeService.GetSeatTypeById(1);

        // Assert
        Assert.NotNull(seatType);
        Assert.Equal(1, seatType.SeatTypeId);
        Assert.Equal("Standard Seat", seatType.TypeName);
        Assert.Equal(1.0m, seatType.PriceMultiplier);
        Assert.Null(seatType.BerthLevel);
        Assert.NotNull(seatType.Seats); // Should include seats
    }

    [Fact]
    public void GetSeatTypeById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var seatType = _seatTypeService.GetSeatTypeById(999);

        // Assert
        Assert.Null(seatType);
    }

    [Fact]
    public void GetSeatTypeByName_WithExistingName_ShouldReturnSeatType()
    {
        // Act
        var seatType = _seatTypeService.GetSeatTypeByName("Upper Berth");

        // Assert
        Assert.NotNull(seatType);
        Assert.Equal("Upper Berth", seatType.TypeName);
        Assert.Equal(1.5m, seatType.PriceMultiplier);
        Assert.Equal(3, seatType.BerthLevel);
    }

    [Fact]
    public void GetSeatTypeByName_WithCaseInsensitiveName_ShouldReturnSeatType()
    {
        // Act
        var seatType1 = _seatTypeService.GetSeatTypeByName("upper berth");
        var seatType2 = _seatTypeService.GetSeatTypeByName("UPPER BERTH");

        // Assert
        Assert.NotNull(seatType1);
        Assert.NotNull(seatType2);
        Assert.Equal("Upper Berth", seatType1.TypeName);
        Assert.Equal("Upper Berth", seatType2.TypeName);
        Assert.Equal(seatType1.SeatTypeId, seatType2.SeatTypeId);
    }

    [Fact]
    public void GetSeatTypeByName_WithNonExistentName_ShouldReturnNull()
    {
        // Act
        var seatType = _seatTypeService.GetSeatTypeByName("Luxury Cabin");

        // Assert
        Assert.Null(seatType);
    }

    [Fact]
    public void GetSeatTypesByBerthLevel_WithValidLevel_ShouldReturnMatchingTypes()
    {
        // Act
        var level1Types = _seatTypeService.GetSeatTypesByBerthLevel(1).ToList();

        // Assert
        Assert.NotNull(level1Types);
        Assert.Single(level1Types);
        Assert.Equal("Lower Berth", level1Types[0].TypeName);
        Assert.Equal(1, level1Types[0].BerthLevel);
    }

    [Fact]
    public void GetSeatTypesByBerthLevel_WithNullLevel_ShouldReturnNonBerthTypes()
    {
        // Act
        var nonBerthTypes = _seatTypeService.GetSeatTypesByBerthLevel(null).ToList();

        // Assert
        Assert.NotNull(nonBerthTypes);
        Assert.Equal(2, nonBerthTypes.Count); // Standard Seat and Window Seat
        Assert.All(nonBerthTypes, st => Assert.Null(st.BerthLevel));
        
        // Verify ordering
        Assert.Equal("Standard Seat", nonBerthTypes[0].TypeName);
        Assert.Equal("Window Seat", nonBerthTypes[1].TypeName);
    }

    [Fact]
    public void GetSeatTypesByBerthLevel_WithNonExistentLevel_ShouldReturnEmptyList()
    {
        // Act
        var result = _seatTypeService.GetSeatTypesByBerthLevel(5).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetBerthSeatTypes_ShouldReturnOnlyBerthTypesOrderedByLevel()
    {
        // Act
        var berthTypes = _seatTypeService.GetBerthSeatTypes().ToList();

        // Assert
        Assert.NotNull(berthTypes);
        Assert.Equal(3, berthTypes.Count); // Lower, Middle, Upper berths
        Assert.All(berthTypes, st => Assert.NotNull(st.BerthLevel));
        
        // Verify ordering by berth level, then by name
        Assert.Equal("Lower Berth", berthTypes[0].TypeName);
        Assert.Equal(1, berthTypes[0].BerthLevel);
        Assert.Equal("Middle Berth", berthTypes[1].TypeName);
        Assert.Equal(2, berthTypes[1].BerthLevel);
        Assert.Equal("Upper Berth", berthTypes[2].TypeName);
        Assert.Equal(3, berthTypes[2].BerthLevel);
    }

    [Fact]
    public void GetNonBerthSeatTypes_ShouldReturnOnlyNonBerthTypes()
    {
        // Act
        var nonBerthTypes = _seatTypeService.GetNonBerthSeatTypes().ToList();

        // Assert
        Assert.NotNull(nonBerthTypes);
        Assert.Equal(2, nonBerthTypes.Count); // Standard Seat and Window Seat
        Assert.All(nonBerthTypes, st => Assert.Null(st.BerthLevel));
        
        // Verify ordering by TypeName
        Assert.Equal("Standard Seat", nonBerthTypes[0].TypeName);
        Assert.Equal("Window Seat", nonBerthTypes[1].TypeName);
    }

    [Fact]
    public void GetBerthSeatTypes_WithNoBerthTypes_ShouldReturnEmptyList()
    {
        // Arrange - Remove all berth types
        var berthTypes = _context.SeatTypes.Where(st => st.BerthLevel.HasValue).ToList();
        _context.SeatTypes.RemoveRange(berthTypes);
        _context.SaveChanges();

        // Act
        var result = _seatTypeService.GetBerthSeatTypes().ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateSeatType_WithValidData_ShouldReturnCreatedSeatType()
    {
        // Arrange
        var newSeatType = new SeatType
        {
            TypeName = "Premium Seat",
            PriceMultiplier = 2.0m,
            BerthLevel = null,
            Description = "Premium comfort seating"
        };

        // Act
        var result = _seatTypeService.CreateSeatType(newSeatType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Premium Seat", result.TypeName);
        Assert.Equal(2.0m, result.PriceMultiplier);
        Assert.True(result.SeatTypeId > 0);
        
        // Verify it was saved to database
        var savedType = _context.SeatTypes.FirstOrDefault(st => st.TypeName == "Premium Seat");
        Assert.NotNull(savedType);
        Assert.Equal(2.0m, savedType.PriceMultiplier);
    }

    [Fact]
    public void CreateSeatType_WithBerthLevel_ShouldCreateBerthType()
    {
        // Arrange
        var newBerthType = new SeatType
        {
            TypeName = "Side Upper",
            PriceMultiplier = 1.3m,
            BerthLevel = 4,
            Description = "Side upper berth"
        };

        // Act
        var result = _seatTypeService.CreateSeatType(newBerthType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Side Upper", result.TypeName);
        Assert.Equal(4, result.BerthLevel);
        
        // Verify it appears in berth types
        var berthTypes = _seatTypeService.GetBerthSeatTypes().ToList();
        Assert.Contains(berthTypes, st => st.TypeName == "Side Upper");
    }

    [Fact]
    public void UpdateSeatType_WithExistingType_ShouldReturnUpdatedSeatType()
    {
        // Arrange
        var existingType = _context.SeatTypes.First();
        existingType.TypeName = "Updated Standard";
        existingType.PriceMultiplier = 1.1m;
        existingType.Description = "Updated description";

        // Act
        var result = _seatTypeService.UpdateSeatType(existingType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Standard", result.TypeName);
        Assert.Equal(1.1m, result.PriceMultiplier);
        
        // Verify the update in database
        var updatedType = _context.SeatTypes.Find(existingType.SeatTypeId);
        Assert.NotNull(updatedType);
        Assert.Equal("Updated Standard", updatedType.TypeName);
        Assert.Equal(1.1m, updatedType.PriceMultiplier);
    }

    [Fact]
    public void UpdateSeatType_WithInvalidData_ShouldReturnNull()
    {
        // Arrange - Create seat type with invalid properties
        var invalidType = new SeatType
        {
            SeatTypeId = 999,
            TypeName = null!, // Invalid - required field
            PriceMultiplier = -1.0m
        };

        // Act
        var result = _seatTypeService.UpdateSeatType(invalidType);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteSeatType_WithExistingIdAndNotInUse_ShouldReturnTrue()
    {
        // Arrange
        var initialCount = _context.SeatTypes.Count();
        var typeToDelete = _context.SeatTypes.First(st => st.TypeName == "Window Seat");
        var idToDelete = typeToDelete.SeatTypeId;

        // Act
        var result = _seatTypeService.DeleteSeatType(idToDelete);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedType = _context.SeatTypes.Find(idToDelete);
        Assert.Null(deletedType);
        
        // Verify count decreased
        var newCount = _context.SeatTypes.Count();
        Assert.Equal(initialCount - 1, newCount);
    }

    [Fact]
    public void DeleteSeatType_WithExistingIdButInUse_ShouldReturnFalse()
    {
        // Arrange - Create a seat that uses a seat type
        var seatType = _context.SeatTypes.First();
        var seat = new Seat
        {
            SeatId = 100,
            CoachId = 1,
            SeatTypeId = seatType.SeatTypeId,
            SeatNumber = 1,
            SeatName = "Test Seat"
        };
        _context.Seats.Add(seat);
        _context.SaveChanges();

        var initialCount = _context.SeatTypes.Count();

        // Act
        var result = _seatTypeService.DeleteSeatType(seatType.SeatTypeId);

        // Assert
        Assert.False(result);
        
        // Verify seat type still exists
        var stillExists = _context.SeatTypes.Find(seatType.SeatTypeId);
        Assert.NotNull(stillExists);
        
        // Verify count unchanged
        var newCount = _context.SeatTypes.Count();
        Assert.Equal(initialCount, newCount);
    }

    [Fact]
    public void DeleteSeatType_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _seatTypeService.DeleteSeatType(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSeatTypeNameUnique_WithNewUniqueName_ShouldReturnTrue()
    {
        // Act
        var isUnique = _seatTypeService.IsSeatTypeNameUnique("Luxury Suite");

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsSeatTypeNameUnique_WithExistingName_ShouldReturnFalse()
    {
        // Act
        var isUnique = _seatTypeService.IsSeatTypeNameUnique("Standard Seat");

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public void IsSeatTypeNameUnique_WithCaseInsensitiveExistingName_ShouldReturnFalse()
    {
        // Act
        var isUniqueUpper = _seatTypeService.IsSeatTypeNameUnique("STANDARD SEAT");
        var isUniqueLower = _seatTypeService.IsSeatTypeNameUnique("standard seat");

        // Assert
        Assert.False(isUniqueUpper);
        Assert.False(isUniqueLower);
    }

    [Fact]
    public void IsSeatTypeNameUnique_WithExistingNameButExcludingItself_ShouldReturnTrue()
    {
        // Arrange
        var existingType = _context.SeatTypes.First(st => st.TypeName == "Standard Seat");

        // Act
        var isUnique = _seatTypeService.IsSeatTypeNameUnique("Standard Seat", existingType.SeatTypeId);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public void IsSeatTypeInUse_WithUsedSeatType_ShouldReturnTrue()
    {
        // Arrange - Create a seat that uses a seat type
        var seatType = _context.SeatTypes.First();
        var seat = new Seat
        {
            SeatId = 101,
            CoachId = 1,
            SeatTypeId = seatType.SeatTypeId,
            SeatNumber = 2,
            SeatName = "Test Seat 2"
        };
        _context.Seats.Add(seat);
        _context.SaveChanges();

        // Act
        var isInUse = _seatTypeService.IsSeatTypeInUse(seatType.SeatTypeId);

        // Assert
        Assert.True(isInUse);
    }

    [Fact]
    public void IsSeatTypeInUse_WithUnusedSeatType_ShouldReturnFalse()
    {
        // Arrange - Find a seat type that's not used
        var unusedType = _context.SeatTypes.First(st => st.TypeName == "Window Seat");

        // Act
        var isInUse = _seatTypeService.IsSeatTypeInUse(unusedType.SeatTypeId);

        // Assert
        Assert.False(isInUse);
    }

    [Fact]
    public void SeatTypeService_BerthLevelWorkflow_ShouldWorkCorrectly()
    {
        // Test the complete berth level filtering workflow
        
        // 1. Get all berth types
        var allBerthTypes = _seatTypeService.GetBerthSeatTypes().ToList();
        Assert.Equal(3, allBerthTypes.Count);
        
        // 2. Get specific berth level
        var level2Types = _seatTypeService.GetSeatTypesByBerthLevel(2).ToList();
        Assert.Single(level2Types);
        Assert.Equal("Middle Berth", level2Types[0].TypeName);
        
        // 3. Get non-berth types
        var nonBerthTypes = _seatTypeService.GetNonBerthSeatTypes().ToList();
        Assert.Equal(2, nonBerthTypes.Count);
        
        // 4. Verify total count
        Assert.Equal(5, allBerthTypes.Count + nonBerthTypes.Count);
    }

    [Fact]
    public void SeatTypeService_WorkflowTest_CreateUpdateDelete()
    {
        // This test validates the complete CRUD workflow with berth logic
        
        // 1. Create
        var newType = new SeatType
        {
            TypeName = "VIP Berth",
            PriceMultiplier = 5.0m,
            BerthLevel = 1,
            Description = "VIP lower berth"
        };
        
        var createdType = _seatTypeService.CreateSeatType(newType);
        Assert.NotNull(createdType);
        Assert.True(createdType.SeatTypeId > 0);
        
        // 2. Read and verify uniqueness
        var retrievedType = _seatTypeService.GetSeatTypeById(createdType.SeatTypeId);
        Assert.NotNull(retrievedType);
        Assert.Equal("VIP Berth", retrievedType.TypeName);
        
        var nameExists = _seatTypeService.IsSeatTypeNameUnique("VIP Berth");
        Assert.False(nameExists); // Should not be unique now
        
        // 3. Filter by berth level
        var level1Types = _seatTypeService.GetSeatTypesByBerthLevel(1).ToList();
        Assert.Equal(2, level1Types.Count); // Original Lower Berth + VIP Berth
        Assert.Contains(level1Types, st => st.TypeName == "VIP Berth");
        
        // 4. Update
        retrievedType.PriceMultiplier = 6.0m;
        var updatedType = _seatTypeService.UpdateSeatType(retrievedType);
        Assert.NotNull(updatedType);
        Assert.Equal(6.0m, updatedType.PriceMultiplier);
        
        // 5. Verify not in use
        var inUse = _seatTypeService.IsSeatTypeInUse(createdType.SeatTypeId);
        Assert.False(inUse);
        
        // 6. Delete
        var deleteResult = _seatTypeService.DeleteSeatType(createdType.SeatTypeId);
        Assert.True(deleteResult);
        
        // Verify deletion
        var deletedType = _seatTypeService.GetSeatTypeById(createdType.SeatTypeId);
        Assert.Null(deletedType);
    }

    private void SeedTestData()
    {
        _context.SeatTypes.AddRange(
            new SeatType 
            { 
                SeatTypeId = 1, 
                TypeName = "Standard Seat", 
                PriceMultiplier = 1.0m,
                BerthLevel = null,
                Description = "Standard passenger seating"
            },
            new SeatType 
            { 
                SeatTypeId = 2, 
                TypeName = "Window Seat", 
                PriceMultiplier = 1.2m,
                BerthLevel = null,
                Description = "Window-side seating"
            },
            new SeatType 
            { 
                SeatTypeId = 3, 
                TypeName = "Lower Berth", 
                PriceMultiplier = 1.8m,
                BerthLevel = 1,
                Description = "Lower sleeping berth"
            },
            new SeatType 
            { 
                SeatTypeId = 4, 
                TypeName = "Middle Berth", 
                PriceMultiplier = 1.5m,
                BerthLevel = 2,
                Description = "Middle sleeping berth"
            },
            new SeatType 
            { 
                SeatTypeId = 5, 
                TypeName = "Upper Berth", 
                PriceMultiplier = 1.5m,
                BerthLevel = 3,
                Description = "Upper sleeping berth"
            }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}