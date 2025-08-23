import React from 'react';
import { useCustomers, CustomerSummary } from '../state/CustomerContext';
import { api } from '../api/client';
import { useToastMutation } from '../api/useToastMutation';
import { useToast } from '../state/ToastContext';
import { useNavigate } from 'react-router-dom';

export const CustomersPage: React.FC = () => {
  const { customers, loading, error, search, setSearch, page, setPage, total, refresh } = useCustomers() as any;
  const [query, setQuery] = React.useState(search);
  React.useEffect(()=>{ const t=setTimeout(()=> setSearch(query),350); return ()=> clearTimeout(t); },[query,setSearch]);
  const totalPages = Math.max(1, Math.ceil(total/20));
  const navigate = useNavigate();
  const { push } = useToast();
  // Row action menu state
  const [menuFor, setMenuFor] = React.useState<string|null>(null);
  const toggleMenu = (id:string)=> setMenuFor(m=> m===id? null: id);
  // Close menu on outside click / escape
  React.useEffect(()=>{
    if(!menuFor) return;
    const onClick = (e:MouseEvent)=> {
      const target = e.target as HTMLElement;
      if(!target.closest?.('.customer-actions-menu') && !target.closest?.('.customer-actions-trigger')) {
        setMenuFor(null);
      }
    };
    const onKey = (e:KeyboardEvent)=> { if(e.key==='Escape') setMenuFor(null); };
    window.addEventListener('click', onClick);
    window.addEventListener('keydown', onKey);
    return ()=> { window.removeEventListener('click', onClick); window.removeEventListener('keydown', onKey); };
  },[menuFor]);
  // Modals
  const [showAdd, setShowAdd] = React.useState(false);
  const [showEdit, setShowEdit] = React.useState(false);
  const [editCustomer, setEditCustomer] = React.useState<CustomerSummary | null>(null);
  const [addForm, setAddForm] = React.useState({ firstName:'', lastName:'', email:'' });
  const [editForm, setEditForm] = React.useState({ firstName:'', lastName:'', email:'' });

  const openEdit = (c:CustomerSummary)=> { setEditCustomer(c); setEditForm({ firstName:c.firstName, lastName:c.lastName, email:c.email }); setShowEdit(true); setMenuFor(null); };

  const createMutation = useToastMutation(async ()=> { await api.post('/customers', { firstName:addForm.firstName,lastName:addForm.lastName,email:addForm.email,dateOfBirth:null,phone:null,address:null,fitPreference:null,stylePreference:null,fabricPreference:null }); }, { successMessage:'Customer created', onSuccess:()=> { setShowAdd(false); setAddForm({firstName:'',lastName:'',email:''}); refresh(); }});
  const updateMutation = useToastMutation(async ()=> { if(!editCustomer) return; await api.put(`/customers/${editCustomer.id}`, { id:editCustomer.id, firstName:editForm.firstName,lastName:editForm.lastName,email:editForm.email,dateOfBirth:null,phone:null,address:null,fitPreference:null,stylePreference:null,fabricPreference:null }); }, { successMessage:'Customer updated', onSuccess:()=> { setShowEdit(false); setEditCustomer(null); refresh(); }});
  const disableMutation = useToastMutation(async (id:string)=> { await api.delete(`/customers/${id}`); }, { successMessage:'Customer disabled', onSuccess:()=> { refresh(); }});
  const restoreMutation = useToastMutation(async (id:string)=> { await api.post(`/customers/${id}/restore`); }, { successMessage:'Customer restored', onSuccess:()=> { refresh(); }});
  return (
    <div className="stack gap-md">
      <div className="card" style={{padding:'1.25rem 1.25rem 1.75rem'}}>
        <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',gap:16}}>
          <h2 className="card-title" style={{marginTop:0}}>Customers list</h2>
          <div style={{display:'flex',gap:8}}>
            <button className="btn primary" onClick={()=> setShowAdd(true)}>+ Add</button>
            <button className="btn" onClick={refresh}>Refresh</button>
          </div>
        </div>
        <p className="muted" style={{fontSize:'.7rem',margin:'-.5rem 0 1rem'}}>View, select and inspect customer details.</p>
        <div style={{display:'flex',gap:12,alignItems:'center',marginBottom:12}}>
          <input placeholder="Search name, email or number" value={query} onChange={e=> setQuery(e.target.value)} style={{flex:1}} />
          <button className="btn" disabled={loading || page<=1} onClick={()=> setPage(Math.max(1,page-1))}>Prev</button>
          <span className="muted" style={{fontSize:'.65rem'}}>Page {page}/{totalPages}</span>
          <button className="btn" disabled={loading || page>=totalPages} onClick={()=> setPage(Math.min(totalPages,page+1))}>Next</button>
        </div>
  <div className="customer-table-wrapper" style={{overflowX:'visible',position:'relative'}}>
      <table className="customer-table" style={{width:'100%',borderCollapse:'collapse',fontSize:'.75rem'}}>
            <thead>
              <tr style={{textAlign:'left',fontSize:'.6rem',letterSpacing:'.7px',textTransform:'uppercase',color:'var(--color-text-dim)'}}>
        <th style={{padding:'6px 8px'}}>Name</th>
        <th style={{padding:'6px 8px'}}>Email</th>
        <th style={{padding:'6px 8px'}}>Number</th>
        <th style={{padding:'6px 8px'}}>Registered</th>
                <th style={{padding:'6px 8px'}}>Status</th>
                <th style={{padding:'6px 8px',textAlign:'right',width:70}}>Actions</th>
              </tr>
            </thead>
            <tbody>
      {loading && Array.from({length:6}).map((_,i)=>(<tr key={i}><td colSpan={6} style={{padding:0}}><div className="skeleton-line" style={{height:30,margin:'4px 0'}} /></td></tr>))}
              {!loading && customers.map((c:CustomerSummary) => {
                const open = menuFor === c.id;
                return (
                  <tr key={c.id}>
                    <td style={{padding:'8px'}} onClick={()=> navigate(`/customers/${c.id}`)}>
                      <div style={{display:'flex',alignItems:'center',gap:10}}>
                        <div className="avatar-circle" style={{width:42,height:42,borderRadius:'50%',background:'var(--color-surface-alt)',border:'2px solid var(--color-border)',display:'flex',alignItems:'center',justifyContent:'center',fontSize:'.7rem',fontWeight:700,letterSpacing:'.5px',color:'var(--color-text-dim)'}}>{c.firstName?.[0]}{c.lastName?.[0]}</div>
                        <div style={{display:'flex',flexDirection:'column'}}>
          <span style={{fontWeight:600,fontSize:'.8rem'}}>{c.firstName} {c.lastName}</span>
          <span style={{fontSize:'.6rem'}} className="muted">{c.customerNumber}</span>
                        </div>
                      </div>
                    </td>
        <td style={{padding:'8px'}}>{c.email}</td>
        <td style={{padding:'8px'}}>{c.customerNumber}</td>
        <td style={{padding:'8px'}}>{c.registrationDate? new Date(c.registrationDate).toLocaleDateString(): '-'}</td>
        <td style={{padding:'8px'}}><span className={`badge ${c.enabled? 'status-open':'status-cancelled'}`}>{c.enabled? 'Enabled':'Disabled'}</span></td>
                    <td style={{padding:'8px',position:'relative',textAlign:'right'}}>
                      <button type="button" aria-haspopup="menu" aria-expanded={open} className="btn customer-actions-trigger" style={{padding:'4px 8px',fontSize:10}} onClick={(e)=> { e.stopPropagation(); toggleMenu(c.id); }}>⋯</button>
                      {open && (
                        <div role="menu" className="customer-actions-menu" style={{position:'absolute',top:'110%',right:0,background:'#fff',border:'1px solid var(--color-border)',borderRadius:10,boxShadow:'var(--shadow-lg)',padding:6,zIndex:400,minWidth:140,maxHeight:260,overflowY:'auto',display:'flex',flexDirection:'column',gap:4}} onClick={e=> e.stopPropagation()}>
                          <button role="menuitem" className="btn" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} onClick={()=> openEdit(c)}>Edit</button>
                          {c.enabled && <button role="menuitem" className="btn danger" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} disabled={disableMutation.isPending} onClick={()=> disableMutation.mutate(c.id)}>Disable</button>}
                          {!c.enabled && <button role="menuitem" className="btn primary" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} disabled={restoreMutation.isPending} onClick={()=> restoreMutation.mutate(c.id)}>Restore</button>}
                          <button role="menuitem" className="btn" style={{padding:'4px 8px',fontSize:11,justifyContent:'flex-start'}} onClick={()=> { setMenuFor(null); navigate(`/customers/${c.id}`); }}>Open Details</button>
                        </div>
                      )}
                    </td>
                  </tr>
                );
              })}
              {!loading && customers.length===0 && (
                <tr><td colSpan={6} style={{padding:'14px 8px'}} className="muted">No customers found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
      {/* Modals */}
      {(showAdd || showEdit) && (
        <div className="modal-overlay" onClick={()=>{ setShowAdd(false); setShowEdit(false); }}>
          <div className="modal-shell" style={{width:560}} onClick={e=> e.stopPropagation()}>
            <div className="modal-header">
              <h3>{showAdd? 'Add Customer':'Edit Customer'}</h3>
              <button className="modal-close" onClick={()=> { setShowAdd(false); setShowEdit(false); }}>×</button>
            </div>
            <form onSubmit={e=>{ e.preventDefault(); showAdd? createMutation.mutate(): updateMutation.mutate(); }} style={{display:'contents'}}>
              <div className="modal-body">
                <div className="form-grid">
                  <div className="form-field"><label>First Name</label><input value={showAdd? addForm.firstName: editForm.firstName} onChange={e=> showAdd? setAddForm(f=>({...f,firstName:e.target.value})): setEditForm(f=>({...f,firstName:e.target.value}))} required /></div>
                  <div className="form-field"><label>Last Name</label><input value={showAdd? addForm.lastName: editForm.lastName} onChange={e=> showAdd? setAddForm(f=>({...f,lastName:e.target.value})): setEditForm(f=>({...f,lastName:e.target.value}))} required /></div>
                  <div className="form-field" style={{gridColumn:'1 / -1'}}><label>Email</label><input type="email" value={showAdd? addForm.email: editForm.email} onChange={e=> showAdd? setAddForm(f=>({...f,email:e.target.value})): setEditForm(f=>({...f,email:e.target.value}))} required /></div>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn outline" onClick={()=> { setShowAdd(false); setShowEdit(false); }}>Cancel</button>
                <button type="submit" className="btn primary" disabled={createMutation.isPending || updateMutation.isPending}>{showAdd? (createMutation.isPending? 'Creating...':'Create'):(updateMutation.isPending? 'Saving...':'Save')}</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default CustomersPage;
