# MovieVault

A small ASP.NET Core 8 Web API backed by MongoDB via EF Core's MongoDB provider, with JWT-based
authentication (access token + rotating refresh token) in front of a CRUD movies resource.

This project started as a hands-on test of JWT authentication — getting the access/refresh
token flow, password hashing, and claims-based authorization right. The movies CRUD resource was
added afterward as a concrete, protected domain to exercise that auth layer against, which is why
the auth code is the more deliberately designed half of the project.

## Architecture

```
Controllers          -> HTTP boundary only: model binding, [Authorize]/[AllowAnonymous],
                         mapping a result to a status code. No business logic.
Handler / JWT         -> business logic. IMoviesHandler / IAuthenticationService are the
                         seams the controllers depend on, so the logic underneath is
                         swappable and unit-testable without spinning up Mongo.
Context (MongoDbContext) -> the only place that talks to MongoDB, via EF Core's provider.
Models                -> DTOs / entities, shared across the layers above.
```

Errors are not caught locally in the handler/service layer. They're left to bubble up to a
single global exception handler in `Program.cs`, which logs the exception and returns a
consistent JSON error. That keeps the business-logic methods focused on the happy path and the
failure path in exactly one place, instead of a `try/catch { Console.WriteLine }` repeated in
every method.

## Auth design

- **Access token**: short-lived signed JWT (`Jwt:TokenLifeSpan` minutes), returned as the
  `Token` response header on login/account-creation/refresh, sent back as
  `Authorization: Bearer <token>` on every `[Authorize]` request.
- **Refresh token**: long-lived opaque random value (`Jwt:RefreshTokenLifeSpan` minutes),
  stored against the account and returned as the `RefreshToken` header. It is *not* a JWT and
  carries no claims — it exists only to be exchanged for a new access token via
  `POST /api/Account/RefreshToken`, and is rotated (replaced) on every use.
- Passwords are hashed with BCrypt (`BCrypt.Net-Next`) before they're stored; login verifies
  with `BCrypt.Verify`, nothing is ever compared as plain text.

## Running locally

1. A MongoDB instance reachable at the connection string in `appsettings.json`
   (`mongodb://localhost:27017` by default).
2. The JWT signing key is **not** committed to `appsettings.json` — set it once via
   [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

   ```
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "<a long random string>"
   ```

   In any environment other than local Development, supply it as an environment variable
   (`Jwt__Key`) or from a proper secret store instead. The app throws a clear startup error if
   the key is missing, rather than failing later with a confusing null-reference exception.
3. `dotnet run`, then open `/swagger` for the interactive API docs, or use `MovieVault.http`
   for ready-made sample requests.
