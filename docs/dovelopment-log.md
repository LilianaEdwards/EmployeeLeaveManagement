# Development Log â€” Employee Leave Management System

## Entry 001: Initial Build Verification

**Date:** 23 October 2026
**Phase:** Day 1 â€” Development Environment and Employee Service
**Status:** In progress

### Objective
Verify the solution structure and establish a successful build before implementing the Employee Service.

### Environment
- Operating system: Windows
- IDE/terminal: To be confirmed
- Target framework: .NET 11 preview (`net11.0`)
- Project type: ASP.NET Core Web API and .NET Worker Service

### Initial Build Results
- Contracts: Build succeeded
- Notification Service: Build succeeded
- Notification Service Unit Tests: Build succeeded
- Leave Service Unit Tests: Build succeeded
- Leave Service: Build succeeded
- Employee Service: Build failed
- Employee Service Unit Tests: Restore completed with security warnings

### Issue 001 â€” Duplicate EmployeeManager Definition

**Observed error:** CS0101 and CS0111.

**Affected component:** `src/EmployeeService/Services/EmployeeManager.cs`

**Symptoms:** Duplicate class, constructor, and method definitions for `EmployeeManager`, `CreateAsync`, `GetByIdAsync`, `GetAllAsync`, and `ToResponse`.

**Root cause:** Under investigation. Suspected duplicate class or method declarations.

**Resolution:** Search the Employee Service project for duplicate declarations, retain one correct implementation, and rebuild.

**Verification evidence:** Record the build output after the fix.

### Issue 002 â€” Vulnerable NuGet Dependency

**Observed warnings:** NU1902 and NU1903.

**Affected package:** `System.Security.Cryptography.Pkcs` version `9.0.11`.

**Impact:** NuGet reports moderate- and high-severity security advisories.

**Resolution plan:** Inspect the dependency graph, update to a compatible patched version, restore packages, and verify the resolved version and remaining warnings.

**Verification evidence:** Record package-list output after the update.

### Issue 003 â€” Preview Target Framework

**Observed warning:** NETSDK1057.

**Current target:** `net11.0`.

**Impact:** The build uses a preview version of .NET, reducing the suitability of the environment for a stable deliverable.

**Resolution plan:** Verify installed SDKs and runtimes, then evaluate retargeting all projects to stable .NET 10 LTS and compatible EF Core 10 packages.

### Next Actions
1. Resolve duplicate EmployeeManager declarations.
2. Verify `dotnet build` and `dotnet test`.
3. Resolve the vulnerable package warning.
4. Decide on the target framework and align project dependencies.
5. Implement and test Employee CRUD endpoints.
6. Record final verification evidence.

### Completion Status
Initial verification completed. Employee Service compilation and dependency cleanup remain in progress.

### Development Log â€” 23 October 2026

**Task:** EmployeeService compilation troubleshooting

**Problem identified**
Two files contained duplicate `EmployeeManager` class definitions. After resolving the duplicate declarations, the controller file also had multiple file-scoped namespace declarations and incorrect namespace resolution.

**Fix implemented**
- Retained one `EmployeeManager` implementation in the service layer.
- Replaced the controller file contents with the correct `EmployeesController` implementation.
- Corrected the controller namespace and imports.

**Verification**
- EmployeeService project build: **Succeeded**.
- Full solution unit tests: Pending verification.

**Remaining technical debt**
- Review the .NET 11 preview framework warning.
- Review and resolve any outstanding NuGet vulnerability warnings.

**Next action**
Run the complete unit test suite and record the results.

# Development Log â€” Employee Leave Management System

**Date:** 23 October 2026

## Entry: EmployeeService Build Fix and Test Verification

### Objective
Resolve EmployeeService compilation errors and verify the solution using automated unit tests.

### Issues identified
1. Duplicate `EmployeeManager` class definitions in separate files caused compiler errors CS0101 and CS0111.
2. The controller file contained multiple file-scoped namespace declarations, causing CS8954.
3. Incorrect namespace resolution caused CS0234 errors.

### Resolution
- Retained one `EmployeeManager` implementation in the service layer.
- Corrected the controller file so it contains the `EmployeesController` class rather than a duplicate manager class.
- Corrected namespace declarations and imports.
- Rebuilt the EmployeeService project and then ran the full solution build and test suite.

### Verification results

