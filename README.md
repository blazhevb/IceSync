# IceSync

IceSync is a web application designed to monitor workflows and synchronize data for The Ice Cream Company using the Universal Loader API. The application connects to the Universal Loader API using JWT Authentication and provides both a user interface and background synchronization functionality.

## Features

- **Workflow Monitoring**: Displays a list of workflows with details including Workflow Id, Workflow Name, Is Active, and Multi Exec Behavior.
- **Manual Workflow Execution**: Allows users to manually run a workflow via a button in the UI.
- **Automatic Synchronization**: Periodically synchronizes workflow data between the Universal Loader API and a SQL Server database every 30 minutes.
- **Database Management**: Inserts new workflows, updates existing ones, and deletes workflows not returned by the API.

## Prerequisites

- .NET 7 or .NET 8
- SQL Server
- Node.js (LTS version recommended)
- npm (comes with Node.js)
- Universal Loader API credentials

## Getting Started

### Clone the Repository

```sh
git clone https://github.com/blazhevb/IceSync.git
```
## Configuration

### Universal Loader API
This application uses the Universal Loader API for managing workflows.

### API Credentials

Store the following API credentials in the user secrets:

1. Open your command line and navigate to the Server project directory.

2. Run the following command to initialize user secrets if you haven't already:

    ```sh
    dotnet user-secrets init
    ```

3. Add your API credentials to the user secrets:

    ```sh
    dotnet user-secrets set "CredentialsOptions:CompanyID" "{companyID}"
    dotnet user-secrets set "CredentialsOptions:UserID" "{userID}"
    dotnet user-secrets set "CredentialsOptions:UserSecret" "{userSecret}"
    ```

### Database Connection

The database connection string can optionally be edited in `appsettings.json` with your SQL Server connection string:

```sh
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IceSyncDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```
### Run the Application

Start the application:

```sh
dotnet run
```
### Client Setup

1. **Navigate to the client directory**:

    ```sh
    cd IceSync/icesync.client
    ```

2. **Install dependencies**:

    ```sh
    npm install
    ```

3. **Run the development server**:

    ```sh
    npm run dev
    ```
## User Interface

### Workflows Page

The application includes a page where users can view a list of workflows. Each workflow displays the following details:

- **Workflow Id**
- **Workflow Name**
- **Is Active**
- **Multi Exec Behavior**

Users can manually run a workflow by clicking the "Run" button next to each workflow. After running, the application will display whether the call was successful or not.

### Background Synchronization

IceSync automatically synchronizes workflow data every 30 minutes. The synchronization process performs the following actions:

- Inserts new workflows that are returned from the API but not present in the database.
- Deletes workflows from the database that are not returned by the API.
- Updates existing workflows with the latest data from the API.

### Logging

All logs are stored in the `Logs` directory. These logs include information about API calls, workflow executions, synchronization activities, and any errors encountered.

## Built With

- **.NET** - Web framework
- **Entity Framework Core** - ORM for database operations
- **Serilog** - Simple .NET logging with fully-structured events
- **React** - JavaScript library for building user interfaces
- **Tailwind CSS** - Utility-first CSS framework for styling
- **Vite** - Next generation frontend tooling for building web applications

## Authors

- **Borislav Blazhev** - Initial work - [blazhevb](https://github.com/blazhevb)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

