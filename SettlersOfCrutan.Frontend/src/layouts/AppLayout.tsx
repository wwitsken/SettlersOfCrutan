import { Outlet } from "react-router";

export default function AppLayout() {
  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <header className="bg-white shadow">
        <div className="mx-auto max-w-6xl px-4 py-3 flex items-center justify-between">
          <a href="/" className="text-lg font-semibold text-gray-900">
            Settlers Of Crutan
          </a>
          <nav className="flex items-center gap-4 text-sm">
            <a href="/" className="text-gray-700 hover:text-gray-900">
              Home
            </a>
            <a
              href="/preferences"
              className="text-gray-700 hover:text-gray-900"
            >
              Preferences
            </a>
            <span className="text-gray-300">|</span>
            <a href="/login" className="text-gray-700 hover:text-gray-900">
              Login
            </a>
            <a href="/logout" className="text-gray-700 hover:text-gray-900">
              Logout
            </a>
            <a
              href="/create-user"
              className="text-gray-700 hover:text-gray-900"
            >
              Create User
            </a>
            <a
              href="/reset-password"
              className="text-gray-700 hover:text-gray-900"
            >
              Reset Password
            </a>
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
