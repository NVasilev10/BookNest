# BookNest - Personal Book Collection Management System

BookNest is a professional ASP.NET Core web application for managing a personal book collection. It allows users to organize, search, rate, and track their reading progress with a comprehensive set of features.

## Project Overview

BookNest implements advanced ASP.NET Core concepts and follows best practices for web application development. The application is built with .NET 10, uses Entity Framework Core for data access, and incorporates ASP.NET Identity for user management.

**Course:** ASP.NET Advanced - SoftUni October 2026
**Technology Stack:** ASP.NET Core .NET 10, Entity Framework Core, SQL Server, Bootstrap 5

## Features

### Core Features
- **Book Management:** Browse and search books; administrators can add, edit and delete books
- **Author Management:** Browse author profiles; administrators can manage authors
- **Category Management:** Browse categories; administrators can manage categories
- **Book Search & Filtering:**
  - Search by title, description, or author name
  - Filter by author and category
  - Sort by rating, publish date, and title
  - Favorites-only view
- **Rating System:** 1-5 star rating system with user reviews
- **Book Collections:** Create custom book collections
- **Personal Library:** Track favorite books and mark as read
- **Pagination:** Efficient data display with pagination

### User Features
- **Registration and login:** Register an account and sign in using a username and password. New registrations are assigned the `User` role.
- **Remember me:** Check the option at login to keep the session across application restarts. Without it, the session is invalidated when the application restarts.
- **Personal library:** Signed-in users can manage favorites, collections and ratings.
- **Reading progress:** Each book stores its reading status, page counts and dates. These fields are shared book data, not separate progress per user.

### Admin Features
- **Admin Dashboard:** Administrators see a link in the navigation bar and can open `/Admin` for statistics and quick actions.
- **Catalog:** Add, edit and delete books, authors and categories from their respective pages. These actions require `Administrator`.
- **Users:** Create accounts, edit names and email, reset passwords, delete accounts and manage `User` and `Administrator` roles under **Manage Users**.
- **Roles:** The dashboard also lists users and provides buttons to grant or remove administrator access. The current administrator cannot remove their own administrator role or delete their own account.
- **Reviews:** **Manage Reviews** lists reviews and supports deleting them and changing their approval status. Currently, changing approval status does **not** hide reviews from public book pages or exclude them from ratings; use deletion if a review must be removed.

## Architecture

### Project Structure
```
BookNest/
├── Controllers/           # MVC Controllers (Home, Books, Authors, Categories, Account, Error)
├── Areas/Admin/           # Admin area with administration features
│   ├── Controllers/       # Admin controllers (Dashboard, Users, Reviews)
│   └── Views/            # Admin views
├── Models/               # Entity models (Book, Author, Category, Review, etc.)
├── Views/                # Razor views (HTML/CSS UI)
├── Data/                 # Database context and seeding
├── Helpers/              # Helper classes (PaginatedList)
├── ViewModels/           # View models for forms
├── Migrations/           # EF Core database migrations
├── Tests/                # Unit tests
└── wwwroot/              # Static files (CSS, JavaScript)
```

### Database Models
- **ApplicationUser** - Custom user model extending IdentityUser
- **Book** - Main entity for books
- **Author** - Authors of books
- **Category** - Book categories
- **Review** - User reviews and ratings
- **BookCollection** - Custom user collections

### Relationships
- One Author to Many Books
- One Category to Many Books
- Many Books to Many BookCollections (Many-to-Many)
- One ApplicationUser to Many Reviews
- One ApplicationUser to Many BookCollections

## Technologies Used

- **Framework:** ASP.NET Core .NET 10
- **Database:** SQL Server + Entity Framework Core 10.0.12
- **ORM:** Entity Framework Core with migrations
- **UI Framework:** Bootstrap 5
- **Authentication:** ASP.NET Core Identity
- **Testing:** xUnit + Moq
- **Validation:** Data annotations and ASP.NET Core model validation

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- Visual Studio with .NET 10 support or another compatible IDE
- SQL Server or SQL Server LocalDB

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/NVasilev10/BookNest.git
   cd BookNest
   ```

2. **Configure Database Connection**
   - Update connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=BookNestDb;Trusted_Connection=true;"
   }
   ```

