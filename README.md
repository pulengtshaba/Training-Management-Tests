# Training Management System Web API

# Training Management API

[![Build](https://github.com/pulengtshaba/Training-Management-Solution/actions/workflows/dotnet.yml/badge.svg)](https://github.com/pulengtshaba/Training-Management-Solution/actions/workflows/dotnet.yml)

[![codecov](https://codecov.io/gh/pulengtshaba/Training-Management-Solution/branch/main/graph/badge.svg)](https://codecov.io/gh/pulengtshaba/Training-Management-Solution)

ASP.NET Core Web API for managing training programs, employees,
events, participants, authentication and authorization.
## Overview

The Training Management System (TMS) is a monolithic ASP.NET Core Web API designed to manage employee training across multiple branches within an organization.

The system provides a centralized platform for managing employees, trainers, training programs, venues, events, attendance, notifications, and reporting.

It is designed using enterprise software engineering principles including Clean Architecture, CQRS, Dependency Injection, Repository Pattern, SOLID principles, and RESTful API design.

---

# Project Objectives

The primary objectives of this project are to:

* Centralize employee training management
* Simplify course scheduling and attendance tracking
* Improve communication through automated notifications
* Generate management reports and dashboards
* Demonstrate production-ready ASP.NET Core development practices

---

# Key Features

### Authentication & Authorization

* JWT Authentication
* Refresh Tokens
* Role-Based Authorization
* Claims-Based Access Control

### User Management

* Employee Management
* Trainer Management
* Manager Management
* Administrator Management

### Training Management

* Create Training Programs
* Schedule Training Sessions
* Assign Trainers
* Manage Venues
* Register Participants

### Attendance

* Check-in Employees
* QR Code Support (Future)
* Attendance Status
* Attendance Reports

### Notifications

* Email Notifications
* In-App Notifications
* Background Processing

### Reporting

* Employee Training History
* Attendance Reports
* Branch Statistics
* Course Statistics
* Dashboard KPIs

---

# Technology Stack

## Backend

* ASP.NET Core Web API
* C#
* Entity Framework Core

## Database

* Microsoft SQL Server

## Authentication

* JWT Bearer Authentication

## Documentation

* Swagger / OpenAPI

## Testing

* xUnit
* Moq
* FluentAssertions

## Containerization

* Docker
* Docker Compose

## Caching

* Redis (Planned)

## Messaging

* RabbitMQ (Planned)

---

# Architecture

This project follows a Monolithic Clean Architecture.

```text
Clients
    │
    ▼
ASP.NET Core API
    │
Application Layer
    │
Domain Layer
    │
Infrastructure Layer
    │
SQL Server
```

The architecture separates business rules from infrastructure concerns, making the application easier to maintain, test, and extend.

---

# User Roles

The system supports the following roles:

* Administrator
* Manager
* Trainer
* Employee

Each role has different permissions enforced through Role-Based Authorization.

---

# Project Structure

```text
src/

    TrainingManagement.API/

    TrainingManagement.Application/

    TrainingManagement.Domain/

    TrainingManagement.Infrastructure/

tests/

    UnitTests/

    IntegrationTests/
```

---

# Core Modules

* Authentication
* Users
* Branches
* Training Programs
* Training Sessions
* Venues
* Registrations
* Attendance
* Notifications
* Reporting

---

# API Documentation

Interactive API documentation is available through Swagger when the application is running.

---

# Getting Started

## Prerequisites

Install:

* .NET SDK
* SQL Server
* Visual Studio 2022 or later
* Git

---

## Clone Repository

```bash
git clone https://github.com/pulengtshaba/Training-Management-System.git
```

---

## Restore Packages

```bash
dotnet restore
```

---

## Update Database

```bash
dotnet ef database update
```

---

## Run the Application

```bash
dotnet run
```

---

# Running Tests

```bash
dotnet test
```

---

# Future Enhancements

* Redis Distributed Cache
* RabbitMQ Event Processing
* OAuth 2.0 / OpenID Connect
* Azure Deployment
* File Uploads
* Certificate Generation
* Multi-tenancy
* Mobile API Support

---

# Documentation

Additional documentation is available in the `/docs` directory:

* System Overview
* Software Architecture
* Database Design
* API Documentation
* Security Guide
* Deployment Guide
* Testing Guide
* Contributing Guide

---

# Development Principles

This project follows:

* SOLID Principles
* Clean Architecture
* CQRS
* Repository Pattern
* RESTful API Design
* Dependency Injection
* Domain-Driven Design (selected concepts)
* Test-Driven Development principles where appropriate

---

# License

This project is provided for educational and portfolio purposes.

---

# Author

Puleng Tshaba

BSc Computer Science

Backend Software Developer

South Africa
"# Training-Management-Tests" 
