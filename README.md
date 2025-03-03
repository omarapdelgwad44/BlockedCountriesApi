# IP Country Blocker API

A .NET Core API to manage blocked countries and validate IP addresses using third-party geolocation APIs. This application uses in-memory data storage instead of a database.

## Features

- Block/unblock countries by country code
- Temporarily block countries for specified durations
- IP address lookup and validation
- Check if an IP is from a blocked country
- Log failed blocked attempts
- Pagination, filtering, and search capabilities

## Technology Stack

- .NET Core (Version 8/9)
- In-memory data storage (ConcurrentDictionary)
- Third-party geolocation API integration
- Swagger for API documentation

## Setup Instructions

1. Clone the repository
2. Install required NuGet packages:
   - Microsoft.Extensions.Http
   - Newtonsoft.Json (if used)
3. Add your geolocation API key to appsettings.json
4. Run the application using `dotnet run`
5. Access Swagger UI at https://localhost:5001/swagger

## API Endpoints

### Blocked Countries
- `POST /api/countries/block` - Add a blocked country
- `DELETE /api/countries/block/{countryCode}` - Delete a blocked country
- `GET /api/countries/blocked` - Get all blocked countries (with pagination and filtering)
- `POST /api/countries/temporal-block` - Temporarily block a country

### IP Operations
- `GET /api/ip/lookup?ipAddress={ip}` - Find country via IP lookup
- `GET /api/ip/check-block` - Verify if caller's IP is blocked

### Logs
- `GET /api/logs/blocked-attempts` - Get logs of blocked attempts

## Swagger Documentation

![Swagger UI Screenshot](/Screenshot%202025-03-03%20234308.png)
## Implementation Details

- Thread-safe in-memory storage using ConcurrentDictionary
- Background service that runs every 5 minutes to remove expired temporal blocks
- Proper error handling and validation
