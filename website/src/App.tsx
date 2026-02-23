import { Toaster } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import NotFound from "@/pages/NotFound";
import { Route, Switch } from "wouter";
import ErrorBoundary from "./components/ErrorBoundary";
import { ThemeProvider } from "./contexts/ThemeContext";
import Home from "./pages/Home";
import QuickStart from "./pages/QuickStart";
import WindowsDeployment from "./pages/WindowsDeployment";
import PodmanDeployment from "./pages/PodmanDeployment";
import Architecture from "./pages/Architecture";
import Documentation from "./pages/Documentation";
import Phase31 from "@/pages/Phase31";
import Phase32 from "@/pages/Phase32";
import Phase33 from "@/pages/Phase33";
import Phase34 from "@/pages/Phase34";
import Features from "@/pages/Features";
import Accessibility from "@/pages/Accessibility";
import SystemStatus from "@/pages/SystemStatus";
import AIDemo from "@/pages/AIDemo";
import TestingWorkflow from "@/pages/TestingWorkflow";

function Router() {
  return (
    <Switch>
      <Route path={"/"} component={Home} />
      <Route path={"/quick-start"} component={QuickStart} />
      <Route path={"/windows-deployment"} component={WindowsDeployment} />
      <Route path={"/podman-deployment"} component={PodmanDeployment} />
      <Route path={"/architecture"} component={Architecture} />
      <Route path={"/documentation"} component={Documentation} />
      <Route path="/features" component={Features} />
      <Route path="/system-status" component={SystemStatus} />
        <Route path="/ai-demo" component={AIDemo} />
        <Route path="/testing-workflow" component={TestingWorkflow} />
      <Route path="/phase-3-1" component={Phase31} />
      <Route path="/phase-3-2" component={Phase32} />
      <Route path="/phase-3-3" component={Phase33} />
      <Route path="/phase-3-4" component={Phase34} />
      <Route path="/accessibility" component={Accessibility} />
      <Route path={"/404"} component={NotFound} />
      <Route component={NotFound} />
    </Switch>
  );
}

function App() {
  return (
    <ErrorBoundary>
      <ThemeProvider defaultTheme="light">
        <TooltipProvider>
          <Toaster />
          <Router />
        </TooltipProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}

export default App;
