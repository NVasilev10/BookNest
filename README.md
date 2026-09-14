# BookNest

BookNest is an ASP.NET Core MVC web application for managing a personal book collection.

The application allows users to add, edit, view, search and delete books, as well as manage authors and categories. Users can also mark books as favorites and filter the book collection.

## Main Features

- View all books in the collection
- Search books by title
- Filter books by author
- Filter books by category
- Filter only favorite books
- Add new books
- Edit existing books
- View book details
- Delete books
- Mark and unmark books as favorites
- Manage authors
- Manage categories
- Display statistics on the home page
- Server-side validation
- Client-side validation
- Responsive user interface using Bootstrap

## Technologies

- ASP.NET Core MVC
- .NET
- Entity Framework Core
- SQL Server
- Razor Views
- Bootstrap
- HTML5
- CSS
- C#

## Project Structure

The project follows the MVC architecture.

```text
BookNest
│
├── Controllers
│   ├── HomeController.cs
│   ├── BooksController.cs
│   ├── AuthorsController.cs
│   └── CategoriesController.cs
│
├── Data
│   └── ApplicationDbContext.cs
│
├── Migrations
│
├── Models
│   ├── Book.cs
│   ├── Author.cs
│   ├── Category.cs
│   └── ErrorViewModel.cs
│
├── Views
│   ├── Home
│   ├── Books
│   ├── Authors
│   ├── Categories
│   └── Shared
│
├── wwwroot
│   ├── css
│   ├── js
│   └── lib
│
├── Program.cs
├── appsettings.json
└── BookNest.csproj
```

## Database

BookNest uses Entity Framework Core with SQL Server.

The main entities are:

- Book
- Author
- Category

A book belongs to an author and a category.

The project contains Entity Framework Core migrations that create and update the database schema.

## Validation

The application uses ASP.NET Core model validation.

Examples include:

- Book title is required
- Book description has a maximum length
- Published year must be within the allowed range
- Image URL must be a valid URL when provided

Validation is performed both on the server side and through client-side validation in the Razor views.

## Getting Started

### Requirements

Before running the project, make sure you have:

- .NET SDK installed
- SQL Server or SQL Server LocalDB
- Visual Studio or another compatible IDE

### 1. Clone the repository

```bash
git clone https://github.com/NVasilev10/BookNest.git
```

Open the project folder:

```bash
cd BookNest
```

### 2. Restore dependencies

Run:

```bash
dotnet restore
```

### 3. Configure the database connection

The database connection is configured in:

```text
appsettings.json
```

Check the `ConnectionStrings` section and make sure the configured SQL Server instance is available on your machine.

If necessary, update the connection string according to your local SQL Server or SQL Server LocalDB installation.

### 4. Apply Entity Framework migrations

Run:

```bash
dotnet ef database update
```

This will create or update the database using the migrations included in the project.

If the `dotnet ef` command is not installed, install it with:

```bash
dotnet tool install --global dotnet-ef
```

Then run:

```bash
dotnet ef database update
```

### 5. Run the application

Start the application with:

```bash
dotnet run
```

Alternatively, the project can be started directly from Visual Studio.

## Configuration and Credentials

The project is intended to run with a local SQL Server or SQL Server LocalDB configuration.

The database connection string is stored in `appsettings.json`.

If a different SQL Server instance is used, update the connection string before running the Entity Framework migrations.

No external API keys or additional application credentials are required for the main functionality of the project.

## Navigation

The application provides navigation between the main sections:

- Home
- Books
- Authors
- Categories

The CRUD operations for the main entities can be accessed through the corresponding pages.

## Books

The Books section is the main part of the application.

Users can:

- See all books
- Search by title
- Filter by author
- Filter by category
- Show only favorite books
- Add a book
- Edit a book
- View book details
- Delete a book
- Mark a book as favorite

## Authors

The Authors section allows users to manage authors.

Available operations include:

- View authors
- Add an author
- Edit an author
- View author details
- Delete an author

## Categories

The Categories section allows users to manage book categories.

Available operations include:

- View categories
- Add a category
- Edit a category
- View category details
- Delete a category

## Home Page

The home page provides an overview of the collection, including statistics such as:

- Total number of books
- Number of favorite books
- Number of authors
- Number of categories

## GitHub Repository

The source code is available here:

https://github.com/NVasilev10/BookNest

## License

This project was created for educational purposes as part of an ASP.NET Core MVC project assignment.
