import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';

// Common garment types for consistency with edit mode
const GARMENT_TYPES = [
  'Suit', 'Jacket', 'Pant', 'Vest', 'Shirt', 'Top', 'Dress', 'Skirt', 'Coat', 'Other'
];

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
  chestMeasurement?: number;
  waistMeasurement?: number;
  hipsMeasurement?: number;
  sleeveMeasurement?: number;
  measurementNotes?: string;
}

interface NewItem {
  garmentType: string;
  quantity: number;
  unitPrice: number;
  notes: string;
  measurements: {
    chest: string;
    waist: string;
    hips: string;
    sleeve: string;
    notes: string;
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
  const [showAddItem, setShowAddItem] = React.useState(false);
  const [newItem, setNewItem] = React.useState<NewItem>({
    garmentType: GARMENT_TYPES[0], // Default to first garment type
    quantity: 1,
    unitPrice: 0,
    notes: '',
    measurements: {
      chest: '',
      waist: '',
      hips: '',
      sleeve: '',
      notes: ''
    }
  });

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

  const handleAddItem = async () => {
    if (!workOrder || !newItem.garmentType || newItem.garmentType.trim() === '' || !newItem.notes || newItem.notes.trim() === '') {
      push('Please fill in required fields', 'error');
      return;
    }

    // Prevent adding items to completed work orders
    if (workOrder.status.toLowerCase() === 'completed') {
      push('Cannot add items to completed work orders', 'error');
      return;
    }

    try {
      const calculatedTotal = newItem.quantity * newItem.unitPrice;
      
      const itemData = {
        Description: newItem.notes, // Use notes as description (now required)
        Currency: workOrder.currency, // Get currency from work order
        GarmentType: newItem.garmentType,
        Quantity: newItem.quantity,
        UnitPrice: newItem.unitPrice, // Keep as decimal, not convert to cents
        Total: calculatedTotal, // Send calculated total to backend
        ChestMeasurement: newItem.measurements.chest ? parseFloat(newItem.measurements.chest) : null,
        WaistMeasurement: newItem.measurements.waist ? parseFloat(newItem.measurements.waist) : null,
        HipsMeasurement: newItem.measurements.hips ? parseFloat(newItem.measurements.hips) : null,
        SleeveMeasurement: newItem.measurements.sleeve ? parseFloat(newItem.measurements.sleeve) : null,
        MeasurementNotes: newItem.measurements.notes || null
      };

      await api.post(`/customers/${workOrder.customerId}/workorders/${workOrder.id}/items`, itemData);
      push('Item added successfully', 'success');
      setShowAddItem(false);
      setNewItem({
        garmentType: GARMENT_TYPES[0], // Default to first garment type
        quantity: 1,
        unitPrice: 0,
        notes: '',
        measurements: {
          chest: '',
          waist: '',
          hips: '',
          sleeve: '',
          notes: ''
        }
      });
      refetch();
    } catch(e: any) {
      push(e?.message || 'Failed to add item', 'error');
    }
  };

  const resetAddItemForm = () => {
    setNewItem({
      garmentType: GARMENT_TYPES[0], // Default to first garment type
      quantity: 1,
      unitPrice: 0,
      notes: '',
      measurements: {
        chest: '',
        waist: '',
        hips: '',
        sleeve: '',
        notes: ''
      }
    });
    setShowAddItem(false);
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
            {workOrder.status.toLowerCase() === 'draft' && (
              <button className="btn primary" onClick={() => updateStatus('inprogress')}>
                Start Work
              </button>
            )}
            {workOrder.status.toLowerCase() === 'inprogress' && (
              <button className="btn primary" onClick={() => updateStatus('completed')}>
                Mark Complete
              </button>
            )}
            {(workOrder.status.toLowerCase() === 'draft' || workOrder.status.toLowerCase() === 'inprogress') && (
              <button className="btn" onClick={() => updateStatus('cancelled')}>
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
              {workOrder.currency} {(() => {
                // Calculate total from items as fallback
                const calculatedTotal = workOrder.items.reduce((sum, item) => {
                  const itemTotal = item.total && item.total > 0 ? item.total : (item.quantity || 0) * (item.unitPrice || 0);
                  return sum + itemTotal;
                }, 0);
                const displayTotal = workOrder.total && workOrder.total > 0 ? workOrder.total : calculatedTotal;
                return displayTotal.toFixed(2);
              })()}
            </div>
          </div>
        </div>

        <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem'}}>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Created</label>
            <div>{workOrder.createdAt ? new Date(workOrder.createdAt).toLocaleString() : '—'}</div>
          </div>
          <div>
            <label className="muted" style={{fontSize: '.7rem', textTransform: 'uppercase'}}>Last Updated</label>
            <div>{workOrder.updatedAt ? new Date(workOrder.updatedAt).toLocaleString() : '—'}</div>
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
                <div>{workOrder.currency} {(() => {
                  // Calculate subtotal from items as fallback
                  const calculatedSubtotal = workOrder.items.reduce((sum, item) => {
                    const itemTotal = item.total && item.total > 0 ? item.total : (item.quantity || 0) * (item.unitPrice || 0);
                    return sum + itemTotal;
                  }, 0);
                  const displaySubtotal = workOrder.subtotal && workOrder.subtotal > 0 ? workOrder.subtotal : calculatedSubtotal;
                  return displaySubtotal.toFixed(2);
                })()}</div>
              </div>
              <div>
                <label className="muted">Discount</label>
                <div>{workOrder.currency} {(workOrder.discount || 0).toFixed(2)}</div>
              </div>
            </div>
          </div>
        )}

        {tab === 'items' && (
          <div>
            <div style={{display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem'}}>
              <h3 style={{marginTop: 0}}>Order Items</h3>
              {workOrder.status.toLowerCase() !== 'completed' && (
                <button className="btn primary" onClick={() => setShowAddItem(true)}>+ Add Item</button>
              )}
            </div>
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
                    {workOrder.items.map(item => {
                      // Calculate total on frontend as fallback
                      const calculatedTotal = (item.quantity || 0) * (item.unitPrice || 0);
                      const displayTotal = item.total && item.total > 0 ? item.total : calculatedTotal;
                      
                      return (
                        <tr key={item.id} style={{borderBottom: '1px solid var(--color-border-light)'}}>
                          <td style={{padding: '8px', fontWeight: 600}}>{item.garmentType}</td>
                          <td style={{padding: '8px'}}>{item.quantity}</td>
                          <td style={{padding: '8px'}}>{workOrder.currency} {(item.unitPrice || 0).toFixed(2)}</td>
                          <td style={{padding: '8px', fontWeight: 600}}>{workOrder.currency} {displayTotal.toFixed(2)}</td>
                          <td style={{padding: '8px', fontSize: '.7rem'}}>
                            {item.chestMeasurement && <div>Chest: {item.chestMeasurement}"</div>}
                            {item.waistMeasurement && <div>Waist: {item.waistMeasurement}"</div>}
                            {item.hipsMeasurement && <div>Hips: {item.hipsMeasurement}"</div>}
                            {item.sleeveMeasurement && <div>Sleeve: {item.sleeveMeasurement}"</div>}
                            {item.measurementNotes && <div className="muted">Notes: {item.measurementNotes}</div>}
                            {!item.chestMeasurement && !item.waistMeasurement && !item.hipsMeasurement && !item.sleeveMeasurement && !item.measurementNotes && <span className="muted">—</span>}
                          </td>
                          <td style={{padding: '8px', fontSize: '.7rem'}}>
                            {item.notes || <span className="muted">—</span>}
                          </td>
                        </tr>
                      );
                    })}
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

      {/* Add Item Modal */}
      {showAddItem && (
        <div className="modal-overlay" onClick={resetAddItemForm}>
          <div className="modal-shell" style={{width: 600, maxHeight: '90vh', overflowY: 'auto'}} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Add Item to Work Order</h3>
              <button className="modal-close" onClick={resetAddItemForm}>×</button>
            </div>
            <div className="modal-body">
              <div className="form-vertical">
                <div className="form-field">
                  <label>Garment Type *</label>
                  <select
                    value={newItem.garmentType}
                    onChange={e => setNewItem({...newItem, garmentType: e.target.value})}
                    required
                  >
                    {GARMENT_TYPES.map(type => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </select>
                </div>
                
                <div className="form-row">
                  <div className="form-field">
                    <label>Quantity *</label>
                    <input
                      type="number"
                      min="1"
                      value={newItem.quantity}
                      onChange={e => setNewItem({...newItem, quantity: parseInt(e.target.value) || 1})}
                    />
                  </div>
                  <div className="form-field">
                    <label>Unit Price ({workOrder.currency}) *</label>
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={newItem.unitPrice}
                      onChange={e => setNewItem({...newItem, unitPrice: parseFloat(e.target.value) || 0})}
                    />
                  </div>
                </div>

                <div className="form-field">
                  <label>Total ({workOrder.currency})</label>
                  <input
                    type="text"
                    value={`${workOrder.currency} ${(newItem.quantity * newItem.unitPrice).toFixed(2)}`}
                    disabled
                    style={{backgroundColor: 'var(--color-surface-alt)', fontWeight: 600}}
                  />
                </div>

                <div className="form-field">
                  <label>Description *</label>
                  <input
                    type="text"
                    value={newItem.notes}
                    onChange={e => setNewItem({...newItem, notes: e.target.value})}
                    placeholder="e.g., Custom suit with special requirements"
                    required
                  />
                </div>

                <div className="form-section">
                  <div className="form-section-title">Measurements (inches)</div>
                  <div className="form-row">
                    <div className="form-field">
                      <label>Chest</label>
                      <input
                        type="number"
                        min="0"
                        step="0.5"
                        value={newItem.measurements.chest}
                        onChange={e => setNewItem({...newItem, measurements: {...newItem.measurements, chest: e.target.value}})}
                        placeholder="e.g., 40"
                      />
                    </div>
                    <div className="form-field">
                      <label>Waist</label>
                      <input
                        type="number"
                        min="0"
                        step="0.5"
                        value={newItem.measurements.waist}
                        onChange={e => setNewItem({...newItem, measurements: {...newItem.measurements, waist: e.target.value}})}
                        placeholder="e.g., 32"
                      />
                    </div>
                  </div>

                  <div className="form-row">
                    <div className="form-field">
                      <label>Hips</label>
                      <input
                        type="number"
                        min="0"
                        step="0.5"
                        value={newItem.measurements.hips}
                        onChange={e => setNewItem({...newItem, measurements: {...newItem.measurements, hips: e.target.value}})}
                        placeholder="e.g., 38"
                      />
                    </div>
                    <div className="form-field">
                      <label>Sleeve</label>
                      <input
                        type="number"
                        min="0"
                        step="0.5"
                        value={newItem.measurements.sleeve}
                        onChange={e => setNewItem({...newItem, measurements: {...newItem.measurements, sleeve: e.target.value}})}
                        placeholder="e.g., 25"
                      />
                    </div>
                  </div>

                  <div className="form-field">
                    <label>Measurement Notes</label>
                    <textarea
                      value={newItem.measurements.notes}
                      onChange={e => setNewItem({...newItem, measurements: {...newItem.measurements, notes: e.target.value}})}
                      placeholder="Additional measurement details"
                      rows={2}
                    />
                  </div>
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn outline" onClick={resetAddItemForm}>Cancel</button>
              <button 
                type="button" 
                className="btn primary" 
                onClick={handleAddItem}
                disabled={!newItem.garmentType || newItem.garmentType.trim() === '' || !newItem.notes || newItem.notes.trim() === ''}
              >
                Add Item
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WorkOrderDetailPage;
