# BulletinBoard API

ASP.NET Core 10 REST API backend for the Bulletin Board app. Data is persisted to local JSON files — no database required.

## Features

- CRUD endpoints for advertisements and categories
- JWT-based authentication (register / login)
- Swagger UI available in development
- JSON file storage (no external database)

## Tech stack

- **.NET 10** / ASP.NET Core
- **JWT Bearer** authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger** via Swashbuckle
- **xUnit** for unit tests

## Project structure

```
BulletinBoardAPI/
├── Controllers/
│   ├── AdvertisementsController.cs   # CRUD for ads
│   ├── CategoriesController.cs       # CRUD for categories
│   └── AuthController.cs             # Register & login
├── Services/                         # Business logic + interfaces
├── Models/
│   ├── Requests/                     # Input DTOs
│   ├── Responses/                    # Output DTOs
│   └── Mappers/                      # Request/response mapping
├── Data/
│   ├── Advertisements.json           # Ad storage
│   └── Categories.json               # Category storage
└── appsettings.json                  # JWT config
BulletinBoardAPI.Tests/               # Unit tests
```

## Getting started

### Prerequisites

- .NET 10 SDK

### Run the API

```bash
dotnet run --project BulletinBoardAPI
```

The API starts on `http://localhost:5149` by default. Swagger UI is available at `/swagger` in development.

### Run the tests

```bash
dotnet test
```

## API endpoints

### Auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/register` | — | Register a new user |
| POST | `/api/auth/login` | — | Sign in and receive a JWT |

### Advertisements

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/advertisements` | — | List all ads (optional `?categoryId=`) |
| GET | `/api/advertisements/{id}` | — | Get a single ad |
| POST | `/api/advertisements` | Required | Create an ad |
| PUT | `/api/advertisements/{id}` | Required | Update an ad |
| DELETE | `/api/advertisements/{id}` | Required | Delete an ad |

### Categories

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/categories` | — | List all categories |
| GET | `/api/categories/{id}` | — | Get a single category |
| POST | `/api/categories` | Required | Create a category |
| PUT | `/api/categories/{id}` | Required | Update a category |
| DELETE | `/api/categories/{id}` | Required | Delete a category |

## Configuration

JWT settings live in `appsettings.json`. Change `Jwt:Key` before deploying to production:

```json
"Jwt": {
  "Key": "your-secret-key-min-32-chars",
  "Issuer": "BulletinBoardAPI",
  "Audience": "BulletinBoardAPI",
  "ExpiresInMinutes": "60"
}
```
