# English Learning Platform

An English learning web application for students from **Grade 1 to Grade 12**.

The platform focuses mainly on **vocabulary learning**, while also supporting quizzes, exercises, dictionary lookup, user accounts, learning progress, and future features such as grammar, reading, listening, speaking, and AI-assisted learning.

The project is developed with **C# and ASP.NET Core** using **Visual Studio Code** and a **local MySQL database**.

---

## 📌 Project Overview

The English Learning Platform is designed to help students learn English according to their school grade.

Students can:

- Choose a grade from Grade 1 to Grade 12
- Learn vocabulary by official learning topics
- View Vietnamese meanings
- View UK and US phonetic transcriptions
- Listen to UK and US pronunciation
- View example sentences
- Practise vocabulary through exercises
- Complete quizzes
- Search words using the dictionary
- Save learning progress
- Review previously learned vocabulary
- Track quiz scores and learning history

The system is designed so that new modules can be added later without rebuilding the entire project.

Possible future modules:

- Grammar
- Reading
- Listening
- Writing
- Speaking
- AI Tutor
- Spaced Repetition
- Gamification
- Teacher Dashboard
- Parent Dashboard

---

## 🎯 Project Objectives

The main objectives are:

- Provide structured English learning content from Grade 1 to Grade 12
- Organise vocabulary by grade and topic
- Help students improve vocabulary retention
- Provide meanings, phonetics, pronunciation, and examples
- Allow students to practise through exercises and quizzes
- Allow registered users to save their learning progress
- Build a practical full-stack web application using ASP.NET Core
- Practise database design and Entity Framework Core
- Build a scalable architecture that can support future features
- Develop the project completely in Visual Studio Code

---

# 🏫 Grade Structure

The system contains three main education levels.

## Primary School

- Grade 1
- Grade 2
- Grade 3
- Grade 4
- Grade 5

## Secondary School

- Grade 6
- Grade 7
- Grade 8
- Grade 9

## High School

- Grade 10
- Grade 11
- Grade 12

Each grade contains different English learning topics.

Example:

```text
Grade 6
│
├── School
├── My Home
├── Friends
├── Food
├── Sports
└── Environment
```

Each topic contains vocabulary and learning activities.

> Topic names for school content should come from the selected official textbook sources.  
> Duplicate or highly similar topics can be merged while keeping their original source information.

---

# 📚 Learning Structure

Main learning flow:

```text
Grade
  ↓
Topic
  ↓
Vocabulary
  ↓
Practice
  ↓
Quiz
  ↓
Review
```

Future structure:

```text
Grade
  ↓
Topic
  ↓
├── Vocabulary
├── Grammar
├── Reading
├── Listening
├── Writing
├── Speaking
└── Quiz
```

---

# 🔤 Vocabulary Module

Vocabulary is the core feature of the platform.

Each vocabulary item may contain:

- Word
- Vietnamese meaning
- Part of speech
- UK phonetic transcription
- US phonetic transcription
- UK pronunciation audio URL
- US pronunciation audio URL
- CEFR level
- Example sentence
- Vietnamese translation
- Related words
- Word family
- Grade
- Topic
- Source / textbook
- Unit

Example:

```text
Word: environment
Part of Speech: noun
CEFR: B1

Vietnamese Meaning:
Môi trường

UK:
UK /ɪnˈvaɪ.rən.mənt/

US:
US /ɪnˈvaɪ.rən.mənt/

Example:
We need to protect the environment.

Translation:
Chúng ta cần bảo vệ môi trường.
```

---

# 📖 Dictionary

The platform provides a dictionary that allows users to search for English words.

Dictionary results may contain:

- Word
- Meaning
- Part of speech
- UK pronunciation
- US pronunciation
- Phonetic transcription
- CEFR level
- Example sentences
- Related words

The dictionary is independent of the Grade 1–12 lesson vocabulary.

This means users can search for words that are not included in school lessons.

Example:

```text
Search:
sustainability

Result:

sustainability
noun
CEFR: B2

Meaning:
Tính bền vững

Example:
Sustainability is becoming increasingly important in modern society.
```

The dictionary may use an external API in later versions.

---

# 📝 Exercises

Students can practise vocabulary using different exercise types.

Planned exercise types:

### Multiple Choice

Choose the correct meaning of a word.

### Matching

Match vocabulary with the correct meaning.

### Fill in the Blank

Complete a sentence using the correct vocabulary.

### Listening

Listen to a pronunciation and choose or type the correct word.

