import React, { createContext, useContext, useState, useCallback } from 'react';

export interface Toast { id: number; message: string; type?: 'error' | 'info' | 'success'; ttl?: number; }

interface ToastCtx {
  toasts: Toast[];
  push: (msg: string, type?: Toast['type'], ttlMs?: number) => void;
  dismiss: (id: number) => void;
}

const ToastContext = createContext<ToastCtx | undefined>(undefined);

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const push = useCallback((message: string, type: Toast['type'] = 'info', ttlMs = 4000) => {
    setToasts(ts => [...ts, { id: Date.now() + Math.random(), message, type, ttl: Date.now() + ttlMs }]);
  }, []);
  const dismiss = useCallback((id: number) => setToasts(ts => ts.filter(t => t.id !== id)), []);

  React.useEffect(() => {
    const i = setInterval(() => {
      const now = Date.now();
      setToasts(ts => ts.filter(t => !t.ttl || t.ttl > now));
    }, 1000);
    return () => clearInterval(i);
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, push, dismiss }}>
      {children}
      <div className="toast-container">
        {toasts.map(t => (
          <div key={t.id} className={`toast toast-${t.type}`}> 
            <span>{t.message}</span>
            <button onClick={() => dismiss(t.id)} aria-label="Dismiss" className="toast-x">×</button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
};
