# GameHubStore

GameHubStore is a full-stack ASP.NET Core MVC digital game store where users can browse games, add them to cart, place orders, make payments, and receive digital activation keys.

## Features

### User Features

* User registration and login
* Browse games
* View game details
* Add games to cart
* Checkout and place orders
* Razorpay payment integration
* Payment simulation for development
* View order history
* View purchased games and activation keys

### Admin Features

* Admin dashboard with live statistics
* Manage categories
* Manage platforms
* Manage games
* Upload game cover images
* Manage digital game keys
* Manage orders
* Manage payments
* View registered users

## Tech Stack

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* ASP.NET Core Identity
* Razorpay Payment Gateway
* Bootstrap
* HTML, CSS, JavaScript

## Database Tables

* AspNetUsers
* Categories
* Platforms
* Games
* GamePlatforms
* Carts
* CartItems
* Orders
* OrderItems
* Payments
* GameKeys

## Main Concepts Used

* MVC Architecture
* Authentication
* Role-Based Authorization
* Entity Framework Relationships
* One-to-Many Relationship
* Many-to-Many Relationship
* File Upload
* Payment Gateway Integration
* Admin Area
* ViewModels
* Database Seeding

## Default Login

### Admin

Email: [admin@gmail.com](mailto:admin@gmail.com)
Password: Admin@123

### User

Email: [user@gmail.com](mailto:user@gmail.com)
Password: User@123

## How to Run

1. Clone the repository
2. Update the connection string in `appsettings.json`
3. Run migrations
4. Run the project

```bash
dotnet restore
dotnet ef database update
dotnet run
```

## Payment Notes

This project uses Razorpay test mode during development. Real live payments should only be enabled after completing Razorpay KYC, deploying with HTTPS, and configuring production secrets securely.

## Security Notes

Do not commit:

* API keys
* Razorpay secrets
* User secrets
* Database passwords

Use:

* User Secrets for local development
* Azure App Settings or Key Vault for production

## Project Status

Core development completed:

* Admin panel
* Store
* Cart
* Orders
* Payments
* Game key delivery
* User library

Future improvements:

* Email invoice
* Coupon system
* Reviews and ratings
* Wishlist
* Azure deployment
* Live Razorpay mode
