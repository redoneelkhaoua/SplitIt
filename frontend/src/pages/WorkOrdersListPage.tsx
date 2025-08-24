import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { api, PagingEnvelope } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';
import { CreateWorkOrderForm } from '../components/CreateWorkOrderForm';

interface WorkOrderSummary { 
  id: string; 
  customerId: string;
  status: string; 
  total: number; 
  subtotal: number; 
  discount: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
}

interface NewWorkOrder {
  customerId: string;
  priority: string;
  dueDate: string;
  notes: string;
  currency: string;
}

interface PageRes { items: WorkOrderSummary[]; total: number; page: number; pageSize: number; }

async function fetchWorkOrders(
  customerId: string | undefined, 
  page: number, 
  pageSize: number = 20,
  status?: string
): Promise<PageRes> {
  const params: any = { page, pageSize };
  if (customerId) params.customerId = customerId;
  if (status && status !== 'all') params.status = status;
  
  // If no customer selected, get all work orders from all customers
  const endpoint = customerId ? `/customers/${customerId}/workorders` : '/workorders';
  const resp = await api.get(endpoint, { params });
  return resp.data;
}

export const WorkOrdersListPage: React.FC = () => {
  const { customers } = useCustomers();
  const { push } = useToast();
  const navigate = useNavigate();
  
  // Data / paging
  const [page, setPage] = React.useState(1);
  const [loading, setLoading] = React.useState(false);
  
  // UI state
  const [showCreateForm, setShowCreateForm] = React.useState(false);
  const [showCreateWorkOrder, setShowCreateWorkOrder] = React.useState(false);
  const [filterCustomer, setFilterCustomer] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<'all'|'draft'|'inprogress'|'completed'|'cancelled'>('all');
  const [rawSearch, setRawSearch] = React.useState('');
  const [search, setSearch] = React.useState('');
  const [newWorkOrder, setNewWorkOrder] = React.useState<NewWorkOrder>({
    customerId: '',
    priority: 'medium',
    dueDate: '',
    notes: '',
    currency: 'USD'
  });
  
  // Debounced search
  React.useEffect(() => { 
    const t = setTimeout(() => setSearch(rawSearch.trim().toLowerCase()), 350); 
    return () => clearTimeout(t); 
  }, [rawSearch]);

  // Use filterCustomer for API calls, no longer dependent on currentCustomerId
  const effectiveCustomerId = filterCustomer || undefined;

  const { data, isLoading, error, refetch } = useQuery({ 
    queryKey: ['workOrders', effectiveCustomerId, page, statusFilter], 
    queryFn: () => fetchWorkOrders(effectiveCustomerId, page, 20, statusFilter), 
    enabled: true  // Always enabled now since we can show all work orders
  });

  const totalPages = data ? Math.max(1, Math.ceil(data.total / 20)) : 1;

  const handleWorkOrderClick = (workOrderId: string) => {
    navigate(`/workorders/${workOrderId}`);
  };

  const refresh = () => refetch();

  const handleCreateWorkOrder = async () => {
    if (!newWorkOrder.customerId.trim()) {
      push('Please select a customer', 'error');
      return;
    }

    try {
      const workOrderData = {
        ...newWorkOrder,
        dueDate: newWorkOrder.dueDate ? new Date(newWorkOrder.dueDate).toISOString() : undefined
      };

      await api.post(`/customers/${newWorkOrder.customerId}/workorders`, workOrderData);
      push('Work order created successfully', 'success');
      setShowCreateWorkOrder(false);
      setNewWorkOrder({
        customerId: '',
        priority: 'medium',
        dueDate: '',
        notes: '',
        currency: 'USD'
      });
      refetch();
    } catch(e: any) {
      push(e?.message || 'Failed to create work order', 'error');
    }
  };

  const resetCreateWorkOrderForm = () => {
    setNewWorkOrder({
      customerId: '',
      priority: 'medium',
      dueDate: '',
      notes: '',
      currency: 'USD'
    });
    setShowCreateWorkOrder(false);
  };

  // Filter locally by search (work order ID or customer name) AFTER fetch
  const filtered = React.useMemo(() => {
    if (!data) return [] as WorkOrderSummary[];
    if (!search) return data.items;
    return data.items.filter(w => {
      const customer = customers.find(c => c.id === w.customerId);
      const customerName = customer ? `${customer.firstName} ${customer.lastName}`.toLowerCase() : '';
      return w.id.toLowerCase().includes(search) ||
             w.status.toLowerCase().includes(search) ||
             customerName.includes(search);
    });
  }, [data, search, customers]);

  // Add interface for work order that includes customerId
  const selectedCustomer = customers.find(c => c.id === effectiveCustomerId);

  return (
    <div className="stack gap-md">
      {effectiveCustomerId && showCreateForm && (
        <div className="card">
          <CreateWorkOrderForm onCreated={() => {
            setShowCreateForm(false);
            refetch();
          }} />
        </div>
      )}

      <div className="card" style={{padding:'1.25rem 1.25rem 1.75rem'}}>
        <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',gap:16}}>
          <h2 className="card-title" style={{marginTop:0}}>Work Orders list</h2>
          <div style={{display:'flex',gap:8}}>
            <button 
              className="btn primary" 
              onClick={() => setShowCreateWorkOrder(true)}
            >
              + Add Work Order
            </button>
            {effectiveCustomerId && (
              <button 
                className="btn outline" 
                onClick={() => setShowCreateForm(!showCreateForm)}
              >
                {showCreateForm ? 'Cancel' : '+ Add Item'}
              </button>
            )}
            <button className="btn" onClick={refresh}>Refresh</button>
          </div>
        </div>
        <p className="muted" style={{fontSize:'.7rem',margin:'-.5rem 0 1rem'}}>
          View, filter and manage work orders. {selectedCustomer ? `Showing orders for ${selectedCustomer.firstName} ${selectedCustomer.lastName}` : 'Showing all work orders from all customers.'}
        </p>
        
        <div style={{display:'flex',gap:12,alignItems:'center',marginBottom:12}}>
          <input 
            placeholder="Search work order, customer or status" 
            value={rawSearch} 
            onChange={e => { setRawSearch(e.target.value); setPage(1); }} 
            style={{flex:1}} 
          />
          <select 
            value={filterCustomer} 
            onChange={e => { setFilterCustomer(e.target.value); setPage(1); }} 
            style={{fontSize:'.65rem',padding:'6px 10px',borderRadius:10,border:'1px solid var(--color-border)',background:'var(--color-surface-alt)'}}
          >
            <option value="">All Customers</option>
            {customers.map(c => <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>)}
          </select>
          <select 
            value={statusFilter} 
            onChange={e => { setStatusFilter(e.target.value as any); setPage(1); }} 
            style={{fontSize:'.65rem',padding:'6px 10px',borderRadius:10,border:'1px solid var(--color-border)',background:'var(--color-surface-alt)'}}
          >
            <option value="all">All Statuses</option>
            <option value="draft">Draft</option>
            <option value="inprogress">In Progress</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
          </select>
          <button className="btn" disabled={isLoading || page<=1} onClick={() => setPage(p => Math.max(1,p-1))}>Prev</button>
          <span className="muted" style={{fontSize:'.65rem'}}>Page {page}/{totalPages}</span>
          <button className="btn" disabled={isLoading || page>=totalPages} onClick={() => setPage(p => Math.min(totalPages,p+1))}>Next</button>
        </div>

        {error && <div className="alert error">Failed to load work orders.</div>}
        
        <div style={{overflowX:'auto', overflowY:'visible'}}>
          <table style={{width:'100%',borderCollapse:'collapse',fontSize:'.75rem'}}>
            <thead>
              <tr style={{textAlign:'left',fontSize:'.6rem',letterSpacing:'.7px',textTransform:'uppercase',color:'var(--color-text-dim)'}}>
                <th style={{padding:'6px 8px'}}>Work Order ID</th>
                <th style={{padding:'6px 8px'}}>Customer</th>
                <th style={{padding:'6px 8px'}}>Status</th>
                <th style={{padding:'6px 8px'}}>Total</th>
                <th style={{padding:'6px 8px'}}>Created</th>
                <th style={{padding:'6px 8px'}}>Updated</th>
                <th style={{padding:'6px 8px',textAlign:'right'}}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {isLoading && Array.from({length:6}).map((_,i)=>(<tr key={i}><td colSpan={7} style={{padding:0}}><div className="skeleton-line" style={{height:30,margin:'4px 0'}} /></td></tr>))}
              {!isLoading && filtered.map(w => {
                const customer = customers.find(c => c.id === w.customerId);
                return (
                  <tr key={w.id} style={{cursor:'pointer'}} onClick={() => handleWorkOrderClick(w.id)}>
                    <td style={{padding:'8px',fontFamily:'ui-monospace'}}>{w.id.slice(0,8)}…</td>
                    <td style={{padding:'8px'}}>{customer ? `${customer.firstName} ${customer.lastName}` : '-'}</td>
                    <td style={{padding:'8px'}}><span className={`badge status-${w.status.toLowerCase().replace(' ', '-')}`}>{w.status}</span></td>
                    <td style={{padding:'8px',fontWeight:600}}>{w.currency} {(w.total || 0).toFixed(2)}</td>
                    <td style={{padding:'8px',fontFamily:'ui-monospace'}}>
                      {w.createdAt ? new Date(w.createdAt).toLocaleString([], { month:'short', day:'2-digit', hour:'2-digit', minute:'2-digit'}) : '—'}
                    </td>
                    <td style={{padding:'8px',fontFamily:'ui-monospace'}}>
                      {w.updatedAt ? new Date(w.updatedAt).toLocaleString([], { month:'short', day:'2-digit', hour:'2-digit', minute:'2-digit'}) : '—'}
                    </td>
                    <td style={{padding:'8px',position:'relative',textAlign:'right'}}>
                      <WorkOrderActions workOrder={w} onChanged={refetch} />
                    </td>
                  </tr>
                );
              })}
              {!isLoading && filtered.length===0 && (
                <tr><td colSpan={7} style={{padding:'14px 8px'}} className="muted">No work orders found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Work Order Modal */}
      {showCreateWorkOrder && (
        <div className="modal-overlay" onClick={resetCreateWorkOrderForm}>
          <div className="modal-shell" style={{width: 500}} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Create New Work Order</h3>
              <button className="modal-close" onClick={resetCreateWorkOrderForm}>×</button>
            </div>
            <div className="modal-body">
              <div className="form-vertical">
                <div className="form-field">
                  <label>Customer *</label>
                  <select
                    value={newWorkOrder.customerId}
                    onChange={e => setNewWorkOrder({...newWorkOrder, customerId: e.target.value})}
                    required
                  >
                    <option value="">Select a customer</option>
                    {customers.map(customer => (
                      <option key={customer.id} value={customer.id}>
                        {customer.firstName} {customer.lastName}
                      </option>
                    ))}
                  </select>
                </div>
                
                <div className="form-row">
                  <div className="form-field">
                    <label>Priority</label>
                    <select
                      value={newWorkOrder.priority}
                      onChange={e => setNewWorkOrder({...newWorkOrder, priority: e.target.value})}
                    >
                      <option value="low">Low</option>
                      <option value="medium">Medium</option>
                      <option value="high">High</option>
                      <option value="urgent">Urgent</option>
                    </select>
                  </div>

                  <div className="form-field">
                    <label>Currency *</label>
                    <select
                      value={newWorkOrder.currency}
                      onChange={e => setNewWorkOrder({...newWorkOrder, currency: e.target.value})}
                      required
                    >
                      <option value="USD">USD ($)</option>
                      <option value="EUR">EUR (€)</option>
                      <option value="GBP">GBP (£)</option>
                      <option value="CAD">CAD (C$)</option>
                      <option value="AUD">AUD (A$)</option>
                      <option value="JPY">JPY (¥)</option>
                    </select>
                  </div>
                </div>

                <div className="form-field">
                  <label>Due Date</label>
                  <input
                    type="date"
                    value={newWorkOrder.dueDate}
                    onChange={e => setNewWorkOrder({...newWorkOrder, dueDate: e.target.value})}
                  />
                </div>

                <div className="form-field">
                  <label>Notes</label>
                  <textarea
                    value={newWorkOrder.notes}
                    onChange={e => setNewWorkOrder({...newWorkOrder, notes: e.target.value})}
                    placeholder="Additional notes or instructions"
                    rows={3}
                  />
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn outline" onClick={resetCreateWorkOrderForm}>Cancel</button>
              <button 
                type="button" 
                className="btn primary" 
                onClick={handleCreateWorkOrder}
                disabled={!newWorkOrder.customerId.trim() || !newWorkOrder.currency.trim()}
              >
                Create Work Order
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WorkOrdersListPage;

// Inline actions component
const WorkOrderActions: React.FC<{ workOrder: WorkOrderSummary; onChanged: ()=>void; }> = ({ workOrder, onChanged }) => {
  const { push } = useToast();
  const navigate = useNavigate();
  const [open, setOpen] = React.useState(false);

  const viewDetails = () => {
    navigate(`/workorders/${workOrder.id}`);
  };

  const updateStatus = async (newStatus: string) => {
    try {
      await api.patch(`/workorders/${workOrder.id}/status`, { status: newStatus });
      push(`Work order ${newStatus.toLowerCase()}`, 'success');
      setOpen(false);
      onChanged();
    } catch(e: any) { 
      push(e?.message || 'Status update failed', 'error'); 
    }
  };

  const deleteWorkOrder = async () => {
    if (!confirm('Are you sure you want to delete this work order?')) return;
    try {
      await api.delete(`/workorders/${workOrder.id}`);
      push('Work order deleted', 'success');
      setOpen(false);
      onChanged();
    } catch(e: any) { 
      push(e?.message || 'Delete failed', 'error'); 
    }
  };

  React.useEffect(() => {
    if (!open) return;
    const onClick = (e: MouseEvent) => { 
      const t = e.target as HTMLElement; 
      if (!t.closest('.workorder-actions-wrapper')) setOpen(false); 
    };
    const onKey = (e: KeyboardEvent) => { 
      if (e.key === 'Escape') setOpen(false); 
    };
    window.addEventListener('click', onClick); 
    window.addEventListener('keydown', onKey);
    return () => { 
      window.removeEventListener('click', onClick); 
      window.removeEventListener('keydown', onKey); 
    };
  }, [open]);

  return (
    <div className="workorder-actions-wrapper" style={{display:'inline-block'}}>
      <button 
        type="button" 
        className="btn" 
        style={{padding:'4px 8px',fontSize:10}} 
        onClick={(e) => { e.stopPropagation(); setOpen(o => !o); }}
      >
        ⋯
      </button>
      {open && (
        <div style={{
          position:'absolute',
          top:'110%',
          right:0,
          background:'#fff',
          border:'1px solid var(--color-border)',
          borderRadius:10,
          boxShadow:'var(--shadow-lg)',
          padding:6,
          zIndex:1000, // Increased z-index
          minWidth:150,
          display:'flex',
          flexDirection:'column',
          gap:4
        }}>
          <button 
            className="btn" 
            style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} 
            onClick={viewDetails}
          >
            View Details
          </button>
          {workOrder.status.toLowerCase() === 'draft' && (
            <button 
              className="btn primary" 
              style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} 
              onClick={() => updateStatus('inprogress')}
            >
              Start Work
            </button>
          )}
          {workOrder.status.toLowerCase() === 'inprogress' && (
            <button 
              className="btn primary" 
              style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} 
              onClick={() => updateStatus('completed')}
            >
              Mark Complete
            </button>
          )}
          {(workOrder.status.toLowerCase() === 'draft' || workOrder.status.toLowerCase() === 'inprogress') && (
            <button 
              className="btn" 
              style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} 
              onClick={() => updateStatus('cancelled')}
            >
              Cancel
            </button>
          )}
          {workOrder.status.toLowerCase() === 'cancelled' && (
            <button 
              className="btn danger" 
              style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} 
              onClick={deleteWorkOrder}
            >
              Delete
            </button>
          )}
          {(workOrder.status.toLowerCase() === 'completed' || workOrder.status.toLowerCase() === 'cancelled') && workOrder.status.toLowerCase() !== 'cancelled' && (
            <div className="muted" style={{padding:'2px 6px',fontSize:10}}>Limited actions</div>
          )}
        </div>
      )}
    </div>
  );
};
