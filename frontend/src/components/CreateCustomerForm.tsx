import React from 'react';
import { useForm } from 'react-hook-form';
import { api } from '../api/client';
import { useToastMutation } from '../api/useToastMutation';
import { useCustomers } from '../state/CustomerContext';

interface FormValues {
  customerNumber: string;
  firstName: string;
  lastName: string;
  email: string;
}

export const CreateCustomerForm: React.FC<{ onCreated?: () => void }> = ({ onCreated }) => {
  const { refresh, setCurrentCustomerId } = useCustomers();
  const { register, handleSubmit, reset } = useForm<FormValues>({
    defaultValues: { customerNumber: '', firstName: '', lastName: '', email: '' }
  });

  const mutation = useToastMutation<{ id: string }, any, FormValues>(async v => {
    const resp = await api.post('/customers', {
      CustomerNumber: v.customerNumber,
      FirstName: v.firstName,
      LastName: v.lastName,
      Email: v.email
    });
    return resp.data;
  }, {
    successMessage: 'Customer created',
    errorMessage: 'Failed to create customer',
    onSuccess: data => {
      setCurrentCustomerId(data.id);
      refresh();
      reset();
      onCreated?.();
    }
  });

  const onSubmit = (vals: FormValues) => mutation.mutate(vals);

  return (
    <form onSubmit={handleSubmit(onSubmit)} style={{ background:'#fff', padding:'1rem', borderRadius:8, boxShadow:'0 1px 4px rgba(0,0,0,.08)', marginBottom:'1rem' }}>
      <h3 style={{ marginTop:0 }}>New Customer</h3>
      <div style={{ display:'flex', gap:'1rem', flexWrap:'wrap' }}>
        <label style={{ flex:'1 1 160px' }}>Number<input required {...register('customerNumber')} /></label>
        <label style={{ flex:'1 1 160px' }}>First<input required {...register('firstName')} /></label>
        <label style={{ flex:'1 1 160px' }}>Last<input required {...register('lastName')} /></label>
        <label style={{ flex:'1 1 220px' }}>Email<input type='email' required {...register('email')} /></label>
      </div>
      <div style={{ marginTop: '0.75rem', display:'flex', gap:8, alignItems:'center' }}>
  <button type="submit" disabled={mutation.isPending}>Create</button>
  {mutation.isPending && <span>Saving...</span>}
      </div>
    </form>
  );
};
