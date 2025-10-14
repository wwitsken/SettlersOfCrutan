// src/pages/Forbidden.tsx
import { useNavigate } from "react-router";

export default function Forbidden() {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col max-w-md m-auto text-center mt-12">
      <div className="space-y-4">
        <h1 className="text-3xl font-semibold text-gray-900">Access Denied</h1>
        <p className="text-gray-600">
          You don’t have permission to view this page.
        </p>

        <div className="flex justify-center">
          <svg
            className="w-24 h-24 text-red-500"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            viewBox="0 0 24 24"
          >
            <circle cx="12" cy="12" r="10" strokeWidth="1.5" />
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M9.75 9.75l4.5 4.5m0-4.5l-4.5 4.5"
            />
          </svg>
        </div>

        <p className="text-sm text-gray-500">
          If you believe this is an error, please contact your administrator.
        </p>

        <div className="space-y-2">
          <button
            onClick={() => navigate(-1)}
            className="w-full rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Go Back
          </button>
          <button
            onClick={() => navigate("/")}
            className="w-full rounded border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50"
          >
            Return Home
          </button>
        </div>
      </div>
    </div>
  );
}
