# ShortUrl

ShortUrl is a .NET 8 URL-shortening product for creating, sharing, and understanding lightweight links. It supports generated or custom codes, optional expiration, redirect tracking, and a same-origin web UI. MongoDB stores the short-link records.

## Technology

- ASP.NET Core 8 minimal APIs
- Carter endpoint modules
- MongoDB
- React and Vite frontend
- C#

## Run locally

Prerequisites:

- .NET 8 SDK
- MongoDB running on `mongodb://localhost:27017`

Start the API from the repository root:

```bash
dotnet restore
dotnet run --project ShortUrl/ShortUrl.csproj
```

Open `http://localhost:5179` for the built-in web UI. It lets you create, copy, open, and revisit recent short links from the same device.

Swagger is available in Development at `https://localhost:7245/swagger` or `http://localhost:5179/swagger`.

### Frontend development

The React source lives in `frontend`. The production build is emitted to `ShortUrl/wwwroot`, so the API serves the UI from the same origin.

```bash
cd frontend
npm install
npm run dev
```

Vite runs on `http://localhost:5173` and proxies `/api` requests to the .NET server on port `5179`. Build the UI for the built-in server with:

```bash
npm run build
```

## API

### Create a short URL

`POST /api/url`

The original `/ShortenUrl` route remains available for compatibility.

```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com/very-long-url","customCode":"launch","expirationHours":168}' \
  http://localhost:5179/api/url
```

Example response:

```json
{
  "shortUrl": "http://localhost:5179/launch",
  "shortCode": "launch",
  "destinationUrl": "https://example.com/very-long-url",
  "createdOn": "2026-08-08T12:00:00Z",
  "expiresAt": "2026-08-15T12:00:00Z",
  "clickCount": 0
}
```

`customCode` is optional and accepts 4–32 letters, numbers, hyphens, or underscores. `expirationHours` is optional and can be between 1 hour and 30 days. Only absolute `http` and `https` URLs are accepted.

### Resolve a short URL

`GET /{shortCode}` redirects to the stored destination URL.

```bash
curl -L http://localhost:5179/Ab3xYz
```

Unknown short codes return `404 Not Found`; expired links return `410 Gone`. Successful redirects increment the link's click count.

### View link stats

`GET /api/url/{shortCode}/stats` returns the destination, expiry, click count, and last access time. This endpoint is intentionally simple for the no-login MVP; it should be protected once links belong to users or teams.

## Configuration

Database and collection settings are in `ShortUrl/appsettings.json`. The generated link base URL is configured through `UrlSettings:BaseUrl` and should be overridden for deployed environments.

CORS is disabled by default. Set `Cors:AllowedOrigins` to an array of trusted frontend origins when browser clients need access.

## Future improvements

- Add user accounts, link ownership, workspaces, and team sharing.
- Add rate limiting, abuse detection, CAPTCHA, and an admin moderation view.
- Add richer analytics: referrer, device, country, time series, and export.
- Add QR codes, branded domains, bulk creation, and CSV import/export.
- Add automatic cleanup for expired links and custom-domain support.
- Add integration tests using a disposable MongoDB instance.
- Add health checks and structured logging for production deployments.
