# Enterprise Transaction Platform

A production-oriented transaction processing API built with .NET 10, following Clean Architecture principles and designed with reliability, security, maintainability, and operational readiness in mind.

## Overview

The Enterprise Transaction Platform is a backend transaction processing API designed to demonstrate how a financial transaction service can be structured using modern .NET engineering practices.

The platform provides capabilities for submitting, retrieving, updating, and searching transactions while incorporating validation, centralized exception handling, rate limiting, health checks, strongly typed configuration, and request observability.

This project is designed as a portfolio-quality enterprise API rather than a simple CRUD application.

---

## Key Features

- Transaction submission
- Transaction retrieval by ID
- Transaction retrieval by reference
- Transaction status updates
- Transaction searching
- Pagination support
- Currency validation
- Domain-driven transaction modelling
- Centralized exception handling
- RFC-style `ProblemDetails` responses
- API rate limiting
- Liveness and readiness health checks
- Strongly typed configuration
- Startup configuration validation
- Correlation ID support
- Request duration measurement
- Structured request logging
- Comprehensive automated testing
- Production-oriented API hardening

---

## Architecture

The solution follows a Clean Architecture approach with clear separation of responsibilities.

```text
Enterprise Transaction Platform
│
├── Domain
│   ├── Entities
│   ├── Enums
│   ├── Value Objects
│   └── Exceptions
│
├── Application
│   ├── Abstractions
│   ├── Commands
│   ├── Queries
│   ├── Validators
│   └── Dependency Injection
│
├── Infrastructure
│   ├── Persistence
│   ├── Repositories
│   ├── Currencies
│   └── Dependency Injection
│
├── API
│   ├── Controllers
│   ├── Configuration
│   ├── Exceptions
│   └── Middleware
│
└── Tests
    ├── Application Tests
    ├── Infrastructure Tests
    └── API Tests
