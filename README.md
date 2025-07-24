# 🚂 Train Booking System
[![.NET CI](https://github.com/thaideptrai218/TrainBookingPRN/actions/workflows/dotnet-ci.yml/badge.svg?branch=main)](https://github.com/thaideptrai218/TrainBookingPRN/actions/workflows/dotnet-ci.yml)

<div align="center">
  <h3>A Desktop Train Booking System built with .NET 8 WPF</h3>
  <p>Professional train ticket booking and management application following MVVM architecture</p>
</div>


HELLo toi muon dong gop
---

## 📋 Table of Contents

- [📖 Overview](#-overview)
- [🚀 Quick Start](#-quick-start)
- [✨ Features](#-features)
- [🏗️ Project Structure](#️-project-structure)
- [🗂️ Project Index](#️-project-index)
- [🛣️ Roadmap](#️-roadmap)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [🙏 Acknowledgements](#-acknowledgements)

---

## 📖 Overview

The **Train Booking System** is a comprehensive desktop application designed for managing train reservations, built using modern .NET 8 WPF technology. The system supports both customer booking operations and administrative management functions with a clean, professional interface.

### 🎯 Key Highlights
- **Platform**: .NET 8 WPF (.NET 8.0-windows)
- **Language**: C# with nullable reference types and implicit usings
- **Architecture**: Strict MVVM (Model-View-ViewModel) pattern
- **Database**: Entity Framework Core 9.0.7 with SQL Server
- **Testing**: xUnit with in-memory database testing
- **Design**: Modern card-based UI with Bootstrap-inspired styling

---

## 🚀 Quick Start

### Prerequisites
- **.NET 8 SDK** or later
- **SQL Server** (LocalDB or full instance)
- **Visual Studio 2022** (recommended) or VS Code

### 🔧 Installation

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd TrainBookingPRN
   ```

2. **Navigate to the application directory**
   ```bash
   cd TrainBookingApp/TrainBookingApp
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

### 🗃️ Database Setup
The application uses a SQL Server database with the connection string configured in `Context.cs`:
```
Server=localhost;Database=PRN212_TrainBookingSystem;Trusted_Connection=True;TrustServerCertificate=True
```

### 🧪 Running Tests
```bash
dotnet test
```

---

## ✨ Features

### 👤 Customer Features
- **User Authentication**: Secure registration and login system
- **Trip Search**: Advanced search with filters for routes, dates, and preferences  
- **Seat Selection**: Interactive visual seat layout with real-time availability
- **Booking Management**: Complete booking workflow with passenger details
- **Booking Confirmation**: Detailed confirmation with booking codes and receipts
- **Booking History**: View and manage previous bookings

### 👨‍💼 Manager Features
- **Station Management**: Add, edit, and manage railway stations
- **Train Management**: Configure trains, types, and coach arrangements
- **Route Management**: Design and manage railway routes
- **Trip Scheduling**: Schedule trips with departure/arrival times
- **Pricing Rules**: Configure complex pricing with multipliers
- **Coach & Seat Types**: Manage different classes and seat configurations
- **Passenger Types**: Set up different passenger categories with discounts

### 🛡️ System Features
- **Role-based Access**: Separate interfaces for customers and managers
- **Temporary Seat Holds**: Seats held during booking process
- **Refund System**: Comprehensive refund processing with policies
- **Data Integrity**: Robust database constraints and validation
- **Modern UI**: Professional card-based design with hover effects

---

## 🏗️ Project Structure

```
TrainBookingApp/
├── 📁 TrainBookingApp/                 # Main Application
│   ├── 📁 Models/                      # EF Core Entities (Database-First)
│   │   ├── 📄 Context.cs              # Main DbContext with configurations
│   │   ├── 📄 User.cs                 # User authentication & profiles
│   │   ├── 📄 Booking.cs              # Booking management
│   │   ├── 📄 Ticket.cs               # Individual ticket records
│   │   ├── 📄 Station.cs              # Railway stations
│   │   ├── 📄 Route.cs                # Train routes
│   │   ├── 📄 Trip.cs                 # Scheduled trips
│   │   ├── 📄 Train.cs                # Train configurations  
│   │   ├── 📄 Coach.cs                # Train coaches
│   │   ├── 📄 Seat.cs                 # Individual seats
│   │   └── 📄 *Type.cs                # Various type configurations
│   ├── 📁 Services/                    # Business Logic Services
│   │   ├── 📄 I*Service.cs            # Service interfaces
│   │   ├── 📄 AuthenticationService.cs # User authentication
│   │   ├── 📄 BookingService.cs       # Booking operations
│   │   └── 📄 *Service.cs             # Entity-specific services
│   ├── 📁 ViewModels/                  # MVVM ViewModels
│   │   ├── 📄 BaseViewModel.cs        # Base class with INotifyPropertyChanged
│   │   ├── 📄 RelayCommand.cs         # ICommand implementation
│   │   ├── 📁 Manager/                # Admin management ViewModels
│   │   └── 📄 *ViewModel.cs           # Feature-specific ViewModels
│   ├── 📁 Views/                       # WPF XAML Views
│   │   ├── 📁 Manager/                # Admin management Views
│   │   └── 📄 *Window.xaml            # Application windows
│   ├── 📁 Converters/                  # Value converters for data binding
│   └── 📄 App.xaml.cs                 # DI container configuration
└── 📁 TrainManagementWPF.Tests/       # Unit Tests
    ├── 📄 *ServiceTests.cs            # Service layer tests
    └── 📄 TrainManagementWPF.Tests.csproj # Test project configuration
```

### 🎯 Architecture Principles

#### MVVM Implementation
- **Models**: EF Core entities generated from database-first approach
- **Views**: XAML files with minimal code-behind (only DataContext initialization)
- **ViewModels**: All business logic and data binding logic
- **Services**: Data access and business operations

#### Key Architectural Rules
1. **No business logic in code-behind** - Pure separation of concerns
2. **All user actions through ICommand** - Using `RelayCommand` pattern
3. **Data binding only** - No direct UI manipulation from code
4. **Dependency injection** - All services injected via constructor
5. **DbContext scoped lifetime** - Thread-safe EF Core operations

---

## 🗂️ Project Index

### 🔧 Core Components
| Component | Description | Location |
|-----------|-------------|----------|
| **Authentication** | User login/registration system | `Services/AuthenticationService.cs` |
| **Booking Engine** | Core booking logic and seat management | `Services/BookingService.cs` |
| **Database Context** | EF Core configuration and entity mappings | `Models/Context.cs` |
| **UI Foundation** | Base classes for MVVM implementation | `ViewModels/BaseViewModel.cs` |

### 💾 Database Entities
| Entity | Purpose | Key Relationships |
|--------|---------|-------------------|
| **User** | Customer and manager accounts | → Bookings, Passengers |
| **Station** | Railway stations | → Routes, Trips |
| **Route** | Train routes between stations | → Trips, Pricing |
| **Trip** | Scheduled train journeys | → Tickets, Seats |
| **Booking** | Customer reservations | → Tickets, Users |
| **Ticket** | Individual journey segments | → Passengers, Seats |

### 🎨 User Interface
| Window | Role | Purpose |
|--------|------|---------|
| **LoginWindow** | All Users | Authentication entry point |
| **CustomerWindow** | Customers | Booking and trip search |
| **ManagerWindow** | Administrators | System management |
| **SeatSelectionWindow** | Customers | Interactive seat selection |
| **BookingConfirmationWindow** | Customers | Booking finalization |

---

## 🛣️ Roadmap

### Phase 1: Core Enhancements ✅
- [x] Implement basic booking system
- [x] Create admin management interface  
- [x] Setup database relationships
- [x] Add comprehensive testing suite

### Phase 2: Advanced Features 🚧
- [ ] **Real-time Updates**: Live seat availability updates
- [ ] **Payment Integration**: Multiple payment gateway support
- [ ] **Email Notifications**: Booking confirmations and reminders
- [ ] **Mobile Responsive**: Web interface for mobile access
- [ ] **Reporting Dashboard**: Analytics and business intelligence

### Phase 3: Enterprise Features 🔮
- [ ] **Multi-language Support**: Internationalization
- [ ] **API Development**: REST API for third-party integrations
- [ ] **Advanced Security**: Two-factor authentication
- [ ] **Performance Optimization**: Caching and query optimization
- [ ] **Cloud Deployment**: Azure/AWS deployment options

---

## 🤝 Contributing

We welcome contributions to improve the Train Booking System! Here's how you can help:

### 🛠️ Development Setup
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Follow the established coding conventions
4. Write tests for new functionality
5. Ensure all tests pass: `dotnet test`
6. Commit changes: `git commit -m 'Add amazing feature'`
7. Push to branch: `git push origin feature/amazing-feature`
8. Open a Pull Request

### 📝 Coding Standards
- **C# Conventions**: Follow Microsoft C# coding standards
- **MVVM Compliance**: Maintain strict MVVM separation
- **Database First**: Use EF Core scaffolding for model changes
- **Testing**: Maintain test coverage for business logic
- **Documentation**: Comment complex business logic

### 🐛 Bug Reports
Please use the GitHub Issues page to report bugs with:
- Detailed description of the issue
- Steps to reproduce
- Expected vs actual behavior
- Screenshots (if applicable)

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgements

### 🛠️ Technologies Used
- **[.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** - Application framework
- **[WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)** - Desktop UI framework  
- **[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)** - Object-relational mapping
- **[SQL Server](https://www.microsoft.com/en-us/sql-server)** - Database management
- **[xUnit](https://xunit.net/)** - Testing framework

### 🎨 Design Inspiration
- **Bootstrap** - Color scheme and component styling
- **Material Design** - Card-based layout principles
- **Modern Windows Apps** - Native Windows 11 design language

### 📚 Learning Resources
- Microsoft .NET Documentation
- WPF MVVM Pattern Guides
- Entity Framework Core Best Practices
- Clean Architecture Principles

---

<div align="center">
  <p>Made with ❤️ for efficient train travel management</p>
  <p>© 2024 Train Booking System. All rights reserved.</p>
</div>