### Spelling

Listen to a word and type its spelling.

### Word Ordering

Arrange words into the correct sentence.

### Flashcards

Review vocabulary quickly using flashcards.

---

# 🧠 Quiz System

Each topic can contain one or more quizzes.

The quiz system may provide:

- Multiple-choice questions
- Vocabulary questions
- Listening questions
- Sentence completion
- Quiz score
- Correct answers
- Incorrect answers
- Quiz history

Example:

```text
Topic: Environment

Questions: 10
Correct: 8
Incorrect: 2

Score: 80%
```

---

# 📊 Learning Progress

Registered users can track their learning progress.

The system may store:

- Learned vocabulary
- Unlearned vocabulary
- Vocabulary that needs review
- Completed topics
- Quiz scores
- Quiz history
- Learning streak
- Study history

Example:

```text
Learning Progress

Words Learned: 426
Topics Completed: 18
Average Quiz Score: 84%
Current Streak: 7 days
```

Topic progress example:

```text
School        90%
Family        85%
Environment   63%
Technology    42%
```

---

# 🔄 Vocabulary Review

Vocabulary can have different learning statuses:

```text
New
Learning
Learned
Needs Review
```

A future version may implement a Spaced Repetition System.

Example:

```text
Learn
  ↓
Review after 1 day
  ↓
Review after 3 days
  ↓
Review after 7 days
  ↓
Review after 30 days
```

---

# 👤 User Account

Users can create accounts to save learning data.

Main account features:

- Register
- Login
- Logout
- Forgot Password
- User Profile
- Select Current Grade
- Learning History
- Quiz History
- Vocabulary Progress

Authentication can be implemented using **ASP.NET Core Identity**.

---

# 📊 Student Dashboard

The Student Dashboard gives an overview of learning activity.

Example:

```text
Welcome back!

Current Grade: Grade 8

🔥 Study Streak
7 Days

📚 Words Learned
426

✅ Topics Completed
18

🎯 Quiz Accuracy
84%
```

Daily goal example:

```text
Today's Goal

Learn 10 words
Review 15 words
Complete 1 quiz
```

---

# 🛠️ Admin Dashboard

Administrators can manage learning content.

Admin features:

- Manage Grades
- Manage Topics
- Manage Vocabulary
- Manage Vocabulary Examples
- Manage Pronunciation Audio
- Manage Exercises
- Manage Questions
- Manage Answers
- Manage Users
- View Learning Statistics

Example:

```text
Admin Dashboard
      ↓
Grade 6
      ↓
Environment
      ↓
Vocabulary
      ↓
Add / Edit / Delete
```

---

# 🗄️ Database

The project uses a **local MySQL database**.

Database server:

```text
Host: localhost
Port: 3306
Database: EnglishLearningDb
```

The database runs directly on the developer's computer.

No cloud database is required.

No Docker database is required for the initial version.

Recommended software:

- MySQL Community Server
- MySQL Workbench

---

# 🗃️ Main Database Entities

Main entities may include:

```text
Users
Roles
Grades
Topics
Vocabulary
VocabularyExamples
VocabularyAudios
GrammarLessons
Exercises
Questions
Answers
UserVocabularyProgress
UserTopicProgress
QuizAttempts
QuizAnswers
StudyHistory
```

Main learning relationship:

```text
Grade
  │
  └── Topic
        │
        ├── Vocabulary
        ├── GrammarLesson
        └── Exercise
```

Example:

```text
Grade 6
   ↓
School
   ↓
Vocabulary
   ↓
student
teacher
classroom
subject
homework
```

---

# 🔗 Suggested Database Relationships

```text
Grade
1 ─────── * Topic

Topic
1 ─────── * Vocabulary

Vocabulary
1 ─────── * VocabularyExample

Vocabulary
1 ─────── * VocabularyAudio

Topic
1 ─────── * Exercise

Exercise
1 ─────── * Question

Question
1 ─────── * Answer

User
1 ─────── * UserVocabularyProgress

User
1 ─────── * UserTopicProgress

User
1 ─────── * QuizAttempt

QuizAttempt
1 ─────── * QuizAnswer
```

---

# 🏗️ Project Architecture

The application uses a layered architecture.

```text
EnglishLearningPlatform
│
├── EnglishLearning.Domain
│
├── EnglishLearning.Application
│
├── EnglishLearning.Infrastructure
│
└── EnglishLearning.Web
```

## Domain

Contains:

- Entities
- Enums
- Core business objects

