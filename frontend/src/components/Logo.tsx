import React from 'react';

// Simple tailor-inspired logo (needle & thread stylized) - placeholder customizable
export const Logo: React.FC<{ size?: number; text?: boolean }>=({ size=42, text=true })=>{
  const stroke = '#1d6fd1';
  return (
    <div style={{display:'flex',alignItems:'center',gap:10}}>
      <svg width={size} height={size} viewBox="0 0 64 64" role="img" aria-label="El Khaoua logo">
        <defs>
          <linearGradient id="lg" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0%" stopColor="#133d66"/>
            <stop offset="60%" stopColor="#1d6fd1"/>
            <stop offset="100%" stopColor="#1d7ea8"/>
          </linearGradient>
        </defs>
        {/* Thread curve */}
        <path d="M8 48c18-10 30-26 48-32" fill="none" stroke="url(#lg)" strokeWidth={4} strokeLinecap="round" strokeLinejoin="round"/>
        {/* Needle */}
        <path d="M40 8 27 57" stroke={stroke} strokeWidth={4} strokeLinecap="round"/>
        <circle cx="39" cy="12" r="3.2" fill="#fff" stroke={stroke} strokeWidth={2} />
        {/* Minimal spool */}
        <rect x="12" y="44" width="10" height="6" rx="2" fill={stroke} opacity={.15} />
      </svg>
      {text && <div style={{display:'flex',flexDirection:'column'}}><span className="brand" style={{fontSize:'1.9rem'}}>El Khaoua</span><span className="tag" style={{fontSize:'.7rem'}}>Tailoring & Work Orders</span></div>}
    </div>
  );
};

export default Logo;
