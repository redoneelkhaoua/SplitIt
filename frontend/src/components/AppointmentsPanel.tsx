import React from 'react';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';
import { useToastMutation } from '../api/useToastMutation';

interface AppointmentDto { id:string; startUtc:string; endUtc:string; notes:string|null; status:string; }
interface PageResult { items: AppointmentDto[]; total:number; page:number; pageSize:number; }

export const AppointmentsPanel: React.FC = () => {
  const { currentCustomerId } = useCustomers();
  const { push } = useToast();
  const [data,setData] = React.useState<PageResult|null>(null);
  const [loading,setLoading] = React.useState(false);
  const [error,setError] = React.useState(false);
  const [page,setPage] = React.useState(1);
  const [scheduleOpen,setScheduleOpen] = React.useState(false);
  const [form,setForm] = React.useState({ start:'', duration:30, notes:'' });
  const [manage,setManage] = React.useState<AppointmentDto|null>(null);
  const [reschedule,setReschedule] = React.useState({ start:'', duration:30 });
  const [noteEdit,setNoteEdit] = React.useState('');

  const load = React.useCallback(async ()=>{
    if(!currentCustomerId){ setData(null); return; }
    setLoading(true); setError(false);
    try { const r = await api.get(`/customers/${currentCustomerId}/appointments`, { params:{ page, pageSize:20 }}); setData(r.data);} catch { setError(true); } finally { setLoading(false);} },[currentCustomerId,page]);
  React.useEffect(()=>{ load(); },[load]);

  const scheduleMutation = useToastMutation(async ()=>{
    if(!currentCustomerId) return; if(!form.start) return; const startLocal = new Date(form.start); const endLocal = new Date(startLocal.getTime()+ form.duration*60000);
    await api.post(`/customers/${currentCustomerId}/appointments`, { customerId: currentCustomerId, startUtc: startLocal.toISOString(), endUtc: endLocal.toISOString(), notes: form.notes|| null });
  }, { successMessage:'Appointment scheduled', errorMessage:'Failed to schedule', onSuccess:()=>{ setScheduleOpen(false); setForm({ start:'', duration:30, notes:'' }); load(); }});

  function actionMutation(label:string, fn:()=>Promise<any>) {
    return useToastMutation(fn,{ successMessage: label + ' success', errorMessage: label + ' failed', onSuccess: load });
  }

  const cancelMut = actionMutation('Cancel', async ()=>{ if(!currentCustomerId|| !manage) return; await api.delete(`/customers/${currentCustomerId}/appointments/${manage.id}`); setManage(null); });
  const completeMut = actionMutation('Complete', async ()=>{ if(!currentCustomerId|| !manage) return; await api.post(`/customers/${currentCustomerId}/appointments/${manage.id}/complete`); setManage(null); });
  const rescheduleMut = actionMutation('Reschedule', async ()=>{ if(!currentCustomerId|| !manage || !reschedule.start) return; const start = new Date(reschedule.start); const end = new Date(start.getTime()+ reschedule.duration*60000); await api.put(`/customers/${currentCustomerId}/appointments/${manage.id}`, { appointmentId: manage.id, customerId: currentCustomerId, startUtc: start.toISOString(), endUtc: end.toISOString() }); setManage(null); });
  const updateNotesMut = actionMutation('Update notes', async ()=>{ if(!currentCustomerId|| !manage) return; await api.patch(`/customers/${currentCustomerId}/appointments/${manage.id}/notes`, JSON.stringify(noteEdit), { headers:{ 'Content-Type':'application/json' }}); setManage(null); });

  const statusClass = (s:string)=>{
    const k = s.toLowerCase();
    if(k==='scheduled') return 'status-open';
    if(k==='completed') return 'status-completed';
    if(k==='cancelled') return 'status-cancelled';
    return 'status-open';
  };

  if(!currentCustomerId) return null;

  return (
    <div className="card" style={{position:'relative'}}>
      <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',marginBottom:'1rem'}}>
        <h2 className="card-title" style={{margin:0}}>Appointments</h2>
        <div style={{display:'flex',gap:8}}>
          <button className="btn primary" onClick={()=>setScheduleOpen(true)}>Schedule</button>
        </div>
      </div>
      {loading && <ul>{Array.from({length:4}).map((_,i)=><li key={i} className="skeleton-line" style={{height:22}} />)}</ul>}
      {error && <div className="alert error">Failed to load appointments.</div>}
      {data && (
        <ul>
          {data.items.map(a => {
            return (
              <li key={a.id} style={{cursor:'pointer'}} onClick={()=>{ setManage(a); setReschedule({ start:a.startUtc.slice(0,16), duration: Math.max(5, Math.round((new Date(a.endUtc).getTime()- new Date(a.startUtc).getTime())/60000)) }); setNoteEdit(a.notes||''); }}>
                <span style={{flex:1,fontFamily:'ui-monospace'}}>{new Date(a.startUtc).toLocaleString([], { hour:'2-digit', minute:'2-digit', month:'short', day:'2-digit'})}</span>
                <span className={`badge ${statusClass(a.status)}`}>{a.status}</span>
                <span style={{fontSize:'.65rem',color:'var(--color-text-dim)'}}>Manage</span>
              </li>
            );
          })}
        </ul>
      )}
      {data && data.total > data.pageSize && (
        <div style={{display:'flex',justifyContent:'flex-end',gap:8,marginTop:12}}>
          <button className="btn" disabled={page<=1} onClick={()=> setPage(p=> Math.max(1,p-1))}>Prev</button>
          <span className="muted" style={{alignSelf:'center',fontSize:'.65rem'}}>Page {page}</span>
          <button className="btn" disabled={data.items.length < data.pageSize} onClick={()=> setPage(p=> p+1)}>Next</button>
        </div>
      )}

      {/* Schedule Modal */}
      {scheduleOpen && (
        <div className="modal-overlay" onClick={()=> setScheduleOpen(false)}>
          <div className="modal-shell" onClick={e=> e.stopPropagation()}>
            <div className="modal-header">
              <h3>Schedule Appointment</h3>
              <button className="modal-close" onClick={()=> setScheduleOpen(false)}>×</button>
            </div>
            <form onSubmit={e=>{ e.preventDefault(); scheduleMutation.mutate(); }} style={{display:'contents'}}>
              <div className="modal-body">
                <div className="modal-form-structure">
                  <div className="field-row">
                    <div className="form-field">
                      <label style={{fontSize:'.6rem'}}>Start</label>
                      <input type="datetime-local" value={form.start} onChange={e=>setForm(f=>({...f,start:e.target.value}))} required />
                    </div>
                    <div className="form-field narrow">
                      <label style={{fontSize:'.6rem'}}>Duration (min)</label>
                      <input type="number" min={5} max={480} value={form.duration} onChange={e=>setForm(f=>({...f,duration:parseInt(e.target.value)||30}))} />
                    </div>
                  </div>
                  <div className="form-field" style={{marginBottom:0}}>
                    <label style={{fontSize:'.6rem'}}>Notes (optional)</label>
                    <textarea rows={4} value={form.notes} onChange={e=>setForm(f=>({...f,notes:e.target.value}))} />
                  </div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn outline" onClick={()=> setScheduleOpen(false)}>Cancel</button>
                <button type="submit" className="btn primary" disabled={scheduleMutation.isPending}>{scheduleMutation.isPending? 'Scheduling...':'Save'}</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Manage Appointment Modal */}
      {manage && (
        <div className="modal-overlay" onClick={()=> setManage(null)}>
          <div className="modal-shell" onClick={e=> e.stopPropagation()}>
            <div className="modal-header">
              <h3>Manage Appointment</h3>
              <button className="modal-close" onClick={()=> setManage(null)}>×</button>
            </div>
            <div className="modal-body">
              <div className="form-grid">
                <div className="form-field"><label style={{fontSize:'.55rem'}}>Status</label><div className={`badge ${statusClass(manage.status)}`}>{manage.status}</div></div>
                <div className="form-field"><label style={{fontSize:'.55rem'}}>Current Start</label><div style={{fontSize:'.75rem'}}>{new Date(manage.startUtc).toLocaleString()}</div></div>
                <div className="form-field"><label style={{fontSize:'.55rem'}}>Current End</label><div style={{fontSize:'.75rem'}}>{new Date(manage.endUtc).toLocaleString()}</div></div>
                <div className="form-field" style={{gridColumn:'1 / -1'}}><label style={{fontSize:'.55rem'}}>Notes</label><textarea rows={3} value={noteEdit} onChange={e=> setNoteEdit(e.target.value)} /></div>
                <div className="form-field"><label style={{fontSize:'.55rem'}}>New Start</label><input type="datetime-local" value={reschedule.start} onChange={e=> setReschedule(r=>({...r,start:e.target.value}))} /></div>
                <div className="form-field"><label style={{fontSize:'.55rem'}}>Duration (min)</label><input type="number" min={5} max={480} value={reschedule.duration} onChange={e=> setReschedule(r=>({...r,duration: parseInt(e.target.value)|| r.duration}))} /></div>
              </div>
            </div>
            <div className="modal-footer" style={{flexWrap:'wrap'}}>
              {manage.status==='Scheduled' && <button className="btn success" disabled={completeMut.isPending} onClick={()=> completeMut.mutate()}>Complete</button>}
              {manage.status==='Scheduled' && <button className="btn danger" disabled={cancelMut.isPending} onClick={()=> cancelMut.mutate()}>Cancel</button>}
              <div style={{flex:1}} />
              <button className="btn outline" disabled={updateNotesMut.isPending} onClick={()=> updateNotesMut.mutate()}>Update Notes</button>
              <button className="btn" disabled={rescheduleMut.isPending} onClick={()=> rescheduleMut.mutate()}>Reschedule</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AppointmentsPanel;
