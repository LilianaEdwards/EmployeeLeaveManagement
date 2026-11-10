# Employee Leave Management System
## Project Setup & Command Reference

**Project:** Employee Leave Management System (ELMS)
**Technology:** C# / .NET 10, ASP.NET Core, Entity Framework Core, SQL Server, Docker, React, TypeScript, Vite
**Environment:** Windows + PowerShell
**Last Updated:** 3 November 2026

---

# 1. Project Directory

Project root:

```powershell
C:\Users\swd\EmployeeLeaveManagement
```

Frontend directory:

```powershell
C:\Users\swd\EmployeeLeaveManagement\frontend
```

Main project structure:

```text
EmployeeLeaveManagement/
â”œâ”€â”€ src/
â”‚   â”œâ”€â”€ Contracts/
â”‚   â”œâ”€â”€ EmployeeService/
â”‚   â”œâ”€â”€ LeaveService/
â”‚   â””â”€â”€ NotificationService/
â”œâ”€â”€ tests/
â”‚   â”œâ”€â”€ EmployeeService.UnitTests/
â”‚   â”œâ”€â”€ LeaveService.UnitTests/
â”‚   â””â”€â”€ NotificationService.UnitTests/
â”œâ”€â”€ frontend/
â”œâ”€â”€ docs/
â”œâ”€â”€ docker-compose.yml
â”œâ”€â”€ .env
â”œâ”€â”€ .env.example
â”œâ”€â”€ global.json
â”œâ”€â”€ EmployeeLeaveManagement.slnx
â”œâ”€â”€ start-backend.ps1
â””â”€â”€ stop-backend.ps1
```

# 2. Required Software

Install the following software before running the project.

| Software | Purpose |
|---|---|
| .NET 10 SDK | Backend development |
| Docker Desktop | Run microservices and SQL Server |
| Node.js LTS | React frontend |
| npm | Frontend package management |
| Navicat | Database management |
| Postman | API testing |
| Visual Studio Code / Visual Studio | Code editing |

## 2.1 Check installed software

Open PowerShell:

```powershell
dotnet --version
dotnet --list-sdks
docker --version
docker compose version
node --version
npm --version
```

Check Docker:

```powershell
docker info
```

Docker Desktop must be running.

## 2.2 Install software using Winget

If the required software is not installed, use:

```powershell
winget install --id Microsoft.DotNet.SDK.10 -e
winget install --id OpenJS.NodeJS.LTS -e
winget install --id Docker.DockerDesktop -e
```

Restart PowerShell after installation. Docker Desktop may require a Windows restart or WSL 2 setup.

Recheck the installed versions using Section 2.1.

**Note:** The project pins .NET SDK 10.0.402 in `global.json`. Make sure a compatible .NET 10 SDK is installed.

# 3. Initial Project Setup

Navigate to the project root:

```powershell
cd C:\Users\swd\EmployeeLeaveManagement
```

## 3.1 Check project files

```powershell
Get-ChildItem
```

Check the solution:

```powershell
Test-Path .\EmployeeLeaveManagement.slnx
```

Check Docker Compose:

```powershell
Test-Path .\docker-compose.yml
```

Check environment configuration:

```powershell
Test-Path .\.env
```

The `.env` file must exist and contain the required local configuration.

**Security:** Never display, upload, or share the actual `.env` contents. It contains credentials and secret keys.

## 3.2 Restore .NET packages

```powershell
dotnet restore .\EmployeeLeaveManagement.slnx
```

## 3.3 Build backend projects

```powershell
dotnet build .\EmployeeLeaveManagement.slnx
```

## 3.4 Install frontend packages

```powershell
cd .\frontend
npm install
```

Return to the root:

```powershell
cd ..
```

For a clean frontend installation on a machine with an existing `package-lock.json`, use:

```powershell
cd .\frontend
npm ci
cd ..
```

Use `npm ci` for reproducible installations when the lock file is available and up to date.

# 4. Environment Configuration

The project uses `.env` for Docker configuration.

Required configuration categories include:

- SQL Server SA password
- JWT signing key
- Internal EmployeeService API key
- Initial HOD account information

Use `.env.example` as a reference when setting up a new environment.

Check that the file exists without displaying its contents:

```powershell
Test-Path .\.env
```

Check the Compose configuration:

```powershell
docker compose config --quiet
```

If configuration validation succeeds, Compose is able to parse the configuration.

Do not commit `.env` to Git.

# 5. Start the Backend

Make sure Docker Desktop is running.

From the project root:

```powershell
.\start-backend.ps1
```

Alternatively, start all Compose services directly:

```powershell
docker compose up --build -d
```

Check running services:

```powershell
docker compose ps
```

Expected services:

