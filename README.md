# Project Overview
This project is a first release of a web application that can be used to 
upload tournament results from your Golf Genius Leaderboard. It was written 
for use by the Valley Golfers Association in Phoenix, Arizona, USA. 

The process is to score the tournament using Golf Genius, then export the 
results to an Excel spreadsheet. This requires the use of specific Tournament definitions
in your Golf Genius system. You upload the resulys into a MongoDB database. 
From there, the results can be displayed in various ways, payouts calculated
and statistics accumulated for the year. Changes to the tournament formats require
modifications to this program. If you are inclined to use it, you should 
fork this project and modify it to suit your needs.

It is built using the latest .NET 8.0 technologies, including Blazor WebAssembly, Blazor Server, and ASP.NET Core. The application is designed to be modular, with separate projects for the server-side logic, client-side interface, shared resources, and results/statistics pages.

This repository contains multiple projects that together form a 
comprehensive application. Below is a description of each project and 
its role within the application.

## VgaUI.Server

The `VgaUI.Server` project is an ASP.NET Core Web project that serves as the backend for the application. It includes the following key features:

- **Target Framework**: .NET 8.0
- **Nullable Reference Types**: Enabled
- **AOT Compilation**: Enabled
- **Invariant Globalization**: Enabled
- **Docker Support**: Configured for Linux
- **Authentication**: Uses Microsoft Authentication Library (MSAL) for WebAssembly
- **Database**: Connects to MongoDB for data storage
- **Swagger**: Integrated for API documentation

### Key Packages

- `Microsoft.AspNetCore.Components.WebAssembly.Server`
- `Microsoft.VisualStudio.Azure.Containers.Tools.Targets`
- `Microsoft.VisualStudio.Web.CodeGeneration.Design`
- `MongoDB.Bson`
- `MongoDB.Driver`
- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Authentication.OpenIdConnect`
- `Microsoft.Identity.Web`
3. Access the application in your browser at `http://localhost:5000`.

## Contributing

We are not open to contributions at this time, although we will open once this is stable! Please message with any issues you find and I will try to address them as time allows.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Conclusion

Each project in this repository plays a crucial role in the overall application, providing backend services, frontend interfaces, shared resources, and results/statistics pages. Together, they form a cohesive and functional application built on the latest .NET technologies.
- `Microsoft.Identity.Web.UI`

### Configuration

The `appsettings.json` file includes configurations for Azure AD, MongoDB, logging, and Kestrel server settings.

## VgaUI.Client

The `VgaUI.Client` project is a Blazor WebAssembly project that serves as the frontend for the application. It includes the following key features:

- **Target Framework**: .NET 8.0
- **Nullable Reference Types**: Enabled
- **Service Worker**: Configured for offline support

### Key Packages

- `Microsoft.AspNetCore.Components.WebAssembly`
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer`
- `Microsoft.Authentication.WebAssembly.Msal`
- `Microsoft.Extensions.Http`
- `Microsoft.Extensions.Logging.Console`

### Project References

- `ExcelInterface`
- `VgaUI.Shared`
- `VGARazorLib`

## VGAResults

The `VGAResults` project is a Blazor Server project that provides the results 
and statistics pages for the application. It includes the following key 
features:

- **Target Framework**: .NET 8.0
- **Nullable Reference Types**: Enabled
- **Docker Support**: Configured for Linux
- **Designed for embedding** in an iFrame
- **Allows user to sort** various tables

### Key Packages

- `Microsoft.AspNetCore.Components`
- `Microsoft.AspNetCore.Components.QuickGrid`
- `Microsoft.AspNetCore.Components.Web`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Hosting`
- `MongoDB.Driver`
- `Quick.AspNetCore.Components.Web.Extensions`

### Project References

- `VgaUI.Shared`
- `VGARazorLib`

## VgaUI.Shared

The `VgaUI.Shared` project contains shared code and resources used by other 
projects in the application. It showcases the ability to use common Blazor
components in multiple projects and pages. 
It includes the following key features:

- **Target Framework**: .NET 8.0
- **Nullable Reference Types**: Enabled
- **Reusable Blazor Controls**
- **MongoDB Data Access** reusable routines used to avoid using 
    data access methods inside Api and UI controllers.
- **Helper class** for UI content formatting
- **Data and View Model Classes** used by multiple projects

### Key Packages

- `Microsoft.Extensions.Options`
- `MongoDB.Bson`
- `MongoDB.Driver`

### Supported Platform

- `browser`

## Conclusion

Each project in this repository plays a crucial role in the overall application, providing backend services, frontend interfaces, shared resources, and results/statistics pages. Together, they form a cohesive and functional application built on the latest .NET technologies.
