# CPU Agents Deployment Website

Interactive deployment guide and documentation website for the CPU Agents for SDLC system.

## Overview

This directory contains the complete React-based deployment website that provides:

- **Interactive Documentation**: Complete system documentation with examples
- **Architecture Visualization**: System architecture diagrams with cardinality notation
- **AI Capabilities Demo**: Interactive demonstrations of AI-powered features
- **Testing Workflow Examples**: Requirements → Manual Tests → Automated Tests pipeline
- **Quick Start Guide**: Step-by-step deployment instructions
- **System Status Dashboard**: Real-time module health monitoring

## Technology Stack

- **Framework**: React 19 + Vite
- **Styling**: Tailwind CSS 4
- **UI Components**: shadcn/ui
- **Routing**: Wouter (client-side routing)
- **Icons**: Lucide React

## Project Structure

```
website/
├── src/
│   ├── pages/              # Page components
│   │   ├── Home.tsx        # Homepage with value proposition
│   │   ├── Features.tsx    # Phase 3.1-3.4 feature showcase
│   │   ├── Architecture.tsx # System architecture
│   │   ├── SystemStatus.tsx # Module health dashboard
│   │   ├── AIDemo.tsx      # AI capabilities demo
│   │   ├── TestingWorkflow.tsx # Testing example
│   │   ├── QuickStart.tsx  # Deployment guide
│   │   └── Documentation.tsx # Documentation hub
│   ├── components/         # Reusable components
│   │   ├── Layout.tsx      # Main layout with navigation
│   │   ├── SEO.tsx         # SEO meta tags
│   │   └── ui/             # shadcn/ui components
│   ├── App.tsx             # Routes configuration
│   ├── main.tsx            # React entry point
│   └── index.css           # Global styles
├── public/                 # Static assets
├── index.html              # HTML template
└── package.json            # Dependencies

```

## Key Features

### 1. Local AI Focus
- Emphasizes vLLM and Ollama for local CPU-based AI inference
- Supports Granite 4, Phi-3, Llama 3 models
- 100% local execution, zero cloud dependencies

### 2. Mobile Micro Agents
- Native iOS (Swift/Neural Engine) and Android (Kotlin/TPU) agents
- On-device SLMs (1-3B parameters)
- Offline-first capabilities with Azure DevOps sync

### 3. Comprehensive Testing
- 8 test types: Unit, Integration, System, E2E, Functional, Non-Functional, Security, Accessibility
- WCAG 2.2 AAA certification
- 95%+ code coverage target

### 4. Production-Grade Architecture
- 45 classes across Phase 3.1-3.4
- 302 acceptance criteria
- OpenTelemetry observability
- Resilience patterns (circuit breaker, retry, timeout)

## Development

### Prerequisites
- Node.js 22+
- pnpm package manager

### Setup
```bash
cd website
pnpm install
pnpm dev
```

### Build
```bash
pnpm build
```

### Preview Production Build
```bash
pnpm preview
```

## Deployment

The website is designed for static hosting and can be deployed to:

- **Manus Platform**: Integrated deployment via Manus webdev tools
- **GitHub Pages**: Static site hosting
- **Netlify/Vercel**: Automatic deployments from Git
- **Azure Static Web Apps**: Enterprise hosting with CDN
- **Self-Hosted**: Nginx/Apache static file serving

### Build Output

The production build generates optimized static files in the `dist/` directory:

```bash
pnpm build
# Output: dist/
```

## Pages Overview

### Home (`/`)
System overview, value proposition, key metrics (95% complete, 45 classes, 4 AI features)

### Features (`/features`)
Complete Phase 3.1-3.4 module showcase with detailed capabilities:
- Phase 3.1: Authentication, Concurrency, Secrets, Work Items
- Phase 3.2: Test Plans, Git, Offline Sync, Workspace
- Phase 3.3: Resilience, Observability, Performance
- Phase 3.4: Test Lifecycle, Migration
- Testing & QA: 8 test types with coverage metrics

### Architecture (`/architecture`)
System architecture diagram showing:
- Workstation minions (Windows/Linux)
- Mobile micro agents (iOS/Android)
- Agent orchestration and cardinality relationships
- Deployment topology

### System Status (`/system-status`)
Real-time module health dashboard:
- 13 modules across Phase 3.1-3.4
- Build health indicators
- Test results (68/68 passing)
- Deployment readiness status

### AI Demo (`/ai-demo`)
Interactive AI capabilities demonstration:
- Code Review Analysis
- Test Obsolescence Detection
- Conflict Resolution Intelligence
- Root Cause Analysis
- Auto-scroll to expanded examples

### Testing Workflow (`/testing-workflow`)
Complete example showing Requirements → Manual Tests → Automated Tests:
- User Login Authentication user story
- AI-generated manual test cases
- xUnit (C#) and Playwright (TypeScript) automated tests

### Quick Start (`/quick-start`)
Step-by-step deployment guide:
- Prerequisites (Windows 11, .NET 8.0, vLLM/Ollama)
- Installation steps
- Configuration examples (appsettings.json)
- Troubleshooting guide

### Documentation (`/documentation`)
Comprehensive documentation hub:
- Getting Started guides
- Architecture documents
- Phase 3.1-3.4 specifications
- API documentation
- Deployment guides

## Design System

### Typography
- **Display Font**: Space Grotesk (headings)
- **Body Font**: Inter (content)

### Color Palette
- **Primary**: Blue (#2563eb)
- **Background**: Light gray (#f9fafb)
- **Foreground**: Dark gray (#0f172a)
- **Accent**: Blue/10 (#dbeafe)

### Layout
- **Container**: Max-width with auto-centering and responsive padding
- **Spacing**: Consistent 4px grid system
- **Breakpoints**: Mobile-first responsive design

## SEO Optimization

Each page includes comprehensive SEO meta tags:
- Title and description
- Keywords
- Open Graph tags
- Twitter Card tags

## Accessibility

- WCAG 2.2 AA compliant
- Semantic HTML
- Keyboard navigation support
- Screen reader friendly
- Focus indicators
- Alt text for all images

## Browser Support

- Chrome/Edge (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Mobile browsers (iOS Safari, Chrome Mobile)

## License

This website is part of the CPU Agents for SDLC open source project.

## Links

- **Live Website**: [Deployment URL]
- **GitHub Repository**: https://github.com/Lev0n82/CPU-Agents-for-SDLC
- **Documentation**: See `/docs` directory in repository root
