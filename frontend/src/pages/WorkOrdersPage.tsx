import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { api, PagingEnvelope } from '../api/client';
import { useAuth } from '../state/AuthContext';
import { CustomerSelector } from '../components/CustomerSelector';
import { CreateCustomerForm } from '../components/CreateCustomerForm';
import { CreateWorkOrderForm } from '../components/CreateWorkOrderForm';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';
import { CustomerDetail } from '../components/CustomerDetail';
import { Logo } from '../components/Logo';
import { AppointmentsPanel } from '../components/AppointmentsPanel';

interface WorkOrderSummary {
  id: string;
  status: string;
  total: number;
  subtotal: number;
  discount: number;
}

async function fetchWorkOrders(customerId: string, page: number): Promise<PagingEnvelope<WorkOrderSummary>> {
  const resp = await api.get(`/customers/${customerId}/workorders`, { params: { page, pageSize: 10 } });
  return resp.data;
}

export const WorkOrdersPage: React.FC = () => {
  const { token, logout } = useAuth();
  const { currentCustomerId: customerId } = useCustomers();
  const { push } = useToast();
  const [showCreateCustomer, setShowCreateCustomer] = React.useState(false);
  const [showCreateWorkOrder, setShowCreateWorkOrder] = React.useState(false);
  const [page, setPage] = React.useState(1);
  const { data, isLoading, error } = useQuery({
    queryKey: ['workOrders', customerId, page],
    queryFn: () => fetchWorkOrders(customerId!, page),
    enabled: !!token && !!customerId
  });
  React.useEffect(() => { if (error) push('Failed to load work orders', 'error'); }, [error, push]);
  const totalPages = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1;

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div style={{display:'flex', flexDirection:'column', gap:'.35rem'}}>
          <Logo />
          <p className="tag" style={{margin:0}}>Tailoring Console</p>
        </div>
        <div className="stack gap-md">
          <CustomerSelector />
          <button className="btn primary" onClick={() => setShowCreateCustomer(s => !s)}>
            {showCreateCustomer ? 'Cancel New Customer' : 'New Customer'}
          </button>
          <button className="btn outline" onClick={() => setShowCreateWorkOrder(s => !s)} disabled={!customerId}>
            {showCreateWorkOrder ? 'Cancel Work Order' : 'New Work Order'}
          </button>
          <button className="btn danger" onClick={logout}>Logout</button>
        </div>
      </aside>
      <div className="main">
        <div className="topbar">
          <div className="muted" style={{fontSize:'.8rem'}}>Customer: {customerId ? customerId.slice(0,8)+'…' : 'None selected'}</div>
          <div className="muted" style={{fontSize:'.7rem'}}>v0.1 preview</div>
        </div>
        <div className="content">
          <div className="stack gap-md">
            {showCreateCustomer && <div className="card"><CreateCustomerForm onCreated={() => setShowCreateCustomer(false)} /></div>}
            {showCreateWorkOrder && customerId && <div className="card"><CreateWorkOrderForm onCreated={() => setShowCreateWorkOrder(false)} /></div>}
            <div style={{display:'grid', gridTemplateColumns: customerId ? 'minmax(300px,420px) 1fr' : '1fr', gap:'1.25rem'}}>
              {customerId && <CustomerDetail />}
              <div className="stack gap-md">
              <div className="card">
              <div style={{display:'flex', justifyContent:'space-between', alignItems:'center', marginBottom:'1rem'}}>
                <h2 className="card-title" style={{margin:0}}>Work Orders</h2>
                <div style={{display:'flex', gap:'.5rem'}}>
                  <button className="btn" disabled={page<=1 || isLoading} onClick={() => setPage(p => Math.max(1, p-1))}>Prev</button>
                  <span className="muted" style={{alignSelf:'center',fontSize:'.7rem'}}>Page {page} / {totalPages}</span>
                  <button className="btn" disabled={page>=totalPages || isLoading} onClick={() => setPage(p => Math.min(totalPages, p+1))}>Next</button>
                </div>
              </div>
              {!customerId && <div className="alert error">Select a customer to view work orders.</div>}
              {isLoading && (<ul>{Array.from({ length: 6 }).map((_,i) => <li key={i} className="skeleton-line" style={{height:24}} />)}</ul>)}
              {error && <div className="alert error">Failed to load work orders.</div>}
              {customerId && data && (
                <ul>
                  {data.items.map(w => (
                    <li key={w.id}>
                      <span style={{flex:1,fontFamily:'ui-monospace'}}>{w.id.slice(0,8)}…</span>
                      <span className={`badge status-${w.status.toLowerCase()}`}>{w.status}</span>
                      <span style={{fontWeight:600}}>{(w.total/100).toFixed(2)}</span>
                    </li>
                  ))}
                </ul>
              )}
              </div>
              {customerId && <AppointmentsPanel />}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
