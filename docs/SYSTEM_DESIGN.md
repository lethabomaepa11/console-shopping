# System Design and Architecture

This document explains how the `console-shopping` application is structured and why the current design choices were made.

## 1. System Overview

`console-shopping` is a console-based e-commerce backend simulation. It supports:

- User registration and login
- Role-based access (`Customer`, `Administrator`)
- Product catalog and inventory management
- Shopping cart and checkout
- Wallet-based payment
- Order tracking and status updates
- Product reviews
- Sales and stock reporting
- AI shopping assistant (external API-backed)
- Hybrid product recommendations
- Digital twin demand simulation (non-destructive)
- JSON persistence for application state

Entry point:

- [Program.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Program.cs)
- [ShoppingApplication.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/ShoppingApplication.cs)

## 2. Architectural Style

The codebase uses a layered architecture:

- Presentation layer: `App/`
- Application/business services: `Services/`
- Domain model and contracts: `Domain/`
- Data access implementations: `Data/`

Why this was chosen:

- Keeps console UI logic separate from business rules.
- Allows business services to evolve without rewriting UI code.
- Enables storage implementation changes behind repository interfaces.

Key examples:

- Presentation orchestration: [ShoppingApplication.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/ShoppingApplication.cs)
- Business logic: [OrderService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/OrderService.cs)
- Domain entities: [Order.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Order.cs)
- Data access implementation: [OrderRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Data/OrderRepository.cs)

## 3. Runtime Composition

Dependencies are wired manually in one place (`ShoppingApplication`), which acts as a composition root.

What is instantiated there:

- Store and persistence
- Repositories
- Services
- Observer subscription for order notifications
- UI flows (`CustomerConsoleFlow`, `AdminConsoleFlow`)

Why this choice:

- Simple and explicit for a console app.
- No framework dependency on a DI container.
- Easy to trace object graph and startup behavior.

Reference:

- [ShoppingApplication.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/ShoppingApplication.cs)

## 4. Design Patterns Used

### 4.1 Repository Pattern

Contracts:

- [IRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Repositories/IRepository.cs)
- [IUserRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Repositories/IUserRepository.cs)
- [IProductRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Repositories/IProductRepository.cs)
- [IOrderRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Repositories/IOrderRepository.cs)

Implementations:

- [UserRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Data/UserRepository.cs)
- [ProductRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Data/ProductRepository.cs)
- [OrderRepository.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Data/OrderRepository.cs)

Why:

- Decouples services from storage details.
- Keeps service logic focused on business rules.
- Makes future data source changes easier.

### 4.2 Strategy Pattern (Payments)

Strategy contract and implementation:

- [IPaymentStrategy.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/IPaymentStrategy.cs)

Consumer:

- [OrderService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/OrderService.cs)

Why:

- `OrderService` delegates payment processing behavior.
- New payment methods can be added by implementing `IPaymentStrategy`.
- Checkout flow remains stable while payment logic varies.

### 4.3 Observer Pattern (Order updates)

Observer contract:

- [IOrderObserver.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Observers/IOrderObserver.cs)

Subject:

- [OrderService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/OrderService.cs)

Observer:

- [OrderNotificationLogger.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/OrderNotificationLogger.cs)

Why:

- Allows order status notifications without hard-coding output logic into all update paths.
- New listeners can be attached without changing core order workflow.

### 4.4 Factory Pattern (User creation)

Factory:

- [UserFactory.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/UserFactory.cs)

Usage:

- [AuthService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/AuthService.cs)

Why:

- Centralizes role-based object creation.
- Avoids repeated construction branching in authentication code.

### 4.5 Singleton Pattern (In-memory store)

Store:

- [InMemoryStore.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/InMemoryStore.cs)

Why:

- Provides a single shared state instance across repositories/services.
- Appropriate for this app size and in-memory-first model.

### 4.6 State Pattern (Order lifecycle)

State contracts and implementations:

- [IOrderState.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/States/IOrderState.cs)
- [OrderStates.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/States/OrderStates.cs)
- [OrderStateFactory.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/States/OrderStateFactory.cs)

State owner:

- [Order.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Order.cs)

Why:

- Encapsulates allowed transition rules per state.
- Prevents invalid lifecycle jumps (for example `Pending -> Shipped`).
- Keeps transition validation in the domain model, not scattered across UI/services.

## 5. Core Domain and Service Choices

### 5.1 Domain model choices

Entities represent the core business concepts:

- Users: [User.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/User.cs), [Customer.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Customer.cs), [Administrator.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Administrator.cs)
- Catalog: [Product.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Product.cs)
- Ordering: [Order.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Order.cs), [OrderItem.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/OrderItem.cs)
- Payment: [Payment.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Payment.cs)
- Cart: [Cart.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Cart.cs), [CartItem.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/CartItem.cs)
- Reviews: [Review.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/Models/Review.cs)

Domain rule violations throw:

- [DomainException.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Domain/DomainException.cs)

Why:

- Keeps domain concepts explicit.
- Uses typed enums (`UserRole`, `OrderStatus`, `PaymentStatus`) for safer state handling.

