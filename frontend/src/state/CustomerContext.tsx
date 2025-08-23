import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { api, PagingEnvelope } from '../api/client';

export interface CustomerSummary {
  id: string;
  customerNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  enabled: boolean;
  registrationDate?: string; // added for list display
}

interface CustomerContextValue {
  currentCustomerId: string | null;
  setCurrentCustomerId: (id: string | null) => void;
  customers: CustomerSummary[];
  search: string;
  setSearch: (s: string) => void;
  page: number;
  setPage: (p: number) => void;
  total: number;
  refresh: () => void;
  loading: boolean;
  error: boolean;
}

const CustomerContext = createContext<CustomerContextValue | undefined>(undefined);

async function fetchCustomers(page: number, search: string): Promise<PagingEnvelope<CustomerSummary>> {
  const resp = await api.get('/customers', { params: { page, pageSize: 20, status: 'enabled', sortBy: 'registrationDate', sortDir: 'desc', search: search || undefined } });
  return resp.data;
}

export const CustomerProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [currentCustomerId, setCurrentCustomerIdState] = useState<string | null>(() => localStorage.getItem('currentCustomerId'));
  const [customers, setCustomers] = useState<CustomerSummary[]>([]);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(false);

  const load = useCallback(async () => {
    setLoading(true); setError(false);
    try {
      const data = await fetchCustomers(page, search);
      setCustomers(data.items);
      setTotal(data.total);
    } catch {
      setError(true);
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  useEffect(() => { load(); }, [load]);
  // Reset page when search changes
  useEffect(() => { setPage(1); }, [search]);

  const setCurrentCustomerId = useCallback((id: string | null) => {
    setCurrentCustomerIdState(id);
    if (id) localStorage.setItem('currentCustomerId', id); else localStorage.removeItem('currentCustomerId');
  }, []);

  const value: CustomerContextValue = {
    currentCustomerId,
    setCurrentCustomerId,
  customers,
  search,
  setSearch,
  page,
  setPage,
  total,
    refresh: load,
    loading,
    error
  };

  return <CustomerContext.Provider value={value}>{children}</CustomerContext.Provider>;
};

export const useCustomers = () => {
  const ctx = useContext(CustomerContext);
  if (!ctx) throw new Error('useCustomers must be used within CustomerProvider');
  return ctx;
};
