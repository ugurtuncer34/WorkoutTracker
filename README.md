# WorkoutTracker API

A robust, containerized ASP.NET Core Web API built with **.NET 10** for tracking workouts, managing exercises, and logging sets. This backend service features JWT-based authentication, a comprehensive catalog system for muscle groups and exercises, and detailed workout session logging.

## 🚀 Features

* **User Authentication:** Secure registration and login using JWT (JSON Web Tokens) and bcrypt password hashing.
* **Catalog Management:** Full CRUD operations to manage Muscle Groups, Target Muscles, and Exercises.
* **Dynamic Exercise Types:** Support for various exercise formats (Reps & Weight, Reps Only, Duration Only).
* **Workout Sessions:** Start, track, complete, or cancel workout sessions.
* **Detailed Set Logging:** Log individual sets with variables like reps, weight, duration, and personal notes.
* **Total Volume Calculation:** Automatically calculates total workout volume based on sets and weights.
* **Ready for Production:** Fully dockerized with an SQLite database and a pre-configured `docker-compose` setup.

## 🛠️ Tech Stack

* **Framework:** .NET 10 (ASP.NET Core Web API)
* **ORM:** Entity Framework Core
* **Database:** SQLite
* **Authentication:** JWT Bearer Authentication
* **Containerization:** Docker & Docker Compose
* **Documentation:** Swagger / OpenAPI

## 📂 Architecture & Data Model

The database is pre-seeded with standard muscle groups (Arms, Chest, Back, Shoulders, Legs, Core) and common exercises.

* `MuscleGroup` (1) -> (N) `TargetMuscle`
* `TargetMuscle` (1) -> (N) `Exercise`
* `User` (1) -> (N) `WorkoutSession`
* `WorkoutSession` (1) -> (N) `SetLog`
* `Exercise` (1) -> (N) `SetLog`

## ⚙️ Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (For local development)
* [Docker](https://www.docker.com/) (For containerized deployment)

### Local Development Setup

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/yourusername/WorkoutTracker.git](https://github.com/yourusername/WorkoutTracker.git)
   cd WorkoutTracker
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure Environment:**
   Update the `appsettings.json` file to set your custom `JwtSettings:Secret`. The database `workouttracker.db` will be created automatically in the root directory upon running.

4. **Run the application:**
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5065`. You can explore the endpoints using Swagger at `http://localhost:5065/swagger`.

### 🐳 Docker Deployment

The project is fully prepared for containerized deployment, making it easy to host on any VPS.

1. **Build and run using Docker Compose:**
   ```bash
   docker compose up -d --build
   ```
2. **Persistent Storage:** The `compose.yaml` is configured with a Docker volume (`workout_data_v2`) mapped to `/app/data` inside the container. This ensures your SQLite database (`workouttracker.db`) persists across container restarts.

## 📡 Core API Endpoints

Once the application is running, authenticate via `/api/Auth/login` to receive your JWT token. Add this token to the `Authorization` header (`Bearer <token>`) for secure endpoints.

### Authentication
* `POST /api/Auth/register` - Create a new user account.
* `POST /api/Auth/login` - Authenticate and retrieve a JWT token.

### Catalog (Secured)
* `GET /api/Catalog/muscle-groups` - Retrieve all muscle groups.
* `GET /api/Catalog/muscle-groups/{id}/exercises` - Get exercises for a specific muscle group.
* `POST /api/Catalog/exercises` - Create a new exercise.

### Workouts (Secured)
* `POST /api/Workout/sessions` - Start a new workout session.
* `POST /api/Workout/logs` - Add a set log to a specific session and exercise.
* `PUT /api/Workout/sessions/{sessionId}/complete` - Mark a session as completed.
* `GET /api/Workout/sessions` - Retrieve the user's workout history.
* `DELETE /api/Workout/sessions/{sessionId}` - Permanently delete a workout session.

## 🔒 Configuration & Security Notes

* **JWT Secret:** Ensure you change the default JWT secret in `appsettings.json` before deploying to production.
* **Registration Control:** You can disable new user sign-ups by setting `"AllowRegistration": false` in your `appsettings.json`.
* **CORS:** The default policy allows origins from `http://localhost:5173` (React/Vite default). Update `CorsOrigins` in `appsettings.json` to match your frontend domain.
