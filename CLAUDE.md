# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
- **Application Type**: Desktop Train Booking System
- **Platform**: .NET 8 WPF (.NET 8.0-windows)  
- **Language**: C# with nullable reference types and implicit usings enabled
- **Architecture**: Strict Model-View-ViewModel (MVVM)
- **Working Directory**: `C:\Users\PC\Desktop\TrainBookingPRN\TrainBookingApp\TrainBookingApp\`

## Development Commands
```bash
# Build the project
dotnet build

# Run the application  
dotnet run

# Add a NuGet package
dotnet add package <PackageName>

# Test (when tests are added)
dotnet test
```

## Architecture Overview

### MVVM Implementation
This project enforces **strict MVVM compliance**:
- **Models**: Data entities generated from EF Core scaffolding (`Models/`)
- **Views**: XAML files with minimal code-behind (`Views/`)
- **ViewModels**: Business logic and data binding (`ViewModels/`)
- **Services**: Data access and business services (`Services/`)

### Key Architectural Rules
1. **No business logic in code-behind** - Code-behind files only initialize DataContext
2. **All user actions through ICommand** - Use `RelayCommand` for button clicks
3. **Data binding only** - No direct UI manipulation
4. **Dependency injection** - All services injected via constructor
5. **DbContext scoped lifetime** - Avoids threading issues with EF Core

### Database Architecture
- **Entity Framework Core 9.0.7** with SQL Server
- **Database-first approach** - Models scaffolded from existing database
- **Complex relationships** - Booking system with stations, routes, trips, coaches, seats
- **Connection string** in `Context.cs` (hardcoded for development)

## Project Structure

```
TrainBookingApp/
├── Models/                 # EF Core entities (scaffolded)
│   ├── Context.cs         # Main DbContext
│   ├── User.cs, Booking.cs, Ticket.cs
│   ├── Station.cs, Route.cs, Trip.cs
│   └── Train.cs, Coach.cs, Seat.cs
├── Services/              # Business logic services
│   ├── I*Service.cs       # Service interfaces
│   ├── AuthenticationService.cs
│   ├── BookingService.cs
│   └── *Service.cs        # Entity services
├── ViewModels/            # MVVM ViewModels
│   ├── BaseViewModel.cs   # Base class with INotifyPropertyChanged
│   ├── RelayCommand.cs    # ICommand implementation
│   ├── Manager/           # Admin management ViewModels
│   └── *ViewModel.cs      # Feature ViewModels
├── Views/                 # WPF XAML Views
│   ├── Manager/           # Admin management Views
│   └── *Window.xaml       # Application windows
├── Converters/            # Value converters for data binding
└── App.xaml.cs           # DI container configuration
```

## Dependency Injection Configuration

The application uses .NET's built-in DI container configured in `App.xaml.cs`:

- **DbContext**: Scoped lifetime (critical for thread safety)
- **Services**: Transient lifetime for business logic
- **ViewModels**: Transient lifetime 
- **Views**: Transient lifetime

## Common Patterns

### ViewModel Pattern
```csharp
public class ExampleViewModel : BaseViewModel
{
    private readonly IExampleService _service;
    
    public ExampleViewModel(IExampleService service)
    {
        _service = service;
        LoadDataCommand = new RelayCommand(LoadData);
    }
    
    public ICommand LoadDataCommand { get; }
    
    private void LoadData(object? parameter)
    {
        // Business logic here
    }
}
```

### Service Pattern
```csharp

public class ExampleService : IExampleService
{
    private readonly Context _context;
    
    public ExampleService(Context context)
    {
        _context = context;
    }
}
```

## Critical Threading Considerations

**DbContext Threading Issue**: The application previously had threading issues with EF Core. Fixed by:
1. Using `ServiceLifetime.Scoped` for DbContext registration
2. Avoiding `Task.Run()` with database operations
3. Using synchronous operations in UI thread when appropriate

## UI Design Standards

The application uses a **modern card-based design** with:
- **Three-section layout**: Header → Main Content → Status Bar
- **Bootstrap-inspired colors**: Primary (#007bff), Success (#28a745), etc.
- **Consistent spacing**: 10-15px margins, 5px padding
- **Professional styling**: Rounded corners, shadows, hover effects
- **Click-to-edit workflow**: Grid selection populates edit forms automatically

## User Roles and Features

### Customer Features
- User registration/login
- Trip search and booking
- Seat selection with visual layout
- Passenger details management
- Booking confirmation and details

### Manager Features  
- Station management
- Train and coach type management
- Route and trip management
- Pricing rule configuration
- Seat type management

## Database Entities Overview

### Core Booking Flow
- **User** → **Booking** → **Ticket** → **Passenger**
- **Trip** (Train + Route + Schedule) → **Coach** → **Seat**
- **Station** ↔ **Route** ↔ **Trip**

### Business Rules
- Seats are temporarily held during booking process
- Pricing calculated from base rules + multipliers (coach type, seat type, passenger type)
- Tickets support journey segments (start/end stations within route)
- Cancellation policies with time-based fee structures

## Developer Background Notes

Developer has Java expertise transitioning to C#/WPF:
- **C# Properties** ≈ Java getters/setters
- **C# LINQ** ≈ Java Stream API  
- **C# ICommand** ≈ Java ActionListener pattern
- **WPF Data Binding** ≈ Java Swing property binding patterns