| Verification | Result |
|---|---|
| EmployeeService build | Passed |
| Full solution build | Passed |
| LeaveService unit tests | Passed |
| NotificationService unit tests | Passed |
| EmployeeService unit tests | Passed |
| Total unit tests reported | 3 passed, 0 failed, 0 skipped |

**Commands executed:**

`dotnet build src/EmployeeService/EmployeeService.csproj`

`dotnet test`

### Remaining technical debt
- Review the .NET 11 preview framework warning and decide whether to target a supported stable release.
- Recheck NuGet dependency security warnings and confirm that vulnerable package versions are resolved.
- Verify API behavior through Swagger or Postman; successful compilation and unit tests do not establish that all endpoints work correctly against the configured database.

### Current status
**Build and unit test verification: successful.**

### Next actions
1. Verify EmployeeService endpoints using Swagger or Postman.
2. Test employee creation, retrieval, listing, duplicate-email handling, and not-found responses.
3. Record API test evidence and any defects.
4. Continue implementation and integration testing for LeaveService and NotificationService.
5. Keep architecture, API, database, messaging, test, and presentation documentation synchronized with implementation changes.

### Issue: EmployeeService Startup Failure â€” Invalid JSON

**Symptom:** EmployeeService terminated during startup with `System.IO.InvalidDataException`.

**Root cause:** `appsettings.Development.json` contained an extra opening brace and a missing comma between the `Logging` and `ConnectionStrings` properties.

**Fix:** Removed the extra brace and added the missing comma while preserving the existing database connection string.

**Verification:** JSON corrected. Application restart and database connectivity verification pending.

**Lesson learned:** ASP.NET Core loads its configuration before starting the application. Invalid JSON prevents startup even when the C# project compiles successfully.

**Status:** Fix applied; runtime verification pending.

**Verification update â€” 23 October 2026**

- Corrected the invalid JSON structure in `appsettings.Development.json`.
- Restarted EmployeeService successfully.
- The application passed the previous configuration-parsing failure.
- Database connectivity and API endpoint behavior still require verification.

**Status:** Application startup succeeded; API testing pending.

### Task: Swagger UI Setup

**Objective:** Enable interactive API documentation and endpoint testing for EmployeeService.

**Action:** Add and configure Swashbuckle.AspNetCore, register Swagger services, and enable Swagger UI in the Development environment.

**Verification:** Pending successful launch of the Swagger UI and inspection of the employee endpoints.

**Status:** In progress.

### Issue: Swagger API Definition Returns HTTP 500

**Date:** 23 October 2026

**Symptom:** Swagger UI loads, but fetching `/swagger/v1/swagger.json` returns HTTP 500.

**Impact:** Interactive API documentation is currently unavailable, so EmployeeService endpoints cannot yet be tested through Swagger.

**Root cause:** Under investigation. The server-side exception must be inspected.

**Next actions:**
1. Reproduce the error by refreshing Swagger UI.
2. Inspect the EmployeeService terminal output.
3. Identify and fix the specific exception.
4. Verify that `/swagger/v1/swagger.json` returns HTTP 200 and that employee endpoints appear in Swagger UI.

**Status:** Troubleshooting in progress.

### 2026-10-23 â€” Swagger Restored and API Verification Started

**Issue**
- Swagger UI previously loaded without displaying the API definition because `/swagger/v1/swagger.json` returned HTTP 500.
- NuGet restore also failed with `NU1605` due to incompatible `Microsoft.OpenApi` package versions.

**Resolution**
- Adjusted the EmployeeService OpenAPI package references to resolve the dependency conflict.
- Confirmed that the EmployeeService project builds successfully.
- Confirmed that Swagger UI loads and displays the API endpoints.
- Confirmed that `/swagger/v1/swagger.json` returns an OpenAPI document successfully.

**Verification**
- EmployeeService build: Passed.
- Swagger UI: Passed.
- OpenAPI JSON endpoint: Passed.
- Employee API GET test: Pending.
- Employee creation POST test: Pending.
- GET-after-POST persistence check: Pending.
- Database record verification in Navicat: Pending.
- Full solution build and test run: Pending.

**Remaining Warning**
- `NETSDK1057`: The project targets a preview version of .NET. Review framework support and migration options separately.

**Next Steps**
1. Test the employee-list GET endpoint in Postman.
2. Create a test employee using the POST endpoint.
3. Confirm the new employee appears in a subsequent GET response.
4. Verify the record in the configured database using Navicat.
5. Run `dotnet build` and `dotnet test` for the full solution.
6. Record actual HTTP status codes, test totals, failures, and any remaining issues.

