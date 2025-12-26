# Voting System API

This repository contains the source code for a robust and feature-rich Voting System API built with ASP.NET Core. The system is designed to manage polls, questions, and voting, with a comprehensive user management and role-based permission system.

## Key Features

*   **Authentication & Authorization:** Secure user authentication using JWT with refresh tokens. A fine-grained, permission-based authorization system allows for precise control over API endpoints.
*   **User & Role Management:** Full CRUD operations for users and roles. Manage permissions assigned to each role. Default `Admin` and `Member` roles are seeded with pre-configured permissions.
*   **Poll & Question Management:** Create, update, and delete polls with start and end dates. Add and manage questions with multiple-choice answers for each poll.
*   **Voting Mechanism:** Authenticated members can submit their votes on active polls. The system prevents users from voting more than once on the same poll.
*   **Result Analytics:** Endpoints for retrieving detailed poll results, including raw vote data, votes aggregated per day, and votes tallied per question and answer.
*   **Background Jobs:** Utilizes Hangfire to run background tasks, such as sending email notifications for newly published polls. The Hangfire dashboard is secured and available for monitoring jobs.
*   **Email Notifications:** Integrated with MailKit to send HTML-formatted emails for user registration confirmation, password resets, and new poll alerts.
*   **Clean Architecture:** Follows a clean, service-oriented architecture with distinct layers for API, services, data persistence, and domain entities.
*   **Robust API Infrastructure:** Implements RESTful principles, FluentValidation for request validation, Mapster for object-to-object mapping, and Entity Framework Core for ORM. API documentation is provided through Swagger/OpenAPI.

## Technology Stack

*   **.NET 9**
*   **ASP.NET Core Web API**
*   **Entity Framework Core 9** with SQL Server
*   **ASP.NET Core Identity** for user management
*   **JWT (JSON Web Tokens)** for stateless authentication
*   **Hangfire** for background job processing
*   **Mapster** for object mapping
*   **FluentValidation** for model and DTO validation
*   **Swagger (OpenAPI)** for API documentation
*   **MailKit** for sending emails

## Project Structure

The solution is organized into logical components to promote separation of concerns and maintainability.

-   `SurveyBasket/`: The main ASP.NET Core project.
    -   `Controllers/`: Contains the API endpoints for handling HTTP requests.
    -   `Services/`: Houses the business logic and core functionalities.
    -   `Persistence/`: Includes the `ApplicationDbContext`, entity configurations, and database migrations.
    -   `Entities/`: Defines the core domain models (Poll, Question, User, etc.).
    -   `Dtos/`: Contains Data Transfer Objects used for API requests and responses, along with their validators.
    -   `PremisonsAuth/`: Implements the custom permission-based authorization logic.
    -   `Errors/`: Custom error types and exception handling.
    -   `Templates/`: HTML templates for email notifications.
-   `SurveyBasket.Tests/`: xUnit project for integration tests.

## Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

*   .NET 9 SDK
*   SQL Server (e.g., LocalDB, Express, or a full instance)

### Installation

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/yossefmahmoud1/voting-system.git
    cd voting-system
    ```

2.  **Configure Application Settings:**
    Open `SurveyBasket/appsettings.json` and update the connection strings:
    *   `DefaultConnection`: The connection string for the main application database.
    *   `HangfireConnection`: The connection string for the Hangfire jobs database.

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SurveySystem;Trusted_Connection=True;Encrypt=False",
      "HangfireConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SurveySystemJobs;Trusted_Connection=True;Encrypt=False"
    },
    ```

    For production environments, it is highly recommended to use User Secrets or another secure configuration provider for sensitive data like `MailSettings` and `HangFireSettings`.

3.  **Apply Database Migrations:**
    Navigate to the project folder and run the Entity Framework Core migration command to create the database schema.
    ```sh
    cd SurveyBasket
    dotnet ef database update
    ```
    This will also seed the database with a default Admin user and default roles/permissions.
    *   **Admin Username:** `admin1`
    *   **Admin Password:** `P@ssword1235Efsa`

4.  **Run the Application:**
    ```sh
    dotnet run
    ```
    The API will be available at `https://localhost:7133` and `http://localhost:5299`.

5.  **Access API Documentation:**
    Navigate to `/swagger` in your browser to view and interact with the API endpoints using the Swagger UI.
    *   Example: `https://localhost:7133/swagger`

## API Endpoints Overview

-   **`Auth` Controller:** Handles user registration, login, and token management (refresh, revoke).
-   **`Account` Controller:** Manages the current authenticated user's profile and password.
-   **`Polls` Controller:** Provides CRUD operations for polls and allows toggling their publish status.
-   **`Questions` Controller:** Manages questions and their associated answers within a specific poll.
-   **`Votes` Controller:** Allows authenticated members to retrieve available questions and submit their votes.
-   **`Results` Controller:** Exposes endpoints to retrieve aggregated poll results and analytics.
-   **`Users` Controller:** (Admin) Manage application users, including their roles and status (enable/disable, unlock).
-   **`Roles` Controller:** (Admin) CRUD operations for roles and their associated permissions.
