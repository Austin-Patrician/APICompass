import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface SettingsState {
  theme: 'light' | 'dark' | 'system';
  apiEndpoint: string;
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  setApiEndpoint: (endpoint: string) => void;
}

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      theme: 'system',
      apiEndpoint: 'http://localhost:5000',
      setTheme: (theme) => set({ theme }),
      setApiEndpoint: (endpoint) => set({ apiEndpoint: endpoint }),
    }),
    {
      name: 'keychecker-settings',
    }
  )
);
