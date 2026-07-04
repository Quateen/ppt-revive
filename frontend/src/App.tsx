// App.tsx
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "@/components/layouts/Layout";
import Index from "@/pages/Index";
import About from "@/pages/About";
import PresentationAnalyzer from "@/pages/PresentationAnalyzer";
import NotFound from "@/pages/NotFound";
import { useAppDispatch } from "./hooks/reduxHooks";
import { EventEmitter } from "@/common/eventEmitter";
import { showAuthModal } from "@/app-redux/ui/uiSlice";
import { useEffect } from "react";
import { Toaster } from "./components/ui/toaster";
import Profile from "./pages/Profile";
import ForgotPassword from "./pages/ForgotPassword";
import ResetPassword from "./pages/ResetPassword";
import PublicRoute from "./routes/PublicRoute";
import PrivateRoute from "./routes/PrivateRoute";
import ErrorBoundary from "@/components/ErrorBoundary";

const App = () => {

  const dispatch = useAppDispatch();

  useEffect(() => {
    const handler = () => {
      dispatch(showAuthModal()); // ✅ Open auth modal on 401
    };

    EventEmitter.on("unauthorized", handler);

    return () => {
      EventEmitter.off("unauthorized", handler);
    };
  }, [dispatch]);
  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Routes>
          {/* ✅ Routes that use Layout */}
          <Route element={<Layout />}>
            {/* Protected */}
            <Route path="/profile" element={
              <PrivateRoute><Profile /></PrivateRoute>
            } />

            {/* Open */}
            <Route path="/" element={<Index />} />
            <Route path="/about" element={<About />} />
            <Route path="/analyzer" element={<PresentationAnalyzer />} />

            {/* Public Only (not for logged-in users) */}
            <Route path="/forgot-password" element={
              <PublicRoute><ForgotPassword /></PublicRoute>
            } />
            <Route path="/reset-password" element={
              <PublicRoute><ResetPassword /></PublicRoute>
            } />
          </Route>

          {/* ❌ Routes that skip layout */}
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
      <Toaster />
    </ErrorBoundary>
  );
};

export default App;