**Current Status:** Swagger verification passed; API and database verification in progress.

### 2026-10-24 â€” Employee Database and API Verification

**Objective**
- Resolve the EmployeeService database login failure and verify employee data persistence.

**Issue and Root Cause**
- EmployeeService initially failed because the `EmployeeDb` database was missing.
- The initial EF Core migration had not yet been created.
- A running EmployeeService process locked the executable during a subsequent build attempt.

**Resolution**
- Verified that `MSSQLLocalDB` was running.
- Inspected `EmployeeDbContext` and the `Employee` entity.
- Generated the `InitialCreate` EF Core migration.
- Resolved the executable file lock and successfully built EmployeeService.
- Applied the migration to create the database schema.

**Verification Results**
- EmployeeService build: Passed.
- Initial migration generation: Passed.
- Database update: Passed.
- `EmployeeDb` database: Verified in Navicat.
- `dbo.Employees` table: Verified.
- `__EFMigrationsHistory`: Verified.
- Employee GET endpoint in Swagger: Passed.
- Employee POST test: Passed, according to the reported result.
- Database persistence verification: Reported working.

**Technical Notes**
- The `Employees` table includes a GUID primary key, required `FullName`, `Email`, and `Department` columns, and a unique index on `Email`.
- The project currently targets `net11.0`, while EF Core packages and CLI are version `10.0.13`. This compatibility configuration still needs a separate review.

**Next Steps**
1. Test validation for missing required fields and invalid email values.
2. Test duplicate email handling.
3. Verify employee update and delete operations, if implemented.
4. Record the exact HTTP status codes and test results.
5. Run the full solution build and test suite.
6. Review framework and package compatibility.

**Current Status:** Employee database setup and basic API testing completed successfully.

### 2026-10-24 â€” Employee Duplicate Email Validation

**Objective**
- Verify that the EmployeeService prevents duplicate employee email addresses.

**Test Procedure**
1. Submitted a POST request to create an employee using `duplicate.test@example.com`.
2. Submitted a second POST request using the same email but different employee details.
3. Queried `dbo.Employees` in Navicat to count records matching the email.

**Result**
- Duplicate email constraint: Passed.
- Database verification: Only one employee record exists for the tested email.
- Data integrity: Preserved.

**Technical Note**
- The unique index `IX_Employees_Email` enforces email uniqueness at the database level.
- API-level duplicate detection and a consistent client-facing error response should also be verified.

**Status:** Duplicate-email data-integrity test passed.

### 2026-10-24 â€” Employee API Input Validation

**Objective**
- Verify that EmployeeService rejects invalid employee data.

**Test Case**
- Submitted an employee request with an invalid or missing `FullName`.

**Observed Result**
- HTTP status: `400 Bad Request`.
- ASP.NET Core returned a structured validation response.
- The response reported that `FullName` is required and must contain between 2 and 100 characters.

**Technical Notes**
- The response indicates that request validation is active.
- The `traceId` can help correlate the request with application logs.
- Database persistence must be checked separately to confirm that invalid requests do not create records.

**Status:** API validation response verified; database non-insertion check pending.

### 2026-10-24 â€” EmployeeService Validation Testing

**Objective**
- Verify EmployeeService input validation and database integrity.

**Tests Completed**
- Employee GET endpoint: returned employee data.
- Employee POST endpoint: successfully created employee records.
- Database persistence: verified using Navicat.
- Duplicate email: tested repeated email addresses; duplicate-email data-integrity test completed.
- Required field: missing or invalid `FullName` returned HTTP 400.
- Name length: verified the minimum and maximum length validation.
- Invalid email: tested an incorrectly formatted email address.

**Results**
- API validation returned structured error responses for invalid input.
- Invalid requests were verified not to create database records.
- Database email uniqueness was tested.

**Next Steps**
- Run the full solution build and automated tests.
- Review API error handling and remaining warnings.
- Continue implementing and testing the leave-management workflow.

**Status:** EmployeeService CRUD and validation checks completed; full regression checks pending.

## 2026-10-24 â€” LeaveService API and Database Verification

