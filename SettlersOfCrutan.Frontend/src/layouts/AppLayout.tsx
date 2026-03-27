import { Outlet, Link } from "react-router";
import { useMsal } from "@azure/msal-react";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";

export default function AppLayout() {
  const { instance } = useMsal();
  return (
    <div className="flex min-h-screen flex-col bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <Link
            to="/"
            className="text-lg font-semibold tracking-tight text-slate-900"
          >
            Settlers of Crutan
          </Link>
          <nav className="flex items-center gap-4 text-sm">
            <Link to="/" className="text-slate-600 hover:text-slate-900">
              Home
            </Link>
            <UnauthenticatedTemplate>
              <button
                type="button"
                className="rounded-lg bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800"
                onClick={async () => await instance.loginRedirect()}
              >
                Sign in
              </button>
            </UnauthenticatedTemplate>
            <AuthenticatedTemplate>
              <button
                type="button"
                className="text-slate-600 hover:text-slate-900"
                onClick={async () => await instance.logout()}
              >
                Sign out
              </button>
            </AuthenticatedTemplate>
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <div className="mx-auto max-w-6xl p-4 md:p-6">
          <Outlet />
        </div>
      </main>
      <footer className="border-t border-slate-200 bg-white">
        <div className="mx-auto max-w-6xl px-4 py-3 text-xs text-slate-500">
          {new Date().getFullYear()} Settlers of Crutan
        </div>
      </footer>
    </div>
  );
}
