# Employee Leave Management System

## Project Overview

The **Employee Leave Management System (ELMS)** is a microservices-based web application designed to simplify and manage employee leave processes.

The system provides two main portals:

- **Employee Portal:** Allows employees to submit leave requests, view their own leave balances, and track request statuses.
- **Management Portal:** Allows HOD and HR users to review, approve, and reject employee leave requests while also managing their own leave information.

The system includes leave allowance validation, role-based access control, authentication, and notification processing.

## Architecture

The application follows a **Microservices Architecture** with three backend services.

| Service | Responsibility |
|---|---|
| EmployeeService | Employee information, authentication, and role management |
| LeaveService | Leave requests, allowance validation, and approval management |
| NotificationService | Background notification processing |

### Database Architecture

The application uses two separate Microsoft SQL Server databases:

- **EmployeeDb:** Stores employee information and authentication-related data.
- **LeaveDb:** Stores leave requests and related leave-management data.

Entity Framework Core Code First is used to manage database structures and migrations.

### Application Flow

1. The React frontend sends requests to the backend APIs.
2. EmployeeService handles authentication and employee information.
3. LeaveService handles leave requests and business rules.
4. EmployeeApiClient enables communication between LeaveService and EmployeeService.
5. NotificationService processes background notification tasks.
6. Each service accesses its respective database through Entity Framework Core.

Dependency Injection (DI) manages service dependencies and supports separation of concerns.

## Technology Stack

| Category | Technology |
|---|---|
| Backend | C# / ASP.NET Core |
| Frontend | React 19 / TypeScript |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Architecture | Microservices |
| Authentication | JWT |
| Dependency Management | Dependency Injection |
| Containerization | Docker / Docker Compose |
| API Testing | Swagger / Postman |
| Frontend Build Tool | Vite |
| Validation | Fluent business rules / application validation |

## Prerequisites

Make sure the following tools are installed:

- .NET 10 SDK
- Docker Desktop
- Node.js and npm
- Git
- Visual Studio or Visual Studio Code
- Postman (optional, for API testing)

You should also have Docker Desktop running before starting the backend services.

## How to Run

### 1. Clone the Repository

```bash
git clone <repository-url>
cd EmployeeLeaveManagement
```

### 2. Configure Environment Variables

Check the `.env.example` file and prepare your `.env` file.

Configure the required database connection settings, service API keys, and bootstrap HOD account settings.

Do not commit your `.env` file or expose credentials.

### 3. Start the Backend Services

From the project root, run:

```powershell
.\start-backend.ps1
```

Alternatively, start the Docker Compose services manually:

```bash
docker compose up -d --build
```

### 4. Check Backend Services

Check the running containers:

```bash
docker compose ps
```

Check service logs:

```bash
docker compose logs -f
```

### 5. Access API Documentation

Open the Swagger interfaces:

- EmployeeService: http://localhost:5076/swagger
- LeaveService: http://localhost:5080/swagger

### 6. Start the Frontend

Open another terminal:

```bash
cd frontend
npm install
npm run dev
```

Open the local URL displayed by Vite in your browser.

### 7. Stop the Backend Services

From the project root:

```powershell
.\stop-backend.ps1
```

Alternatively:

```bash
docker compose down
```

## API Documentation

The backend services expose RESTful APIs for authentication, employee management, and leave management.

### EmployeeService

Responsible for:

- Employee authentication and login.
- Employee information management.
- Role-based access control.
- Employee data retrieval.

Swagger URL:

http://localhost:5076/swagger

### LeaveService

Responsible for:

- Submitting leave requests.
- Retrieving employee leave requests.
- Checking leave allowances.
- Approving and rejecting leave requests.
- Retrieving leave-management information.

Swagger URL:

http://localhost:5080/swagger

### Authentication

The application uses JWT-based authentication.

After successful login, the frontend receives an access token and uses it to authenticate subsequent API requests.

Management and employee permissions are enforced through role-based authorization.

## Testing

The project includes backend unit tests to verify business logic and application behavior.

### Run Backend Unit Tests

From the project root:

```bash
dotnet test
```

To test the LeaveService unit test project specifically:

```bash
dotnet test tests/LeaveService.UnitTests/LeaveService.UnitTests.csproj
```

### Frontend Build Verification

From the frontend directory:

```bash
npm run build
```

### Manual API Testing

Use Swagger or Postman to test:

- User login and JWT authentication.
- Employee information retrieval.
- Leave request submission.
- Leave allowance validation.
- Leave request approval and rejection.
- Authorization for Employee, HOD, and HR roles.

### Docker Verification

Check whether the backend containers are running:

```bash
docker compose ps
```

Review service logs:

```bash
docker compose logs -f
```

## Known Limitations

- EmployeeDb and LeaveDb are separate databases. Employee references in leave records are logical references rather than physical cross-database foreign keys.
- Notification processing runs as a separate background service; notification delivery depends on its configured infrastructure and implementation.
- The application requires valid environment variables and service-to-service API key configuration.
- Docker Desktop must be running before starting the containerized backend.
- The system's behavior depends on the configured leave policies and employee roles.
- Production deployment requires additional configuration for secrets management, HTTPS, monitoring, logging, backups, and infrastructure security.