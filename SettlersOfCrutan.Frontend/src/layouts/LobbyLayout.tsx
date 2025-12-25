import { Outlet } from "react-router";
import { useSignalR } from "../hooks/useSignalR";
// import { useLobbyStore } from "../stores/lobbyStore";

export function LobbyLayout() {
  // const joinLobby = useLobbyStore((s) => s.joinLobby);
  // const leaveLobby = useLobbyStore((s) => s.leaveLobby);
  // const setReady = useLobbyStore((s) => s.setReady);
  // const setUnready = useLobbyStore((s) => s.setUnready);
  const { isConnected, isConnecting, error } = useSignalR("/api/realtime-hub", {
    autoConnect: true,
    handlers: {
      LobbyReceive: (lobbyId: string, eventName: string, payload: object) =>
        console.log(
          `lobbyId: ${lobbyId}\n eventName: ${eventName}\n payload: ${JSON.stringify(
            payload
          )}`
        ),
    },
  });

  if (isConnecting && !isConnected) {
    return <div className="p-4">Connecting to game services...</div>;
  }

  if (error && !isConnected) {
    return (
      <div className="p-4 text-red-600">
        Could not connect to game services: {error}
      </div>
    );
  }

  return <Outlet />;
}
