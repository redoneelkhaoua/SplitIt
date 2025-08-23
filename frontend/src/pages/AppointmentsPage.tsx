import React from 'react';
import { api } from '../api/client';
import { useCustomers } from '../state/CustomerContext';
import { useToast } from '../state/ToastContext';

interface Appt { id:string; customerId:string; startUtc:string; endUtc:string; notes:string|null; status:string; }
interface PageRes { items:Appt[]; total:number; page:number; pageSize:number; }

export const AppointmentsPage: React.FC = () => {
  const { customers } = useCustomers();
  const { push } = useToast();
  // Data / paging
  const [page,setPage] = React.useState(1);
  const [data,setData] = React.useState<PageRes|null>(null);
  const [loading,setLoading] = React.useState(false);
  // UI state
  const [showAdd,setShowAdd] = React.useState(false);
  const [filterCustomer,setFilterCustomer] = React.useState('');
  const [statusFilter,setStatusFilter] = React.useState<'all'|'scheduled'|'completed'|'cancelled'>('all');
  const [rawSearch,setRawSearch] = React.useState('');
  const [search,setSearch] = React.useState('');
  React.useEffect(()=> { const t=setTimeout(()=> setSearch(rawSearch.trim().toLowerCase()),350); return ()=> clearTimeout(t); },[rawSearch]);
  // Form state
  const [form,setForm] = React.useState({ customerId:'', start:'', duration:30, notes:'' });

  const load = React.useCallback(async ()=> {
    setLoading(true);
    try {
  const r = await api.get('/appointments', { params:{ page, pageSize:20, customerId: filterCustomer || undefined, status: statusFilter==='all'? undefined: statusFilter } });
      setData(r.data);
    } catch { push('Load appointments failed','error'); } finally { setLoading(false); }
  },[page, filterCustomer, statusFilter, push]);
  React.useEffect(()=> { load(); },[load]);

  const refresh = ()=> load();

  const save = async ()=> {
    try {
      if(!form.customerId || !form.start) return;
      const start=new Date(form.start);
      const end=new Date(start.getTime()+ form.duration*60000);
      await api.post(`/customers/${form.customerId}/appointments`, { customerId:form.customerId, startUtc:start.toISOString(), endUtc:end.toISOString(), notes: form.notes|| null });
      push('Appointment created','success');
      setShowAdd(false);
      setForm({ customerId:'', start:'', duration:30, notes:'' });
      load();
    } catch(e:any){ push(e?.message||'Create failed','error'); }
  };

  // Filter locally by search (customer name or notes) AFTER fetch
  const filtered = React.useMemo(()=> {
    if(!data) return [] as Appt[];
    if(!search) return data.items;
    return data.items.filter(a=> {
      const cust = customers.find(c=> c.id===a.customerId);
      const name = `${cust?.firstName||''} ${cust?.lastName||''}`.toLowerCase();
      return name.includes(search) || (a.notes||'').toLowerCase().includes(search);
    });
  },[data, search, customers]);
  const totalPages = data? Math.max(1, Math.ceil(data.total / 20)): 1;

  return (
  <div className="stack gap-md">
      <div className="card" style={{padding:'1.25rem 1.25rem 1.75rem'}}>
        <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',gap:16}}>
          <h2 className="card-title" style={{marginTop:0}}>Appointments list</h2>
          <div style={{display:'flex',gap:8}}>
            <button className="btn primary" onClick={()=> setShowAdd(true)}>+ Add</button>
            <button className="btn" onClick={refresh}>Refresh</button>
          </div>
        </div>
        <p className="muted" style={{fontSize:'.7rem',margin:'-.5rem 0 1rem'}}>View, filter and schedule customer appointments.</p>
        <div style={{display:'flex',gap:12,alignItems:'center',marginBottom:12}}>
          <input placeholder="Search customer or notes" value={rawSearch} onChange={e=> { setRawSearch(e.target.value); setPage(1);} } style={{flex:1}} />
          <select value={filterCustomer} onChange={e=> { setFilterCustomer(e.target.value); setPage(1);} } style={{fontSize:'.65rem',padding:'6px 10px',borderRadius:10,border:'1px solid var(--color-border)',background:'var(--color-surface-alt)'}}>
            <option value="">All Customers</option>
            {customers.map(c=> <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>)}
          </select>
          <select value={statusFilter} onChange={e=> { setStatusFilter(e.target.value as any); setPage(1);} } style={{fontSize:'.65rem',padding:'6px 10px',borderRadius:10,border:'1px solid var(--color-border)',background:'var(--color-surface-alt)'}}>
            <option value="all">All Statuses</option>
            <option value="scheduled">Scheduled</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
          </select>
          <button className="btn" disabled={loading || page<=1} onClick={()=> setPage(p=> Math.max(1,p-1))}>Prev</button>
            <span className="muted" style={{fontSize:'.65rem'}}>Page {page}/{totalPages}</span>
          <button className="btn" disabled={loading || page>=totalPages} onClick={()=> setPage(p=> Math.min(totalPages,p+1))}>Next</button>
        </div>
        <div style={{overflowX:'auto'}}>
          <table style={{width:'100%',borderCollapse:'collapse',fontSize:'.75rem'}}>
            <thead>
              <tr style={{textAlign:'left',fontSize:'.6rem',letterSpacing:'.7px',textTransform:'uppercase',color:'var(--color-text-dim)'}}>
                <th style={{padding:'6px 8px'}}>Start</th>
                <th style={{padding:'6px 8px'}}>Customer</th>
                <th style={{padding:'6px 8px'}}>Duration</th>
                <th style={{padding:'6px 8px'}}>Status</th>
                <th style={{padding:'6px 8px'}}>Notes</th>
                <th style={{padding:'6px 8px',textAlign:'right'}}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading && Array.from({length:6}).map((_,i)=>(<tr key={i}><td colSpan={5} style={{padding:0}}><div className="skeleton-line" style={{height:30,margin:'4px 0'}} /></td></tr>))}
              {!loading && filtered.map(a=> {
                const cust = customers.find(c=> c.id===a.customerId);
                const duration = Math.round((new Date(a.endUtc).getTime()- new Date(a.startUtc).getTime())/60000);
                return (
                  <tr key={a.id}>
                    <td style={{padding:'8px',fontFamily:'ui-monospace'}}>{new Date(a.startUtc).toLocaleString([], { month:'short', day:'2-digit', hour:'2-digit', minute:'2-digit'})}</td>
                    <td style={{padding:'8px'}}>{cust? `${cust.firstName} ${cust.lastName}`:'-'}</td>
                    <td style={{padding:'8px'}}>{duration}m</td>
                    <td style={{padding:'8px'}}><span className={`badge status-${a.status.toLowerCase()}`}>{a.status}</span></td>
                    <td style={{padding:'8px'}}>{a.notes? (a.notes.length>40? a.notes.slice(0,40)+'…': a.notes): <span className="muted">—</span>}</td>
                    <td style={{padding:'8px',position:'relative',textAlign:'right'}}>
                      <AppointmentActions appt={a} onChanged={load} />
                    </td>
                  </tr>
                );
              })}
              {!loading && filtered.length===0 && (
                <tr><td colSpan={6} style={{padding:'14px 8px'}} className="muted">No appointments found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
      {(showAdd) && (
        <div className="modal-overlay" onClick={()=> setShowAdd(false)}>
          <div className="modal-shell" style={{width:560}} onClick={e=> e.stopPropagation()}>
            <div className="modal-header">
              <h3>New Appointment</h3>
              <button className="modal-close" onClick={()=> setShowAdd(false)}>×</button>
            </div>
            <form onSubmit={e=> { e.preventDefault(); save(); }} style={{display:'contents'}}>
              <div className="modal-body">
                <div className="form-grid">
                  <div className="form-field" style={{gridColumn:'1 / -1'}}>
                    <label>Customer</label>
                    <select required value={form.customerId} onChange={e=> setForm(f=> ({...f,customerId:e.target.value}))}>
                      <option value="">Select...</option>
                      {customers.map(c=> <option key={c.id} value={c.id}>{c.firstName} {c.lastName} ({c.customerNumber})</option>)}
                    </select>
                  </div>
                  <div className="form-field"><label>Start</label><input type="datetime-local" required value={form.start} onChange={e=> setForm(f=> ({...f,start:e.target.value}))} /></div>
                  <div className="form-field"><label>Duration (min)</label><input type="number" min={5} max={480} value={form.duration} onChange={e=> setForm(f=> ({...f,duration:parseInt(e.target.value)||30}))} /></div>
                  <div className="form-field" style={{gridColumn:'1 / -1'}}><label>Notes (optional)</label><textarea rows={4} value={form.notes} onChange={e=> setForm(f=> ({...f,notes:e.target.value}))} /></div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn outline" onClick={()=> setShowAdd(false)}>Cancel</button>
                <button type="submit" className="btn primary">Save</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AppointmentsPage;

// Inline actions component
const AppointmentActions: React.FC<{ appt: Appt; onChanged: ()=>void; }> = ({ appt, onChanged }) => {
  const { customers } = useCustomers();
  const { push } = useToast();
  const [open,setOpen] = React.useState(false);
  const [editing,setEditing] = React.useState(false);
  const cust = customers.find(c=> c.id===appt.customerId);
  const [form,setForm] = React.useState(()=>({ start:new Date(appt.startUtc).toISOString().slice(0,16), duration: Math.round((new Date(appt.endUtc).getTime()- new Date(appt.startUtc).getTime())/60000), notes: appt.notes||'' }));

  React.useEffect(()=> {
    if(editing) {
      setForm({ start:new Date(appt.startUtc).toISOString().slice(0,16), duration: Math.round((new Date(appt.endUtc).getTime()- new Date(appt.startUtc).getTime())/60000), notes: appt.notes||'' });
    }
  },[editing, appt]);

  const closeAll = ()=> { setOpen(false); setEditing(false); };

  const reschedule = async ()=> {
    try {
      const start = new Date(form.start);
      const end = new Date(start.getTime()+ form.duration*60000);
      await api.put(`/customers/${appt.customerId}/appointments/${appt.id}`, { customerId: appt.customerId, appointmentId: appt.id, startUtc: start.toISOString(), endUtc: end.toISOString() });
      if(form.notes !== (appt.notes||'')) {
        await api.patch(`/customers/${appt.customerId}/appointments/${appt.id}/notes`, form.notes||'');
      }
      push('Appointment updated','success');
      closeAll();
      onChanged();
    } catch(e:any){ push(e?.message||'Update failed','error'); }
  };
  const complete = async ()=> { try { await api.post(`/customers/${appt.customerId}/appointments/${appt.id}/complete`); push('Marked complete','success'); closeAll(); onChanged(); } catch(e:any){ push(e?.message||'Complete failed','error'); } };
  const cancel = async ()=> { try { await api.delete(`/customers/${appt.customerId}/appointments/${appt.id}`); push('Cancelled','success'); closeAll(); onChanged(); } catch(e:any){ push(e?.message||'Cancel failed','error'); } };

  React.useEffect(()=>{
    if(!open) return;
    const onClick=(e:MouseEvent)=> { const t=e.target as HTMLElement; if(!t.closest('.appt-actions-wrapper')) setOpen(false); };
    const onKey=(e:KeyboardEvent)=> { if(e.key==='Escape') setOpen(false); };
    window.addEventListener('click',onClick); window.addEventListener('keydown',onKey);
    return ()=> { window.removeEventListener('click',onClick); window.removeEventListener('keydown',onKey); };
  },[open]);

  return (
    <div className="appt-actions-wrapper" style={{display:'inline-block'}}>
      <button type="button" className="btn" style={{padding:'4px 8px',fontSize:10}} onClick={(e)=> { e.stopPropagation(); setOpen(o=> !o); }}>⋯</button>
      {open && !editing && (
        <div style={{position:'absolute',top:'110%',right:0,background:'#fff',border:'1px solid var(--color-border)',borderRadius:10,boxShadow:'var(--shadow-lg)',padding:6,zIndex:300,minWidth:150,display:'flex',flexDirection:'column',gap:4}}>
          {appt.status === 'Scheduled' && <button className="btn" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} onClick={()=> setEditing(true)}>Edit / Reschedule</button>}
          {appt.status === 'Scheduled' && <button className="btn danger" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} onClick={cancel}>Cancel</button>}
          {appt.status === 'Scheduled' && <button className="btn primary" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} onClick={complete}>Mark Complete</button>}
          {appt.status !== 'Scheduled' && <div className="muted" style={{padding:'2px 6px',fontSize:10}}>No actions</div>}
        </div>
      )}
      {open && editing && (
        <div style={{position:'absolute',top:'110%',right:0,background:'#fff',border:'1px solid var(--color-border)',borderRadius:12,boxShadow:'var(--shadow-xl)',padding:10,zIndex:320,width:320}} onClick={e=> e.stopPropagation()}>
          <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',marginBottom:4}}>
            <strong style={{fontSize:12}}>Edit Appointment</strong>
            <button className="btn" style={{padding:'2px 6px',fontSize:10}} onClick={()=> setEditing(false)}>×</button>
          </div>
            <div className="form-field" style={{marginBottom:6}}>
              <label style={{fontSize:10}}>Customer</label>
              <div style={{fontSize:11}}>{cust? `${cust.firstName} ${cust.lastName}`:'-'}</div>
            </div>
            <div style={{display:'flex',gap:8,marginBottom:6}}>
              <div className="form-field" style={{flex:1}}>
                <label style={{fontSize:10}}>Start</label>
                <input type="datetime-local" value={form.start} onChange={e=> setForm(f=> ({...f,start:e.target.value}))} />
              </div>
              <div className="form-field" style={{width:90}}>
                <label style={{fontSize:10}}>Duration</label>
                <input type="number" min={5} max={480} value={form.duration} onChange={e=> setForm(f=> ({...f,duration: parseInt(e.target.value)||30}))} />
              </div>
            </div>
            <div className="form-field" style={{marginBottom:10}}>
              <label style={{fontSize:10}}>Notes</label>
              <textarea rows={3} value={form.notes} onChange={e=> setForm(f=> ({...f,notes:e.target.value}))} />
            </div>
            <div style={{display:'flex',justifyContent:'flex-end',gap:6}}>
              <button className="btn outline" style={{fontSize:11,padding:'6px 10px'}} onClick={()=> setEditing(false)} type="button">Cancel</button>
              <button className="btn primary" style={{fontSize:11,padding:'6px 12px'}} onClick={reschedule} type="button">Save</button>
            </div>
        </div>
      )}
    </div>
  );
};
