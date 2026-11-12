# BookNest - Personal Book Collection Management System

BookNest is a professional ASP.NET Core web application for managing a personal book collection. It allows users to organize, search, rate, and track their reading progress with a comprehensive set of features.

## Project Overview

BookNest implements advanced ASP.NET Core concepts and follows best practices for web application development. The application is built with .NET 10, uses Entity Framework Core for data access, and incorporates ASP.NET Identity for user management.

**Course:** ASP.NET Advanced - SoftUni October 2026
**Technology Stack:** ASP.NET Core .NET 10, Entity Framework Core, SQL Server, Bootstrap 5

## Features

### Core Features
- **Book Management:** Add, edit, view, delete, and search books
- **Author Management:** Manage book authors with detailed profiles
- **Category Management:** Organize books into categories
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
- **User Registration & Authentication:** Secure registration and login
- **User Roles:** User and Administrator roles
- **Reading Progress:** Track reading progress for each book
- **Personal Statistics:** View reading statistics and book ratings

### Admin Features
- **Admin Dashboard:** Overview of system statistics
- **User Management:** Manage users, assign roles, and delete accounts
- **Role Management:** Assign and revoke roles

## Architecture

### Project Structure
```
BookNest/
├── Controllers/           # MVC Controllers (Home, Books, Authors, Categories, Account, Error)
├── Areas/Admin/           # Admin area with administration features
│   ├── Controllers/       # Admin controllers (Dashboard, Users)
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
- **Validation:** Data annotations and fluentvalidation

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 / JetBrains Rider
- SQL Server

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

3. **Apply Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Open browser: `https://localhost:7281`

### Default Admin Account
- **Username:** admin
- **Password:** Admin@123

## API Endpoints

### Public Endpoints
- `GET /` - Home page
- `GET /Books` - Book list with search/filter
- `GET /Books/{id}` - Book details
- `GET /Authors` - Author list
- `GET /Categories` - Category list
- `GET /Account/Register` - User registration
- `GET /Account/Login` - User login

### Authenticated Endpoints
- `POST /Books/Create` - Add new book
- `POST /Books/Edit/{id}` - Edit book
- `POST /Books/Delete/{id}` - Delete book
- `POST /Books/AddToFavorites/{id}` - Mark as favorite

### Admin Endpoints
- `GET /Admin/Dashboard` - Admin dashboard
- `GET /Admin/Users` - User management
- `POST /Admin/Users/UpdateRole` - Update user role

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

- **CSRF Protection:** AntiForgeryToken on all forms
- **XSS Protection:** HTML encoding in views
- **SQL Injection Prevention:** Parameterized queries via EF Core
- **Password Security:** Strong password requirements via Identity
- **Authorization:** Role-based access control
- **Authentication:** ASP.NET Identity system

## Error Handling

- **Custom Error Pages:**
  - 404 Not Found page
  - 500 Internal Server Error page
  - General error page
- **Exception Handling:** Global exception handler in middleware
- **Logging:** Error logging for debugging

## Testing

### Unit Tests
The application includes comprehensive unit tests covering:
- Book repository operations (Create, Read, Update, Delete, Search)
- Author repository operations
- Category repository operations
- Review and rating functionality
- Search and filter functionality

### Test Coverage
- **Target Coverage:** 65%+ of business logic
- **Testing Framework:** xUnit
- **Mocking Framework:** Moq
- **Test Database:** In-memory SQL Server for testing

### Running Tests
```bash
dotnet test
```

## Seeding

The application automatically seeds the database with:
- 5 Sample Authors
- 6 Book Categories
- Sample books for testing (when needed)
- Required Identity roles (User, Administrator)

Seeding occurs on application startup if the database is empty.

## Performance Considerations

- **Pagination:** Limits data retrieval per page
- **Lazy Loading Prevention:** Explicit includes in queries
- **Indexing:** Database indexes on frequently searched columns
- **Caching:** Static asset caching with browser caching

## Future Enhancements

- Reading progress bar for books
- Recommendation system based on ratings
- Book review moderation
- Export/import collection (CSV, JSON)
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

**Last Updated:** December 2026
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

The CRUD operations for the main entities can be accessed through the corresponding pages.

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
- Add a book
- Edit a book
- View book details with reviews and average rating
- Delete a book
- Mark a book as favorite
- View author profiles

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
