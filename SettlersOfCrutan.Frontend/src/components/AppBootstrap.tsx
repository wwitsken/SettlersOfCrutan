import { useEffect } from "react";
import { useAuthStore } from "../auth/store";

export default function AppBootstrap({
  children,
}: {
  children: React.ReactNode;
}) {
  const init = useAuthStore((s) => s.init);
  const refresh = useAuthStore((s) => s.refresh);

  useEffect(() => {
    init();

    const onFocus = () => refresh();
    window.addEventListener("focus", onFocus);
    document.addEventListener("visibilitychange", onFocus);
    return () => {
      window.removeEventListener("focus", onFocus);
      document.removeEventListener("visibilitychange", onFocus);
    };
  }, [init, refresh]);

  return <>{children}</>;
}
