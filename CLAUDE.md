# Train Booking WPF Application - Project Context

## Project Overview
- **Application Type**: Desktop Train Booking System
- **Platform**: .NET 8 WPF (.NET 8.0-windows)
- **Language**: C# with nullable reference types and implicit usings enabled
- **Architecture**: Strict Model-View-ViewModel (MVVM)
- **Project Location**: `C:\Users\PC\Desktop\TrainBookingPRN\TrainBookingApp\`

## Developer Profile
- **Skill Level**: Beginner in C# and WPF
- **Background**: Expert in Java
- **Learning Approach**: C# concepts explained through Java comparisons
- **Development Environment**: VS Code (NOT Visual Studio)

## Project Structure
```
TrainBookingApp/
├── Views/           (for all .xaml files)
├── ViewModels/      (for all ViewModel classes)
├── Models/          (for data entity classes - Route, Station, Train)
├── Data/            (for Data Access Objects/Repositories using EF Core)
├── Services/        (for business logic services injected into ViewModels)
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── TrainBookingApp.csproj
```

## Architecture Rules (CRITICAL)
1. **MVVM Strict Compliance**: No business logic in code-behind files
2. **Data Binding**: Always use data binding to connect Views and ViewModels
3. **Commands**: All actions (button clicks) implemented using ICommand in ViewModel
4. **Code-Behind**: Empty except for setting DataContext - NO event handlers
5. **Dependency Injection**: All services/repositories injected via constructor
6. **Data Access**: Entity Framework Core for all database interactions
7. **Async Operations**: Use async/await for all I/O-bound operations

## Technology Stack
- **.NET 8**: SDK-style project format
- **WPF**: Windows Presentation Foundation
- **Entity Framework Core**: Database ORM
- **MVVM Pattern**: Architectural pattern
- **Dependency Injection**: Built-in .NET DI container
- **xUnit**: Unit testing framework (when needed)

## Forbidden Technologies
- **Advanced XAML**: No Styles, Resources, ControlTemplates, complex styling
- **Direct UI Manipulation**: No code-behind event handlers
- **Manual Service Creation**: All services must be injected

## Development Workflow
1. Create Models (data entities)
2. Setup DbContext and repositories
3. Create ViewModels with ICommand properties
4. Create Views with data binding
5. Configure dependency injection
6. Implement async operations
7. Add error handling
8. Write unit tests (when requested)

## Key C# to Java Mappings
- **C# delegate** ↔ **Java Functional Interface**
- **C# LINQ** ↔ **Java Stream API**
- **C# Properties** ↔ **Java getters/setters**
- **C# ICommand** ↔ **Java ActionListener pattern**
- **C# ObservableCollection** ↔ **Java Observable List**
- **C# async/await** ↔ **Java CompletableFuture**

## Current Status
- ✅ Basic WPF project created with proper .NET 8 configuration
- ✅ Database scaffolded with Entity Framework Core
- ✅ MVVM folder structure created (Views/, ViewModels/, Models/, Services/, Data/)
- ✅ Password hashing service implemented (PBKDF2 with SHA-256)
- ✅ Authentication service with async operations
- ✅ LoginViewModel with ICommand and data binding
- ✅ LoginWindow with pure MVVM data binding
- ✅ Value converters for UI binding
- ⏳ **Next**: Setup dependency injection and test login system

## Commands to Remember
- **Build**: `dotnet build`
- **Run**: `dotnet run`
- **Test**: `dotnet test` (when tests are added)
- **Add Package**: `dotnet add package <PackageName>`

## Next Steps
- Create folder structure (Views/, ViewModels/, Models/, Data/, Services/)
- Add Entity Framework Core packages
- Implement first feature based on user selection