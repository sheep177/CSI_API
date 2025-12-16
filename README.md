# CivicFlow – Secure Civic Issue Management API

CivicFlow is a production-style **ASP.NET Core Web API** designed for managing civic issue reports (tickets) with **secure authentication, role-based authorization, and real-world workflows**.

This project demonstrates how a modern backend system handles **user identity, permissions, and domain-driven business logic**.

---

## 🚀 Key Features

### 🔐 Authentication & Security
- JWT-based authentication
- Secure password hashing with BCrypt
- Role-based authorization (`Citizen`, `Officer`, `Admin`)
- Claims-based identity (`UserId`, `Email`, `Role`)
- Token expiration & validation

### 📧 Email & Account Management
- Email verification via one-time codes
- Forgot password / reset password flow
- SMTP-based email service abstraction

### 🎟 Ticket Management
- Citizens can create and track tickets
- Officers can view and manage assigned tickets
- Admins can assign tickets to officers
- Status & priority management
- Access control enforced at API level

### 👤 User Profile
- Authenticated `/me` endpoint
- View & update profile details
- Secure identity resolution via JWT claims

---

## 🛠 Tech Stack

- **ASP.NET Core Web API**
- **Entity Framework Core**
- **JWT Authentication**
- **BCrypt Password Hashing**
- **SQLite / SQL-based persistence**
- **SMTP Email Service**
- **Swagger / OpenAPI**

---

## 🧱 Architecture Overview
Client
│
│  JWT Bearer Token
▼
ASP.NET Core API
├── Controllers (Auth, Tickets, Users, Me)
├── Domain Models
├── Authorization (JWT + Roles)
├── Email Service
└── Entity Framework Core
▼
Database
---

## 🔐 Authorization Model

| Role     | Capabilities |
|----------|--------------|
| Citizen  | Create & view own tickets |
| Officer  | Manage assigned tickets |
| Admin    | Assign tickets, manage users |

---

## ▶️ Run Locally

```bash
dotnet restore
dotnet ef database update
dotnet run

📌 Notes
	•	Authentication is enforced via [Authorize]
	•	Role checks use [Authorize(Roles = "...")]
	•	Passwords are never stored in plaintext
	•	Designed to reflect real enterprise backend patterns

👨‍💻 Author

ZiYang Zhou
GitHub: https://github.com/sheep177