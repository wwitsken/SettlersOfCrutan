import { Outlet, Link } from "react-router";
import { useAuthStore } from "../auth/store";

export default function AppLayout() {
  // Select booleans so conditional rendering works correctly
  const isAuthed = useAuthStore((s) => s.status === "authenticated");
  const isAdmin = useAuthStore((s) => s.hasRole("Admin"));
  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <header className="bg-white shadow">
        <div className="mx-auto max-w-6xl px-4 py-3 flex items-center justify-between">
          <Link to="/" className="text-lg font-semibold text-gray-900">
            Settlers Of Crutan
          </Link>
          <nav className="flex items-center gap-4 text-sm">
            <Link to="/" className="text-gray-700 hover:text-gray-900">
              Home
            </Link>
            {isAuthed && (
              <Link
                to="/preferences"
                className="text-gray-700 hover:text-gray-900"
              >
                Preferences
              </Link>
            )}
            <span className="text-gray-300">|</span>
            {!isAuthed && (
              <Link to="/login" className="text-gray-700 hover:text-gray-900">
                Login
              </Link>
            )}
            {isAuthed && (
              <Link to="/logout" className="text-gray-700 hover:text-gray-900">
                Logout
              </Link>
            )}
            {isAdmin && (
              <Link
                to="/create-user"
                className="text-gray-700 hover:text-gray-900"
              >
                Create User
              </Link>
            )}
            {isAuthed && (
              <Link
                to="/reset-password"
                className="text-gray-700 hover:text-gray-900"
              >
                Reset Password
              </Link>
            )}
          </nav>
        </div>
      </header>
      <main className="flex-1">
        <div className="mx-auto max-w-6xl p-4 pt-20">
          <Outlet />
        </div>
      </main>
      <footer className="bg-white border-t">
        <div className="mx-auto max-w-6xl px-4 py-3 text-xs text-gray-500">
          {new Date().getFullYear()} Settlers Of Crutan
        </div>
      </footer>
    </div>
  );
}
