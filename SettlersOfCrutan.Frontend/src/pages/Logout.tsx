import { useEffect } from "react";
import { useNavigate } from "react-router";
import { api } from "../api/client";

export default function Logout() {
  const navigate = useNavigate();
  useEffect(() => {
    (async () => {
      try {
        await api.POST("/api/auth/logout");
      } catch {
        // ignore
      } finally {
        navigate("/login", { replace: true });
      }
    })();
  }, [navigate]);

  return (
    <div className="min-h-screen flex items-center justify-center text-gray-700">
      Logging out…
    </div>
  );
}
