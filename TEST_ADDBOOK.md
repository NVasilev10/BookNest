# Testing "Add New Book" Functionality

## Prerequisites
- Application should be running
- Database should be seeded with Authors and Categories

## Steps to Test

### 1. Navigate to Add Book Page
- Open browser to: `http://localhost:5000/Books/Create` (or your app URL)
- You should see a form with:
  - Title input field
  - Description textarea
  - Published Year input
  - Image URL input
  - **Author dropdown** (should be populated with seeded authors)
  - **Category dropdown** (should be populated with seeded categories)
  - "Add Book" submit button

### 2. Fill in the Form
- Title: "Test Book" (required, 3-150 characters)
- Description: "This is a test book" (optional, max 2000 characters)
- Published Year: 2024 (required, 1000-2100)
- Image URL: https://via.placeholder.com/300x400 (optional, must be valid URL)
- Author: Select any author from dropdown
- Category: Select any category from dropdown

### 3. Submit the Form
- Click "Add Book" button
- You should be redirected to Books Index page
- The new book should appear in the list with:
  - Correct title
  - Associated author name
  - Associated category name
  - Book cover image (if URL provided)

## Expected Database Content

### Authors (Seeded)
- Harper Lee
- J.K. Rowling
- George Orwell
- Jane Austen
- Stephen King

### Categories (Seeded)
- Fiction
- Mystery
- Fantasy
- Romance
- Science Fiction
- Horror

## Troubleshooting

### Dropdowns are empty
1. Check if database connection string in `appsettings.json` is correct
2. In Visual Studio, try: Delete `bin` and `obj` folders, rebuild
3. Database may not be initialized - seed data is added automatically on first run

### Validation errors
- Title is required and must be 3-150 characters
- Published Year must be between 1000 and 2100
- Author and Category are required fields
- Image URL (if provided) must be a valid URL format

### Form not submitting
- Ensure you've selected an Author from the dropdown
- Ensure you've selected a Category from the dropdown
- Check browser console (F12) for client-side validation errors
