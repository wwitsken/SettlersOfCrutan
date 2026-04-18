import { useState } from "react";

export function useIdentity() {
  const [name, setNameState] = useState<string>(
    () => localStorage.getItem("catan_display_name") ?? "Adventurer",
  );
  const [color, setColorState] = useState<string>(
    () => localStorage.getItem("catan_color") ?? "blue",
  );

  const setName = (n: string) => {
    setNameState(n);
    localStorage.setItem("catan_display_name", n);
    // STUB: wire API call here to update display name in lobby
    // e.g. await api.POST("/api/lobby/{lobbyId}/display-name", { body: { displayName: n } })
  };
  const setColor = (c: string) => {
    setColorState(c);
    localStorage.setItem("catan_color", c);
    // STUB: wire API call here when backend supports player color selection
  };

  return { name, color, setName, setColor };
}
