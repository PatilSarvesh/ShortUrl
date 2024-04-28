# ShortUrl

ShortUrl is a simple URL shortening service built with .NET and MongoDB. It allows users to convert long URLs into shorter, more manageable links. This project provides an API for creating short URLs, as well as redirecting users to the original long URLs.

## Features

- Shorten long URLs into short, easy-to-share links.
- Customizable short code generation.
- Simple API for generating and resolving short URLs.
- Integration with MongoDB for data storage.
- Secure URL generation using a random code generation algorithm.

## Technologies Used

- ASP.NET Core
- MongoDB
- C#
- Minimal APIs

## Installation

1. Clone the repository to your local machine.
   
   ```bash
   git clone https://github.com/your-username/ShortUrl.git
    ```

## Give a Star! ⭐
If you find this project helpful or interesting, please consider giving it a star on GitHub. It helps to support the project and gives recognition to the contributors.

## Usage

### Creating Short URLs

To create a short URL, send a POST request to the `/api/url` endpoint with the long URL in the request body. The API will return the generated short URL.

**Example:**

```bash
curl -X POST -H "Content-Type: application/json" -d '{"url": "https://example.com/very-long-url"}' http://localhost:5000/api/url
```
### Resolving Short URLs

To resolve a short URL and get the original long URL, send a GET request to the short URL. The API will redirect you to the original long URL.

**Example:**

```bash
curl -L http://localhost:5000/abc123
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request.