### Completed
- Configured `LeaveService` to use EF Core with SQL Server LocalDB and the `LeaveDb` database.
- Created and applied the initial EF Core migration.
- Verified `LeaveDb`, `LeaveRequests`, and `__EFMigrationsHistory` in Navicat.
- Implemented leave-request creation, retrieval, filtering, and review endpoints.
- Resolved the Swagger/OpenAPI runtime exception by removing the conflicting, unnecessary `Microsoft.AspNetCore.OpenApi` package from LeaveService.
- Confirmed the LeaveService project builds successfully and Swagger loads.
- Manually tested the leave-request API endpoints successfully.

### Current Architecture
- EmployeeService manages employee records in `EmployeeDb`.
- LeaveService manages leave requests in `LeaveDb`.
- Leave requests reference employees using `EmployeeId`; cross-service employee validation remains to be implemented.

### Remaining Work
- Verify approval and rejection workflows, including invalid state transitions.
- Add automated unit and integration tests for LeaveService.
- Implement employee existence validation through EmployeeService integration.
- Implement authentication and authorization for manager-only review operations.
- Continue updating the development log with test evidence and troubleshooting results.

### Notes
- The solution targets the preview version of .NET 11 and may display `NETSDK1057`.
- Swagger/API testing currently uses a local development environment.

## 2026-10-28 â€” EmployeeService Integration

### Completed
- Added `EmployeeApiClient` to allow LeaveService to validate employee IDs through EmployeeService using HTTP.
- Configured EmployeeService base URL as `http://localhost:5076` in LeaveService development settings.
- Updated `LeaveManager` to verify that the employee exists before creating a leave request.
- Updated `LeaveRequestsController` to log EmployeeService connection failures and return `503 Service Unavailable`.
- Updated unit-test setup to use a fake HTTP handler, avoiding a dependency on a running EmployeeService.

### Verification
- `dotnet build`: passed.
- `dotnet test`: passed after updating the LeaveService test helper.
- Existing employee: leave request created successfully.
- Nonexistent employee: `400 Bad Request`.
- EmployeeService unavailable: expected error message received; verify the HTTP status code is `503`.

### Notes
- EmployeeService and LeaveService continue to use separate databases.
- .NET 11 preview SDK warning remains.
- Employee existence validation is implemented; authentication and authorization for leave review remain future work.

### Next Step
Add dedicated unit tests for employee-not-found and EmployeeService failure scenarios, then verify the full integration and document the final HTTP status codes.

### EmployeeApiClient Unit Tests
- Added tests for HTTP 200, HTTP 404, and HTTP 500 responses.
- LeaveService unit tests: 12 passed, 0 failed, 0 skipped.
- Build succeeded.
- .NET 11 preview SDK warning remains.

## 2026-10-28 â€” EmployeeService Integration and Unit Tests

### Completed
- Added `EmployeeApiClient` to let LeaveService validate employee IDs through EmployeeService over HTTP.
- Configured EmployeeService base URL as `http://localhost:5076` in LeaveService development settings.
- Updated `LeaveManager` to check that an employee exists before creating a leave request.
- Updated `LeaveRequestsController` to handle EmployeeService connection failures and return `503 Service Unavailable`.
- Updated LeaveService unit-test setup to use a fake HTTP handler so tests do not require EmployeeService to be running.
- Added `EmployeeApiClientTests` for successful responses, missing employees, and server errors.

### Verification
- LeaveService unit tests: **12 passed, 0 failed, 0 skipped**.
- Full solution `dotnet test`: **14 passed, 0 failed, 0 skipped**.
- Build performed by `dotnet test`: succeeded.
- Existing employee: leave request creation succeeded.
- Nonexistent employee: returned `400 Bad Request`.
- EmployeeService unavailable: confirmed HTTP 503 Service Unavailable with the expected error message.

### Remaining Notes
- EmployeeService and LeaveService use separate databases.
- The .NET 11 preview SDK notice (`NETSDK1057`) remains.
- Authentication and authorization for leave-request review are future work.

### Next Steps
1. Confirm that EmployeeService unavailability returns HTTP `503`.
2. Continue integration testing and document the verified HTTP status codes.
3. Proceed to the next Leave Management feature after integration checks are complete.

## 2026-10-28 â€” EmployeeApiClient Reliability Tests

### Completed
- Added automated tests for EmployeeService HTTP 200, HTTP 404, and HTTP 500 responses.
- Added tests for simulated connection failures and request timeout/cancellation.
- Configured a short timeout in the test helper to prevent unnecessarily long test runs.
- Confirmed that LeaveService returns HTTP 503 when EmployeeService is unavailable.

