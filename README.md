# 🚗 Ride Connect 
Ride Connect is a full-stack carpooling web application dbuilt with ASP.NET Core 9.0, following Clean Architecture principles. . It connects drivers and passengers for safe, efficient, and eco-friendly shared rides.


## 🎯 Project Goal

To reduce traffic congestion and transportation costs by building a smart carpooling platform that enables seamless ride booking and management, supported by real-time mapping and seat-based booking features.

---

## 🚀 Features

### Core Functionality
- **User Management**: Registration, authentication, profile management with role-based access
- **Trip Management**: Create, book, and manage carpooling trips
- **Delivery System**: Integrated delivery requests for packages and items
- **Rating & Feedback**: User rating system and feedback mechanism
- **Document Verification**: Driver license and national ID verification
- **Email Notifications**: Automated email system for user communications

### User Roles
- **Passenger**: Book trips, manage profile, provide feedback
- **Driver**: Create trips, manage vehicles, accept passengers
- **Admin**: User management, system oversight, verification approvals

### Advanced Features
- **Real-time Location Tracking**: GPS coordinates for trip sources and destinations
- **Gender Preferences**: Optional gender-based matching for trips
- **Delivery Integration**: Package delivery alongside passenger transport
- **Document Upload**: Cloudinary integration for secure file storage
- **JWT Authentication**: Secure token-based authentication system

## 🏗️ Architecture

This project follows **Clean Architecture** principles with a layered structure:

```
CarPooling-Backend/
├── CarPooling.API/           # Presentation Layer (Controllers, Middleware)
├── CarPooling.Application/   # Application Layer (Services, DTOs, Interfaces)
├── CarPooling.Domain/        # Domain Layer (Entities, Enums, Exceptions)
└── CarPooling.Infrastructure/# Infrastructure Layer (Data, External Services)
```

### Technology Stack
- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity + JWT
- **File Storage**: Cloudinary
- **Email Service**: SMTP (Gmail)
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture 

## 📋 Prerequisites

Before running this application, ensure you have:

- **.NET 9.0 SDK** or later
- **SQL Server** (LocalDB, Express, or full version)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd Car-Pooling-Backend
```

### 2. Database Setup
1. **Update Connection String**: Modify the connection string in `CarPooling.API/appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CarPoolingDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

2. **Run Migrations**: Open Package Manager Console and run:
```bash
Update-Database
```

### 3. External Services Configuration

#### Cloudinary Setup
Update Cloudinary settings in `appsettings.json`:
```json
"CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
}
```

#### Email Configuration
Configure SMTP settings in `appsettings.json`:
```json
"Email": {
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "DisplayName": "Car Pooling App"
}
```

### 4. Build and Run
```bash
# Navigate to API project
cd CarPooling.API/CarPooling.API

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```
---

## 🎓 Acknowledgements

This project was developed as part of our graduation requirement at [ITI (Information Technology Institute)](https://www.iti.gov.eg/). 
We are thankful for the mentorship, teamwork, and late-night debugging sessions that made it possible!

---
