# Voting System API

This repository contains the source code for a robust and feature-rich **Voting System API** built with ASP.NET Core.  
The system is designed to manage polls, questions, voting, and results, with a comprehensive user management and role-based permission system.

---

##  Key Features

- **Authentication & Authorization**  
  Secure user authentication using JWT with refresh tokens.  
  Fine-grained, permission-based authorization for precise endpoint control.

- **User & Role Management**  
  Full CRUD operations for users and roles.  
  Default `Admin` and `Member` roles are seeded with predefined permissions.

- **Poll & Question Management**  
  Create, update, and delete polls with start and end dates.  
  Manage questions with multiple-choice answers per poll.

- **Voting Mechanism**  
  Authenticated members can vote on active polls.  
  Users are prevented from voting more than once per poll.

- **Result Analytics**  
  Retrieve detailed voting results:
  - Raw votes
  - Votes aggregated per day
  - Votes per question and answer

- **Background Jobs**  
  Uses **Hangfire** for background tasks (e.g. poll notification emails).  
  Secured Hangfire dashboard for job monitoring.

- **Email Notifications**  
  Integrated with **MailKit** to send HTML emails for:
  - Registration confirmation
  - Password reset
  - New poll notifications

- **Clean Architecture**  
  Service-oriented architecture with clear separation of concerns.

- **Robust API Infrastructure**  
  RESTful APIs, FluentValidation, Mapster, EF Core, and Swagger/OpenAPI.

---

## 🛠 Technology Stack

- .NET 9  
- ASP.NET Core Web API  
- Entity Framework Core 9 (SQL Server)  
- ASP.NET Core Identity  
- JWT Authentication  
- Hangfire  
- Mapster  
- FluentValidation  
- Swagger (OpenAPI)  
- MailKit  
## 🧪 Unit Testing

The project includes a dedicated **Unit Test** project to ensure the reliability of business logic and services.

### Testing Stack
- xUnit
- Moq
- FluentAssertions

### Covered Areas
- Role management logic
- User services
- Permission and role assignment scenarios

### Run Tests
```bash
dotnet test




## 📁 Project Structure

```text
VotingSystem/
│
├── Controllers/        # API endpoints
├── Services/           # Business logic
├── Persistence/        # DbContext, configurations, migrations
├── Entities/           # Domain models (Poll, Question, Vote, User, etc.)
├── Dtos/               # Request / Response DTOs + Validators
├── PermissionsAuth/    # Permission-based authorization
├── Errors/             # Custom errors & Result pattern
├── Templates/          # HTML email templates
````


## 📸 API Documentation Preview

<img
  src="https://github.com/user-attachments/assets/1abad245-2ab5-46e6-bce8-a6ca51cc5e37"
  alt="Swagger API Documentation"
  width="100%"
/>