### Verification
- LeaveService unit tests: 14 passed, 0 failed, 0 skipped; duration 2.0 seconds.
- Full solution tests: 16 passed, 0 failed, 0 skipped; duration 1.7 seconds.
- Full solution build: succeeded.
- .NET 11 preview SDK notice (`NETSDK1057`) remains.

### Notes
- The short timeout is configured in the test helper; production HTTP client timeout configuration remains unchanged.
- EmployeeService and LeaveService continue to use separate databases.

### Next Step
- Review leave-request validation and review workflows, then continue implementing the next Employee Leave Management feature.

## 2026-10-28 â€” Reviewer Validation and Full-Solution Test Verification

### Changes
- Added reviewer employee existence validation to the leave-review workflow using `EmployeeApiClient`.
- The reviewer must correspond to an existing employee before a leave request can be reviewed.

### Verification
- LeaveService unit tests: 14 passed, 0 failed, 0 skipped.
- Full solution tests: 16 passed, 0 failed, 0 skipped.
- Full solution build: succeeded.
- Full solution test duration: 2.3 seconds.
- Build duration: 3.6 seconds.

### Issue and Resolution
- An earlier test run failed because the running `LeaveService` process locked `LeaveService.exe`.
- After resolving the file lock, the LeaveService unit tests and full solution tests passed.

### Remaining Warning
- `NETSDK1057`: the project uses a preview version of .NET 11. The warning does not currently block builds or tests.

### Next Step
- Verify reviewer validation through the API, including an existing reviewer, a nonexistent reviewer, a missing leave request, and a request that has already been reviewed.

## 2026-10-28 â€” Reviewer Validation API Integration Tests

### Verified
- Started EmployeeService and LeaveService in the Development environment.
- Confirmed both Swagger endpoints were reachable.
- Created a test reviewer employee successfully.
- Created a pending Annual leave request for an existing employee.
- Approved the request using a valid reviewer; the API operation succeeded.
- Submitted a review request using a nonexistent reviewer ID.
- API returned the expected message: `The specified reviewer employee does not exist.`

### Remaining Verification
- Confirm the invalid-reviewer response uses HTTP 400 Bad Request.
- Verify that a missing leave request returns HTTP 404.
- Verify that attempting to review an already-reviewed request returns HTTP 409 Conflict.

### Test Notes
- The valid-reviewer test request was approved successfully.
- A separate pending request was used for the nonexistent-reviewer test.

## 2026-10-28 â€” Leave Review API Integration Verification

### Environment
- EmployeeService running in Development mode at `http://localhost:5076`.
- LeaveService running in Development mode at `http://localhost:5080`.
- Swagger endpoints verified accessible.

### Integration Tests
- Valid reviewer: HTTP 200 OK; review succeeded.
- Nonexistent reviewer: HTTP 400 Bad Request; validation error returned.
- Nonexistent leave request: HTTP 404 Not Found.
- Already-reviewed leave request: HTTP 409 Conflict; second review rejected.

### Test Data
- Test reviewer employee created successfully.
- A pending Annual leave request was created and approved successfully.
- A separate pending request was used to test nonexistent-reviewer validation.
- The previously approved request was reused to test the duplicate-review restriction.

### Result
- All four manually tested review scenarios produced the expected HTTP status codes.
- The API enforces reviewer existence, handles missing leave requests, and prevents reviewing a request that is no longer pending.

### Remaining Warning
- `NETSDK1057`: the project uses a preview version of .NET 11. The full solution build and tests passed despite this warning.

### Next Step
- Add automated unit tests for reviewer existence validation and review edge cases where coverage is missing, then rerun the full test suite.

### 2026-10-28 â€” Reviewer Validation Unit Test

**Work completed**
- Updated the fake EmployeeService HTTP handler in `UnitTest1.cs` to simulate an employee-not-found response for a specified employee ID.
- Added `ReviewAsync_NonexistentReviewer_ThrowsArgumentException` to verify that leave reviews reject nonexistent reviewer employees.
- Confirmed that the test checks for the expected reviewer-validation error message.

**Verification**
- Command: `dotnet test .\tests\LeaveService.UnitTests\LeaveService.UnitTests.csproj`
- Total tests: 15
- Passed: 15
- Failed: 0
- Skipped: 0
- Duration: 2.5 seconds

**Result:** All LeaveService unit tests passed, including the new nonexistent-reviewer validation test.

### 2026-10-28 â€” Full Solution Test Verification