| Service | Purpose |
|---|---|
| sqlserver | SQL Server database |
| employeeservice | Employee management and authentication |
| leaveservice | Leave request and balance management |
| notificationservice | Notification worker placeholder |

SQL Server should report a healthy status.

## 5.1 Check backend logs

All services:

```powershell
docker compose logs --tail 50
```

EmployeeService:

```powershell
docker compose logs --tail 50 employeeservice
```

LeaveService:

```powershell
docker compose logs --tail 50 leaveservice
```

NotificationService:

```powershell
docker compose logs --tail 50 notificationservice
```

SQL Server:

```powershell
docker compose logs --tail 50 sqlserver
```

Follow logs continuously:

```powershell
docker compose logs -f
```

Press `Ctrl + C` to stop following logs. This does not stop the containers.

# 6. Start the Frontend

Open a second PowerShell window.

```powershell
cd C:\Users\swd\EmployeeLeaveManagement\frontend
```

Install dependencies if necessary:

```powershell
npm install
```

Start Vite:

```powershell
npm run dev
```

Open the local frontend URL displayed by Vite, normally:

```text
http://localhost:5173
```

Keep the frontend terminal open while using the application.

Press `Ctrl + C` to stop the frontend development server.

# 7. Application URLs

## Frontend

```text
http://localhost:5173
```

## EmployeeService Swagger

```text
http://localhost:5076/swagger/index.html
```

## LeaveService Swagger

```text
http://localhost:5080/swagger/index.html
```

Check API availability:

```powershell
Invoke-WebRequest http://localhost:5076/swagger/index.html -UseBasicParsing
Invoke-WebRequest http://localhost:5080/swagger/index.html -UseBasicParsing
```

A successful response should return HTTP 200.

# 8. Database Management

The project uses SQL Server with two databases.

| Database | Purpose |
|---|---|
| EmployeeDb | Employee accounts, roles, and authentication |
| LeaveDb | Leave requests and related data |

## 8.1 Navicat connection

Use the following connection settings:

| Setting | Value |
|---|---|
| Connection type | SQL Server |
| Host | 127.0.0.1 |
| Port | 1434 |
| Authentication | SQL Server Authentication |
| Username | sa |
| Password | Existing SQL Server SA password |

The password must match the password configured when the persistent SQL Server database was initialized. Changing `.env` afterward does not automatically change the existing SQL Server login password.

## 8.2 Check database container

```powershell
docker compose ps sqlserver
```

## 8.3 Enter SQL Server container

```powershell
docker compose exec sqlserver bash
```

Exit:

```bash
exit
```

## 8.4 Database verification queries

Run these queries in Navicat.

Employee count:

```sql
SELECT COUNT(*) AS EmployeeCount
FROM dbo.Employees;
```

Employee roles:

```sql
SELECT Id, FullName, Email, Role
FROM dbo.Employees;
```

Leave request count:

```sql
SELECT COUNT(*) AS LeaveRequestCount
FROM dbo.LeaveRequests;
```

Leave request status summary:

```sql
SELECT Status, COUNT(*) AS Total
FROM dbo.LeaveRequests
GROUP BY Status;
```

Approved requests:

```sql
SELECT *
FROM dbo.LeaveRequests
WHERE Status = 'Approved';
```

# 9. Automated Testing

Navigate to the project root:

```powershell
cd C:\Users\swd\EmployeeLeaveManagement
```

Run all tests:

```powershell
dotnet test .\EmployeeLeaveManagement.slnx --logger "console;verbosity=normal"
```

Run EmployeeService tests:

```powershell
dotnet test .\tests\EmployeeService.UnitTests\EmployeeService.UnitTests.csproj
```

Run LeaveService tests:

```powershell
dotnet test .\tests\LeaveService.UnitTests\LeaveService.UnitTests.csproj
```

Run NotificationService tests:

```powershell
dotnet test .\tests\NotificationService.UnitTests\NotificationService.UnitTests.csproj
```

Expected result:

- All tests passed.
- Zero failed tests.
- No unexpected skipped tests.

# 10. Frontend Verification

Navigate to the frontend:

```powershell
cd .\frontend
```

Run TypeScript and Vite production build:

```powershell
npm run build
```

Expected result:

- TypeScript compilation succeeds.
- Vite production build succeeds.
- A `dist` directory is generated.

Return to project root:

```powershell
cd ..
```

# 11. Role-Based Functional Testing

Test the following workflows through the frontend.

## Employee

- Login successfully.
- View personal leave balances.
- Submit a leave request.
- View personal leave history.
- Verify leave allowance validation.
- Confirm restricted access to other employees' requests.

## HOD

- Login successfully.
- View company-wide leave requests.
- Approve and reject eligible Employee requests.
- View employee balances.
- Create employees.
- Set employee passwords.
- Confirm HOD cannot approve their own request.
- Confirm HOD cannot review another HOD's request.

