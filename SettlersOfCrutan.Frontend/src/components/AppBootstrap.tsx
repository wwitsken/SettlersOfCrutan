import { useEffect } from "react";
import { useAuthStore } from "../stores/authStore";

export default function AppBootstrap({
  children,
}: {
  children: React.ReactNode;
}) {
  const init = useAuthStore((s) => s.init);

  useEffect(() => {
    init();
  }, [init]);

  return <>{children}</>;
}