Example:

```text
Entities/
├── Grade.cs
├── Topic.cs
├── Vocabulary.cs
├── Exercise.cs
└── UserVocabularyProgress.cs
```

## Application

Contains:

- DTOs
- Interfaces
- Services
- Validators
- Business logic

Example:

```text
Application/
├── DTOs/
├── Interfaces/
├── Services/
└── Validators/
```

## Infrastructure

Contains:

- Entity Framework Core
- MySQL configuration
- DbContext
- Repositories
- Identity
- External services

Example:

```text
Infrastructure/
├── Data/
│   └── ApplicationDbContext.cs
│
├── Repositories/
│
└── Identity/
```

## Web

Contains:

- Controllers
- Views
- ViewModels
- Static files
- Authentication
- User interface

Example:

```text
Web/
├── Controllers/
├── Views/
├── ViewModels/
└── wwwroot/
```

---

# 💻 Technology Stack

## Backend

- C#
- .NET
- ASP.NET Core
- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity

## Frontend

- HTML5
- CSS3
- JavaScript
- Bootstrap

## Database

- MySQL Local

## ORM / Database Provider

Recommended:

```text
Entity Framework Core
Pomelo.EntityFrameworkCore.MySql
```

## Development Tools

- Visual Studio Code
- .NET SDK
- Git
- GitHub
- MySQL Community Server
- MySQL Workbench

---

# ⚙️ Development Environment

## 1. Install .NET SDK

Install the current supported .NET SDK.

Check installation:

```bash
dotnet --version
```

---

## 2. Install Visual Studio Code

Recommended VS Code extensions:

- C#
- C# Dev Kit
- IntelliCode
- GitLens
- REST Client (optional)
- MySQL extension (optional)

---

## 3. Install MySQL Locally

Install:

```text
MySQL Community Server
MySQL Workbench
```

Default local configuration:

```text
Server: localhost
Port: 3306
Username: root
```

Create the database:

```sql
CREATE DATABASE EnglishLearningDb
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
```

---

# 🔐 Local Database Connection

Example `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=EnglishLearningDb;User=root;Password=YOUR_PASSWORD;"
  }
}
```

Replace:

```text
YOUR_PASSWORD
```

with the password of your local MySQL account.

> Do not commit real database passwords to GitHub.

For development, credentials can later be moved to .NET User Secrets.

---

# 📦 Required NuGet Packages

From the project folder, install Entity Framework Core packages.

Example:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

For authentication:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

Install EF Core command-line tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

Check:

```bash
dotnet ef --version
```

---

# 🔌 Configure Entity Framework Core

Example `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using EnglishLearning.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

# 🧱 Entity Framework Core Migrations

Create the first migration:

```bash
dotnet ef migrations add InitialCreate
```

Create/update the local database tables:

```bash
dotnet ef database update
```

When an entity changes:

```bash
dotnet ef migrations add UpdateName
dotnet ef database update
```

Example:

```bash
dotnet ef migrations add AddVocabularyTable
dotnet ef database update
```

---

# ▶️ Running the Project in VS Code

Open the project folder:

```bash
code .
```

Restore packages:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

Or:

```bash
dotnet watch run
```

The terminal will display a local URL similar to:

```text
https://localhost:7000
```

or:

```text
http://localhost:5000
```

Open that URL in the browser.

---

# 📁 Suggested Project Structure

```text
EnglishLearningPlatform/
│
├── src/
│   │
│   ├── EnglishLearning.Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   │
│   ├── EnglishLearning.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Validators/
│   │
│   ├── EnglishLearning.Infrastructure/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   └── Identity/
│   │
│   └── EnglishLearning.Web/
│       ├── Controllers/
│       ├── Views/
│       ├── ViewModels/
│       ├── wwwroot/
│       │   ├── css/
│       │   ├── js/
│       │   └── images/
│       ├── appsettings.json
│       └── Program.cs
│
├── tests/
│
├── .gitignore
│
├── README.md
│
└── EnglishLearningPlatform.sln
```

---

# 🚀 Development Roadmap

## Phase 1 — Core System

Build the basic platform.

- Create solution and projects
- Configure layered architecture
- Connect local MySQL database
- Configure Entity Framework Core
- Create migrations
- Implement user authentication
- Manage Grades
- Manage Topics
- Manage Vocabulary

Recommended development order:

```text
Project Setup
     ↓
Local MySQL
     ↓
Entity Framework Core
     ↓
Authentication
     ↓
Grades
     ↓
