import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { useToastMutation } from '../api/useToastMutation';
import { useCustomers } from '../state/CustomerContext';

interface WorkOrderItemDto {
  description: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  garmentType?: string;
  chestMeasurement?: number;
  waistMeasurement?: number;
  hipsMeasurement?: number;
  sleeveMeasurement?: number;
  measurementNotes?: string;
}

interface WorkOrderDto {
  id: string;
  customerId: string;
  appointmentId?: string;
  currency: string;
  status: string;
  createdDate: string;
  items: WorkOrderItemDto[];
  subtotal: number;
  discount: number;
  total: number;
}

const GARMENT_TYPES = [
  'Suit', 'Jacket', 'Pant', 'Vest', 'Shirt', 'Top', 'Dress', 'Skirt', 'Coat', 'Other'
];

interface Props {
  workOrderId: string;
  onItemAdded?: () => void;
}

export const WorkOrderDetail: React.FC<Props> = ({ workOrderId, onItemAdded }) => {
  const { currentCustomerId } = useCustomers();
  const [showAddItem, setShowAddItem] = React.useState(false);
  const [itemForm, setItemForm] = React.useState({
    description: '',
    quantity: 1,
    unitPrice: 0,
    garmentType: 'Other',
    chestMeasurement: '',
    waistMeasurement: '',
    hipsMeasurement: '',
    sleeveMeasurement: '',
    measurementNotes: ''
  });

  const { data: workOrder, isLoading, error, refetch } = useQuery({
    queryKey: ['workOrder', currentCustomerId, workOrderId],
    queryFn: async () => {
      if (!currentCustomerId) throw new Error('No customer selected');
      const resp = await api.get(`/customers/${currentCustomerId}/workorders/${workOrderId}`);
      return resp.data as WorkOrderDto;
    },
    enabled: !!currentCustomerId && !!workOrderId
  });

  const addItemMutation = useToastMutation(async () => {
    if (!currentCustomerId || !workOrder) throw new Error('Invalid state');
    
    const payload: any = {
      description: itemForm.description,
      quantity: itemForm.quantity,
      unitPrice: itemForm.unitPrice,
      currency: workOrder.currency
    };

    // Add garment type if not 'Other'
    if (itemForm.garmentType && itemForm.garmentType !== 'Other') {
      payload.garmentType = itemForm.garmentType;
    }

    // Add measurements if provided
    if (itemForm.chestMeasurement) payload.chestMeasurement = parseFloat(itemForm.chestMeasurement);
    if (itemForm.waistMeasurement) payload.waistMeasurement = parseFloat(itemForm.waistMeasurement);
    if (itemForm.hipsMeasurement) payload.hipsMeasurement = parseFloat(itemForm.hipsMeasurement);
    if (itemForm.sleeveMeasurement) payload.sleeveMeasurement = parseFloat(itemForm.sleeveMeasurement);
    if (itemForm.measurementNotes.trim()) payload.measurementNotes = itemForm.measurementNotes.trim();

    await api.post(`/customers/${currentCustomerId}/workorders/${workOrderId}/items`, payload);
  }, {
    successMessage: 'Item added successfully',
    errorMessage: 'Failed to add item',
    onSuccess: () => {
      setItemForm({
        description: '',
        quantity: 1,
        unitPrice: 0,
        garmentType: 'Other',
        chestMeasurement: '',
        waistMeasurement: '',
        hipsMeasurement: '',
        sleeveMeasurement: '',
        measurementNotes: ''
      });
      setShowAddItem(false);
      refetch();
      onItemAdded?.();
    }
  });

  const startMutation = useToastMutation(async () => {
    if (!currentCustomerId) throw new Error('No customer selected');
    await api.post(`/customers/${currentCustomerId}/workorders/${workOrderId}/start`);
  }, {
    successMessage: 'Work order started',
    errorMessage: 'Failed to start work order',
    onSuccess: () => refetch()
  });

  const completeMutation = useToastMutation(async () => {
    if (!currentCustomerId) throw new Error('No customer selected');
    await api.post(`/customers/${currentCustomerId}/workorders/${workOrderId}/complete`);
  }, {
    successMessage: 'Work order completed',
    errorMessage: 'Failed to complete work order',
    onSuccess: () => refetch()
  });

  if (isLoading) return <div className="card"><div className="skeleton-line" style={{height: 200}} /></div>;
  if (error) return <div className="card"><div className="alert error">Failed to load work order details</div></div>;
  if (!workOrder) return <div className="card"><div className="alert error">Work order not found</div></div>;

  const handleSubmitItem = (e: React.FormEvent) => {
    e.preventDefault();
    addItemMutation.mutate();
  };

  return (
    <div className="card">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h2 className="card-title" style={{ margin: 0 }}>Work Order Details</h2>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          <span className={`badge status-${workOrder.status.toLowerCase()}`}>{workOrder.status}</span>
          {workOrder.status === 'Draft' && (
            <>
              <button 
                className="btn primary" 
                onClick={() => setShowAddItem(!showAddItem)}
                disabled={addItemMutation.isPending}
              >
                {showAddItem ? 'Cancel' : 'Add Item'}
              </button>
              <button 
                className="btn success" 
                onClick={() => startMutation.mutate()}
                disabled={startMutation.isPending || workOrder.items.length === 0}
              >
                Start
              </button>
            </>
          )}
          {workOrder.status === 'InProgress' && (
            <button 
              className="btn success" 
              onClick={() => completeMutation.mutate()}
              disabled={completeMutation.isPending}
            >
              Complete
            </button>
          )}
        </div>
      </div>

      <div style={{ marginBottom: '1rem', fontSize: '0.875rem', color: 'var(--color-muted)' }}>
        Created: {new Date(workOrder.createdDate).toLocaleDateString()} | 
        Currency: {workOrder.currency} | 
        ID: {workOrder.id.slice(0, 8)}...
      </div>

      {showAddItem && (
        <form onSubmit={handleSubmitItem} className="form-vertical" style={{ 
          background: 'var(--color-surface)', 
          padding: '1rem', 
          borderRadius: '8px', 
          marginBottom: '1rem',
          border: '1px solid var(--color-border)'
        }}>
          <h3 style={{ marginTop: 0, marginBottom: '1rem' }}>Add New Item</h3>
          
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div className="form-field">
              <label>Description</label>
              <input 
                type="text" 
                value={itemForm.description}
                onChange={e => setItemForm(f => ({ ...f, description: e.target.value }))}
                required 
                placeholder="e.g., Custom tailored jacket"
              />
            </div>
            
            <div className="form-field">
              <label>Garment Type</label>
              <select 
                value={itemForm.garmentType}
                onChange={e => setItemForm(f => ({ ...f, garmentType: e.target.value }))}
              >
                {GARMENT_TYPES.map(type => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label>Quantity</label>
              <input 
                type="number" 
                min="1" 
                value={itemForm.quantity}
                onChange={e => setItemForm(f => ({ ...f, quantity: parseInt(e.target.value) || 1 }))}
                required 
              />
            </div>

            <div className="form-field">
              <label>Unit Price ({workOrder.currency})</label>
              <input 
                type="number" 
                min="0" 
                step="0.01"
                value={itemForm.unitPrice}
                onChange={e => setItemForm(f => ({ ...f, unitPrice: parseFloat(e.target.value) || 0 }))}
                required 
              />
            </div>
          </div>

          <div style={{ marginTop: '1rem' }}>
            <h4 style={{ marginBottom: '0.5rem' }}>Measurements (optional)</h4>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem' }}>
              <div className="form-field">
                <label>Chest</label>
                <input 
                  type="number" 
                  step="0.1" 
                  value={itemForm.chestMeasurement}
                  onChange={e => setItemForm(f => ({ ...f, chestMeasurement: e.target.value }))}
                  placeholder="in inches"
                />
              </div>
              <div className="form-field">
                <label>Waist</label>
                <input 
                  type="number" 
                  step="0.1" 
                  value={itemForm.waistMeasurement}
                  onChange={e => setItemForm(f => ({ ...f, waistMeasurement: e.target.value }))}
                  placeholder="in inches"
                />
              </div>
              <div className="form-field">
                <label>Hips</label>
                <input 
                  type="number" 
                  step="0.1" 
                  value={itemForm.hipsMeasurement}
                  onChange={e => setItemForm(f => ({ ...f, hipsMeasurement: e.target.value }))}
                  placeholder="in inches"
                />
              </div>
              <div className="form-field">
                <label>Sleeve</label>
                <input 
                  type="number" 
                  step="0.1" 
                  value={itemForm.sleeveMeasurement}
                  onChange={e => setItemForm(f => ({ ...f, sleeveMeasurement: e.target.value }))}
                  placeholder="in inches"
                />
              </div>
            </div>
            <div className="form-field" style={{ marginTop: '1rem' }}>
              <label>Measurement Notes</label>
              <textarea 
                rows={3}
                value={itemForm.measurementNotes}
                onChange={e => setItemForm(f => ({ ...f, measurementNotes: e.target.value }))}
                placeholder="Special fitting instructions, preferences, etc."
              />
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
            <button type="submit" className="btn primary" disabled={addItemMutation.isPending}>
              {addItemMutation.isPending ? 'Adding...' : 'Add Item'}
            </button>
            <button 
              type="button" 
              className="btn outline" 
              onClick={() => setShowAddItem(false)}
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      <div>
        <h3 style={{ marginBottom: '1rem' }}>Items</h3>
        {workOrder.items.length === 0 ? (
          <div className="alert">No items added yet. Click "Add Item" to get started.</div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {workOrder.items.map((item, index) => (
              <div 
                key={index} 
                style={{ 
                  padding: '1rem', 
                  border: '1px solid var(--color-border)', 
                  borderRadius: '6px',
                  background: 'var(--color-surface)'
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
                      <strong>{item.description}</strong>
                      {item.garmentType && (
                        <span className="badge" style={{ fontSize: '0.7rem' }}>{item.garmentType}</span>
                      )}
                    </div>
                    <div style={{ fontSize: '0.875rem', color: 'var(--color-muted)' }}>
                      Qty: {item.quantity} × {item.currency} {item.unitPrice.toFixed(2)} = {item.currency} {(item.quantity * item.unitPrice).toFixed(2)}
                    </div>
                  </div>
                </div>
                
                {(item.chestMeasurement || item.waistMeasurement || item.hipsMeasurement || item.sleeveMeasurement || item.measurementNotes) && (
                  <div style={{ marginTop: '0.5rem', fontSize: '0.8rem', color: 'var(--color-muted)' }}>
                    <strong>Measurements:</strong>
                    <div style={{ display: 'flex', gap: '1rem', marginTop: '0.25rem' }}>
                      {item.chestMeasurement && <span>Chest: {item.chestMeasurement}"</span>}
                      {item.waistMeasurement && <span>Waist: {item.waistMeasurement}"</span>}
                      {item.hipsMeasurement && <span>Hips: {item.hipsMeasurement}"</span>}
                      {item.sleeveMeasurement && <span>Sleeve: {item.sleeveMeasurement}"</span>}
                    </div>
                    {item.measurementNotes && (
                      <div style={{ marginTop: '0.25rem', fontStyle: 'italic' }}>
                        Notes: {item.measurementNotes}
                      </div>
                    )}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <div style={{ marginTop: '1.5rem', padding: '1rem', background: 'var(--color-surface)', borderRadius: '6px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', marginBottom: '0.5rem' }}>
          <span>Subtotal:</span>
          <span>{workOrder.currency} {(workOrder.subtotal / 100).toFixed(2)}</span>
        </div>
        {workOrder.discount > 0 && (
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', marginBottom: '0.5rem', color: 'var(--color-success)' }}>
            <span>Discount:</span>
            <span>-{workOrder.currency} {(workOrder.discount / 100).toFixed(2)}</span>
          </div>
        )}
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '1.1rem', fontWeight: '600', borderTop: '1px solid var(--color-border)', paddingTop: '0.5rem' }}>
          <span>Total:</span>
          <span>{workOrder.currency} {(workOrder.total / 100).toFixed(2)}</span>
        </div>
      </div>
    </div>
  );
};
