// src/auth/guards.tsx
import { Navigate, Outlet, useLocation } from "react-router";
import type { PropsWithChildren } from "react";
import { useAuthStore } from "../stores/authStore";

export function RequireAuth({ children }: PropsWithChildren) {
  const status = useAuthStore((s) => s.status);
  const location = useLocation();

  if (status === "loading" || status === "idle") {
    return <div className="p-6">Loading…</div>;
  }
  if (status === "unauthenticated") {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }
  return children ? <>{children}</> : <Outlet />;
}

export function RequireRole({
  roles,
  children,
}: PropsWithChildren<{ roles: string[] }>) {
  const status = useAuthStore((s) => s.status);
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);

  if (status === "loading" || status === "idle") {
    return <div className="p-6">Loading…</div>;
  }
  if (status === "unauthenticated") {
    return <Navigate to="/login" replace />;
  }
  if (!hasAnyRole(roles)) {
    return <Navigate to="/forbidden" replace />;
  }
  return children ? <>{children}</> : <Outlet />;
}
