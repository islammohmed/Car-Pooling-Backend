# CarPooling Application - Comprehensive Overview

## 🚗 What is this project?

This is a **CarPooling Backend API** built with **C# .NET Core** that facilitates ride-sharing services. The application allows users to create, book, and manage carpooling trips with comprehensive user authentication and trip management features.

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

### **1. CarPooling.Domain** - Core Business Logic
- **Entities**: Core business objects
  - `User`: Extends IdentityUser with carpooling-specific properties
  - `Trip`: Main trip entity with source, destination, pricing, and scheduling
  - `TripParticipant`: Manages trip bookings and participants
  - `Car`: Vehicle information for drivers
  - `Chat`, `Payment`, `Feedback`: Additional features
  - `DocumentVerification`: User verification system

- **Enums**: Business rules and constants
  - `UserRole`, `TripStatus`, `JoinStatus`, `Gender`, `VerificationStatus`

- **Exceptions**: Domain-specific exceptions

### **2. CarPooling.Application** - Business Logic Layer
- **Services**: Business logic implementation
  - `AuthService`: Handles user authentication, registration, password management
  - `UserService`: User profile management
  - `BookTripService`: Trip booking logic

- **DTOs**: Data transfer objects for API communication
- **Interfaces**: Contracts for services and repositories
- **Commands & Validators**: CQRS pattern implementation for trip creation

### **3. CarPooling.Infrastructure** - Data & External Services
- **Data**: Entity Framework Core implementation
  - `AppDbContext`: Database context with Identity integration
  - **Repositories**: Data access implementations
  - **Migrations**: Database schema management

- **Services**: Infrastructure services
  - `JwtService`: JWT token generation and validation

- **Seeders**: Database initialization

### **4. CarPooling.API** - Presentation Layer
- **Controllers**: REST API endpoints
  - `AuthController`: Complete authentication system
  - `TripController`: Trip management
  - `UserController`: User profile management

## 🔑 Key Features

### **Authentication & Security**
- **JWT Authentication** with refresh tokens
- **ASP.NET Core Identity** integration
- **Email confirmation** system
- **Password reset** functionality
- **Role-based authorization** (Driver/Passenger)
- **Document verification** system

### **Trip Management**
- **Create trips** with detailed information
- **Book available seats** in trips
- **Search and filter** trips
- **Trip status management** (Scheduled, In Progress, Completed, Cancelled)
- **Gender preferences** for trips
- **Pricing per seat** system

### **User Features**
- **User profiles** with personal information
- **Vehicle management** for drivers
- **Rating system** (average rating tracking)
- **SSN and document verification**
- **Profile editing** capabilities

### **Additional Features**
- **Chat system** for trip communication
- **Payment integration** ready
- **Feedback system** for trip reviews
- **Delivery requests** (additional service)

## 🛠️ Technology Stack

- **Backend Framework**: ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT + ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Architecture Pattern**: Clean Architecture
- **Design Patterns**: Repository Pattern, CQRS (Command Query Responsibility Segregation)

## 📡 API Endpoints

### **Authentication Endpoints** (`/api/auth/`)
- `POST /register` - User registration
- `POST /login` - User login
- `POST /refresh-token` - Token refresh
- `POST /confirm-email` - Email confirmation
- `POST /resend-confirmation` - Resend confirmation code
- `POST /forgot-password` - Initiate password reset
- `POST /reset-password` - Reset password
- `POST /logout` - User logout
- `GET /me` - Get current user info
- `GET /validate-token` - Validate JWT token

### **Trip Management** (`/api/trip/`)
- Trip creation, booking, and management endpoints

### **User Management** (`/api/user/`)
- User profile management endpoints

## 🔧 Configuration

The application supports:
- **CORS configuration** for Angular frontend (`http://localhost:4200`)
- **JWT configuration** with issuer, audience, and secret key
- **Database connection** string configuration
- **Swagger documentation** in development mode

## 📝 Key Models

### **User Model**
```csharp
- FirstName, LastName
- UserRole (Driver/Passenger)
- SSN, Gender
- DrivingLicenseImage, NationalIdImage
- AvgRating, IsVerified
- Cars collection
- TripParticipations collection
```

### **Trip Model**
```csharp
- DriverId, PricePerSeat
- SourceLocation, Destination
- StartTime, EstimatedDuration
- AvailableSeats, Status
- GenderPreference
- Participants collection
```

## 🚀 Getting Started

1. **Database Setup**: Configure SQL Server connection string
2. **JWT Configuration**: Set up JWT settings in appsettings.json
3. **Run Migrations**: Entity Framework migrations will create the database
4. **Seed Data**: Application includes database seeding
5. **API Documentation**: Access Swagger UI at `/swagger` in development

## 🎯 Use Cases

This application supports:
- **Drivers** creating and managing trips
- **Passengers** finding and booking trips
- **Secure authentication** and user verification
- **Trip communication** through chat
- **Payment processing** (framework ready)
- **Rating and feedback** system
- **Document verification** for safety

The application is designed as a complete carpooling solution with enterprise-level features and clean, maintainable code architecture.