**Work completed**
- Ran the full solution test suite after adding the nonexistent-reviewer validation unit test.
- Verified that the LeaveService reviewer-validation changes did not cause failures in the other test projects.

**Verification**
- Command: `dotnet test`
- Total tests: 17
- Passed: 17
- Failed: 0
- Skipped: 0
- Duration: 2.3 seconds

**Result:** All tests in the full solution passed. The existing .NET 11 preview SDK warning, if still present, remains a separate environment consideration.

## 2026-10-28 â€” Leave Allowance and Approval Validation

### Implemented
- Added working-day calculation that excludes Saturdays and Sundays.
- Implemented leave allowance validation during approval:
  - Annual Leave: maximum 14 working days per calendar year.
  - Sick Leave: maximum 5 working days per calendar year.
  - Paid Leave: maximum 5 working days per calendar year.
  - Unpaid Leave: maximum 5 working days per calendar month.
- Allowance validation counts previously approved leave for the same employee and leave type.
- Added validation to prevent overlapping approved leave dates.
- Cleaned up `LeaveManager.ReviewAsync` to remove duplicate reviewer-existence checks.

### Verification
- LeaveService unit tests: 24 passed, 0 failed.
- Full solution tests: 26 passed, 0 failed, 0 skipped.
- Test duration for the full solution: 2.2 seconds.

### Next Steps
- Implement an employee leave-balance endpoint showing used and remaining leave days.
- Add unit tests for leave-balance calculations.
- Verify the new endpoint through Swagger.

## Employee Leave Balance Feature

### Implementation
- Added `LeaveBalanceDto` to represent leave allowance, used days, remaining days, and the applicable period.
- Added `GetEmployeeBalanceAsync()` to `LeaveManager`.
- Added `GET /api/LeaveRequests/{employeeId}/balance`.
- Annual Leave: 14 days per calendar year.
- Sick Leave: 5 days per calendar year.
- Paid Leave: 5 days per calendar year.
- Unpaid Leave: 5 days per calendar month.
- Only approved leave requests reduce the balance.
- Pending and rejected requests are excluded.
- Working-day calculations exclude Saturdays and Sundays.
- Balance is recalculated from the database whenever the endpoint is called.

### Testing
- Verified the balance endpoint through Swagger.
- Added three unit tests for full allowance, approved leave deduction, and pending/rejected leave handling.
- Latest LeaveService unit test result: 27 passed, 0 failed, 0 skipped.

### Result
Employee leave balances are now available through the API and are calculated dynamically using the current calendar period and approved leave records.

## Final Integration Testing â€” 3 November 2026

### 1. Automated Testing

**Status: Passed**

- Executed the complete solution test suite using `dotnet test`.
- All automated tests passed.
- No test failures were reported.

### 2. Docker Deployment Verification

**Status: Passed**

Verified the Docker Compose services:

- EmployeeService
- LeaveService
- NotificationService
- SQL Server

All four services were running, and SQL Server reported a healthy status.

### 3. Role-Based Access Control Testing

**Status: Passed**

Verified the application workflows for all three roles.

**Employee**
- View personal leave requests and balances.
- Submit leave requests.
- View request history.
- Receive validation when exceeding leave allowances.
- Cannot access other employees' restricted information or review requests.

**HOD**
- View company-wide leave requests.
- Approve and reject Employee leave requests.
- View employee leave balances.
- Submit and view personal leave requests.
- Create employees and set employee passwords.
- Cannot approve their own requests or review HOD requests.

**HR**
- View company-wide leave requests.
- Review eligible leave requests.
- Submit and view personal leave requests.
- View personal leave balances.
- Follow the HR-specific review restrictions.

### 4. Database and Application Integration

**Status: Functionality Verified**

- Employee and Leave services communicate successfully.
- SQL Server stores employee and leave request records.
- Leave request creation, review, and balance calculations work correctly.
- Role information is correctly reflected after logging in again with updated credentials.

### 5. Pending Verification

**Database Persistence After Container Restart**

The following verification is still required:

1. Record employee and leave request counts before restarting containers.
2. Restart application containers without deleting Docker volumes.
3. Confirm all services become available again.
4. Compare database record counts after restarting.
5. Verify that existing employee accounts and leave requests remain accessible.

### 6. Overall Progress

The application has successfully passed automated testing, Docker service verification, and functional testing for Employee, HOD, and HR workflows.

Database persistence after container restart and the remaining deployment security checks are scheduled for the final review.

**Next step:** Complete database persistence verification and configuration security review.
