import React from 'react';
import { useToastMutation } from '../api/useToastMutation';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';

interface WorkOrderCreateValues { currency: string; }

export const CreateWorkOrderForm: React.FC<{ onCreated?: (id: string) => void }> = ({ onCreated }) => {
  const { currentCustomerId } = useCustomers();
  const [currency, setCurrency] = React.useState('USD');
  const mutation = useToastMutation<{ id: string }, any, WorkOrderCreateValues>(async v => {
    if (!currentCustomerId) throw new Error('No customer selected');
    const resp = await api.post(`/customers/${currentCustomerId}/workorders`, { currency: v.currency, appointmentId: null });
    return resp.data;
  }, {
    successMessage: 'Work order created',
    errorMessage: 'Failed to create work order',
    invalidateKeys: [['workOrders', currentCustomerId]],
    onSuccess: data => { onCreated?.(data.id); }
  });

  const submit = (e: React.FormEvent) => { e.preventDefault(); mutation.mutate({ currency }); };

  return (
    <form onSubmit={submit} style={{ background:'#fff', padding:'0.75rem 1rem', borderRadius:6, boxShadow:'0 1px 3px rgba(0,0,0,.08)', marginBottom:'1rem', display:'flex', gap:8, alignItems:'center', flexWrap:'wrap' }}>
      <strong style={{ marginRight:4 }}>New Work Order</strong>
      <label style={{ display:'flex', alignItems:'center', gap:4 }}>Currency
        <select value={currency} onChange={e => setCurrency(e.target.value)}>
          <option value="USD">USD</option>
          <option value="EUR">EUR</option>
        </select>
      </label>
      <button type='submit' disabled={mutation.isPending || !currentCustomerId}>Create</button>
      {mutation.isPending && <span>Saving...</span>}
    </form>
  );
};