3. **Database setup**
   - The application applies its EF Core migrations when it starts. Make sure SQL Server is running and the configured account can create and update the database.
   - If startup reports an error seeding the database, check the connection and permissions. You can also apply migrations manually with `dotnet ef database update` when the `dotnet-ef` tool is installed.

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Open `https://localhost:7281` (or `http://localhost:5112`, which redirects to HTTPS). For local development, trust the .NET development HTTPS certificate if prompted.

### Create the first administrator

There is **no default administrator account or password**. Start the application once so that the database and roles are created, then register an account through `/Account/Register`. To give that account the `Administrator` role initially, connect to the configured SQL Server instance in Visual Studio's **SQL Server Object Explorer**, open a **New Query** for `BookNestDb`, and run the following SQL. Replace `your_username` with the registered username.

```sql
USE [BookNestDb];
GO
DECLARE @UserName nvarchar(256) = N'your_username';
DECLARE @UserId nvarchar(450);
DECLARE @RoleId nvarchar(450);

SELECT @UserId = Id FROM dbo.AspNetUsers WHERE NormalizedUserName = UPPER(@UserName);
SELECT @RoleId = Id FROM dbo.AspNetRoles WHERE NormalizedName = N'ADMINISTRATOR';

IF @UserId IS NULL
    THROW 50001, 'User not found.', 1;
IF @RoleId IS NULL
    THROW 50002, 'Administrator role not found. Start the application first.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
```

Sign out and sign back in to refresh the role in the authentication cookie. The account keeps its existing `User` role. After that, administrators can manage users and roles in the browser without SQL. If the query reports `Invalid object name`, make sure it runs against the correct database and that migrations completed.

## MVC Routes

### Public Endpoints
- `GET /` - Home page
- `GET /Books` - Book list with search/filter
- `GET /Books/Details/{id}` - Book details
- `GET /Authors` - Author list
- `GET /Categories` - Category list
- `GET /Account/Register` - User registration
- `GET /Account/Login` - User login

### Authenticated Routes
- `POST /Books/ToggleFavorite/{id}` - Add or remove a favorite
- `GET /Books/MyCollections` and `GET /Books/MyRatings` - Personal collections and ratings
- `POST /Books/CreateReview` - Add a review

### Administrator Routes
- `GET /Admin` or `GET /Admin/Dashboard` - Dashboard and administrator role actions
- `GET /Admin/Users` - Create, edit, delete and manage users and roles
- `GET /Admin/Reviews` - Manage review approval status and delete reviews
- `POST /Books/Create`, `/Books/Edit/{id}`, `/Books/Delete/{id}` - Modify books; equivalent administrator actions exist for authors and categories

## Data Validation

### Client-Side Validation
- HTML5 form validation
- JavaScript validation
- ASP.NET Razor validation messages

### Server-Side Validation
- Data annotations on models
- Custom validation logic
- Entity Framework constraints

## Security Features

- **CSRF protection:** State-changing administrative forms use antiforgery validation.
- **Authentication and authorization:** ASP.NET Core Identity and role-based access control.
- **Password policy:** The configured minimum is six characters; digits, mixed case and special characters are not required. Strengthen this policy before production use.
- **Sessions:** Persistent cookies are issued only when Remember me is selected. Temporary sessions are tied to the running application instance.

## Error Handling

- **Custom Error Pages:**
  - 404 Not Found page
  - 500 Internal Server Error page
  - General error page
- **Exception Handling:** Global exception handler in middleware
- **Logging:** Error logging for debugging

## Testing

### Unit Tests
The `Tests` directory contains xUnit tests for selected book, author, category and review operations using the EF Core in-memory provider. These tests do not cover the full application or administrative workflows.

### Running Tests
```bash
dotnet test
```

## Seeding

