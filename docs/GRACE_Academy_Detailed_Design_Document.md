# GRACE Academy Detailed Design Document

**Document Version:** 1.0.0  
**Document Date:** January 31, 2026  
**Classification:** Technical Specification  
**Repository:** github.com/Lev0n82/AskMarilyn

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Overview](#2-system-overview)
3. [Architecture Design](#3-architecture-design)
4. [Decision Trees and Design Rationale](#4-decision-trees-and-design-rationale)
5. [Software Version Registry](#5-software-version-registry)
6. [Database Schema Documentation](#6-database-schema-documentation)
7. [API and tRPC Procedure Documentation](#7-api-and-trpc-procedure-documentation)
8. [Frontend Component Architecture](#8-frontend-component-architecture)
9. [Code Structure and Line-by-Line Explanations](#9-code-structure-and-line-by-line-explanations)
10. [Installation and Configuration Guide](#10-installation-and-configuration-guide)
11. [Environment Variables Specification](#11-environment-variables-specification)
12. [Testing Specifications](#12-testing-specifications)
13. [Accessibility Compliance](#13-accessibility-compliance)
14. [Appendices](#14-appendices)

---

## 1. Executive Summary

### 1.1 Purpose

This Detailed Design Document (DDD) provides a comprehensive technical specification for the GRACE Academy learning management system. The document is designed to enable any AI agent or human developer to replicate the entire system without variance, including all architectural decisions, code structures, and configuration requirements.

### 1.2 Scope

GRACE Academy is an enterprise-grade learning platform that delivers a structured curriculum for autonomous agentic Action-Based Testing (ABT) with human-in-the-loop capabilities. The system comprises 30 modules organized into three progressive tracks: Foundation (modules 1-10), Intermediate (modules 11-20), and Advanced (modules 21-30).

### 1.3 System Objectives

The GRACE Academy system achieves the following objectives:

| Objective | Description | Success Criteria |
|-----------|-------------|------------------|
| Structured Learning | Deliver 30 modules across three certification tracks | All modules accessible with proper sequencing |
| Interactive Assessment | Provide 5-question quizzes with instant feedback | Quiz scores recorded, 60% pass threshold enforced |
| Hands-on Challenges | Enable The Crucible challenge submissions | Text submissions stored and reviewable by admins |
| Progress Tracking | Track completion across all four module sections | Real-time dashboard with accurate progress data |
| Certification | Award track certificates and GRACE Diploma | Certificates generated with unique verification codes |
| Role-Based Access | Distinguish between student and admin users | Admin panel accessible only to admin role users |

### 1.4 Naming Conventions

The system employs specific naming conventions that must be preserved:

| Term | Definition | Context |
|------|------------|---------|
| **The Spark** | 3-minute reading section | Core concept introduction in Marilyn vos Savant's intellectual style |
| **The Gauntlet** | 5-minute quiz section | 5-question multiple choice assessment |
| **The Crucible** | 5-minute challenge section | Hands-on text submission challenge |
| **The Imprint** | 2-minute thought experiment | Visual aid and lasting retention section |
| **GRACE Diploma** | Final certification | Awarded upon completion of all 30 modules |

---

## 2. System Overview

### 2.1 High-Level Architecture

The GRACE Academy system follows a modern full-stack architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                                 │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    React 19 + TypeScript                      │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────────┐ │   │
│  │  │  Pages  │  │Components│  │ Contexts │  │  tRPC Client   │ │   │
│  │  └────┬────┘  └────┬────┘  └────┬────┘  └────────┬────────┘ │   │
│  │       │            │            │                 │          │   │
│  │       └────────────┴────────────┴─────────────────┘          │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ HTTP/WebSocket
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         SERVER LAYER                                 │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    Express 4 + tRPC 11                        │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────────────┐ │   │
│  │  │ Routers │  │   Auth  │  │   DB    │  │    Storage      │ │   │
│  │  └────┬────┘  └────┬────┘  └────┬────┘  └────────┬────────┘ │   │
│  │       │            │            │                 │          │   │
│  │       └────────────┴────────────┴─────────────────┘          │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ SQL/S3
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         DATA LAYER                                   │
│  ┌─────────────────────────┐  ┌─────────────────────────────────┐  │
│  │      MySQL/TiDB         │  │           AWS S3                 │  │
│  │  ┌─────────────────┐   │  │  ┌─────────────────────────┐    │  │
│  │  │  Users Table    │   │  │  │   File Storage          │    │  │
│  │  │  GRACE Tables   │   │  │  │   (Images, Assets)      │    │  │
│  │  │  Progress Data  │   │  │  └─────────────────────────┘    │  │
│  │  └─────────────────┘   │  │                                  │  │
│  └─────────────────────────┘  └─────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Technology Stack Summary

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| Frontend Framework | React | 19.2.1 | Component-based UI rendering |
| Type System | TypeScript | 5.9.3 | Static type checking |
| Styling | Tailwind CSS | 4.1.14 | Utility-first CSS framework |
| State Management | TanStack Query | 5.90.2 | Server state management |
| Routing | Wouter | 3.3.5 | Lightweight client-side routing |
| Backend Framework | Express | 4.21.2 | HTTP server framework |
| API Layer | tRPC | 11.6.0 | End-to-end typesafe APIs |
| ORM | Drizzle ORM | 0.44.5 | TypeScript-first ORM |
| Database | MySQL/TiDB | - | Relational data storage |
| Authentication | Manus OAuth | - | OAuth 2.0 authentication |
| Build Tool | Vite | 7.1.7 | Frontend build and dev server |
| Package Manager | pnpm | 10.4.1 | Fast, disk-efficient package manager |

### 2.3 File Structure

The project follows a structured directory layout that separates concerns and promotes maintainability:

```
AskMarilyn/
├── client/                          # Frontend application
│   ├── public/                      # Static assets
│   │   └── images/                  # Image files including ABT philosophy graphics
│   └── src/
│       ├── _core/                   # Core utilities
│       │   └── hooks/
│       │       └── useAuth.ts       # Authentication hook
│       ├── components/              # Reusable UI components
│       │   ├── ui/                  # shadcn/ui components
│       │   ├── Layout.tsx           # Main layout wrapper
│       │   └── ...                  # Other shared components
│       ├── contexts/                # React contexts
│       │   ├── ThemeContext.tsx     # Theme management
│       │   └── ProgressContext.tsx  # Learning progress context
│       ├── hooks/                   # Custom React hooks
│       ├── lib/
│       │   ├── trpc.ts              # tRPC client configuration
│       │   └── utils.ts             # Utility functions
│       ├── pages/
│       │   ├── grace-academy/       # GRACE Academy pages
│       │   │   ├── index.tsx        # Landing page
│       │   │   ├── GraceModule.tsx  # Dynamic module page
│       │   │   ├── GraceDashboard.tsx # Progress dashboard
│       │   │   └── GraceAdmin.tsx   # Admin panel
│       │   └── ...                  # Other pages
│       ├── App.tsx                  # Root component with routing
│       ├── main.tsx                 # Application entry point
│       └── index.css                # Global styles
├── server/                          # Backend application
│   ├── _core/                       # Core server utilities
│   │   ├── index.ts                 # Server entry point
│   │   ├── context.ts               # tRPC context
│   │   ├── trpc.ts                  # tRPC initialization
│   │   ├── env.ts                   # Environment configuration
│   │   ├── llm.ts                   # LLM integration
│   │   └── ...                      # Other core modules
│   ├── db.ts                        # Database query helpers
│   ├── routers.ts                   # tRPC routers and procedures
│   └── storage.ts                   # S3 storage helpers
├── drizzle/                         # Database schema and migrations
│   ├── schema.ts                    # Table definitions
│   └── migrations/                  # Generated migrations
├── shared/                          # Shared types and constants
│   └── const.ts                     # Shared constants
├── scripts/                         # Utility scripts
│   └── seed-grace-modules.mjs       # Database seeding script
├── package.json                     # Dependencies and scripts
├── tsconfig.json                    # TypeScript configuration
├── vite.config.ts                   # Vite configuration
└── drizzle.config.ts                # Drizzle configuration
```

---

## 3. Architecture Design

### 3.1 Component Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              GRACE ACADEMY SYSTEM                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                         PRESENTATION LAYER                              │ │
│  │                                                                         │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌────────────┐ │ │
│  │  │   index.tsx  │  │GraceModule.tsx│  │GraceDashboard│  │GraceAdmin  │ │ │
│  │  │  (Landing)   │  │  (Learning)  │  │  (Progress)  │  │  (Manage)  │ │ │
│  │  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └─────┬──────┘ │ │
│  │         │                 │                 │                 │        │ │
│  │         └─────────────────┴─────────────────┴─────────────────┘        │ │
│  │                                    │                                    │ │
│  │                          ┌─────────▼─────────┐                         │ │
│  │                          │   Layout.tsx      │                         │ │
│  │                          │   (Navigation)    │                         │ │
│  │                          └───────────────────┘                         │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                       │
│                                      │ tRPC Hooks                            │
│                                      ▼                                       │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                           API LAYER (tRPC)                              │ │
│  │                                                                         │ │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │ │
│  │  │                        grace Router                              │  │ │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────┐ │  │ │
│  │  │  │ modules  │  │   quiz   │  │ crucible │  │    progress      │ │  │ │
│  │  │  │ Router   │  │  Router  │  │  Router  │  │     Router       │ │  │ │
│  │  │  └──────────┘  └──────────┘  └──────────┘  └──────────────────┘ │  │ │
│  │  │  ┌──────────────────────┐  ┌────────────────────────────────┐  │  │ │
│  │  │  │   certificates       │  │           admin                 │  │  │ │
│  │  │  │      Router          │  │          Router                 │  │  │ │
│  │  │  └──────────────────────┘  └────────────────────────────────┘  │  │ │
│  │  └─────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                       │
│                                      │ Drizzle ORM                           │
│                                      ▼                                       │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                          DATA LAYER                                     │ │
│  │                                                                         │ │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐ │ │
│  │  │  graceModules    │  │graceQuizQuestions│  │graceCrucibleChallenges│ │ │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────────┘ │ │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐ │ │
│  │  │graceUserProgress │  │ graceQuizAttempts│  │graceCrucibleSubmissions│ │ │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────────┘ │ │
│  │  ┌──────────────────┐  ┌──────────────────┐                          │ │
│  │  │graceCertificates │  │      users       │                          │ │
│  │  └──────────────────┘  └──────────────────┘                          │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Data Flow Architecture

The system implements a unidirectional data flow pattern:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA FLOW DIAGRAM                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  USER ACTION                                                                 │
│       │                                                                      │
│       ▼                                                                      │
│  ┌─────────────────┐                                                        │
│  │  React Component │                                                        │
│  │  (e.g., Quiz)    │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 1. User submits quiz answers                                    │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  tRPC Mutation   │  trpc.grace.quiz.submit.useMutation()                 │
│  │  Hook            │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 2. HTTP POST to /api/trpc/grace.quiz.submit                     │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  tRPC Procedure  │  protectedProcedure with Zod validation               │
│  │  (Server)        │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 3. Validate input, check authentication                         │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Database Helper │  createGraceQuizAttempt(attempt)                      │
│  │  (db.ts)         │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 4. Insert into grace_quiz_attempts table                        │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  MySQL Database  │  Drizzle ORM executes INSERT                          │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 5. Return success + check certificates                          │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Certificate     │  checkAndAwardGraceCertificates(userId)               │
│  │  Check           │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 6. Return response with any awarded certificates                │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  React Query     │  Cache invalidation + optimistic update               │
│  │  Cache Update    │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 7. UI re-renders with new state                                 │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Updated UI      │  Shows score, unlocks next section                    │
│  └─────────────────┘                                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Authentication Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           AUTHENTICATION FLOW                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. INITIAL PAGE LOAD                                                        │
│  ┌─────────────────┐                                                        │
│  │  User visits    │                                                        │
│  │  /grace-academy │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐     ┌─────────────────┐                               │
│  │  useAuth() hook │────▶│ trpc.auth.me    │                               │
│  │  checks session │     │ .useQuery()     │                               │
│  └────────┬────────┘     └─────────────────┘                               │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────────────────────────────┐                               │
│  │  Session cookie present?                 │                               │
│  └────────┬───────────────────┬────────────┘                               │
│           │ YES               │ NO                                          │
│           ▼                   ▼                                              │
│  ┌─────────────────┐  ┌─────────────────┐                                  │
│  │  Return user    │  │  Return null    │                                  │
│  │  object         │  │  (guest mode)   │                                  │
│  └─────────────────┘  └─────────────────┘                                  │
│                                                                              │
│  2. LOGIN FLOW (When user clicks "Sign In")                                 │
│  ┌─────────────────┐                                                        │
│  │  Redirect to    │                                                        │
│  │  Manus OAuth    │  window.location.href = getLoginUrl()                  │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Manus OAuth    │  User authenticates with Google/Email                  │
│  │  Portal         │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Callback to    │  /api/oauth/callback?code=...                          │
│  │  /api/oauth     │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Server creates │  JWT token in httpOnly cookie                          │
│  │  session cookie │  Name: defined in COOKIE_NAME constant                 │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Upsert user    │  upsertUser() in db.ts                                 │
│  │  in database    │  Creates or updates user record                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │  Redirect to    │  User now authenticated                                │
│  │  original page  │                                                        │
│  └─────────────────┘                                                        │
│                                                                              │
│  3. PROTECTED ROUTE ACCESS                                                   │
│  ┌─────────────────┐                                                        │
│  │  User accesses  │                                                        │
│  │  /module/1      │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ▼                                                                  │
│  ┌─────────────────────────────────────────┐                               │
│  │  isAuthenticated && !authLoading?       │                               │
│  └────────┬───────────────────┬────────────┘                               │
│           │ YES               │ NO                                          │
│           ▼                   ▼                                              │
│  ┌─────────────────┐  ┌─────────────────┐                                  │
│  │  Render module  │  │  Redirect to    │                                  │
│  │  content        │  │  OAuth login    │                                  │
│  └─────────────────┘  └─────────────────┘                                  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Decision Trees and Design Rationale

### 4.1 Framework Selection Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FRAMEWORK SELECTION DECISION TREE                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  REQUIREMENT: Build enterprise learning platform with                        │
│               - Real-time progress tracking                                  │
│               - Type-safe API communication                                  │
│               - Modern UI with accessibility                                 │
│               - Scalable architecture                                        │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  DECISION POINT 1: Frontend Framework                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  Options Evaluated:                                                          │
│  ┌────────────────┬────────────────┬────────────────┬──────────────────┐   │
│  │    Option      │     Pros       │     Cons       │    Decision      │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ React 19       │ - Latest hooks │ - Learning     │ ✅ SELECTED      │   │
│  │                │ - Concurrent   │   curve for    │                  │   │
│  │                │   rendering    │   new features │ Rationale:       │   │
│  │                │ - Large        │                │ Best ecosystem   │   │
│  │                │   ecosystem    │                │ for tRPC, mature │   │
│  │                │ - tRPC native  │                │ component libs   │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Vue 3          │ - Simpler      │ - Smaller      │ ❌ NOT SELECTED  │   │
│  │                │   syntax       │   ecosystem    │                  │   │
│  │                │ - Good perf    │ - Less tRPC    │                  │   │
│  │                │                │   integration  │                  │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Next.js 14     │ - SSR built-in │ - Overkill for │ ❌ NOT SELECTED  │   │
│  │                │ - App router   │   SPA needs    │                  │   │
│  │                │                │ - More complex │                  │   │
│  └────────────────┴────────────────┴────────────────┴──────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  DECISION POINT 2: API Architecture                                  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  Options Evaluated:                                                          │
│  ┌────────────────┬────────────────┬────────────────┬──────────────────┐   │
│  │    Option      │     Pros       │     Cons       │    Decision      │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ tRPC 11        │ - End-to-end   │ - TypeScript   │ ✅ SELECTED      │   │
│  │                │   type safety  │   required     │                  │   │
│  │                │ - No code gen  │ - Monorepo     │ Rationale:       │   │
│  │                │ - React Query  │   preferred    │ Type safety      │   │
│  │                │   integration  │                │ eliminates API   │   │
│  │                │                │                │ contract bugs    │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ REST + OpenAPI │ - Standard     │ - Code gen     │ ❌ NOT SELECTED  │   │
│  │                │ - Language     │   required     │                  │   │
│  │                │   agnostic     │ - Type drift   │                  │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ GraphQL        │ - Flexible     │ - Complexity   │ ❌ NOT SELECTED  │   │
│  │                │   queries      │ - N+1 issues   │                  │   │
│  │                │ - Schema-first │ - Overkill     │                  │   │
│  └────────────────┴────────────────┴────────────────┴──────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  DECISION POINT 3: Database and ORM                                  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  Options Evaluated:                                                          │
│  ┌────────────────┬────────────────┬────────────────┬──────────────────┐   │
│  │    Option      │     Pros       │     Cons       │    Decision      │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Drizzle ORM    │ - TypeScript   │ - Newer, less  │ ✅ SELECTED      │   │
│  │ + MySQL        │   native       │   mature       │                  │   │
│  │                │ - SQL-like     │                │ Rationale:       │   │
│  │                │   syntax       │                │ Best type        │   │
│  │                │ - Lightweight  │                │ inference, works │   │
│  │                │ - Fast         │                │ with superjson   │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Prisma         │ - Popular      │ - Heavy        │ ❌ NOT SELECTED  │   │
│  │                │ - Good DX      │ - Code gen     │                  │   │
│  │                │                │ - Slower       │                  │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ TypeORM        │ - Mature       │ - Complex      │ ❌ NOT SELECTED  │   │
│  │                │ - Decorators   │ - Type issues  │                  │   │
│  └────────────────┴────────────────┴────────────────┴──────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  DECISION POINT 4: UI Component Library                              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  Options Evaluated:                                                          │
│  ┌────────────────┬────────────────┬────────────────┬──────────────────┐   │
│  │    Option      │     Pros       │     Cons       │    Decision      │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ shadcn/ui      │ - Copy/paste   │ - Manual       │ ✅ SELECTED      │   │
│  │ + Radix        │ - Customizable │   updates      │                  │   │
│  │                │ - Accessible   │                │ Rationale:       │   │
│  │                │ - Tailwind     │                │ Full control,    │   │
│  │                │   native       │                │ WCAG compliant   │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Material UI    │ - Complete     │ - Heavy        │ ❌ NOT SELECTED  │   │
│  │                │ - Google style │ - Hard to      │                  │   │
│  │                │                │   customize    │                  │   │
│  ├────────────────┼────────────────┼────────────────┼──────────────────┤   │
│  │ Chakra UI      │ - Good DX      │ - Performance  │ ❌ NOT SELECTED  │   │
│  │                │ - Accessible   │ - Bundle size  │                  │   │
│  └────────────────┴────────────────┴────────────────┴──────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Module Structure Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MODULE STRUCTURE DECISION TREE                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  REQUIREMENT: Each module must be completable in ~15 minutes                 │
│               with distinct learning phases                                  │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  DECISION: Four-Part Module Structure                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐    │
│  │                                                                     │    │
│  │  ┌─────────────────────────────────────────────────────────────┐  │    │
│  │  │  THE SPARK (3 minutes)                                       │  │    │
│  │  │                                                              │  │    │
│  │  │  Purpose: Introduce core concept                             │  │    │
│  │  │  Format: Markdown text in Marilyn vos Savant style           │  │    │
│  │  │  Rationale: Short attention span, memorable delivery         │  │    │
│  │  │                                                              │  │    │
│  │  │  Implementation:                                             │  │    │
│  │  │  - sparkContent field in graceModules table                  │  │    │
│  │  │  - Rendered with Streamdown component                        │  │    │
│  │  │  - Progress tracked via sparkCompleted flag                  │  │    │
│  │  └─────────────────────────────────────────────────────────────┘  │    │
│  │                              │                                     │    │
│  │                              ▼                                     │    │
│  │  ┌─────────────────────────────────────────────────────────────┐  │    │
│  │  │  THE GAUNTLET (5 minutes)                                    │  │    │
│  │  │                                                              │  │    │
│  │  │  Purpose: Assess understanding                               │  │    │
│  │  │  Format: 5 multiple-choice questions                         │  │    │
│  │  │  Rationale: Quick validation, instant feedback               │  │    │
│  │  │                                                              │  │    │
│  │  │  Implementation:                                             │  │    │
│  │  │  - graceQuizQuestions table (5 per module)                   │  │    │
│  │  │  - 60% pass threshold for completion                         │  │    │
│  │  │  - graceQuizAttempts stores all attempts                     │  │    │
│  │  │  - bestQuizScore tracked in progress                         │  │    │
│  │  └─────────────────────────────────────────────────────────────┘  │    │
│  │                              │                                     │    │
│  │                              ▼                                     │    │
│  │  ┌─────────────────────────────────────────────────────────────┐  │    │
│  │  │  THE CRUCIBLE (5 minutes)                                    │  │    │
│  │  │                                                              │  │    │
│  │  │  Purpose: Apply knowledge practically                        │  │    │
│  │  │  Format: Text submission challenge                           │  │    │
│  │  │  Rationale: Active learning, real-world application          │  │    │
│  │  │                                                              │  │    │
│  │  │  Implementation:                                             │  │    │
│  │  │  - graceCrucibleChallenges table (1 per module)              │  │    │
│  │  │  - graceCrucibleSubmissions stores responses                 │  │    │
│  │  │  - Admin review workflow (pending/approved/needs_revision)   │  │    │
│  │  │  - Minimum 50 characters required                            │  │    │
│  │  └─────────────────────────────────────────────────────────────┘  │    │
│  │                              │                                     │    │
│  │                              ▼                                     │    │
│  │  ┌─────────────────────────────────────────────────────────────┐  │    │
│  │  │  THE IMPRINT (2 minutes)                                     │  │    │
│  │  │                                                              │  │    │
│  │  │  Purpose: Lasting retention through reflection               │  │    │
│  │  │  Format: Thought experiment + visual aid                     │  │    │
│  │  │  Rationale: Memory consolidation, visual learning            │  │    │
│  │  │                                                              │  │    │
│  │  │  Implementation:                                             │  │    │
│  │  │  - imprintContent field in graceModules table                │  │    │
│  │  │  - visualAidUrl and visualAidDescription fields              │  │    │
│  │  │  - Marks module as complete when finished                    │  │    │
│  │  └─────────────────────────────────────────────────────────────┘  │    │
│  │                                                                     │    │
│  └────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  TOTAL TIME: 3 + 5 + 5 + 2 = 15 minutes per module                          │
│  TOTAL COURSE: 30 modules × 15 minutes = 7.5 hours                          │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.3 Certification Logic Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    CERTIFICATION LOGIC DECISION TREE                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  TRIGGER: checkAndAwardGraceCertificates(userId)                     │   │
│  │  Called after: quiz completion, section completion, crucible submit  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 1: Fetch all user progress records                             │   │
│  │          getAllGraceUserProgress(userId)                             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 2: Fetch existing certificates                                 │   │
│  │          getGraceUserCertificates(userId)                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 3: Check Foundation Track (Modules 1-10)                       │   │
│  │                                                                       │   │
│  │  ┌───────────────────────────────────────────────────────────────┐  │   │
│  │  │  foundationComplete = progress.filter(p =>                     │  │   │
│  │  │    p.moduleCompleted && p.moduleId >= 1 && p.moduleId <= 10    │  │   │
│  │  │  ).length === 10                                               │  │   │
│  │  └───────────────────────────────────────────────────────────────┘  │   │
│  │                                                                       │   │
│  │  ┌─────────────────────────────────────────────────────────┐        │   │
│  │  │  foundationComplete && !hasFoundationCert?              │        │   │
│  │  └──────────────┬─────────────────────┬────────────────────┘        │   │
│  │                 │ YES                 │ NO                          │   │
│  │                 ▼                     ▼                              │   │
│  │  ┌──────────────────────┐  ┌──────────────────────┐                │   │
│  │  │ Award Foundation     │  │ Skip                 │                │   │
│  │  │ Certificate          │  │                      │                │   │
│  │  │ Code: GRACE-FOUNDATION-{nanoid}               │                │   │
│  │  └──────────────────────┘  └──────────────────────┘                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 4: Check Intermediate Track (Modules 11-20)                    │   │
│  │                                                                       │   │
│  │  ┌───────────────────────────────────────────────────────────────┐  │   │
│  │  │  intermediateComplete = progress.filter(p =>                   │  │   │
│  │  │    p.moduleCompleted && p.moduleId >= 11 && p.moduleId <= 20   │  │   │
│  │  │  ).length === 10                                               │  │   │
│  │  └───────────────────────────────────────────────────────────────┘  │   │
│  │                                                                       │   │
│  │  ┌─────────────────────────────────────────────────────────┐        │   │
│  │  │  intermediateComplete && !hasIntermediateCert?          │        │   │
│  │  └──────────────┬─────────────────────┬────────────────────┘        │   │
│  │                 │ YES                 │ NO                          │   │
│  │                 ▼                     ▼                              │   │
│  │  ┌──────────────────────┐  ┌──────────────────────┐                │   │
│  │  │ Award Intermediate   │  │ Skip                 │                │   │
│  │  │ Certificate          │  │                      │                │   │
│  │  │ Code: GRACE-INTERMEDIATE-{nanoid}             │                │   │
│  │  └──────────────────────┘  └──────────────────────┘                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 5: Check Advanced Track (Modules 21-30)                        │   │
│  │                                                                       │   │
│  │  ┌───────────────────────────────────────────────────────────────┐  │   │
│  │  │  advancedComplete = progress.filter(p =>                       │  │   │
│  │  │    p.moduleCompleted && p.moduleId >= 21 && p.moduleId <= 30   │  │   │
│  │  │  ).length === 10                                               │  │   │
│  │  └───────────────────────────────────────────────────────────────┘  │   │
│  │                                                                       │   │
│  │  ┌─────────────────────────────────────────────────────────┐        │   │
│  │  │  advancedComplete && !hasAdvancedCert?                  │        │   │
│  │  └──────────────┬─────────────────────┬────────────────────┘        │   │
│  │                 │ YES                 │ NO                          │   │
│  │                 ▼                     ▼                              │   │
│  │  ┌──────────────────────┐  ┌──────────────────────┐                │   │
│  │  │ Award Advanced       │  │ Skip                 │                │   │
│  │  │ Certificate          │  │                      │                │   │
│  │  │ Code: GRACE-ADVANCED-{nanoid}                 │                │   │
│  │  └──────────────────────┘  └──────────────────────┘                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Step 6: Check GRACE Diploma (All 30 Modules)                        │   │
│  │                                                                       │   │
│  │  ┌─────────────────────────────────────────────────────────┐        │   │
│  │  │  foundationComplete &&                                   │        │   │
│  │  │  intermediateComplete &&                                 │        │   │
│  │  │  advancedComplete &&                                     │        │   │
│  │  │  !hasGraceDiploma?                                       │        │   │
│  │  └──────────────┬─────────────────────┬────────────────────┘        │   │
│  │                 │ YES                 │ NO                          │   │
│  │                 ▼                     ▼                              │   │
│  │  ┌──────────────────────┐  ┌──────────────────────┐                │   │
│  │  │ Award GRACE Diploma  │  │ Skip                 │                │   │
│  │  │ Code: GRACE-GRACE_DIPLOMA-{nanoid}            │                │   │
│  │  │ Average score across all 30 modules           │                │   │
│  │  └──────────────────────┘  └──────────────────────┘                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                               │
│                              ▼                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Return: Array of newly awarded certificate types                    │   │
│  │  e.g., ['foundation'] or ['advanced', 'grace_diploma']               │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Software Version Registry

### 5.1 Production Dependencies

The following table documents all production dependencies with their exact versions:

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| @aws-sdk/client-s3 | ^3.693.0 | AWS S3 file storage operations | Apache-2.0 |
| @aws-sdk/s3-request-presigner | ^3.693.0 | Generate presigned S3 URLs | Apache-2.0 |
| @hookform/resolvers | ^5.2.2 | Form validation resolvers for react-hook-form | MIT |
| @radix-ui/react-accordion | ^1.2.12 | Accessible accordion component | MIT |
| @radix-ui/react-alert-dialog | ^1.1.15 | Accessible alert dialog component | MIT |
| @radix-ui/react-aspect-ratio | ^1.1.7 | Maintain aspect ratios | MIT |
| @radix-ui/react-avatar | ^1.1.10 | User avatar component | MIT |
| @radix-ui/react-checkbox | ^1.3.3 | Accessible checkbox component | MIT |
| @radix-ui/react-collapsible | ^1.1.12 | Collapsible content sections | MIT |
| @radix-ui/react-context-menu | ^2.2.16 | Right-click context menus | MIT |
| @radix-ui/react-dialog | ^1.1.15 | Modal dialog component | MIT |
| @radix-ui/react-dropdown-menu | ^2.1.16 | Dropdown menu component | MIT |
| @radix-ui/react-hover-card | ^1.1.15 | Hover-triggered cards | MIT |
| @radix-ui/react-label | ^2.1.7 | Form label component | MIT |
| @radix-ui/react-menubar | ^1.1.16 | Application menubar | MIT |
| @radix-ui/react-navigation-menu | ^1.2.14 | Navigation menu component | MIT |
| @radix-ui/react-popover | ^1.1.15 | Popover component | MIT |
| @radix-ui/react-progress | ^1.1.7 | Progress bar component | MIT |
| @radix-ui/react-radio-group | ^1.3.8 | Radio button group | MIT |
| @radix-ui/react-scroll-area | ^1.2.10 | Custom scrollable areas | MIT |
| @radix-ui/react-select | ^2.2.6 | Select dropdown component | MIT |
| @radix-ui/react-separator | ^1.1.7 | Visual separator | MIT |
| @radix-ui/react-slider | ^1.3.6 | Range slider component | MIT |
| @radix-ui/react-slot | ^1.2.3 | Slot pattern for composition | MIT |
| @radix-ui/react-switch | ^1.2.6 | Toggle switch component | MIT |
| @radix-ui/react-tabs | ^1.1.13 | Tab navigation component | MIT |
| @radix-ui/react-toggle | ^1.1.10 | Toggle button component | MIT |
| @radix-ui/react-toggle-group | ^1.1.11 | Toggle button group | MIT |
| @radix-ui/react-tooltip | ^1.2.8 | Tooltip component | MIT |
| @tanstack/react-query | ^5.90.2 | Server state management | MIT |
| @trpc/client | ^11.6.0 | tRPC client library | MIT |
| @trpc/react-query | ^11.6.0 | tRPC React Query integration | MIT |
| @trpc/server | ^11.6.0 | tRPC server library | MIT |
| axios | ^1.12.0 | HTTP client for external APIs | MIT |
| class-variance-authority | ^0.7.1 | CSS class composition utility | Apache-2.0 |
| clsx | ^2.1.1 | Conditional class name utility | MIT |
| cmdk | ^1.1.1 | Command palette component | MIT |
| cookie | ^1.0.2 | Cookie parsing and serialization | MIT |
| date-fns | ^4.1.0 | Date utility library | MIT |
| dotenv | ^17.2.2 | Environment variable loading | BSD-2-Clause |
| drizzle-orm | ^0.44.5 | TypeScript ORM | Apache-2.0 |
| embla-carousel-react | ^8.6.0 | Carousel component | MIT |
| express | ^4.21.2 | HTTP server framework | MIT |
| framer-motion | ^12.23.22 | Animation library | MIT |
| input-otp | ^1.4.2 | OTP input component | MIT |
| jose | 6.1.0 | JWT implementation | MIT |
| lucide-react | ^0.453.0 | Icon library | ISC |
| mysql2 | ^3.15.0 | MySQL database driver | MIT |
| nanoid | ^5.1.5 | Unique ID generation | MIT |
| next-themes | ^0.4.6 | Theme management | MIT |
| react | ^19.2.1 | UI framework | MIT |
| react-day-picker | ^9.11.1 | Date picker component | MIT |
| react-dom | ^19.2.1 | React DOM renderer | MIT |
| react-hook-form | ^7.64.0 | Form state management | MIT |
| react-resizable-panels | ^3.0.6 | Resizable panel layout | MIT |
| recharts | ^2.15.2 | Charting library | MIT |
| sonner | ^2.0.7 | Toast notification library | MIT |
| streamdown | ^1.4.0 | Streaming markdown renderer | MIT |
| superjson | ^1.13.3 | JSON serialization with types | MIT |
| tailwind-merge | ^3.3.1 | Tailwind class merging | MIT |
| tailwindcss-animate | ^1.0.7 | Tailwind animation utilities | MIT |
| vaul | ^1.1.2 | Drawer component | MIT |
| wouter | ^3.3.5 | Lightweight router | ISC |
| zod | ^4.1.12 | Schema validation | MIT |

### 5.2 Development Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| @builder.io/vite-plugin-jsx-loc | ^0.1.1 | JSX location tracking | MIT |
| @tailwindcss/typography | ^0.5.15 | Typography plugin | MIT |
| @tailwindcss/vite | ^4.1.3 | Tailwind Vite plugin | MIT |
| @types/express | 4.17.21 | Express type definitions | MIT |
| @types/google.maps | ^3.58.1 | Google Maps types | MIT |
| @types/node | ^24.7.0 | Node.js type definitions | MIT |
| @types/react | ^19.2.1 | React type definitions | MIT |
| @types/react-dom | ^19.2.1 | React DOM type definitions | MIT |
| @vitejs/plugin-react | ^5.0.4 | Vite React plugin | MIT |
| autoprefixer | ^10.4.20 | CSS autoprefixer | MIT |
| drizzle-kit | ^0.31.4 | Drizzle CLI tools | MIT |
| esbuild | ^0.25.0 | JavaScript bundler | MIT |
| postcss | ^8.4.47 | CSS processor | MIT |
| prettier | ^3.6.2 | Code formatter | MIT |
| tailwindcss | ^4.1.14 | CSS framework | MIT |
| tsx | ^4.19.1 | TypeScript execution | MIT |
| tw-animate-css | ^1.4.0 | Tailwind animations | MIT |
| typescript | 5.9.3 | TypeScript compiler | Apache-2.0 |
| vite | ^7.1.7 | Build tool | MIT |
| vite-plugin-manus-runtime | ^0.0.57 | Manus runtime plugin | Proprietary |
| vitest | ^2.1.4 | Test framework | MIT |

### 5.3 Runtime Environment

| Component | Version | Notes |
|-----------|---------|-------|
| Node.js | 22.x LTS | Required for ES modules support |
| pnpm | 10.4.1 | Package manager (specified in packageManager field) |
| MySQL | 8.0+ | Or TiDB for cloud deployment |

---

## 6. Database Schema Documentation

### 6.1 Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ENTITY RELATIONSHIP DIAGRAM                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────┐                                                    │
│  │       users         │                                                    │
│  ├─────────────────────┤                                                    │
│  │ PK id               │◄─────────────────────────────────────────┐        │
│  │    openId           │                                          │        │
│  │    name             │                                          │        │
│  │    email            │                                          │        │
│  │    loginMethod      │                                          │        │
│  │    role             │                                          │        │
│  │    createdAt        │                                          │        │
│  │    updatedAt        │                                          │        │
│  │    lastSignedIn     │                                          │        │
│  └─────────────────────┘                                          │        │
│           │                                                        │        │
│           │ 1:N                                                    │        │
│           ▼                                                        │        │
│  ┌─────────────────────┐     ┌─────────────────────┐              │        │
│  │ graceUserProgress   │     │    graceModules     │              │        │
│  ├─────────────────────┤     ├─────────────────────┤              │        │
│  │ PK id               │     │ PK id               │◄─────┐      │        │
│  │ FK userId ──────────┼─────│    moduleNumber     │      │      │        │
│  │ FK moduleId ────────┼────▶│    track            │      │      │        │
│  │    sparkCompleted   │     │    title            │      │      │        │
│  │    gauntletCompleted│     │    subtitle         │      │      │        │
│  │    crucibleCompleted│     │    sparkContent     │      │      │        │
│  │    imprintCompleted │     │    imprintContent   │      │      │        │
│  │    moduleCompleted  │     │    visualAidUrl     │      │      │        │
│  │    bestQuizScore    │     │    visualAidDesc    │      │      │        │
│  │    completedAt      │     │    estimatedMinutes │      │      │        │
│  │    createdAt        │     │    createdAt        │      │      │        │
│  │    updatedAt        │     │    updatedAt        │      │      │        │
│  └─────────────────────┘     └─────────────────────┘      │      │        │
│           │                           │                    │      │        │
│           │                           │ 1:N                │      │        │
│           │                           ▼                    │      │        │
│           │              ┌─────────────────────┐          │      │        │
│           │              │ graceQuizQuestions  │          │      │        │
│           │              ├─────────────────────┤          │      │        │
│           │              │ PK id               │          │      │        │
│           │              │ FK moduleId ────────┼──────────┘      │        │
│           │              │    questionNumber   │                 │        │
│           │              │    question         │                 │        │
│           │              │    options (JSON)   │                 │        │
│           │              │    correctAnswer    │                 │        │
│           │              │    explanation      │                 │        │
│           │              │    createdAt        │                 │        │
│           │              └─────────────────────┘                 │        │
│           │                                                       │        │
│           │              ┌─────────────────────┐                 │        │
│           │              │graceCrucibleChallenge│                │        │
│           │              ├─────────────────────┤                 │        │
│           │              │ PK id               │                 │        │
│           │              │ FK moduleId (UNIQUE)┼─────────────────┘        │
│           │              │    challengePrompt  │                          │
│           │              │    evaluationCriteria│                         │
│           │              │    sampleResponse   │                          │
│           │              │    createdAt        │                          │
│           │              └─────────────────────┘                          │
│           │                                                               │
│           │ 1:N          ┌─────────────────────┐                          │
│           └─────────────▶│  graceQuizAttempts  │                          │
│           │              ├─────────────────────┤                          │
│           │              │ PK id               │                          │
│           │              │ FK userId ──────────┼──────────────────────────┘
│           │              │ FK moduleId         │
│           │              │    score            │
│           │              │    answers (JSON)   │
│           │              │    timeTaken        │
│           │              │    attemptedAt      │
│           │              └─────────────────────┘
│           │
│           │ 1:N          ┌─────────────────────┐
│           └─────────────▶│graceCrucibleSubmissions│
│           │              ├─────────────────────┤
│           │              │ PK id               │
│           │              │ FK userId           │
│           │              │ FK moduleId         │
│           │              │    submission       │
│           │              │    status           │
│           │              │    adminFeedback    │
│           │              │ FK reviewedBy       │
│           │              │    reviewedAt       │
│           │              │    submittedAt      │
│           │              └─────────────────────┘
│           │
│           │ 1:N          ┌─────────────────────┐
│           └─────────────▶│  graceCertificates  │
│                          ├─────────────────────┤
│                          │ PK id               │
│                          │ FK userId           │
│                          │    certificateType  │
│                          │    certificateCode  │
│                          │    averageScore     │
│                          │    earnedAt         │
│                          └─────────────────────┘
│
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Table Specifications

#### 6.2.1 users Table

This table stores user authentication and profile information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Surrogate primary key |
| openId | VARCHAR(64) | NOT NULL, UNIQUE | Manus OAuth identifier |
| name | TEXT | NULLABLE | User's display name |
| email | VARCHAR(320) | NULLABLE | User's email address |
| loginMethod | VARCHAR(64) | NULLABLE | Authentication method (google, email) |
| role | ENUM('user', 'admin') | NOT NULL, DEFAULT 'user' | User role for access control |
| createdAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Account creation timestamp |
| updatedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() ON UPDATE | Last update timestamp |
| lastSignedIn | TIMESTAMP | NOT NULL, DEFAULT NOW() | Last login timestamp |

#### 6.2.2 graceModules Table

This table stores the 30 learning modules.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Module ID |
| moduleNumber | INT | NOT NULL, UNIQUE | Module sequence (1-30) |
| track | ENUM('foundation', 'intermediate', 'advanced') | NOT NULL | Learning track |
| title | VARCHAR(255) | NOT NULL | Module title |
| subtitle | VARCHAR(500) | NULLABLE | Module subtitle |
| sparkContent | TEXT | NULLABLE | The Spark reading content (Markdown) |
| imprintContent | TEXT | NULLABLE | The Imprint content (Markdown) |
| visualAidUrl | VARCHAR(500) | NULLABLE | URL to visual aid image |
| visualAidDescription | TEXT | NULLABLE | Description of visual aid |
| estimatedMinutes | INT | DEFAULT 15 | Estimated completion time |
| createdAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| updatedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() ON UPDATE | Update timestamp |

#### 6.2.3 graceQuizQuestions Table

This table stores quiz questions for The Gauntlet.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Question ID |
| moduleId | INT | NOT NULL, FK → graceModules.id | Associated module |
| questionNumber | INT | NOT NULL | Question sequence (1-5) |
| question | TEXT | NOT NULL | Question text |
| options | TEXT | NOT NULL | JSON array of 4 options |
| correctAnswer | INT | NOT NULL | Index of correct option (0-3) |
| explanation | TEXT | NULLABLE | Explanation of correct answer |
| createdAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### 6.2.4 graceCrucibleChallenges Table

This table stores The Crucible challenges.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Challenge ID |
| moduleId | INT | NOT NULL, UNIQUE, FK → graceModules.id | Associated module (1:1) |
| challengePrompt | TEXT | NOT NULL | Challenge description (Markdown) |
| evaluationCriteria | TEXT | NULLABLE | Criteria for evaluation |
| sampleResponse | TEXT | NULLABLE | Example of good response |
| createdAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

#### 6.2.5 graceUserProgress Table

This table tracks user progress through modules.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Progress record ID |
| userId | INT | NOT NULL, FK → users.id | User reference |
| moduleId | INT | NOT NULL, FK → graceModules.id | Module reference |
| sparkCompleted | INT | NOT NULL, DEFAULT 0 | 0=false, 1=true |
| gauntletCompleted | INT | NOT NULL, DEFAULT 0 | 0=false, 1=true |
| crucibleCompleted | INT | NOT NULL, DEFAULT 0 | 0=false, 1=true |
| imprintCompleted | INT | NOT NULL, DEFAULT 0 | 0=false, 1=true |
| moduleCompleted | INT | NOT NULL, DEFAULT 0 | 0=false, 1=true |
| bestQuizScore | INT | NULLABLE | Best quiz percentage (0-100) |
| completedAt | TIMESTAMP | NULLABLE | Module completion timestamp |
| createdAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Record creation timestamp |
| updatedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() ON UPDATE | Last update timestamp |

#### 6.2.6 graceQuizAttempts Table

This table stores all quiz attempts for analytics.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Attempt ID |
| userId | INT | NOT NULL, FK → users.id | User reference |
| moduleId | INT | NOT NULL, FK → graceModules.id | Module reference |
| score | INT | NOT NULL | Score out of 5 |
| answers | TEXT | NOT NULL | JSON array of selected answers |
| timeTaken | INT | NULLABLE | Time in seconds |
| attemptedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Attempt timestamp |

#### 6.2.7 graceCrucibleSubmissions Table

This table stores Crucible challenge submissions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Submission ID |
| userId | INT | NOT NULL, FK → users.id | User reference |
| moduleId | INT | NOT NULL, FK → graceModules.id | Module reference |
| submission | TEXT | NOT NULL | User's submission text |
| status | ENUM('pending', 'approved', 'needs_revision') | NOT NULL, DEFAULT 'pending' | Review status |
| adminFeedback | TEXT | NULLABLE | Admin's feedback |
| reviewedBy | INT | NULLABLE, FK → users.id | Admin who reviewed |
| reviewedAt | TIMESTAMP | NULLABLE | Review timestamp |
| submittedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Submission timestamp |

#### 6.2.8 graceCertificates Table

This table stores awarded certificates.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | INT | PRIMARY KEY, AUTO_INCREMENT | Certificate ID |
| userId | INT | NOT NULL, FK → users.id | User reference |
| certificateType | ENUM('foundation', 'intermediate', 'advanced', 'grace_diploma') | NOT NULL | Certificate type |
| certificateCode | VARCHAR(64) | NOT NULL, UNIQUE | Verification code |
| averageScore | INT | NULLABLE | Average quiz score (0-100) |
| earnedAt | TIMESTAMP | NOT NULL, DEFAULT NOW() | Award timestamp |



---

## 7. API and tRPC Procedure Documentation

### 7.1 Router Structure

The GRACE Academy API is organized as a nested tRPC router within the main application router:

```typescript
appRouter = router({
  auth: authRouter,      // Authentication procedures
  system: systemRouter,  // System utilities
  grace: graceRouter,    // GRACE Academy procedures
});
```

### 7.2 GRACE Router Procedures

#### 7.2.1 Module Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.modules.list` | Query | Public | None | `GraceModule[]` | Returns all 30 modules |
| `grace.modules.byId` | Query | Public | `{ moduleId: number }` | `GraceModule \| null` | Returns single module by ID |
| `grace.modules.byTrack` | Query | Public | `{ track: string }` | `GraceModule[]` | Returns modules for a track |

**Example Usage:**
```typescript
// Client-side
const { data: modules } = trpc.grace.modules.list.useQuery();
const { data: module } = trpc.grace.modules.byId.useQuery({ moduleId: 1 });
```

#### 7.2.2 Quiz Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.quiz.questions` | Query | Protected | `{ moduleId: number }` | `QuizQuestion[]` | Returns 5 questions for module |
| `grace.quiz.submit` | Mutation | Protected | `{ moduleId, answers[], timeTaken? }` | `{ score, passed, certificates[] }` | Submits quiz attempt |
| `grace.quiz.attempts` | Query | Protected | `{ moduleId: number }` | `QuizAttempt[]` | Returns user's attempts |

**Quiz Submission Logic:**
```typescript
// Server-side procedure
grace.quiz.submit: protectedProcedure
  .input(z.object({
    moduleId: z.number(),
    answers: z.array(z.number()).length(5),
    timeTaken: z.number().optional(),
  }))
  .mutation(async ({ ctx, input }) => {
    // 1. Fetch correct answers
    const questions = await getGraceQuizQuestions(input.moduleId);
    
    // 2. Calculate score
    let correct = 0;
    questions.forEach((q, i) => {
      if (q.correctAnswer === input.answers[i]) correct++;
    });
    const score = correct;
    const percentage = (correct / 5) * 100;
    
    // 3. Record attempt
    await createGraceQuizAttempt({
      userId: ctx.user.id,
      moduleId: input.moduleId,
      score,
      answers: JSON.stringify(input.answers),
      timeTaken: input.timeTaken,
    });
    
    // 4. Update progress if passed (60% threshold)
    const passed = percentage >= 60;
    if (passed) {
      await updateGraceUserProgress(ctx.user.id, input.moduleId, {
        gauntletCompleted: 1,
        bestQuizScore: Math.max(percentage, existingScore),
      });
    }
    
    // 5. Check for certificate eligibility
    const certificates = await checkAndAwardGraceCertificates(ctx.user.id);
    
    return { score, percentage, passed, certificates };
  });
```

#### 7.2.3 Crucible Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.crucible.challenge` | Query | Protected | `{ moduleId: number }` | `CrucibleChallenge` | Returns challenge for module |
| `grace.crucible.submit` | Mutation | Protected | `{ moduleId, submission }` | `{ success, submissionId }` | Submits challenge response |
| `grace.crucible.mySubmissions` | Query | Protected | `{ moduleId?: number }` | `CrucibleSubmission[]` | Returns user's submissions |

**Submission Validation:**
```typescript
grace.crucible.submit: protectedProcedure
  .input(z.object({
    moduleId: z.number(),
    submission: z.string().min(50, "Submission must be at least 50 characters"),
  }))
  .mutation(async ({ ctx, input }) => {
    // Create submission with pending status
    const result = await createGraceCrucibleSubmission({
      userId: ctx.user.id,
      moduleId: input.moduleId,
      submission: input.submission,
      status: 'pending',
    });
    
    // Mark crucible as completed (auto-approve for MVP)
    await updateGraceUserProgress(ctx.user.id, input.moduleId, {
      crucibleCompleted: 1,
    });
    
    return { success: true, submissionId: result.insertId };
  });
```

#### 7.2.4 Progress Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.progress.forModule` | Query | Protected | `{ moduleId: number }` | `UserProgress` | Returns progress for module |
| `grace.progress.all` | Query | Protected | None | `UserProgress[]` | Returns all progress records |
| `grace.progress.updateSection` | Mutation | Protected | `{ moduleId, section, completed }` | `{ success }` | Updates section completion |
| `grace.progress.stats` | Query | Protected | None | `ProgressStats` | Returns aggregated statistics |

**Progress Update Logic:**
```typescript
grace.progress.updateSection: protectedProcedure
  .input(z.object({
    moduleId: z.number(),
    section: z.enum(['spark', 'gauntlet', 'crucible', 'imprint']),
    completed: z.boolean(),
  }))
  .mutation(async ({ ctx, input }) => {
    const fieldMap = {
      spark: 'sparkCompleted',
      gauntlet: 'gauntletCompleted',
      crucible: 'crucibleCompleted',
      imprint: 'imprintCompleted',
    };
    
    const field = fieldMap[input.section];
    const update = { [field]: input.completed ? 1 : 0 };
    
    // Check if all sections complete
    const progress = await getGraceUserProgress(ctx.user.id, input.moduleId);
    const allComplete = 
      (input.section === 'spark' ? input.completed : progress.sparkCompleted) &&
      (input.section === 'gauntlet' ? input.completed : progress.gauntletCompleted) &&
      (input.section === 'crucible' ? input.completed : progress.crucibleCompleted) &&
      (input.section === 'imprint' ? input.completed : progress.imprintCompleted);
    
    if (allComplete) {
      update.moduleCompleted = 1;
      update.completedAt = new Date();
    }
    
    await updateGraceUserProgress(ctx.user.id, input.moduleId, update);
    
    // Check certificates
    if (allComplete) {
      await checkAndAwardGraceCertificates(ctx.user.id);
    }
    
    return { success: true };
  });
```

#### 7.2.5 Certificate Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.certificates.mine` | Query | Protected | None | `Certificate[]` | Returns user's certificates |
| `grace.certificates.verify` | Query | Public | `{ code: string }` | `Certificate \| null` | Verifies certificate by code |

#### 7.2.6 Admin Procedures

| Procedure | Type | Auth | Input | Output | Description |
|-----------|------|------|-------|--------|-------------|
| `grace.admin.allStudents` | Query | Admin | None | `StudentWithProgress[]` | Returns all students with progress |
| `grace.admin.pendingSubmissions` | Query | Admin | None | `CrucibleSubmission[]` | Returns pending reviews |
| `grace.admin.reviewSubmission` | Mutation | Admin | `{ submissionId, status, feedback }` | `{ success }` | Reviews a submission |
| `grace.admin.updateModule` | Mutation | Admin | `{ moduleId, updates }` | `{ success }` | Updates module content |

---

## 8. Frontend Component Architecture

### 8.1 Component Hierarchy

```
App.tsx
├── ThemeProvider
│   └── TooltipProvider
│       └── Router (wouter)
│           ├── /grace-academy → index.tsx (Landing)
│           │   └── Layout
│           │       ├── Navigation
│           │       ├── TrackCards
│           │       └── CTASection
│           │
│           ├── /grace-academy/module-{n} → GraceModule.tsx
│           │   └── Layout
│           │       ├── ModuleHeader
│           │       ├── ProgressIndicator
│           │       ├── Tabs
│           │       │   ├── SparkTab
│           │       │   │   └── Streamdown (Markdown)
│           │       │   ├── GauntletTab
│           │       │   │   ├── QuizQuestion (x5)
│           │       │   │   └── QuizResults
│           │       │   ├── CrucibleTab
│           │       │   │   ├── ChallengePrompt
│           │       │   │   └── SubmissionForm
│           │       │   └── ImprintTab
│           │       │       ├── ThoughtExperiment
│           │       │       └── VisualAid
│           │       └── NavigationButtons
│           │
│           ├── /grace-academy/dashboard → GraceDashboard.tsx
│           │   └── Layout
│           │       ├── ProgressOverview
│           │       │   ├── TrackProgress (x3)
│           │       │   └── OverallStats
│           │       ├── CertificateDisplay
│           │       │   └── CertificateCard (x4)
│           │       └── RecentActivity
│           │
│           └── /grace-academy/admin → GraceAdmin.tsx
│               └── Layout
│                   ├── AdminTabs
│                   │   ├── StudentsTab
│                   │   │   └── StudentTable
│                   │   ├── SubmissionsTab
│                   │   │   └── SubmissionReviewCard
│                   │   └── ContentTab
│                   │       └── ModuleEditor
│                   └── AdminStats
```

### 8.2 Key Component Specifications

#### 8.2.1 GraceModule.tsx

**Purpose:** Dynamic module page that renders all four learning sections.

**Props:** None (uses URL parameter for moduleId)

**State Management:**
```typescript
interface ModuleState {
  activeTab: 'spark' | 'gauntlet' | 'crucible' | 'imprint';
  quizAnswers: number[];
  quizSubmitted: boolean;
  crucibleText: string;
  crucibleSubmitted: boolean;
}
```

**tRPC Queries Used:**
- `trpc.grace.modules.byId.useQuery({ moduleId })`
- `trpc.grace.progress.forModule.useQuery({ moduleId })`
- `trpc.grace.quiz.questions.useQuery({ moduleId })`
- `trpc.grace.crucible.challenge.useQuery({ moduleId })`

**tRPC Mutations Used:**
- `trpc.grace.progress.updateSection.useMutation()`
- `trpc.grace.quiz.submit.useMutation()`
- `trpc.grace.crucible.submit.useMutation()`

#### 8.2.2 GraceDashboard.tsx

**Purpose:** Student progress dashboard with certificate display.

**Key Features:**
- Track completion percentages
- Module-by-module progress grid
- Certificate cards with verification codes
- Quiz score analytics

**Data Aggregation:**
```typescript
// Calculate track progress
const trackProgress = useMemo(() => {
  const foundation = progress.filter(p => p.moduleId >= 1 && p.moduleId <= 10);
  const intermediate = progress.filter(p => p.moduleId >= 11 && p.moduleId <= 20);
  const advanced = progress.filter(p => p.moduleId >= 21 && p.moduleId <= 30);
  
  return {
    foundation: {
      completed: foundation.filter(p => p.moduleCompleted).length,
      total: 10,
      percentage: (foundation.filter(p => p.moduleCompleted).length / 10) * 100,
    },
    intermediate: { /* ... */ },
    advanced: { /* ... */ },
  };
}, [progress]);
```

#### 8.2.3 GraceAdmin.tsx

**Purpose:** Admin panel for content management and student monitoring.

**Access Control:**
```typescript
// Redirect non-admins
useEffect(() => {
  if (!authLoading && (!isAuthenticated || user?.role !== 'admin')) {
    window.location.href = '/grace-academy';
  }
}, [authLoading, isAuthenticated, user]);
```

**Admin Features:**
1. Student progress overview table
2. Crucible submission review queue
3. Module content editor
4. Certificate verification lookup

---

## 9. Code Structure and Line-by-Line Explanations

### 9.1 Database Schema (drizzle/schema.ts)

```typescript
// Line 1-3: Import Drizzle MySQL column types
// These imports provide type-safe column definitions for MySQL databases
import { 
  int,           // Integer columns (id, scores, etc.)
  mysqlEnum,     // Enum columns with predefined values
  mysqlTable,    // Table definition function
  text,          // Variable-length text columns
  timestamp,     // Timestamp columns with auto-update
  varchar        // Fixed-length string columns
} from "drizzle-orm/mysql-core";

// Line 5-20: Users table definition
// This is the core authentication table used by Manus OAuth
export const users = mysqlTable("users", {
  // Auto-incrementing primary key for internal references
  // Using numeric ID instead of UUID for better join performance
  id: int("id").autoincrement().primaryKey(),
  
  // Manus OAuth identifier - unique per user across all Manus apps
  // Length 64 accommodates various OAuth provider ID formats
  openId: varchar("openId", { length: 64 }).notNull().unique(),
  
  // Display name from OAuth provider (Google, etc.)
  // TEXT type allows unlimited length for international names
  name: text("name"),
  
  // Email address with RFC 5321 max length of 320 characters
  email: varchar("email", { length: 320 }),
  
  // Tracks authentication method for analytics
  loginMethod: varchar("loginMethod", { length: 64 }),
  
  // Role-based access control with two levels
  // 'user' = student, 'admin' = content manager
  role: mysqlEnum("role", ["user", "admin"]).default("user").notNull(),
  
  // Audit timestamps for compliance and debugging
  createdAt: timestamp("createdAt").defaultNow().notNull(),
  updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  lastSignedIn: timestamp("lastSignedIn").defaultNow().notNull(),
});

// Line 22-45: GRACE Modules table
// Stores the 30 learning modules with all content
export const graceModules = mysqlTable("grace_modules", {
  id: int("id").autoincrement().primaryKey(),
  
  // Module sequence number (1-30) for ordering
  // UNIQUE constraint ensures no duplicate module numbers
  moduleNumber: int("moduleNumber").notNull().unique(),
  
  // Track categorization for filtering and certificate logic
  // Foundation=1-10, Intermediate=11-20, Advanced=21-30
  track: mysqlEnum("track", ["foundation", "intermediate", "advanced"]).notNull(),
  
  // Module title displayed in navigation and headers
  title: varchar("title", { length: 255 }).notNull(),
  
  // Optional subtitle for additional context
  subtitle: varchar("subtitle", { length: 500 }),
  
  // The Spark content - Markdown format for rich text rendering
  // TEXT type allows up to 65,535 characters
  sparkContent: text("sparkContent"),
  
  // The Imprint content - thought experiment in Markdown
  imprintContent: text("imprintContent"),
  
  // Visual aid image URL (S3 or CDN path)
  visualAidUrl: varchar("visualAidUrl", { length: 500 }),
  
  // Alt text and description for accessibility
  visualAidDescription: text("visualAidDescription"),
  
  // Estimated completion time in minutes (default 15)
  estimatedMinutes: int("estimatedMinutes").default(15),
  
  createdAt: timestamp("createdAt").defaultNow().notNull(),
  updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
});

// Line 47-65: Quiz Questions table
// Stores 5 questions per module for The Gauntlet
export const graceQuizQuestions = mysqlTable("grace_quiz_questions", {
  id: int("id").autoincrement().primaryKey(),
  
  // Foreign key to graceModules - not enforced at DB level
  // for flexibility, but validated in application code
  moduleId: int("moduleId").notNull(),
  
  // Question order within module (1-5)
  questionNumber: int("questionNumber").notNull(),
  
  // Question text - supports Markdown for formatting
  question: text("question").notNull(),
  
  // JSON array of 4 answer options
  // Format: ["Option A", "Option B", "Option C", "Option D"]
  options: text("options").notNull(),
  
  // Index of correct answer (0-3)
  // Using index instead of text for reliable comparison
  correctAnswer: int("correctAnswer").notNull(),
  
  // Explanation shown after answering
  explanation: text("explanation"),
  
  createdAt: timestamp("createdAt").defaultNow().notNull(),
});

// Line 67-80: Crucible Challenges table
// One challenge per module for hands-on practice
export const graceCrucibleChallenges = mysqlTable("grace_crucible_challenges", {
  id: int("id").autoincrement().primaryKey(),
  
  // UNIQUE constraint ensures exactly one challenge per module
  moduleId: int("moduleId").notNull().unique(),
  
  // Challenge prompt in Markdown format
  challengePrompt: text("challengePrompt").notNull(),
  
  // Criteria for admin evaluation
  evaluationCriteria: text("evaluationCriteria"),
  
  // Example response for reference
  sampleResponse: text("sampleResponse"),
  
  createdAt: timestamp("createdAt").defaultNow().notNull(),
});

// Line 82-105: User Progress table
// Tracks completion status for each user-module combination
export const graceUserProgress = mysqlTable("grace_user_progress", {
  id: int("id").autoincrement().primaryKey(),
  
  // Composite key: userId + moduleId (not enforced as unique)
  userId: int("userId").notNull(),
  moduleId: int("moduleId").notNull(),
  
  // Section completion flags (0=incomplete, 1=complete)
  // Using INT instead of BOOLEAN for MySQL compatibility
  sparkCompleted: int("sparkCompleted").notNull().default(0),
  gauntletCompleted: int("gauntletCompleted").notNull().default(0),
  crucibleCompleted: int("crucibleCompleted").notNull().default(0),
  imprintCompleted: int("imprintCompleted").notNull().default(0),
  
  // Overall module completion (all 4 sections done)
  moduleCompleted: int("moduleCompleted").notNull().default(0),
  
  // Best quiz score as percentage (0-100)
  bestQuizScore: int("bestQuizScore"),
  
  // Timestamp when module was fully completed
  completedAt: timestamp("completedAt"),
  
  createdAt: timestamp("createdAt").defaultNow().notNull(),
  updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
});

// Line 107-120: Quiz Attempts table
// Stores all quiz attempts for analytics and retry tracking
export const graceQuizAttempts = mysqlTable("grace_quiz_attempts", {
  id: int("id").autoincrement().primaryKey(),
  userId: int("userId").notNull(),
  moduleId: int("moduleId").notNull(),
  
  // Raw score (0-5)
  score: int("score").notNull(),
  
  // JSON array of selected answer indices
  // Format: [0, 2, 1, 3, 0] (5 elements)
  answers: text("answers").notNull(),
  
  // Time taken in seconds (optional)
  timeTaken: int("timeTaken"),
  
  attemptedAt: timestamp("attemptedAt").defaultNow().notNull(),
});

// Line 122-140: Crucible Submissions table
// Stores challenge submissions with review workflow
export const graceCrucibleSubmissions = mysqlTable("grace_crucible_submissions", {
  id: int("id").autoincrement().primaryKey(),
  userId: int("userId").notNull(),
  moduleId: int("moduleId").notNull(),
  
  // User's submission text (minimum 50 characters enforced in app)
  submission: text("submission").notNull(),
  
  // Review status workflow
  status: mysqlEnum("status", ["pending", "approved", "needs_revision"])
    .notNull()
    .default("pending"),
  
  // Admin feedback text
  adminFeedback: text("adminFeedback"),
  
  // Admin who reviewed (FK to users.id)
  reviewedBy: int("reviewedBy"),
  
  reviewedAt: timestamp("reviewedAt"),
  submittedAt: timestamp("submittedAt").defaultNow().notNull(),
});

// Line 142-155: Certificates table
// Stores awarded certificates with verification codes
export const graceCertificates = mysqlTable("grace_certificates", {
  id: int("id").autoincrement().primaryKey(),
  userId: int("userId").notNull(),
  
  // Certificate type determines which track was completed
  certificateType: mysqlEnum("certificateType", [
    "foundation",      // Modules 1-10 complete
    "intermediate",    // Modules 11-20 complete
    "advanced",        // Modules 21-30 complete
    "grace_diploma"    // All 30 modules complete
  ]).notNull(),
  
  // Unique verification code for public verification
  // Format: GRACE-{TYPE}-{nanoid}
  certificateCode: varchar("certificateCode", { length: 64 }).notNull().unique(),
  
  // Average quiz score across completed track
  averageScore: int("averageScore"),
  
  earnedAt: timestamp("earnedAt").defaultNow().notNull(),
});

// Line 157-165: TypeScript type exports
// These types are inferred from table definitions for type safety
export type User = typeof users.$inferSelect;
export type InsertUser = typeof users.$inferInsert;
export type GraceModule = typeof graceModules.$inferSelect;
export type GraceQuizQuestion = typeof graceQuizQuestions.$inferSelect;
export type GraceCrucibleChallenge = typeof graceCrucibleChallenges.$inferSelect;
export type GraceUserProgress = typeof graceUserProgress.$inferSelect;
export type GraceQuizAttempt = typeof graceQuizAttempts.$inferSelect;
export type GraceCrucibleSubmission = typeof graceCrucibleSubmissions.$inferSelect;
export type GraceCertificate = typeof graceCertificates.$inferSelect;
```

### 9.2 Database Query Helpers (server/db.ts) - GRACE Functions

```typescript
// GRACE Module Queries
// =====================

/**
 * Retrieves all 30 GRACE modules ordered by module number.
 * Used by: grace.modules.list procedure
 * 
 * @returns Promise<GraceModule[]> Array of all modules
 */
export async function getAllGraceModules() {
  const db = await getDb();
  if (!db) return [];
  
  // Order by moduleNumber ensures consistent display order
  // across all UI components (landing page, navigation, dashboard)
  return db.select()
    .from(graceModules)
    .orderBy(graceModules.moduleNumber);
}

/**
 * Retrieves a single module by its database ID.
 * Used by: grace.modules.byId procedure
 * 
 * @param moduleId - The module's database ID (1-30)
 * @returns Promise<GraceModule | undefined> Module or undefined if not found
 */
export async function getGraceModuleById(moduleId: number) {
  const db = await getDb();
  if (!db) return undefined;
  
  const result = await db.select()
    .from(graceModules)
    .where(eq(graceModules.id, moduleId))
    .limit(1);
  
  return result[0];
}

/**
 * Retrieves all modules for a specific track.
 * Used by: grace.modules.byTrack procedure
 * 
 * @param track - 'foundation' | 'intermediate' | 'advanced'
 * @returns Promise<GraceModule[]> Modules in the specified track
 */
export async function getGraceModulesByTrack(
  track: 'foundation' | 'intermediate' | 'advanced'
) {
  const db = await getDb();
  if (!db) return [];
  
  return db.select()
    .from(graceModules)
    .where(eq(graceModules.track, track))
    .orderBy(graceModules.moduleNumber);
}

// GRACE Quiz Queries
// ==================

/**
 * Retrieves all quiz questions for a module.
 * Used by: grace.quiz.questions procedure
 * 
 * @param moduleId - The module's database ID
 * @returns Promise<GraceQuizQuestion[]> Array of 5 questions
 */
export async function getGraceQuizQuestions(moduleId: number) {
  const db = await getDb();
  if (!db) return [];
  
  return db.select()
    .from(graceQuizQuestions)
    .where(eq(graceQuizQuestions.moduleId, moduleId))
    .orderBy(graceQuizQuestions.questionNumber);
}

/**
 * Records a quiz attempt.
 * Used by: grace.quiz.submit procedure
 * 
 * @param attempt - Quiz attempt data
 * @returns Promise<InsertResult> Insert result with insertId
 */
export async function createGraceQuizAttempt(attempt: {
  userId: number;
  moduleId: number;
  score: number;
  answers: string;  // JSON string
  timeTaken?: number;
}) {
  const db = await getDb();
  if (!db) throw new Error("Database not available");
  
  return db.insert(graceQuizAttempts).values({
    userId: attempt.userId,
    moduleId: attempt.moduleId,
    score: attempt.score,
    answers: attempt.answers,
    timeTaken: attempt.timeTaken ?? null,
  });
}

// GRACE Progress Queries
// ======================

/**
 * Retrieves or creates progress record for a user-module combination.
 * Returns default values if no record exists (never returns undefined).
 * Used by: grace.progress.forModule procedure
 * 
 * @param userId - User's database ID
 * @param moduleId - Module's database ID
 * @returns Promise<GraceUserProgress> Progress record (existing or default)
 */
export async function getGraceUserProgress(userId: number, moduleId: number) {
  const db = await getDb();
  
  // Return default progress if database unavailable
  // This prevents undefined errors in the frontend
  if (!db) {
    return {
      id: 0,
      userId,
      moduleId,
      sparkCompleted: 0,
      gauntletCompleted: 0,
      crucibleCompleted: 0,
      imprintCompleted: 0,
      moduleCompleted: 0,
      bestQuizScore: null,
      completedAt: null,
      createdAt: new Date(),
      updatedAt: new Date(),
    };
  }
  
  const result = await db.select()
    .from(graceUserProgress)
    .where(
      and(
        eq(graceUserProgress.userId, userId),
        eq(graceUserProgress.moduleId, moduleId)
      )
    )
    .limit(1);
  
  // Return existing record or create default
  if (result[0]) {
    return result[0];
  }
  
  // Return default progress object for new user-module combinations
  return {
    id: 0,
    userId,
    moduleId,
    sparkCompleted: 0,
    gauntletCompleted: 0,
    crucibleCompleted: 0,
    imprintCompleted: 0,
    moduleCompleted: 0,
    bestQuizScore: null,
    completedAt: null,
    createdAt: new Date(),
    updatedAt: new Date(),
  };
}

/**
 * Updates progress for a user-module combination.
 * Creates record if it doesn't exist (upsert pattern).
 * Used by: grace.progress.updateSection procedure
 * 
 * @param userId - User's database ID
 * @param moduleId - Module's database ID
 * @param updates - Partial progress updates
 */
export async function updateGraceUserProgress(
  userId: number,
  moduleId: number,
  updates: Partial<{
    sparkCompleted: number;
    gauntletCompleted: number;
    crucibleCompleted: number;
    imprintCompleted: number;
    moduleCompleted: number;
    bestQuizScore: number;
    completedAt: Date;
  }>
) {
  const db = await getDb();
  if (!db) return;
  
  // Check if record exists
  const existing = await db.select()
    .from(graceUserProgress)
    .where(
      and(
        eq(graceUserProgress.userId, userId),
        eq(graceUserProgress.moduleId, moduleId)
      )
    )
    .limit(1);
  
  if (existing[0]) {
    // Update existing record
    await db.update(graceUserProgress)
      .set(updates)
      .where(eq(graceUserProgress.id, existing[0].id));
  } else {
    // Insert new record with defaults + updates
    await db.insert(graceUserProgress).values({
      userId,
      moduleId,
      sparkCompleted: updates.sparkCompleted ?? 0,
      gauntletCompleted: updates.gauntletCompleted ?? 0,
      crucibleCompleted: updates.crucibleCompleted ?? 0,
      imprintCompleted: updates.imprintCompleted ?? 0,
      moduleCompleted: updates.moduleCompleted ?? 0,
      bestQuizScore: updates.bestQuizScore ?? null,
      completedAt: updates.completedAt ?? null,
    });
  }
}

// GRACE Certificate Queries
// =========================

/**
 * Checks completion status and awards certificates if eligible.
 * Called after: quiz completion, section completion, crucible submission
 * 
 * @param userId - User's database ID
 * @returns Promise<string[]> Array of newly awarded certificate types
 */
export async function checkAndAwardGraceCertificates(userId: number) {
  const db = await getDb();
  if (!db) return [];
  
  const awarded: string[] = [];
  
  // Get all progress records for user
  const progress = await db.select()
    .from(graceUserProgress)
    .where(eq(graceUserProgress.userId, userId));
  
  // Get existing certificates
  const existingCerts = await db.select()
    .from(graceCertificates)
    .where(eq(graceCertificates.userId, userId));
  
  const hasCert = (type: string) => 
    existingCerts.some(c => c.certificateType === type);
  
  // Helper to count completed modules in range
  const completedInRange = (start: number, end: number) =>
    progress.filter(p => 
      p.moduleCompleted === 1 && 
      p.moduleId >= start && 
      p.moduleId <= end
    ).length;
  
  // Helper to calculate average score in range
  const avgScoreInRange = (start: number, end: number) => {
    const scores = progress
      .filter(p => p.moduleId >= start && p.moduleId <= end && p.bestQuizScore)
      .map(p => p.bestQuizScore!);
    return scores.length > 0 
      ? Math.round(scores.reduce((a, b) => a + b, 0) / scores.length)
      : null;
  };
  
  // Check Foundation (modules 1-10)
  if (completedInRange(1, 10) === 10 && !hasCert('foundation')) {
    await db.insert(graceCertificates).values({
      userId,
      certificateType: 'foundation',
      certificateCode: `GRACE-FOUNDATION-${nanoid(12)}`,
      averageScore: avgScoreInRange(1, 10),
    });
    awarded.push('foundation');
  }
  
  // Check Intermediate (modules 11-20)
  if (completedInRange(11, 20) === 10 && !hasCert('intermediate')) {
    await db.insert(graceCertificates).values({
      userId,
      certificateType: 'intermediate',
      certificateCode: `GRACE-INTERMEDIATE-${nanoid(12)}`,
      averageScore: avgScoreInRange(11, 20),
    });
    awarded.push('intermediate');
  }
  
  // Check Advanced (modules 21-30)
  if (completedInRange(21, 30) === 10 && !hasCert('advanced')) {
    await db.insert(graceCertificates).values({
      userId,
      certificateType: 'advanced',
      certificateCode: `GRACE-ADVANCED-${nanoid(12)}`,
      averageScore: avgScoreInRange(21, 30),
    });
    awarded.push('advanced');
  }
  
  // Check GRACE Diploma (all 30 modules)
  if (
    completedInRange(1, 10) === 10 &&
    completedInRange(11, 20) === 10 &&
    completedInRange(21, 30) === 10 &&
    !hasCert('grace_diploma')
  ) {
    await db.insert(graceCertificates).values({
      userId,
      certificateType: 'grace_diploma',
      certificateCode: `GRACE-DIPLOMA-${nanoid(12)}`,
      averageScore: avgScoreInRange(1, 30),
    });
    awarded.push('grace_diploma');
  }
  
  return awarded;
}
```

---

## 10. Installation and Configuration Guide

### 10.1 Prerequisites

Before installing the GRACE Academy system, ensure the following prerequisites are met:

| Requirement | Version | Verification Command |
|-------------|---------|---------------------|
| Node.js | 22.x LTS | `node --version` |
| pnpm | 10.4.1+ | `pnpm --version` |
| MySQL | 8.0+ | `mysql --version` |
| Git | 2.x+ | `git --version` |

### 10.2 Installation Steps

#### Step 1: Clone Repository

```bash
# Clone the AskMarilyn repository
git clone https://github.com/Lev0n82/AskMarilyn.git

# Navigate to project directory
cd AskMarilyn
```

#### Step 2: Install Dependencies

```bash
# Install all dependencies using pnpm
pnpm install

# Verify installation
pnpm list
```

#### Step 3: Configure Environment Variables

Create a `.env` file in the project root with the following variables:

```env
# Database Configuration
DATABASE_URL=mysql://user:password@host:3306/database_name

# Authentication
JWT_SECRET=your-secure-jwt-secret-minimum-32-characters
VITE_APP_ID=your-manus-app-id
OAUTH_SERVER_URL=https://api.manus.im
VITE_OAUTH_PORTAL_URL=https://manus.im/oauth

# Owner Configuration
OWNER_OPEN_ID=your-owner-open-id
OWNER_NAME=Your Name

# API Configuration
BUILT_IN_FORGE_API_URL=https://api.manus.im/forge
BUILT_IN_FORGE_API_KEY=your-forge-api-key
VITE_FRONTEND_FORGE_API_KEY=your-frontend-forge-key
VITE_FRONTEND_FORGE_API_URL=https://api.manus.im/forge

# Analytics (Optional)
VITE_ANALYTICS_ENDPOINT=https://analytics.example.com
VITE_ANALYTICS_WEBSITE_ID=your-website-id

# Application Branding
VITE_APP_TITLE=GRACE Academy
VITE_APP_LOGO=/images/grace-logo.png
```

#### Step 4: Initialize Database

```bash
# Generate and run migrations
pnpm db:push

# Verify tables were created
# Connect to MySQL and run:
# SHOW TABLES LIKE 'grace%';
```

#### Step 5: Seed Initial Data

```bash
# Run the GRACE modules seed script
node scripts/seed-grace-modules.mjs

# Verify seeding
# Connect to MySQL and run:
# SELECT COUNT(*) FROM grace_modules;
# Expected: 30
```

#### Step 6: Start Development Server

```bash
# Start the development server
pnpm dev

# Server will start on http://localhost:3000
```

#### Step 7: Build for Production

```bash
# Build the application
pnpm build

# Start production server
pnpm start
```

### 10.3 Verification Checklist

After installation, verify the following:

| Check | Expected Result | Command/Action |
|-------|-----------------|----------------|
| Server starts | No errors in console | `pnpm dev` |
| Database connected | "Database connected" log | Check server logs |
| 30 modules exist | Count = 30 | Query `grace_modules` table |
| Landing page loads | GRACE Academy header visible | Visit `/grace-academy` |
| Authentication works | Redirect to OAuth | Click "Sign In" |
| Module page loads | Four tabs visible | Visit `/grace-academy/module-1` |
| Quiz loads | 5 questions displayed | Click "The Gauntlet" tab |
| Tests pass | All green | `pnpm test` |

---

## 11. Environment Variables Specification

### 11.1 Required Variables

| Variable | Type | Description | Example |
|----------|------|-------------|---------|
| `DATABASE_URL` | String | MySQL connection string | `mysql://user:pass@host:3306/db` |
| `JWT_SECRET` | String | Secret for signing JWTs (min 32 chars) | `your-secure-secret-key-here` |
| `VITE_APP_ID` | String | Manus OAuth application ID | `app_abc123` |
| `OAUTH_SERVER_URL` | URL | Manus OAuth backend URL | `https://api.manus.im` |
| `VITE_OAUTH_PORTAL_URL` | URL | Manus login portal URL | `https://manus.im/oauth` |
| `OWNER_OPEN_ID` | String | Owner's Manus OpenID | `user_xyz789` |
| `OWNER_NAME` | String | Owner's display name | `John Doe` |

### 11.2 Optional Variables

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `BUILT_IN_FORGE_API_URL` | URL | - | Manus Forge API URL |
| `BUILT_IN_FORGE_API_KEY` | String | - | Forge API key (server-side) |
| `VITE_FRONTEND_FORGE_API_KEY` | String | - | Forge API key (client-side) |
| `VITE_FRONTEND_FORGE_API_URL` | URL | - | Forge API URL (client-side) |
| `VITE_ANALYTICS_ENDPOINT` | URL | - | Analytics endpoint |
| `VITE_ANALYTICS_WEBSITE_ID` | String | - | Analytics website ID |
| `VITE_APP_TITLE` | String | "GRACE Academy" | Application title |
| `VITE_APP_LOGO` | String | - | Logo image path |

### 11.3 Environment Variable Security

| Variable Pattern | Exposure | Notes |
|------------------|----------|-------|
| `VITE_*` | Client-side | Bundled into frontend, visible in browser |
| `BUILT_IN_*` | Server-side only | Never exposed to client |
| `JWT_SECRET` | Server-side only | Critical security - never expose |
| `DATABASE_URL` | Server-side only | Contains credentials - never expose |

---

## 12. Testing Specifications

### 12.1 Test Framework Configuration

The project uses Vitest for testing with the following configuration:

```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'node',
    include: ['server/**/*.test.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
    },
  },
});
```

### 12.2 Test Categories

| Category | Location | Purpose |
|----------|----------|---------|
| Unit Tests | `server/*.test.ts` | Test individual functions |
| Integration Tests | `server/integration/*.test.ts` | Test API procedures |
| E2E Tests | `e2e/*.test.ts` | Test full user flows |

### 12.3 Success Criteria by Level

#### Function Level
- Each database helper function has unit tests
- Edge cases covered (null inputs, empty arrays)
- Error handling verified

#### Module Level
- Each tRPC router has integration tests
- Authentication flows tested
- Input validation tested

#### System Level
- Full learning flow tested (Spark → Gauntlet → Crucible → Imprint)
- Certificate generation tested
- Admin workflows tested

### 12.4 Running Tests

```bash
# Run all tests
pnpm test

# Run with coverage
pnpm test --coverage

# Run specific test file
pnpm test server/grace.test.ts

# Run in watch mode
pnpm test --watch
```

---

## 13. Accessibility Compliance

### 13.1 WCAG 2.2 AA Compliance

The GRACE Academy system is designed to meet WCAG 2.2 AA standards:

| Criterion | Implementation | Status |
|-----------|----------------|--------|
| 1.1.1 Non-text Content | All images have alt text | ✅ |
| 1.3.1 Info and Relationships | Semantic HTML structure | ✅ |
| 1.4.1 Use of Color | Color not sole indicator | ✅ |
| 1.4.3 Contrast (Minimum) | 4.5:1 text contrast | ✅ |
| 2.1.1 Keyboard | All interactive elements keyboard accessible | ✅ |
| 2.4.1 Bypass Blocks | Skip navigation links | ✅ |
| 2.4.4 Link Purpose | Descriptive link text | ✅ |
| 3.1.1 Language of Page | `lang` attribute set | ✅ |
| 4.1.1 Parsing | Valid HTML | ✅ |
| 4.1.2 Name, Role, Value | ARIA attributes on custom components | ✅ |

### 13.2 Accessibility Features

- **Keyboard Navigation:** All interactive elements accessible via Tab key
- **Screen Reader Support:** Radix UI components provide ARIA labels
- **Focus Indicators:** Visible focus rings on all interactive elements
- **Color Contrast:** Minimum 4.5:1 ratio for text
- **Responsive Design:** Usable on all screen sizes
- **Reduced Motion:** Respects `prefers-reduced-motion` setting

---

## 14. Appendices

### 14.1 Glossary

| Term | Definition |
|------|------------|
| ABT | Action-Based Testing - testing methodology using reusable action components |
| GRACE | Governed Resilient Autonomous Certification for Enterprises |
| The Spark | 3-minute reading section introducing core concepts |
| The Gauntlet | 5-question multiple choice quiz assessment |
| The Crucible | Hands-on challenge requiring text submission |
| The Imprint | Thought experiment for lasting retention |
| GRACE Diploma | Final certification awarded upon completing all 30 modules |

### 14.2 References

1. React 19 Documentation: https://react.dev
2. tRPC Documentation: https://trpc.io
3. Drizzle ORM Documentation: https://orm.drizzle.team
4. Tailwind CSS Documentation: https://tailwindcss.com
5. shadcn/ui Components: https://ui.shadcn.com
6. WCAG 2.2 Guidelines: https://www.w3.org/WAI/WCAG22/quickref/

### 14.3 Change Log

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-31 | Initial document creation |

---

**Document End**

*This document was generated by Manus AI and represents the complete technical specification for the GRACE Academy learning management system. Any modifications to the system should be reflected in updates to this document.*
