import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import AppointmentsPanel from '../components/AppointmentsPanel';
import { useToast } from '../state/ToastContext';

interface MeasurementEntry {
  date: string;
  chest: number;
  waist: number;
  hips: number;
  sleeve: number;
  [k: string]: any;
}
interface CustomerDetails {
  id: string;
  customerNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  enabled: boolean;
  measurements?: MeasurementEntry[];
}

const metricKeys = ['chest','waist','hips','sleeve'] as const;

const CustomerDetailPage: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { push } = useToast();

  const [loading, setLoading] = useState(true);
  const [details, setDetails] = useState<CustomerDetails | null>(null);
  const [error, setError] = useState<string|null>(null);

  // Measurements UI state
  const [showAllMeasurements, setShowAllMeasurements] = useState(false);
  const [measureOpen, setMeasureOpen] = useState(false);
  const [measurementForm, setMeasurementForm] = useState({ chest:'',waist:'',hips:'',sleeve:'' });
  const [selectedMeasurement, setSelectedMeasurement] = useState<MeasurementEntry | null>(null);
  const [editingMeasurement, setEditingMeasurement] = useState(false);
  const [editForm, setEditForm] = useState({ chest:'',waist:'',hips:'',sleeve:'' });
  const [tab, setTab] = useState<'appointments' | 'workorders'>('appointments');

  const loadDetails = async () => {
    if(!id) return;
    setLoading(true); setError(null);
    try {
      const res = await api.get(`/customers/${id}`);
      setDetails(res.data);
    } catch (e:any) {
      setError(e?.message || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(()=> { loadDetails(); },[id]);

  const addMeasurement = async () => {
    if(!id) return;
    try {
      await api.post(`/customers/${id}/measurements`, {
        customerId:id,
        date: new Date().toISOString(),
        chest: parseFloat(measurementForm.chest),
        waist: parseFloat(measurementForm.waist),
        hips: parseFloat(measurementForm.hips),
        sleeve: parseFloat(measurementForm.sleeve)
      });
  push('Measurement added','success');
      setMeasureOpen(false);
      setMeasurementForm({ chest:'',waist:'',hips:'',sleeve:'' });
      await loadDetails();
    } catch (e:any) {
  push(e?.message || 'Add failed','error');
    }
  };

  const detailsContent = () => {
    if(loading) return <div className="card" style={{flex:1}}>Loading...</div>;
    if(error) return <div className="card" style={{flex:1}}>{error}</div>;
    if(!details) return <div className="card" style={{flex:1}}>Not found</div>;

    return (
      <div style={{display:'flex',gap:24,alignItems:'flex-start'}}>
        {/* Left side summary */}
        <div style={{width:300,display:'flex',flexDirection:'column',gap:16}}>
          <div className="card" style={{padding:'1rem 1rem 1.25rem'}}>
            <h3 style={{margin:'0 0 .75rem',fontSize:'1rem'}}>Customer details</h3>
            <div style={{fontSize:'.75rem',display:'grid',rowGap:6}}>
              <div style={{fontWeight:600}}>{details.firstName} {details.lastName}</div>
              <div className="muted">#{details.customerNumber}</div>
              <div style={{display:'flex',alignItems:'center',gap:6}}>{details.email}</div>
              <div className={`badge ${details.enabled? 'status-open':'status-cancelled'}`} style={{justifySelf:'flex-start'}}>{details.enabled? 'Enabled':'Disabled'}</div>
            </div>
            <hr style={{margin:'1rem 0 .5rem',border:'none',borderTop:'1px solid var(--color-border)'}} />
            <div style={{display:'flex',justifyContent:'space-between',alignItems:'center',marginBottom:8}}>
              <h4 style={{margin:0,fontSize:'.75rem',letterSpacing:'.5px',textTransform:'uppercase'}}>Measurements</h4>
              <button className="add-measure-btn" onClick={()=> { setMeasurementForm({ chest:'',waist:'',hips:'',sleeve:'' }); setMeasureOpen(true); }}>+ Add</button>
            </div>
            {(!details.measurements || details.measurements.length===0) && <div className="muted" style={{fontSize:'.65rem'}}>No measurements yet.</div>}
            {details.measurements && details.measurements.length>0 && (
              <div className="measurement-history">
                {(() => {
                  const sorted = [...details.measurements].sort((a:any,b:any)=> new Date(b.date).getTime()- new Date(a.date).getTime());
                  const shown = showAllMeasurements? sorted : sorted.slice(0,5);
                  return (
                    <>
                      {shown.map((m:any,index:number)=> {
                        // diff vs next (older)
                        const next = sorted[index+1];
                        const makeDiff = (k:string) => {
                          if(!next) return null; const d = parseFloat((m[k]-next[k]).toFixed(1)); if(d===0) return '0'; return (d>0? '+' : '') + d; };
                        return (
                          <div key={m.date} className="measurement-history-card" onClick={()=> setSelectedMeasurement(m)}>
                            <div className="measurement-history-header">
                              <span>{new Date(m.date).toLocaleDateString()}</span>
                              <span style={{fontWeight:500,color:'#738090'}}>{index===0? 'Latest':''}</span>
                            </div>
                            <div className="measurement-history-grid">
                              {metricKeys.map(k=> {
                                const diff = makeDiff(k as string);
                                return (
                                  <div key={k} className="measurement-mini">
                                    <span className="measurement-mini-label">{k}</span>
                                    <span className="measurement-mini-value">{m[k]}"</span>
                                    {diff && diff!== '0' && <span className={`measurement-mini-diff ${diff.startsWith('-')? 'down':''}`}>{diff}"</span>}
                                    {diff=== '0' && <span className="measurement-mini-diff" style={{background:'#eef1f5',color:'#4a5662'}}>0"</span>}
                                  </div>
                                );
                              })}
                            </div>
                          </div>
                        );
                      })}
                      {sorted.length>5 && <button className="btn" style={{padding:'2px 6px',fontSize:10,alignSelf:'flex-start'}} onClick={()=> setShowAllMeasurements(s=> !s)}>{showAllMeasurements? 'Less':'More'}</button>}
                    </>
                  );
                })()}
              </div>
            )}
          </div>
        </div>
        {/* Right panel */}
        <div style={{flex:1,display:'flex',flexDirection:'column',gap:16}}>
          <div style={{display:'flex',gap:8,border:'1px solid var(--color-border)',padding:4,borderRadius:12,background:'var(--color-surface)'}}>
            <button className={tab==='appointments'? 'btn primary':'btn'} style={{flex:1}} onClick={()=> setTab('appointments')}>Appointments</button>
            <button className={tab==='workorders'? 'btn primary':'btn'} style={{flex:1}} onClick={()=> setTab('workorders')}>Work Orders</button>
          </div>
          {tab==='appointments' && <AppointmentsPanel />}
          {tab==='workorders' && <div className="card"><h2 className="card-title" style={{marginTop:0}}>Work Orders (todo)</h2></div>}
        </div>
      </div>
    );
  };

  const sortedMeasurements = details?.measurements ? [...details.measurements].sort((a:any,b:any)=> new Date(b.date).getTime()- new Date(a.date).getTime()) : [];

  return (
    <div style={{padding:'1.25rem 1.5rem'}}>
      <button className="btn" style={{marginBottom:16}} onClick={()=> navigate('/customers')}>← Back</button>
      {detailsContent()}

      {/* Add measurement modal */}
      {measureOpen && (
        <div className="measurement-add-overlay" onClick={()=> setMeasureOpen(false)}>
          <div className="measurement-add-modal" onClick={e=> e.stopPropagation()}>
            <div className="measurement-add-header">
              <h3>Add Measurement</h3>
            </div>
            <form onSubmit={e=> { e.preventDefault(); addMeasurement(); }}>
              <div className="measurement-add-body">
                <div className="measurement-field-grid">
                  {metricKeys.map(k=> (
                    <div key={k} className="measurement-field">
                      <label>{k}</label>
                      <div className="measurement-input-wrap">
                        <input type="number" step="0.1" value={(measurementForm as any)[k]} onChange={e=> setMeasurementForm(f=> ({...f,[k]:e.target.value}))} required />
                        <span className="unit">"</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              <div className="measurement-add-footer">
                <button type="button" className="btn outline" onClick={()=> setMeasureOpen(false)}>Cancel</button>
                <button type="submit" className="btn primary">Save</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Measurement detail modal */}
      {selectedMeasurement && (
        <div style={{position:'fixed',inset:0,background:'rgba(0,0,0,.50)',backdropFilter:'blur(2px)',display:'flex',alignItems:'flex-start',justifyContent:'center',paddingTop:'5vh',zIndex:350}} onClick={()=> { setSelectedMeasurement(null); setEditingMeasurement(false); }}>
          <div className="measurement-modal" style={{width:960,maxWidth:'96%'}} onClick={e=> e.stopPropagation()}>
            <div className="measurement-modal-header">
              <div>
                <h3>Measurement Details</h3>
                <div style={{fontSize:'.62rem',opacity:.85,letterSpacing:.5}}>{new Date(selectedMeasurement.date).toLocaleString()}</div>
              </div>
              <button className="btn" style={{background:'transparent',color:'#fff',borderColor:'rgba(255,255,255,.25)'}} onClick={()=> { setSelectedMeasurement(null); setEditingMeasurement(false); }}>×</button>
            </div>
            <div className="measurement-modal-body">
              <div className="measurement-board">
                <h4 className="measurement-board-title">Current Measurements</h4>
                {metricKeys.map(k=> {
                  const label = k.toUpperCase();
                  let diffStr = ''; let diffNum = 0;
                  const sorted = sortedMeasurements;
                  if(sorted.length>1){
                    const idx = sorted.findIndex(m=> m.date===selectedMeasurement.date);
                    if(idx!==sorted.length-1){
                      const prev = sorted[idx+1];
                      diffNum = parseFloat((selectedMeasurement[k]- prev[k]).toFixed(1));
                      diffStr = (diffNum>=0? '+':'') + diffNum + '"';
                    }
                  }
                  const val = selectedMeasurement[k];
                  const palette:any = {
                    chest:{ bar:'#2563eb', bg:'#f1f6ff' },
                    waist:{ bar:'#059669', bg:'#eefcf5' },
                    hips:{ bar:'#7e22ce', bg:'#f7f1ff' },
                    sleeve:{ bar:'#ea580c', bg:'#fff5ec' }
                  };
                  const p = palette[k];
                  return (
                    <div key={k} className="measurement-item" style={{'--mi-bar':p.bar,'--mi-bg':p.bg} as any}>
                      <div className="mi-text">
                        <span className="mi-label">{label}</span>
                        {!editingMeasurement && <span className="mi-value">{val}<span className="unit">"</span></span>}
                        {editingMeasurement && (
                          <input type="number" step="0.1" value={(editForm as any)[k]} onChange={e=> setEditForm(f=> ({...f,[k]:e.target.value}))} style={{fontSize:'1.2rem',fontWeight:600,padding:4,width:90}} />
                        )}
                      </div>
                      {!editingMeasurement && (
                        diffStr ? (
                          <span className={`mi-diff-chip ${diffNum<0? 'down':''}`}>
                            <span className="arrow" />{diffStr}
                          </span>
                        ) : <span className="mi-diff-chip" style={{background:'#eef1f5',color:'#4a5662'}}><span style={{width:6,height:6,borderRadius:'50%',background:'#9aa7b6',display:'inline-block'}}/>0"</span>
                      )}
                    </div>
                  );
                })}
                {editingMeasurement && (
                  <div style={{display:'flex',gap:8,marginTop:4}}>
                    <button className="btn primary" onClick={async ()=> { if(!id) return; await api.post(`/customers/${id}/measurements`, { customerId:id, date:new Date().toISOString(), chest:parseFloat(editForm.chest), waist:parseFloat(editForm.waist), hips:parseFloat(editForm.hips), sleeve:parseFloat(editForm.sleeve) }); setEditingMeasurement(false); await loadDetails(); push('New measurement version saved','success'); }}>Save New</button>
                    <button className="btn" onClick={()=> setEditingMeasurement(false)}>Cancel</button>
                  </div>
                )}
              </div>
              <div className="measurement-figure">
                {/* SVG body figure for sharper lines */}
                <div style={{position:'relative'}}>
                  <svg width={280} height={400} viewBox="0 0 280 400" style={{color:'#d9e0e7'}}>
                    <ellipse cx="140" cy="40" rx="25" ry="30" fill="currentColor" />
                    <rect x="127" y="65" width="26" height="15" rx="13" fill="currentColor" />
                    <path d="M 100 80 Q 88 115 92 160 Q 96 200 108 240 L 172 240 Q 184 200 188 160 Q 192 115 180 80 Z" fill="currentColor" />
                    <ellipse cx="75" cy="115" rx="12" ry="45" transform="rotate(-12 75 115)" fill="currentColor" />
                    <ellipse cx="205" cy="115" rx="12" ry="45" transform="rotate(12 205 115)" fill="currentColor" />
                    <rect x="115" y="240" width="18" height="80" rx="9" fill="currentColor" />
                    <rect x="147" y="240" width="18" height="80" rx="9" fill="currentColor" />
                    <ellipse cx="124" cy="330" rx="15" ry="8" fill="currentColor" />
                    <ellipse cx="156" cy="330" rx="15" ry="8" fill="currentColor" />
                  </svg>
                  <svg width={280} height={400} style={{position:'absolute',inset:0,pointerEvents:'none'}}>
                    <line x1="88" y1="100" x2="192" y2="100" stroke="#2563eb" strokeWidth="2" strokeDasharray="4,4" opacity=".75" />
                    <circle cx="88" cy="100" r="3" fill="#2563eb" /><circle cx="192" cy="100" r="3" fill="#2563eb" />
                    <line x1="92" y1="140" x2="188" y2="140" stroke="#059669" strokeWidth="2" strokeDasharray="4,4" opacity=".75" />
                    <circle cx="92" cy="140" r="3" fill="#059669" /><circle cx="188" cy="140" r="3" fill="#059669" />
                    <line x1="108" y1="180" x2="172" y2="180" stroke="#7e22ce" strokeWidth="2" strokeDasharray="4,4" opacity=".75" />
                    <circle cx="108" cy="180" r="3" fill="#7e22ce" /><circle cx="172" cy="180" r="3" fill="#7e22ce" />
                    <line x1="180" y1="85" x2="205" y2="110" stroke="#ea580c" strokeWidth="2" strokeDasharray="4,4" opacity=".75" />
                    <circle cx="180" cy="85" r="3" fill="#ea580c" /><circle cx="205" cy="110" r="3" fill="#ea580c" />
                  </svg>
                </div>
              </div>
            </div>
            <div className="measurement-modal-footer">
              <button className="btn" onClick={()=> { setSelectedMeasurement(null); setEditingMeasurement(false); }}>Close</button>
              {!editingMeasurement && <button className="btn dark" onClick={()=> { setEditingMeasurement(true); setEditForm({ chest:String(selectedMeasurement.chest), waist:String(selectedMeasurement.waist), hips:String(selectedMeasurement.hips), sleeve:String(selectedMeasurement.sleeve) }); }}>Edit Measurement</button>}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CustomerDetailPage;