On startup, the application applies migrations and creates the `User` and `Administrator` roles if needed. If their tables are empty, it also adds five sample authors and six categories. It does not create an administrator account or sample books.

## Performance Considerations

- **Pagination:** Limits data retrieval per page
- **Lazy Loading Prevention:** Explicit includes in queries
- **Indexing:** Database indexes on frequently searched columns
- **Caching:** Static asset caching with browser caching

## Future Enhancements

- Recommendation system based on ratings
- Enforce review approval status on public pages and rating calculations
- Import collections (CSV, JSON)
- REST API for mobile clients
- SignalR real-time notifications
- Cloud deployment (Azure)

## Contributing

To contribute to BookNest:
1. Fork the repository
2. Create a feature branch
3. Commit changes
4. Push to the branch
5. Create a Pull Request

## Code Style & Conventions

- **Naming:** PascalCase for classes/methods, camelCase for variables
- **Comments:** Only for complex logic
- **Formatting:** .NET Code Analyzer compliance
- **Best Practices:** SOLID principles, DRY, KISS

## Deployment

### Prerequisites for Deployment
- .NET 10 Runtime
- SQL Server database
- Web server (IIS / App Service)

### Deployment Steps
1. Publish the application: `dotnet publish -c Release`
2. Configure connection string for production
3. Run database migrations: `dotnet ef database update`
4. Deploy to hosting platform

## Licensing

This project is created for educational purposes as part of the ASP.NET Advanced Course at SoftUni.

## Contact

For questions or issues, please open an issue on GitHub or contact the repository owner.

---

**Version:** 1.0.0
- View all user ratings (My Ratings)
- Leave comments on book reviews
- Manage book collections
- Create custom book collections
- Add/remove books from collections
- View author profiles with statistics
- Manage authors
- Manage categories
- Display statistics on the home page
- **Track reading progress** (Want to Read, Currently Reading, Finished)
- **Record pages read** and total pages for books
- **Track reading dates** (start and finish dates)
- **View reading statistics** (books reading, finished, want to read)
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

BookNest uses Entity Framework Core with SQL Server (LocalDB).

The main entities are:

- Book
- Author
- Category
- Review (for rating and comments)
- BookCollection (for organizing books into custom collections)

**Book Properties include:**
- Basic info: Title, Description, PublishedYear, ImageUrl
- Relationships: Author, Category
- Favorites: IsFavorite boolean flag
- Reading Progress Tracking:
  - Status (WantToRead, Reading, Finished)
  - TotalPages (total number of pages in the book)
  - PagesRead (number of pages read)
  - StartDate (when reading started)
  - FinishDate (when reading was completed)
  - ProgressPercentage (calculated as PagesRead / TotalPages)

Relationships:
- A Book belongs to an Author and a Category
- A Book can have multiple Reviews
- A Book can belong to multiple BookCollections (many-to-many)

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

Administrators can access catalog CRUD operations through the corresponding pages and the Admin Dashboard. Other users can browse the catalog and use personal features after signing in.

## Books

The Books section is the main part of the application.

Users can:

- See all books  
- Search by title, description, and author name
- Sort by title, rating, or publish date
- Filter by author
- Filter by category
- Show only favorite books
- Rate books (1-5 stars)
- Manage reviews with comments
- View My Ratings - all books user has rated
- Create and manage book collections
- Add/remove books from collections
- View book details with reviews and average rating
- Administrators can add, edit and delete books
- Mark a book as favorite
- View author profiles

## Authors

The Authors section allows users to browse authors; administrators can manage them.

Available operations include:

- View authors
- Add an author
- Edit an author
- View author details
- Delete an author

## Categories

The Categories section allows users to browse categories; administrators can manage them.

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
- **Books by reading status:**
  - Books user wants to read
  - Books currently being read
  - Books finished
- **Reading statistics:**
  - Total pages read across all books
- Recent books (last 6 added)
- Favorite books (latest 6)

## GitHub Repository

The source code is available here:

https://github.com/NVasilev10/BookNest

## License

This project was created for educational purposes as part of an ASP.NET Core MVC project assignment.
