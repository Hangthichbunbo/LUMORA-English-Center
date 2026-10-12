# LUMORA English Center

**LUMORA English Center** is a web-based platform designed to support English language learning, course management, and online IELTS preparation. The project aims to provide a modern, user-friendly learning environment for students, teachers, and administrators.

## 📌 1. Project Overview

- **Project Name:** LUMORA English Center
- **Project Type:** English Learning and Training Management Website
- **Domain:** Education, English Language Learning, and IELTS Preparation
- **Target Users:** Students, Teachers, and Administrators
- **Main Objective:** To digitize course information management, support course enrollment, and facilitate online learning activities through a centralized platform.

## ✨ 2. Key Features

### 2.1. Course Management
- Display available English courses.
- Provide detailed course descriptions and learning roadmaps.
- Help students search for and select suitable courses.

### 2.2. IELTS Learning and Practice
- Organize IELTS preparation content across four skills: Listening, Reading, Writing, and Speaking.
- Support exercises and practice tests.
- Provide a foundation for recording test results and tracking learning progress.

### 2.3. Teacher Management
- Display a list of teachers.
- Provide teacher profiles and professional introductions.
- Help students explore available teaching staff.

### 2.4. User Account Management
- User registration and login.
- Role-based access to relevant features.
- Access to functions according to user permissions.

### 2.5. Student Dashboard
- Manage personal information.
- View enrolled courses.
- Access learning materials and academic results.

## 🛠️ 3. Technologies

- **Programming Language:** C#
- **Framework:** ASP.NET Core
- **Frontend:** HTML, CSS, JavaScript
- **Development Environment:** Visual Studio
- **Version Control:** Git and GitHub

Database technologies and additional libraries can be documented according to the project's actual configuration.

## ⚙️ 4. Installation and Setup

### Prerequisites

- Windows
- Visual Studio with the ASP.NET and web development workload installed
- A .NET SDK compatible with the project's target framework
- Git

### Step 1: Clone the Repository

```bash
git clone https://github.com/Hangthichbunbo/LUMORA-English-Center.git
```

Navigate to the project directory:

```bash
cd LUMORA-English-Center
```

### Step 2: Open the Project

Open the `.sln` solution file in Visual Studio. If the repository does not contain a solution file in the root directory, open the appropriate `.csproj` project file instead.

### Step 3: Restore Dependencies

Open a terminal in the project directory and run:

```bash
dotnet restore
```

### Step 4: Build the Project

```bash
dotnet build
```

### Step 5: Run the Application

Press **F5** or **Ctrl + F5** in Visual Studio.

The current development launch profiles are configured with the following URLs:

- **HTTP:** `http://localhost:5223`
- **HTTPS:** `https://localhost:7025`

These URLs may differ if the launch configuration changes.

## 📁 5. Project Structure

The exact directory structure depends on the current implementation. A typical ASP.NET Core MVC project may follow this structure:

```text
LUMORA-English-Center/
├── Controllers/       # Handles HTTP requests and application flow
├── Models/            # Data models and business entities
├── Views/             # Razor views and user interfaces
├── wwwroot/           # Static assets: CSS, JavaScript, images
├── Properties/        # Application launch settings
├── appsettings.json   # Application configuration
├── Program.cs         # Application entry point
└── README.md          # Project documentation
```

## 🔐 6. Security and Configuration

- Never commit passwords, sensitive database connection strings, or API keys to GitHub.
- Exclude personal configuration files and sensitive environment files from version control.
- Provide configuration templates, such as `.env.example`, when appropriate.
- Verify all required environment settings before running the project on another machine.

## 🚀 7. Future Improvements

- Complete course management and enrollment workflows.
- Enhance the online IELTS testing module.
- Implement learning progress tracking and result reporting.
- Improve account management and role-based authorization.
- Optimize the interface for desktop, tablet, and mobile devices.
- Strengthen application security and overall user experience.

**This project is developed for educational and software development purposes. Licensing and redistribution terms are subject to the repository owner's decision.**

---

**LUMORA English Center** — Empowering learners through accessible English education.
