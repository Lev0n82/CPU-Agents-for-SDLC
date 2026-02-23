import { Link } from "wouter";
import { Button } from "@/components/ui/button";
import { Menu, X, Github } from "lucide-react";
import { useState } from "react";

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const navigation = [
    { name: "Home", href: "/" },
    { name: "Features", href: "/features" },
    { name: "Architecture", href: "/architecture" },
    { name: "System Status", href: "/system-status" },
    { name: "AI Demo", href: "/ai-demo" },
    { name: "Testing Workflow", href: "/testing-workflow" },
    { name: "Quick Start", href: "/quick-start" },
    { name: "Docs", href: "/documentation" },
  ];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      {/* Header */}
      <header className="sticky top-0 z-50 bg-background border-b-2 border-border">
        <div className="container">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <Link href="/">
              <div className="flex items-center gap-3 cursor-pointer group">
                <div className="w-10 h-10 bg-primary flex items-center justify-center">
                  <div className="w-6 h-6 border-2 border-primary-foreground" />
                </div>
                <div>
                  <div className="font-display font-semibold text-foreground text-sm leading-tight group-hover:text-primary transition-colors">
                    CPU AGENTS
                  </div>
                  <div className="text-xs text-muted-foreground uppercase tracking-wider">
                    Phase 3.1-3.4 // 95% Ready
                  </div>
                </div>
              </div>
            </Link>

            {/* Desktop Navigation */}
            <nav className="hidden md:flex items-center gap-1">
              {navigation.map((item) => (
                <Link key={item.name} href={item.href}>
                  <Button
                    variant="ghost"
                    className="text-sm font-medium text-foreground hover:text-primary hover:bg-accent transition-colors px-4"
                  >
                    {item.name}
                  </Button>
                </Link>
              ))}
              <a
                href="https://github.com/Lev0n82/CPU-Agents-for-SDLC"
                target="_blank"
                rel="noopener noreferrer"
                className="ml-4"
              >
                <Button
                  variant="outline"
                  size="sm"
                  className="border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground font-medium"
                >
                  <Github className="w-4 h-4 mr-2" />
                  GitHub
                </Button>
              </a>
            </nav>

            {/* Mobile Menu Button */}
            <button
              className="md:hidden p-2 text-foreground hover:text-primary transition-colors"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label="Toggle menu"
            >
              {mobileMenuOpen ? (
                <X className="w-6 h-6" />
              ) : (
                <Menu className="w-6 h-6" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <div className="md:hidden border-t-2 border-border bg-card">
            <nav className="container py-4 flex flex-col gap-2">
              {navigation.map((item) => (
                <Link key={item.name} href={item.href}>
                  <Button
                    variant="ghost"
                    className="w-full justify-start text-foreground hover:text-primary hover:bg-accent"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    {item.name}
                  </Button>
                </Link>
              ))}
              <a
                href="https://github.com/Lev0n82/CPU-Agents-for-SDLC"
                target="_blank"
                rel="noopener noreferrer"
                className="mt-2"
              >
                <Button
                  variant="outline"
                  className="w-full border-2 border-primary text-primary hover:bg-primary hover:text-primary-foreground"
                >
                  <Github className="w-4 h-4 mr-2" />
                  View on GitHub
                </Button>
              </a>
            </nav>
          </div>
        )}
      </header>

      {/* Main Content */}
      <main id="main-content" className="flex-1">{children}</main>

      {/* Footer */}
      <footer className="border-t-2 border-border bg-card mt-auto">
        <div className="container py-12">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            {/* About */}
            <div className="md:col-span-2">
              <div className="flex items-center gap-3 mb-4">
                <div className="w-8 h-8 bg-primary flex items-center justify-center">
                  <div className="w-5 h-5 border-2 border-primary-foreground" />
                </div>
                <span className="font-display font-semibold text-foreground">
                  CPU AGENTS
                </span>
              </div>
              <p className="text-sm text-muted-foreground max-w-md leading-relaxed">
                Self-aware autonomous AI agents optimized for CPU execution on
                enterprise desktops. Complete SDLC automation without GPU
                requirements.
              </p>
            </div>

            {/* Quick Links */}
            <div>
              <h3 className="label-swiss mb-4">Quick Links</h3>
              <ul className="space-y-2">
                {navigation.slice(0, 4).map((item) => (
                  <li key={item.name}>
                    <Link href={item.href}>
                      <span className="text-sm text-muted-foreground hover:text-primary transition-colors cursor-pointer">
                        {item.name}
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Resources */}
            <div>
              <h3 className="label-swiss mb-4">Resources</h3>
              <ul className="space-y-2">
                <li>
                  <a
                    href="https://github.com/Lev0n82/CPU-Agents-for-SDLC"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-muted-foreground hover:text-primary transition-colors"
                  >
                    GitHub Repository
                  </a>
                </li>
                <li>
                  <Link href="/documentation">
                    <span className="text-sm text-muted-foreground hover:text-primary transition-colors cursor-pointer">
                      Documentation
                    </span>
                  </Link>
                </li>
                <li>
                  <Link href="/architecture">
                    <span className="text-sm text-muted-foreground hover:text-primary transition-colors cursor-pointer">
                      Architecture
                    </span>
                  </Link>
                </li>
                <li>
                  <Link href="/accessibility">
                    <span className="text-sm text-muted-foreground hover:text-primary transition-colors cursor-pointer">
                      Accessibility
                    </span>
                  </Link>
                </li>
              </ul>
            </div>
          </div>

          {/* Copyright */}
          <div className="mt-12 pt-8 border-t border-border">
            <div className="flex flex-col md:flex-row justify-between items-center gap-4">
              <p className="text-xs text-muted-foreground">
                © 2026 CPU Agents for SDLC. Open source project.
              </p>
              <div className="flex items-center gap-6">
                <span className="text-xs text-muted-foreground uppercase tracking-wider">
                  Version 3.0.0
                </span>
                <span className="text-xs text-muted-foreground uppercase tracking-wider">
                  Build 2026.02.22
                </span>
              </div>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