### 5.2 Service boundaries

Each service owns a focused set of business use cases:

- Auth and registration: [AuthService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/AuthService.cs)
- Product/catalog operations: [CatalogService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/CatalogService.cs)
- Cart operations: [CartService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/CartService.cs)
- Checkout and order lifecycle: [OrderService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/OrderService.cs)
- Reviews: [ReviewService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/ReviewService.cs)
- Reporting: [ReportService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/ReportService.cs)
- Assistant responses: [ShoppingAssistantService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/ShoppingAssistantService.cs)
- Recommendations: [RecommendationService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/RecommendationService.cs)
- Simulation workloads: [SimulationService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/SimulationService.cs)

Why:

- Reduces class-level complexity.
- Clarifies ownership of business rules.
- Improves testability by isolating behavior.

## 6. Persistence Design

Persistence abstraction:

- [JsonStorePersistence.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/JsonStorePersistence.cs)

Design:

- `IStorePersistence` abstracts `Load` and `Save`.
- JSON snapshots (`StoreSnapshot` + nested snapshot classes) map to/from domain objects.
- Data is stored in `Data/store.json`.

Why snapshots are used:

- Decouples serialization shape from domain constructors and invariants.
- Makes schema evolution easier than serializing domain objects directly.

## 7. Presentation and UI Flow

Key components:

- Input helpers: [Input.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/Input.cs)
- Reusable view renderer: [ConsoleView.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/ConsoleView.cs)
- Role-specific flows: [CustomerConsoleFlow.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/CustomerConsoleFlow.cs), [AdminConsoleFlow.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/AdminConsoleFlow.cs)
- Menu definitions: [ApplicationMenus.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/Menus/ApplicationMenus.cs), [MenuDefinition.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/Menus/MenuDefinition.cs), [MenuOption.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/Menus/MenuOption.cs)
- Access checks: [AccessGuard.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/AccessGuard.cs)

Why:

- Keeps interactive I/O concerns out of service/domain logic.
- Reuses a generic menu model to avoid duplicated UI code.
- Enforces role-based behavior consistently.

### 7.1 AI Assistant Interaction

Assistant integration details:

- Endpoint: `https://nwu-vaal-gkss.netlify.app/api/ai`
- Request body contains `message` and `model`
- Response is rendered in a streamed console style
- User manually terminates assistant mode with `/exit`

Primary integration points:

- [ShoppingAssistantService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/ShoppingAssistantService.cs)
- [CustomerConsoleFlow.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/CustomerConsoleFlow.cs)

### 7.2 Recommendation Flow

Recommendations use hybrid scoring:

- Category affinity (from purchases/reviews)
- Similar-customer purchasing overlap
- Product rating signal
- Sales-demand fallback signal

Primary integration points:

- [RecommendationService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/RecommendationService.cs)
- [CustomerConsoleFlow.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/CustomerConsoleFlow.cs)

### 7.3 Digital Twin Simulation

Simulation mode creates synthetic customers/orders to estimate:

- Simulated order volume
- Simulated revenue
- Average order value
- Top simulated products

The simulation is non-destructive and does not mutate persisted store data.

Primary integration points:

- [SimulationService.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/Services/SimulationService.cs)
- [AdminConsoleFlow.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/App/AdminConsoleFlow.cs)

## 8. Tradeoffs and Current Constraints

Current strengths:

- Clear separation of concerns.
- Extensible seams for payments and notifications.
- Predictable and simple startup/runtime composition.

Current constraints:

- `InMemoryStore` singleton is global mutable state.
- Manual dependency wiring can become verbose as app grows.
- Some services still depend directly on `InMemoryStore`, which is tighter coupling than pure repository-only services.
- Persistence is full-snapshot save; no transactions or concurrency control.

## 9. Extension Guidelines

Recommended approach for new features:

- Add/modify domain models first for new business concepts.
- Add service methods for use-case behavior.
- Expand repository contracts only when data access needs change.
- Keep UI flow classes thin; delegate business rules to services.
- Prefer interface-based extension points (`IStorePersistence`, `IPaymentStrategy`, observer interfaces).

Examples:

- New payment method: implement `IPaymentStrategy`, inject into `OrderService`.
- New notification channel: implement `IOrderObserver`, subscribe in `ShoppingApplication`.
- New data store backend: implement repository interfaces and/or `IStorePersistence`.

## 10. Testing Notes

Existing tests are in:

- [tests/ConsoleShoppingApp.Tests](/C:/Users/Lethabo/Desktop/projects/console-shopping/tests/ConsoleShoppingApp.Tests)

Covered examples:

- Access control guard behavior
- Menu composition assumptions
- Observer notification on order status change

Reference files:

- [AccessGuardTests.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/tests/ConsoleShoppingApp.Tests/AccessGuardTests.cs)
- [ApplicationMenusTests.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/tests/ConsoleShoppingApp.Tests/ApplicationMenusTests.cs)
- [OrderObserverTests.cs](/C:/Users/Lethabo/Desktop/projects/console-shopping/tests/ConsoleShoppingApp.Tests/OrderObserverTests.cs)
