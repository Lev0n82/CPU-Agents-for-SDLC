import { ArrowRight, CheckCircle2, Code, FileText, TestTube } from "lucide-react";
import { Card } from "@/components/ui/card";
import { SEO } from "@/components/SEO";

export default function TestingWorkflow() {
  return (
    <>
      <SEO 
        title="Testing Workflow Example - Requirements to Automated Tests"
        description="Complete example showing how CPU Agents analyze requirements, generate manual test cases, and create automated tests using AI-powered analysis with vLLM/Ollama."
        keywords="requirements analysis, manual test cases, automated testing, test generation, AI testing, xUnit, Playwright, test automation workflow"
      />
      
      <div className="container py-16">
        {/* Header */}
        <div className="text-center mb-16">
          <h1 className="text-4xl md:text-5xl font-display font-bold text-foreground mb-4">
            Testing Workflow Example
          </h1>
          <p className="text-lg text-muted-foreground max-w-3xl mx-auto">
            Complete workflow demonstrating how CPU Agents analyze requirements, generate comprehensive manual test cases, 
            and automatically create executable automated tests using local AI models.
          </p>
        </div>

        {/* Workflow Steps */}
        <div className="grid md:grid-cols-3 gap-8 mb-16">
          <Card className="p-6 border-2 border-primary/20">
            <div className="w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
              <FileText className="w-6 h-6 text-primary" />
            </div>
            <h3 className="text-xl font-display font-semibold text-foreground mb-2">
              1. Requirements Analysis
            </h3>
            <p className="text-sm text-muted-foreground">
              AI agent analyzes user story and acceptance criteria to identify test scenarios
            </p>
          </Card>

          <Card className="p-6 border-2 border-primary/20">
            <div className="w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
              <TestTube className="w-6 h-6 text-primary" />
            </div>
            <h3 className="text-xl font-display font-semibold text-foreground mb-2">
              2. Manual Test Cases
            </h3>
            <p className="text-sm text-muted-foreground">
              Generates detailed manual test cases with steps, expected results, and test data
            </p>
          </Card>

          <Card className="p-6 border-2 border-primary/20">
            <div className="w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center mb-4">
              <Code className="w-6 h-6 text-primary" />
            </div>
            <h3 className="text-xl font-display font-semibold text-foreground mb-2">
              3. Automated Tests
            </h3>
            <p className="text-sm text-muted-foreground">
              Converts manual tests into executable xUnit and Playwright test code
            </p>
          </Card>
        </div>

        {/* Example: Step 1 - Requirements */}
        <div className="mb-12">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              1
            </div>
            <h2 className="text-3xl font-display font-bold text-foreground">
              Requirements Analysis
            </h2>
          </div>

          <Card className="p-6 bg-muted/30">
            <h3 className="text-lg font-semibold text-foreground mb-4">Input: User Story</h3>
            <div className="bg-background p-4 rounded-lg border border-border mb-6">
              <p className="text-sm font-mono text-foreground mb-4">
                <strong>User Story #1234:</strong> User Login Authentication
              </p>
              <p className="text-sm text-muted-foreground mb-4">
                As a registered user, I want to log in to the system using my email and password, 
                so that I can access my personalized dashboard and account features.
              </p>
              <p className="text-sm font-semibold text-foreground mb-2">Acceptance Criteria:</p>
              <ul className="text-sm text-muted-foreground space-y-2 list-disc list-inside">
                <li>User can enter email address and password</li>
                <li>System validates credentials against database</li>
                <li>Successful login redirects to dashboard</li>
                <li>Failed login shows error message "Invalid credentials"</li>
                <li>Account locks after 5 failed attempts</li>
                <li>Password must be masked during entry</li>
                <li>Remember me checkbox persists session for 30 days</li>
              </ul>
            </div>

            <h3 className="text-lg font-semibold text-foreground mb-4">AI Analysis Output</h3>
            <div className="bg-background p-4 rounded-lg border border-border">
              <pre className="text-xs text-muted-foreground overflow-x-auto">
{`{
  "requirement_id": "US-1234",
  "test_scenarios_identified": 7,
  "test_categories": ["functional", "security", "ui"],
  "scenarios": [
    {
      "id": "TC-1234-001",
      "title": "Successful login with valid credentials",
      "priority": "high",
      "category": "functional"
    },
    {
      "id": "TC-1234-002",
      "title": "Failed login with invalid password",
      "priority": "high",
      "category": "functional"
    },
    {
      "id": "TC-1234-003",
      "title": "Account lockout after 5 failed attempts",
      "priority": "high",
      "category": "security"
    },
    {
      "id": "TC-1234-004",
      "title": "Password masking verification",
      "priority": "medium",
      "category": "ui"
    },
    {
      "id": "TC-1234-005",
      "title": "Remember me functionality",
      "priority": "medium",
      "category": "functional"
    }
  ]
}`}
              </pre>
            </div>
          </Card>
        </div>

        <div className="flex justify-center mb-12">
          <ArrowRight className="w-8 h-8 text-primary" />
        </div>

        {/* Example: Step 2 - Manual Test Cases */}
        <div className="mb-12">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              2
            </div>
            <h2 className="text-3xl font-display font-bold text-foreground">
              Manual Test Case Generation
            </h2>
          </div>

          <Card className="p-6 bg-muted/30">
            <h3 className="text-lg font-semibold text-foreground mb-4">Generated Manual Test Case</h3>
            <div className="bg-background p-6 rounded-lg border border-border space-y-6">
              <div>
                <p className="text-sm font-semibold text-foreground mb-2">Test Case ID: TC-1234-001</p>
                <p className="text-sm font-semibold text-foreground mb-2">Title: Successful login with valid credentials</p>
                <p className="text-sm text-muted-foreground mb-2"><strong>Priority:</strong> High</p>
                <p className="text-sm text-muted-foreground mb-2"><strong>Category:</strong> Functional</p>
              </div>

              <div>
                <p className="text-sm font-semibold text-foreground mb-3">Preconditions:</p>
                <ul className="text-sm text-muted-foreground space-y-1 list-disc list-inside">
                  <li>User account exists in database (email: test@example.com, password: Test123!)</li>
                  <li>User is not currently logged in</li>
                  <li>Login page is accessible</li>
                </ul>
              </div>

              <div>
                <p className="text-sm font-semibold text-foreground mb-3">Test Steps:</p>
                <ol className="text-sm text-muted-foreground space-y-3 list-decimal list-inside">
                  <li>
                    <strong>Navigate to login page</strong>
                    <p className="ml-6 mt-1">Expected: Login form displays with email and password fields</p>
                  </li>
                  <li>
                    <strong>Enter valid email: test@example.com</strong>
                    <p className="ml-6 mt-1">Expected: Email field accepts input</p>
                  </li>
                  <li>
                    <strong>Enter valid password: Test123!</strong>
                    <p className="ml-6 mt-1">Expected: Password field shows masked characters (•••••••)</p>
                  </li>
                  <li>
                    <strong>Click "Login" button</strong>
                    <p className="ml-6 mt-1">Expected: System validates credentials and redirects to dashboard</p>
                  </li>
                  <li>
                    <strong>Verify dashboard page loads</strong>
                    <p className="ml-6 mt-1">Expected: User dashboard displays with personalized content</p>
                  </li>
                </ol>
              </div>

              <div>
                <p className="text-sm font-semibold text-foreground mb-2">Expected Result:</p>
                <p className="text-sm text-muted-foreground">
                  User successfully logs in and is redirected to dashboard within 2 seconds. 
                  Session token is created and stored. User's name appears in header.
                </p>
              </div>

              <div>
                <p className="text-sm font-semibold text-foreground mb-2">Test Data:</p>
                <ul className="text-sm text-muted-foreground space-y-1 list-disc list-inside">
                  <li>Valid Email: test@example.com</li>
                  <li>Valid Password: Test123!</li>
                  <li>Expected Dashboard URL: /dashboard</li>
                </ul>
              </div>
            </div>
          </Card>
        </div>

        <div className="flex justify-center mb-12">
          <ArrowRight className="w-8 h-8 text-primary" />
        </div>

        {/* Example: Step 3 - Automated Tests */}
        <div className="mb-12">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-primary rounded-full flex items-center justify-center text-white font-bold">
              3
            </div>
            <h2 className="text-3xl font-display font-bold text-foreground">
              Automated Test Generation
            </h2>
          </div>

          <div className="grid md:grid-cols-2 gap-6">
            {/* xUnit Test */}
            <Card className="p-6 bg-muted/30">
              <h3 className="text-lg font-semibold text-foreground mb-4 flex items-center gap-2">
                <Code className="w-5 h-5 text-primary" />
                xUnit Integration Test (C#)
              </h3>
              <div className="bg-background p-4 rounded-lg border border-border">
                <pre className="text-xs text-muted-foreground overflow-x-auto">
{`using Xunit;
using FluentAssertions;

namespace AuthTests
{
    public class LoginTests : IClassFixture<WebAppFactory>
    {
        private readonly HttpClient _client;
        
        public LoginTests(WebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        [Trait("Category", "Functional")]
        [Trait("Priority", "High")]
        public async Task Login_WithValidCredentials_RedirectsToDashboard()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", 
                loginRequest
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();
            
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.RedirectUrl.Should().Be("/dashboard");
            result.SessionToken.Should().NotBeNullOrEmpty();
            
            // Verify session token is valid
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", result.SessionToken);
            
            var dashboardResponse = await _client.GetAsync("/dashboard");
            dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Security")]
        public async Task Login_WithInvalidPassword_ReturnsError()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", 
                loginRequest
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            
            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();
            
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid credentials");
        }
    }
}`}
                </pre>
              </div>
            </Card>

            {/* Playwright E2E Test */}
            <Card className="p-6 bg-muted/30">
              <h3 className="text-lg font-semibold text-foreground mb-4 flex items-center gap-2">
                <Code className="w-5 h-5 text-primary" />
                Playwright E2E Test (TypeScript)
              </h3>
              <div className="bg-background p-4 rounded-lg border border-border">
                <pre className="text-xs text-muted-foreground overflow-x-auto">
{`import { test, expect } from '@playwright/test';

test.describe('User Login Authentication', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('TC-1234-001: Successful login with valid credentials', 
    async ({ page }) => {
    // Step 1: Verify login form displays
    await expect(page.locator('input[name="email"]'))
      .toBeVisible();
    await expect(page.locator('input[name="password"]'))
      .toBeVisible();

    // Step 2: Enter valid email
    await page.fill('input[name="email"]', 'test@example.com');

    // Step 3: Enter valid password and verify masking
    const passwordInput = page.locator('input[name="password"]');
    await expect(passwordInput).toHaveAttribute('type', 'password');
    await passwordInput.fill('Test123!');

    // Step 4: Click login button
    await page.click('button[type="submit"]');

    // Step 5: Verify redirect to dashboard
    await expect(page).toHaveURL('/dashboard', { 
      timeout: 2000 
    });

    // Verify dashboard content
    await expect(page.locator('h1')).toContainText('Dashboard');
    
    // Verify user name in header
    await expect(page.locator('[data-testid="user-name"]'))
      .toBeVisible();
  });

  test('TC-1234-002: Failed login with invalid password', 
    async ({ page }) => {
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'WrongPassword');
    await page.click('button[type="submit"]');

    // Verify error message
    await expect(page.locator('[role="alert"]'))
      .toContainText('Invalid credentials');

    // Verify still on login page
    await expect(page).toHaveURL('/login');
  });

  test('TC-1234-003: Account lockout after 5 failed attempts', 
    async ({ page }) => {
    // Attempt login 5 times with wrong password
    for (let i = 0; i < 5; i++) {
      await page.fill('input[name="email"]', 'test@example.com');
      await page.fill('input[name="password"]', 'Wrong' + i);
      await page.click('button[type="submit"]');
      await page.waitForTimeout(500);
    }

    // Verify account locked message
    await expect(page.locator('[role="alert"]'))
      .toContainText('Account locked');

    // Verify login button is disabled
    await expect(page.locator('button[type="submit"]'))
      .toBeDisabled();
  });
});`}
                </pre>
              </div>
            </Card>
          </div>
        </div>

        {/* Key Benefits */}
        <Card className="p-8 bg-primary/5 border-2 border-primary/20">
          <h2 className="text-2xl font-display font-bold text-foreground mb-6 text-center">
            Key Benefits of AI-Generated Tests
          </h2>
          <div className="grid md:grid-cols-3 gap-6">
            <div className="flex items-start gap-3">
              <CheckCircle2 className="w-5 h-5 text-primary flex-shrink-0 mt-1" />
              <div>
                <h3 className="font-semibold text-foreground mb-1">Comprehensive Coverage</h3>
                <p className="text-sm text-muted-foreground">
                  AI identifies all test scenarios from requirements, ensuring no edge cases are missed
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <CheckCircle2 className="w-5 h-5 text-primary flex-shrink-0 mt-1" />
              <div>
                <h3 className="font-semibold text-foreground mb-1">Time Savings</h3>
                <p className="text-sm text-muted-foreground">
                  Reduces test creation time from hours to minutes, accelerating delivery cycles
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <CheckCircle2 className="w-5 h-5 text-primary flex-shrink-0 mt-1" />
              <div>
                <h3 className="font-semibold text-foreground mb-1">Consistency</h3>
                <p className="text-sm text-muted-foreground">
                  Standardized test structure and naming conventions across all test cases
                </p>
              </div>
            </div>
          </div>
        </Card>

        {/* CTA */}
        <div className="mt-16 text-center">
          <h2 className="text-2xl font-display font-bold text-foreground mb-4">
            Ready to Automate Your Testing?
          </h2>
          <p className="text-muted-foreground mb-8 max-w-2xl mx-auto">
            Deploy CPU Agents to automatically generate comprehensive test suites from your requirements, 
            reducing manual testing overhead by up to 70%.
          </p>
          <div className="flex gap-4 justify-center">
            <a 
              href="/quick-start" 
              className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors font-semibold"
            >
              Get Started
              <ArrowRight className="w-4 h-4" />
            </a>
            <a 
              href="/documentation" 
              className="inline-flex items-center gap-2 px-6 py-3 border-2 border-primary text-primary rounded-lg hover:bg-primary/10 transition-colors font-semibold"
            >
              View Documentation
            </a>
          </div>
        </div>
      </div>
    </>
  );
}