## HR

- Login successfully.
- View company-wide leave requests.
- Review Employee requests.
- Review HOD requests.
- Review requests from another HR employee.
- Submit personal leave requests.
- View personal leave balances.
- Confirm HR cannot access HOD-only employee management functions.

Important: When an employee's role changes in the database, log out and log in again to receive a new JWT containing the updated role.

# 12. Docker Management Commands

Check containers:

```powershell
docker compose ps
```

Start services:

```powershell
docker compose up -d
```

Rebuild and start all services:

```powershell
docker compose up --build -d
```

Rebuild only LeaveService:

```powershell
docker compose up --build -d leaveservice
```

Rebuild only EmployeeService:

```powershell
docker compose up --build -d employeeservice
```

Restart application services:

```powershell
docker compose restart employeeservice leaveservice notificationservice
```

Stop application services:

```powershell
.\stop-backend.ps1
```

Alternatively:

```powershell
docker compose down
```

**Important:** Do not use the following command during normal stopping or restarting:

```powershell
docker compose down -v
```

The `-v` option removes Compose-managed volumes and can delete persistent database data.

# 13. Database Persistence Verification

Before restarting containers, record database counts in Navicat.

Restart application services:

```powershell
docker compose restart employeeservice leaveservice notificationservice
```

Check container status:

```powershell
docker compose ps
```

Check API availability:

```powershell
Invoke-WebRequest http://localhost:5076/swagger/index.html -UseBasicParsing
Invoke-WebRequest http://localhost:5080/swagger/index.html -UseBasicParsing
```

Run the same database count queries from Section 8.4.

Compare the before-and-after counts.

Existing employee accounts and leave requests should remain available.

For a full SQL Server container recreation test, only proceed after confirming the named database volume and backup arrangements. Never remove the volume as part of a routine restart.

# 14. Troubleshooting Commands

## Docker not running

```powershell
docker info
```

Start Docker Desktop and retry.

## Check service logs

```powershell
docker compose logs --tail 100 employeeservice
docker compose logs --tail 100 leaveservice
docker compose logs --tail 100 sqlserver
```

## Check port usage

```powershell
Get-NetTCPConnection -LocalPort 5076,5080,1434 -ErrorAction SilentlyContinue
```

## Check .NET SDK

```powershell
dotnet --info
```

## Check frontend dependencies

```powershell
cd .\frontend
npm ls --depth=0
```

## Clean and rebuild .NET projects

```powershell
cd ..
dotnet clean .\EmployeeLeaveManagement.slnx
dotnet restore .\EmployeeLeaveManagement.slnx
dotnet build .\EmployeeLeaveManagement.slnx
```

## Rebuild frontend

```powershell
cd .\frontend
npm run build
```

## Rebuild Docker services

```powershell
cd ..
docker compose up --build -d
```

# 15. Daily Development Workflow

For normal daily development, use the following sequence.

**Terminal 1 â€” Backend**

```powershell
cd C:\Users\swd\EmployeeLeaveManagement
.\start-backend.ps1
```

**Terminal 2 â€” Frontend**

```powershell
cd C:\Users\swd\EmployeeLeaveManagement\frontend
npm run dev
```

**Terminal 3 â€” Testing**

```powershell
cd C:\Users\swd\EmployeeLeaveManagement
dotnet test .\EmployeeLeaveManagement.slnx
```

When frontend changes are ready:

```powershell
cd .\frontend
npm run build
```

When backend changes are ready:

```powershell
cd C:\Users\swd\EmployeeLeaveManagement
dotnet build .\EmployeeLeaveManagement.slnx
```

# 16. Final Project Checklist

- [x] .NET solution builds successfully.
- [x] Automated unit tests pass.
- [x] Docker services run successfully.
- [x] EmployeeService API works.
- [x] LeaveService API works.
- [x] React frontend builds successfully.
- [x] Employee workflow tested.
- [x] HOD workflow tested.
- [x] HR workflow tested.
- [x] Leave allowance validation tested.
- [x] Role-based access restrictions tested.
- [ ] Database persistence after container restart documented.
- [ ] Final configuration and security review completed.
- [ ] Production deployment configuration reviewed.

---

## Important Operational Notes

1. Keep `.env` private.
2. Never commit passwords, JWT keys, or internal API keys.
3. Do not delete Docker database volumes during routine maintenance.
4. Always run automated tests after backend changes.
5. Always run the frontend production build after frontend changes.
6. Restart and log in again after changing employee roles.
7. The current Docker Compose configuration is intended for local development and testing, not direct production deployment.
8. NotificationService is currently a placeholder worker; do not assume email or push notifications are fully implemented.

**End of Command Reference**
