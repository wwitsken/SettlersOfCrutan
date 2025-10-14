import { useEffect } from "react";
import { useNavigate } from "react-router";
import { useAuthStore } from "../auth/store";

export default function Logout() {
  const navigate = useNavigate();
  const logout = useAuthStore((s) => s.logout);

  useEffect(() => {
    (async () => {
      await logout();
      navigate("/login", { replace: true });
    })();
  }, [navigate, logout]);

  return (
    <div className="min-h-screen flex items-center justify-center text-gray-700">
      Logging out…
    </div>
  );
}
