import React from 'react';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';
import { useToastMutation } from '../api/useToastMutation';

interface MeasurementRecord { date: string; chest: number; waist: number; hips: number; sleeve: number; }
interface CustomerDetailsDto {
  id: string; customerNumber: string; firstName: string; lastName: string; email: string;
  registrationDate: string; enabled: boolean; measurements: MeasurementRecord[]; notes: { date:string; text:string; author?:string }[];
}

export const CustomerDetail: React.FC = () => {
  const { currentCustomerId, refresh } = useCustomers();
  const { push } = useToast();
  const [data, setData] = React.useState<CustomerDetailsDto | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [edit, setEdit] = React.useState(false);
  const [form, setForm] = React.useState({ firstName:'', lastName:'', email:'' });
  const [noteText, setNoteText] = React.useState('');
  const [noteAuthor, setNoteAuthor] = React.useState('');
  const [mForm, setMForm] = React.useState({ chest:'', waist:'', hips:'', sleeve:'' });

  React.useEffect(() => { if (!currentCustomerId) { setData(null); return; } (async ()=>{ setLoading(true); try{ const r = await api.get(`/customers/${currentCustomerId}`); setData(r.data); setForm({ firstName:r.data.firstName, lastName:r.data.lastName, email:r.data.email }); } catch { push('Failed to load customer','error'); } finally { setLoading(false);} })(); },[currentCustomerId,push]);

  const updateMutation = useToastMutation(async () => {
    if(!currentCustomerId) return; await api.put(`/customers/${currentCustomerId}`, { id: currentCustomerId, firstName: form.firstName, lastName: form.lastName, dateOfBirth: null, email: form.email, phone: null, address: null, fitPreference: null, stylePreference: null, fabricPreference: null });
  },{ successMessage:'Customer updated', errorMessage:'Update failed', onSuccess:()=>{ setEdit(false); refresh(); if(currentCustomerId) api.get(`/customers/${currentCustomerId}`).then(r=> setData(r.data)); }});

  const deleteMutation = useToastMutation(async ()=>{ if(!currentCustomerId) return; await api.delete(`/customers/${currentCustomerId}`); },{ successMessage:'Customer disabled', errorMessage:'Disable failed', onSuccess:()=>{ refresh(); if(currentCustomerId) api.get(`/customers/${currentCustomerId}`).then(r=> setData(r.data)); }});
  const restoreMutation = useToastMutation(async ()=>{ if(!currentCustomerId) return; await api.post(`/customers/${currentCustomerId}/restore`); },{ successMessage:'Customer restored', errorMessage:'Restore failed', onSuccess:()=>{ refresh(); if(currentCustomerId) api.get(`/customers/${currentCustomerId}`).then(r=> setData(r.data)); }});
  const addNoteMutation = useToastMutation(async ()=>{ if(!currentCustomerId || !noteText.trim()) return; await api.post(`/customers/${currentCustomerId}/notes`, { customerId: currentCustomerId, text: noteText.trim(), author: noteAuthor.trim()|| null }); }, { successMessage:'Note added', errorMessage:'Failed to add note', onSuccess:()=>{ setNoteText(''); setNoteAuthor(''); if(currentCustomerId) api.get(`/customers/${currentCustomerId}`).then(r=> setData(r.data)); }});
  const addMeasurementMutation = useToastMutation(async ()=>{ if(!currentCustomerId) return; const { chest, waist, hips, sleeve } = mForm; await api.post(`/customers/${currentCustomerId}/measurements`, { customerId: currentCustomerId, date: new Date().toISOString(), chest: parseFloat(chest), waist: parseFloat(waist), hips: parseFloat(hips), sleeve: parseFloat(sleeve) }); }, { successMessage:'Measurement added', errorMessage:'Failed to add measurement', onSuccess:()=>{ setMForm({ chest:'', waist:'', hips:'', sleeve:'' }); if(currentCustomerId) api.get(`/customers/${currentCustomerId}`).then(r=> setData(r.data)); }});

  if(!currentCustomerId) return <div className="card"><em className="muted" style={{fontSize:'.8rem'}}>Select a customer to view details.</em></div>;
  if(loading) return <div className="card"><div className="skeleton-line" style={{height:24,marginBottom:12}}/><div className="skeleton-line" style={{height:18,width:'70%',marginBottom:8}}/><div className="skeleton-line" style={{height:18,width:'45%'}}/></div>;
  if(!data) return null;

  return (
    <div className="card" style={{position:'relative'}}>
      <div style={{display:'flex',justifyContent:'space-between',alignItems:'flex-start',gap:16,flexWrap:'wrap'}}>
        <div style={{flex:'1 1 260px'}}>
          <h2 className="card-title" style={{marginTop:0}}>Customer</h2>
          {!edit && (
            <div className="stack gap-sm" style={{fontSize:'.85rem'}}>
              <div><strong>{data.firstName} {data.lastName}</strong></div>
              <div className="muted">#{data.customerNumber}</div>
              <div>{data.email}</div>
              <div className={data.enabled? 'badge status-open':'badge status-cancelled'} style={{alignSelf:'flex-start'}}>{data.enabled? 'Enabled':'Disabled'}</div>
            </div>
          )}
          {edit && (
            <form onSubmit={e=>{ e.preventDefault(); updateMutation.mutate(); }} className="form-vertical" style={{maxWidth:300}}>
              <div className="form-field"><label>First Name</label><input value={form.firstName} onChange={e=>setForm(f=>({...f,firstName:e.target.value}))} required /></div>
              <div className="form-field"><label>Last Name</label><input value={form.lastName} onChange={e=>setForm(f=>({...f,lastName:e.target.value}))} required /></div>
              <div className="form-field"><label>Email</label><input type="email" value={form.email} onChange={e=>setForm(f=>({...f,email:e.target.value}))} required /></div>
              <div style={{display:'flex',gap:8}}>
                <button type="submit" className="btn primary" disabled={updateMutation.isPending}>{updateMutation.isPending? 'Saving...':'Save'}</button>
                <button type="button" className="btn" onClick={()=>{ setEdit(false); setForm({ firstName:data.firstName, lastName:data.lastName, email:data.email}); }}>Cancel</button>
              </div>
            </form>
          )}
        </div>
        <div style={{display:'flex',flexDirection:'column',gap:8}}>
          {!edit && <button className="btn" onClick={()=>setEdit(true)}>Edit</button>}
          {data.enabled && <button className="btn danger" disabled={deleteMutation.isPending} onClick={()=>deleteMutation.mutate()}>Disable</button>}
          {!data.enabled && <button className="btn primary" disabled={restoreMutation.isPending} onClick={()=>restoreMutation.mutate()}>Restore</button>}
        </div>
      </div>
      <hr style={{margin:'1.25rem 0', border:'none', borderTop:'1px solid var(--color-border)'}} />
      <div style={{display:'flex',flexWrap:'wrap',gap:24}}>
        <div style={{flex:'1 1 300px', display:'flex', flexDirection:'column', gap:12}}>
          <h3 style={{margin:'0 0 .25rem',fontSize:'1rem'}}>Add Note</h3>
          <form onSubmit={e=>{ e.preventDefault(); addNoteMutation.mutate(); }} className="form-vertical" style={{fontSize:'.75rem'}}>
            <div className="form-field"><textarea placeholder="Note text" value={noteText} onChange={e=>setNoteText(e.target.value)} rows={3} required /></div>
            <div className="form-field"><input placeholder="Author (optional)" value={noteAuthor} onChange={e=>setNoteAuthor(e.target.value)} /></div>
            <button type="submit" className="btn primary" disabled={addNoteMutation.isPending || !noteText.trim()}>{addNoteMutation.isPending? 'Adding...':'Add Note'}</button>
          </form>
          <h3 style={{margin:'1rem 0 .25rem',fontSize:'1rem'}}>Notes</h3>
          <div style={{maxHeight:200, overflowY:'auto', paddingRight:4}}>
            {data.notes.length === 0 && <div className="muted" style={{fontSize:'.7rem'}}>No notes.</div>}
            {data.notes.map(n => (
              <div key={n.date} style={{border:'1px solid var(--color-border)',borderRadius:6,padding:'.5rem .6rem',marginBottom:6,background:'var(--color-surface-alt)'}}>
                <div style={{fontSize:'.65rem',letterSpacing:'.6px'}} className="muted">{new Date(n.date).toLocaleDateString()} {n.author && '• '+n.author}</div>
                <div style={{fontSize:'.75rem'}}>{n.text}</div>
              </div>
            ))}
          </div>
        </div>
        <div style={{flex:'1 1 300px', display:'flex', flexDirection:'column', gap:12}}>
            <h3 style={{margin:'0 0 .25rem',fontSize:'1rem'}}>Add Measurement</h3>
            <form onSubmit={e=>{ e.preventDefault(); addMeasurementMutation.mutate(); }} className="form-vertical" style={{display:'grid',gridTemplateColumns:'repeat(auto-fill,minmax(110px,1fr))',gap:8,fontSize:'.7rem'}}>
              {(['chest','waist','hips','sleeve'] as const).map(k => (
                <div key={k} className="form-field" style={{margin:0}}>
                  <label style={{fontSize:'.6rem',textTransform:'uppercase',letterSpacing:'.6px'}}>{k}</label>
                  <input type="number" step="0.1" value={(mForm as any)[k]} onChange={e=> setMForm(f=>({...f,[k]:e.target.value}))} required />
                </div>
              ))}
              <div style={{gridColumn:'1 / -1'}}>
                <button type="submit" className="btn primary" disabled={addMeasurementMutation.isPending}>{addMeasurementMutation.isPending? 'Saving...':'Save Measurement'}</button>
              </div>
            </form>
            <h3 style={{margin:'1rem 0 .25rem',fontSize:'1rem'}}>Measurements</h3>
            <div style={{maxHeight:200, overflowY:'auto', paddingRight:4}}>
              {data.measurements.length === 0 && <div className="muted" style={{fontSize:'.7rem'}}>No measurements.</div>}
              {data.measurements.slice().sort((a,b)=> new Date(b.date).getTime()-new Date(a.date).getTime()).map(m => (
                <div key={m.date} style={{display:'flex',flexDirection:'column',border:'1px solid var(--color-border)',borderRadius:6,padding:'.4rem .55rem',marginBottom:6,background:'var(--color-surface-alt)'}}>
                  <div style={{fontSize:'.6rem'}} className="muted">{new Date(m.date).toLocaleDateString()}</div>
                  <div style={{display:'flex',flexWrap:'wrap',gap:8,fontSize:'.65rem'}}>
                    <span>C:{m.chest}</span><span>W:{m.waist}</span><span>H:{m.hips}</span><span>S:{m.sleeve}</span>
                  </div>
                </div>
              ))}
            </div>
        </div>
      </div>
    </div>
  );
};

export default CustomerDetail;
