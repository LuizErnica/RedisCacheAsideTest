# Redis Cache Aside Test

A sample **ASP.NET Core Web API** demonstrating the **Cache-Aside** caching pattern using **Redis** and **Entity Framework Core**.

The purpose of this project is to show how to reduce database load and improve API response times by storing frequently accessed data in Redis while keeping SQL Server (or another relational database) as the source of truth.

---

## Architecture

```
Client
   │
   ▼
ASP.NET Core API
   │
   ├── Check Redis Cache
   │      │
   │      ├── Cache Hit ─────────────► Return cached data
   │      │
   │      └── Cache Miss
   │              │
   ▼              ▼
 Entity Framework Core
        │
        ▼
     Database
        │
        ▼
 Store result in Redis
        │
        ▼
 Return response
```

---

## Technologies

* ASP.NET Core Web API
* C#
* Entity Framework Core
* Redis
* StackExchange.Redis
* SQLite
* Dependency Injection
* Repository Pattern
* Cache-Aside Pattern

---

## Cache-Aside Workflow

1. Client requests data.
2. API checks Redis.
3. If the data exists:

   * Return cached data immediately.
4. If the data does not exist:

   * Query the database.
   * Store the result in Redis.
   * Return the data to the client.

This approach minimizes database queries while keeping the implementation simple and maintainable.

---

## Project Structure

```
RedisCacheAsideTest
│
├── Controllers/
├── Services/
├── Repositories/
├── Models/
├── Data/
├── Caching/
├── appsettings.json
└── Program.cs
```

---

## Running the Project

### 1. Clone the repository

```bash
git clone https://github.com/LuizErnica/RedisCacheAsideTest.git
```

### 2. Navigate to the project

```bash
cd RedisCacheAsideTest
```

### 3. Configure Redis

Make sure Redis is running locally.

Default configuration:

```json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

If you're using Docker:

```bash
docker run -d -p 6379:6379 redis
```

---

### 4. Configure the Database

Update the connection string in:

```json
appsettings.json
```

Run the EF Core migrations if necessary.

```bash
dotnet ef database update
```

---

### 5. Run the application

```bash
dotnet run
```

or

```bash
dotnet watch
```

---

## Testing the Cache

### First Request

```
GET /api/products/promotions/{id}
```

Result:

* Cache miss
* Reads from database
* Saves to Redis

### Second Request

```
GET /api/products/promotions/{id}
```

Result:

* Cache hit
* Returns data directly from Redis

The second request should be significantly faster.

---

## Benefits of Cache-Aside

* Reduced database load
* Faster response times
* Easy to implement
* Cache remains optional
* Works well for read-heavy applications
* Scales efficiently

---

## Possible Improvements

* Distributed cache support
* Generic cache service
* Metrics with Prometheus/Grafana
* Background cache warming

---

## Learning Objectives

This project demonstrates:

* Using Redis with ASP.NET Core
* Implementing the Cache-Aside pattern
* Entity Framework Core integration
* Dependency Injection
* Clean service architecture
* Improving application performance through caching

---

## References

* Microsoft Documentation – ASP.NET Core
* Microsoft Documentation – Entity Framework Core
* Redis Documentation
* StackExchange.Redis

---

## License

This project is available under the MIT License.

---

## Author

**Luiz Henrique Érnica**

GitHub: https://github.com/LuizErnica

