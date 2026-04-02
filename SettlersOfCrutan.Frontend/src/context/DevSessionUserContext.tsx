import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import { DEV_SESSION_USER_STORAGE_KEY } from "../auth/devSessionUser";

export type DevSessionUserContextValue = {
  devUserId: string;
  setDevUserId: (value: string) => void;
};

const DevSessionUserContext = createContext<DevSessionUserContextValue | null>(
  null,
);

export function DevSessionUserProvider({ children }: PropsWithChildren) {
  const [devUserId, setDevUserIdState] = useState(() => {
    if (!import.meta.env.DEV || typeof sessionStorage === "undefined") return "";
    return sessionStorage.getItem(DEV_SESSION_USER_STORAGE_KEY)?.trim() ?? "";
  });

  const setDevUserId = useCallback((value: string) => {
    setDevUserIdState(value);
    if (!import.meta.env.DEV || typeof sessionStorage === "undefined") return;
    const trimmed = value.trim();
    if (trimmed) sessionStorage.setItem(DEV_SESSION_USER_STORAGE_KEY, trimmed);
    else sessionStorage.removeItem(DEV_SESSION_USER_STORAGE_KEY);
  }, []);

  const value = useMemo(
    () => ({ devUserId, setDevUserId }),
    [devUserId, setDevUserId],
  );

  return (
    <DevSessionUserContext.Provider value={value}>
      {children}
    </DevSessionUserContext.Provider>
  );
}

export function useDevSessionUser(): DevSessionUserContextValue {
  const ctx = useContext(DevSessionUserContext);
  if (!ctx) {
    return {
      devUserId: "",
      setDevUserId: () => {},
    };
  }
  return ctx;
}
