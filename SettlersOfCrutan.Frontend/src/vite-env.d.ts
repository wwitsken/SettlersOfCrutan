/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_AUTH_AUDIENCE?: string;
  readonly VITE_PORT?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
