import React, { useState, useEffect } from 'react';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';

export const CustomerSelector: React.FC = () => {
  const { customers, currentCustomerId, setCurrentCustomerId, loading, error, refresh, search, setSearch, page, setPage, total } = useCustomers();
  const { push } = useToast();
  const [input, setInput] = useState(search);
  // Debounce search
  useEffect(() => {
    const t = setTimeout(() => setSearch(input), 400);
    return () => clearTimeout(t);
  }, [input, setSearch]);
  const totalPages = Math.max(1, Math.ceil(total / 20));
  return (
    <div className="customer-selector">
      <div style={{ display: 'flex', gap: 4, alignItems: 'center', marginBottom: 4 }}>
        <input placeholder="Search customers" value={input} onChange={e => setInput(e.target.value)} style={{ flex: 1 }} />
        <button type="button" onClick={refresh} disabled={loading}>⟳</button>
      </div>
      <label>
        Customer:&nbsp;
        <select value={currentCustomerId ?? ''} onChange={e => setCurrentCustomerId(e.target.value || null)} disabled={loading}>
          <option value="">-- choose --</option>
          {customers.map(c => (
            <option key={c.id} value={c.id}>{c.firstName} {c.lastName} ({c.customerNumber})</option>
          ))}
        </select>
      </label>
      {loading && <span style={{ marginLeft: 8 }}>Loading...</span>}
  {error && <button onClick={() => { push('Customer list failed', 'error'); refresh(); }}>Retry</button>}
      <div style={{ marginTop: 4, display: 'flex', gap: 4, alignItems: 'center' }}>
        <button type="button" disabled={page <= 1 || loading} onClick={() => setPage(page - 1)}>Prev</button>
        <span style={{ fontSize: 12 }}>Page {page}/{totalPages}</span>
        <button type="button" disabled={page >= totalPages || loading} onClick={() => setPage(page + 1)}>Next</button>
      </div>
    </div>
  );
};