Topics
     ↓
Vocabulary
```

---

## Phase 2 — Core Learning Features

- Vocabulary details
- Pronunciation
- Vocabulary examples
- Exercises
- Quiz
- Dictionary
- Learning progress
- Vocabulary review

Recommended flow:

```text
Vocabulary
    ↓
Exercise
    ↓
Quiz
    ↓
Progress
    ↓
Review
```

---

## Phase 3 — Learning Content

Add learning content gradually.

```text
Grade 1–5
     ↓
Grade 6–9
     ↓
Grade 10–12
```

Each vocabulary item should be traceable where applicable:

```text
Grade
  ↓
Topic
  ↓
Textbook Source
  ↓
Unit
  ↓
Vocabulary
```

---

## Phase 4 — Advanced Learning

Possible modules:

- Grammar lessons
- Reading exercises
- Listening exercises
- Writing activities
- Flashcards
- Spaced repetition

---

## Phase 5 — AI Features

Possible AI features:

### AI Dictionary

Explain words using language appropriate for the learner's grade.

### AI Example Generator

Generate example sentences appropriate for a student's level.

Example:

```text
Word: environment
Grade: 6
```

### AI Tutor

Students can ask questions about vocabulary or grammar.

Example:

```text
Explain the Present Perfect tense to a Grade 8 student.
```

### AI Vocabulary Recommendation

Analyse learning performance and recommend vocabulary that needs more practice.

### AI Writing Feedback

Example input:

```text
Yesterday I go to school.
```

Possible feedback:

```text
Yesterday I went to school.

"Yesterday" indicates the past, so "go" changes to "went".
```

---

## Phase 6 — Gamification

Possible features:

- Experience Points
- Study Streak
- Achievements
- Badges
- Daily Goals
- Leaderboard

---

# 🔮 Future Improvements

Possible future improvements include:

- React frontend
- TypeScript
- ASP.NET Core Web API
- Android application
- iOS application
- AI-powered learning assistant
- Personalised vocabulary recommendations
- Speech recognition
- Speaking exercises
- Teacher dashboard
- Parent dashboard
- Classroom management
- Learning analytics
- Cloud deployment

The initial project will continue using:

```text
VS Code
+
ASP.NET Core MVC
+
Entity Framework Core
+
Local MySQL
```

Cloud deployment can be added only when the local version is stable.

---

# 🔒 Security Notes

- Never commit real MySQL passwords to GitHub
- Use environment variables or .NET User Secrets when possible
- Validate all user input
- Use ASP.NET Core Identity for account management
- Hash passwords through Identity
- Protect Admin routes using roles
- Validate uploaded files
- Do not expose sensitive configuration values to the frontend

---

# 🧪 Testing

The project should include tests for important business logic.

Possible test areas:

- Grade service
- Topic service
- Vocabulary service
- Quiz scoring
- Progress calculation
- User permissions

Suggested test project:

```text
tests/
└── EnglishLearning.Tests/
```

---

# 📝 Git Workflow

Initial Git setup:

```bash
git init
git add .
git commit -m "Initial project setup"
```

Recommended `.gitignore` entries include:

```text
bin/
obj/
.vscode/
appsettings.Development.json
```

Do not upload local secrets or database credentials.

---

# 👨‍💻 Development Status

The project is currently under development.

Current implementation priority:

```text
1. Project Architecture
2. Local MySQL Database
3. Entity Framework Core
4. Authentication
5. Grades
6. Topics
7. Vocabulary
8. Exercises
9. Quiz
10. Progress Tracking
11. Dictionary
```

---

# 📄 Licence

This project is developed for educational and portfolio purposes.

Educational content, dictionary data, pronunciation audio, images, textbook data, and other third-party resources must only be used according to their respective licences and copyright policies.

---

# 👤 Author

**Nguyen Phan Ngoc Hieu**

Software Engineering Student

Focus:

- C#
- ASP.NET Core
- Full-Stack Web Development
- Database Development
- Software Engineering

---

# ⭐ Project Vision

The long-term goal is to create a complete English learning platform that supports students throughout their school journey from **Grade 1 to Grade 12**.

The platform should provide:

- Structured vocabulary learning
- Topic-based study
- Pronunciation support
- Exercises and quizzes
- Learning progress tracking
- Vocabulary review
- Personalised learning
- Future AI-assisted learning features

The first development version focuses on building a stable **ASP.NET Core + Local MySQL** application that runs completely on the developer's computer using **Visual Studio Code**.
