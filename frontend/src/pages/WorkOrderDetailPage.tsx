import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';

interface WorkOrderDetailed {
  id: string;
  customerId: string;
  status: string;
  total: number;
  subtotal: number;
  discount: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
  items: WorkOrderItem[];
}

interface WorkOrderItem {
  id: string;
  garmentType: string;
  quantity: number;
  unitPrice: number;
  total: number;
  notes?: string;
  measurements: {
    chest?: number;
    waist?: number;
    hips?: number;
    sleeve?: number;
    notes?: string;
  };
}

async function fetchWorkOrderDetails(workOrderId: string): Promise<WorkOrderDetailed> {
  const resp = await api.get(`/workorders/${workOrderId}`);
  return resp.data;
}

export const WorkOrderDetailPage: React.FC = () => {
  const { workOrderId } = useParams<{ workOrderId: string }>();
  const navigate = useNavigate();
  const { customers } = useCustomers();
  const { push } = useToast();
  const [tab, setTab] = React.useState<'details' | 'items' | 'history'>('details');

  const { data: workOrder, isLoading, error, refetch } = useQuery({
    queryKey: ['workOrder', workOrderId],
    queryFn: () => fetchWorkOrderDetails(workOrderId!),
    enabled: !!workOrderId
  });

  const customer = workOrder ? customers.find(c => c.id === workOrder.customerId) : null;

  const handleBack = () => {
    navigate('/workorders');
  };

  const updateStatus = async (newStatus: string) => {
    if (!workOrder) return;
    try {
      await api.patch(`/workorders/${workOrder.id}/status`, { status: newStatus });
      push(`Work order ${newStatus.toLowerCase()}`, 'success');
      refetch();
    } catch(e: any) {
      push(e?.message || 'Status update failed', 'error');
    }
  };

  if (!workOrderId) {
    return (
      <div className="card">
        <div className="alert error">Invalid work order ID</div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="stack gap-md">
        <div className="card">
          <div className="skeleton-line" style={{height: 24, marginBottom: 16}} />
          <div className="skeleton-line" style={{height: 16, marginBottom: 8}} />
          <div className="skeleton-line" style={{height: 16, width: '60%'}} />
        </div>
      </div>
    );
  }

  if (error || !workOrder) {
    return (
      <div className="card">
        <div className="alert error">Failed to load work order details</div>
        <button className="btn" onClick={handleBack}>← Back to Work Orders</button>
      </div>
    );
  }

  return (
    <div className="stack gap-md">
      {/* Header Card */}
      <div className="card">
        <div style={{display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem'}}>
          <button className="btn" onClick={handleBack}>← Back to Work Orders</button>
          <div style={{display: 'flex', gap: 8}}>
            {workOrder.status === 'Pending' && (
              <button className="btn primary" onClick={() => updateStatus('InProgress')}>
                Start Work
              </button>
            )}
            {workOrder.status === 'InProgress' && (
              <button className="btn primary" onClick={() => updateStatus('Completed')}>
                Mark Complete
              </button>
            )}
            {(workOrder.status === 'Pending' || workOrder.status === 'InProgress') && (
              <button className="btn" onClick={() => updateStatus('Cancelled')}>
                Cancel
              </button>
            )}
          </div>
        </div>
        
        <h1 className="card-title" style={{marginTop: 0, fontSize: '1.5rem'}}>
          Work Order #{workOrder.id.slice(0, 8)}
        </h1>
        
        <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem', marginBottom: '1rem'}}>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Customer</label>
            <div style={{fontWeight: 600}}>
              {customer ? `${customer.firstName} ${customer.lastName}` : 'Unknown Customer'}
            </div>
            {customer && (
              <div className="muted" style={{fontSize: '.8rem'}}>
                {customer.customerNumber}
              </div>
            )}
          </div>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Status</label>
            <div>
              <span className={`badge status-${workOrder.status.toLowerCase().replace(' ', '-')}`}>
                {workOrder.status}
              </span>
            </div>
          </div>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Total</label>
            <div style={{fontWeight: 600, fontSize: '1.1rem'}}>
              {workOrder.currency} {(workOrder.total / 100).toFixed(2)}
            </div>
          </div>
        </div>

        <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem'}}>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Created</label>
            <div>{new Date(workOrder.createdAt).toLocaleString()}</div>
          </div>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Last Updated</label>
            <div>{new Date(workOrder.updatedAt).toLocaleString()}</div>
          </div>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="card">
        <div style={{display: 'flex', borderBottom: '1px solid var(--color-border)', marginBottom: '1rem'}}>
          <button 
            className={tab === 'details' ? 'btn primary' : 'btn'} 
            style={{flex: 1, borderRadius: '0', border: 'none'}} 
            onClick={() => setTab('details')}
          >
            Details
          </button>
          <button 
            className={tab === 'items' ? 'btn primary' : 'btn'} 
            style={{flex: 1, borderRadius: '0', border: 'none'}} 
            onClick={() => setTab('items')}
          >
            Items ({workOrder.items.length})
          </button>
          <button 
            className={tab === 'history' ? 'btn primary' : 'btn'} 
            style={{flex: 1, borderRadius: '0', border: 'none'}} 
            onClick={() => setTab('history')}
          >
            History
          </button>
        </div>

        {/* Tab Content */}
        {tab === 'details' && (
          <div>
            <h3 style={{marginTop: 0}}>Order Summary</h3>
            <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem'}}>
              <div>
                <label className="muted">Subtotal</label>
                <div>{workOrder.currency} {(workOrder.subtotal / 100).toFixed(2)}</div>
              </div>
              <div>
                <label className="muted">Discount</label>
                <div>{workOrder.currency} {(workOrder.discount / 100).toFixed(2)}</div>
              </div>
            </div>
          </div>
        )}

        {tab === 'items' && (
          <div>
            <h3 style={{marginTop: 0}}>Order Items</h3>
            {workOrder.items.length === 0 ? (
              <div className="alert">No items in this work order yet.</div>
            ) : (
              <div style={{overflowX: 'auto'}}>
                <table style={{width: '100%', borderCollapse: 'collapse', fontSize: '.8rem'}}>
                  <thead>
                    <tr style={{textAlign: 'left', borderBottom: '1px solid var(--color-border)'}}>
                      <th style={{padding: '8px'}}>Garment</th>
                      <th style={{padding: '8px'}}>Quantity</th>
                      <th style={{padding: '8px'}}>Unit Price</th>
                      <th style={{padding: '8px'}}>Total</th>
                      <th style={{padding: '8px'}}>Measurements</th>
                      <th style={{padding: '8px'}}>Notes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {workOrder.items.map(item => (
                      <tr key={item.id} style={{borderBottom: '1px solid var(--color-border-light)'}}>
                        <td style={{padding: '8px', fontWeight: 600}}>{item.garmentType}</td>
                        <td style={{padding: '8px'}}>{item.quantity}</td>
                        <td style={{padding: '8px'}}>{workOrder.currency} {(item.unitPrice / 100).toFixed(2)}</td>
                        <td style={{padding: '8px', fontWeight: 600}}>{workOrder.currency} {(item.total / 100).toFixed(2)}</td>
                        <td style={{padding: '8px', fontSize: '.7rem'}}>
                          {item.measurements.chest && <div>Chest: {item.measurements.chest}"</div>}
                          {item.measurements.waist && <div>Waist: {item.measurements.waist}"</div>}
                          {item.measurements.hips && <div>Hips: {item.measurements.hips}"</div>}
                          {item.measurements.sleeve && <div>Sleeve: {item.measurements.sleeve}"</div>}
                          {item.measurements.notes && <div className="muted">Notes: {item.measurements.notes}</div>}
                        </td>
                        <td style={{padding: '8px', fontSize: '.7rem'}}>
                          {item.notes || <span className="muted">—</span>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {tab === 'history' && (
          <div>
            <h3 style={{marginTop: 0}}>Status History</h3>
            <div className="alert">Status history feature coming soon.</div>
          </div>
        )}
      </div>
    </div>
  );
};

export default WorkOrderDetailPage;
