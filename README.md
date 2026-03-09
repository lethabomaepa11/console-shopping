# Console Shopping Application

A comprehensive C# console-based e-commerce application built with .NET 8.0, featuring a layered architecture with domain-driven design principles.

## 📋 Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [System Design Doc](#system-design-doc)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

## 🚀 Features

### Core Functionality
- **User Management**: Complete user registration, authentication, and role-based access control
- **Product Catalog**: Browse, search, and filter products with full inventory management
- **Shopping Cart**: Add/remove items, update quantities, and manage cart contents
- **Order Processing**: Complete checkout flow with payment integration and order tracking
- **Reviews & Ratings**: Customer feedback system for products
- **AI Shopping Assistant**: Customer Q&A assistant with streamed responses
- **Hybrid Recommendations**: "Recommended For You" based on behavior, ratings, and demand signals
- **Digital Twin Simulation**: Admin-only synthetic demand simulation for planning
- **Admin Dashboard**: Comprehensive administrative interface for store management

### Technical Features
- **Layered Architecture**: Clean separation of concerns with Domain, Services, and App layers
- **Domain-Driven Design**: Rich domain models with business logic encapsulation
- **Repository Pattern**: Abstracted data access with in-memory storage
- **Strategy Pattern**: Flexible payment processing system
- **Factory Pattern**: User creation and management
- **Observer Pattern**: Event-driven notifications and updates
- **State Pattern**: Enforced order lifecycle transitions
- **JSON Persistence**: Data persistence using JSON files

## 🏗️ Architecture

The application follows a layered architecture pattern:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│                    (App/ folder)                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   Program   │  │   Shopping  │  │   Input/Output  │  │
│  │     .cs     │  │Application  │  │     Helpers     │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                        │
│                 (Services/ folder)                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   Auth      │  │   Cart      │  │   Order         │  │
│  │   Service   │  │   Service   │  │   Service       │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   Catalog   │  │   Payment   │  │   Persistence   │  │
│  │   Service   │  │   Strategy  │  │   Service       │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│                  (Domain/ folder)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │   Entities  │  │   Value     │  │   Exceptions    │  │
│  │             │  │   Objects   │  │                 │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet) or later
- Any C# compatible IDE (Visual Studio, Visual Studio Code with C# extension, JetBrains Rider)

## 🛠️ Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/lethabomaepa11/console-shopping.git
   cd console-shopping
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

## 🎮 Usage

### Running the Application

1. **From the command line:**
   ```bash
   dotnet run --project console-shopping.csproj
   ```

   Optional AI assistant model override:
   ```bash
   set SHOPPING_ASSISTANT_MODEL=llama-3.1-8b-instant
   ```

2. **From Visual Studio:**
   - Open the solution file (`projects.slnx`)
   - Set `console-shopping` as the startup project
   - Press F5 or click "Start Debugging"

### Application Flow

1. **Welcome Screen**: The application starts with a welcome message and prompts for login or registration
2. **Authentication**: Users can log in with existing credentials or register as new users
3. **Role-Based Menus**: 
   - **Customers**: Access to product browsing, cart management, and order history
   - **Administrators**: Access to product management, user management, and sales reports
4. **Navigation**: Use numbered options to navigate through menus
   - Menu supports Arrow keys + Enter, or typed numbers
5. **Exit**: Type '0' or select the exit option to close the application

### AI Assistant Notes

- The assistant calls: `https://nwu-vaal-gkss.netlify.app/api/ai`
- Request body shape: `message`, `model`
- You can end assistant chat manually by typing `/exit`

## 📘 System Design Doc

Project documentation index:

- [docs/README.md](docs/README.md)

### Default Admin Account

For testing purposes, the application includes a default administrator account:
- **Username**: `admin`
- **Password**: `admin123`

## 📁 Project Structure

```
console-shopping/
├── App/                    # Presentation layer
│   ├── Input.cs           # Input validation and parsing
│   └── ShoppingApplication.cs  # Main application logic
├── Domain/                # Domain layer
│   ├── User.cs           # Base user entity
│   ├── Customer.cs       # Customer-specific functionality
│   ├── Administrator.cs  # Administrator-specific functionality
│   ├── Product.cs        # Product entity
│   ├── Cart.cs           # Shopping cart
│   ├── Order.cs          # Order management
│   ├── Payment.cs        # Payment processing
│   ├── Review.cs         # Product reviews
│   ├── UserRole.cs       # User role enumeration
│   ├── OrderStatus.cs    # Order status enumeration
│   ├── PaymentStatus.cs  # Payment status enumeration
│   ├── DomainException.cs # Custom domain exceptions
│   ├── InMemoryStore.cs  # In-memory data storage
│   └── UserFactory.cs    # User creation factory
├── Services/             # Service layer
│   ├── AuthService.cs    # Authentication and authorization
│   ├── CartService.cs    # Shopping cart operations
│   ├── CatalogService.cs # Product catalog management
│   ├── OrderService.cs   # Order processing
│   ├── ReviewService.cs  # Review management
│   ├── ReportService.cs  # Sales and inventory reports
│   ├── IPaymentStrategy.cs # Payment strategy interface
│   ├── SeedData.cs       # Initial data seeding
│   └── JsonStorePersistence.cs # JSON data persistence
├── Data/                 # Data storage
│   └── store.json        # Persistent data file
├── bin/                  # Compiled binaries
├── obj/                  # Build artifacts
├── console-shopping.csproj # Project configuration
├── projects.slnx         # Solution file
├── README.md            # This file
└── .gitignore           # Git ignore rules
```


### Development Guidelines

- Follow C# naming conventions
- Maintain the layered architecture
- Write unit tests for new features
- Update documentation for significant changes
- Ensure backward compatibility


## 🙏 Acknowledgments

- Built with .NET 10.0
- Uses JSON.NET for data serialization
- Inspired by real-world e-commerce applications

---

**Made with ❤️ by Lethabo**
