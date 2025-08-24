import React from 'react';

// Modern professional tailoring logo - needle, thread, and scissors motif
export const Logo: React.FC<{ size?: number; text?: boolean }>=({ size=42, text=true })=>{
  return (
    <div style={{display:'flex',alignItems:'center',gap:10}}>
      <svg width={size} height={size} viewBox="0 0 64 64" role="img" aria-label="TailoringPro logo">
        <defs>
          <linearGradient id="lg" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0%" stopColor="#1a365d"/>
            <stop offset="50%" stopColor="#2d3748"/>
            <stop offset="100%" stopColor="#1a202c"/>
          </linearGradient>
          <linearGradient id="accent" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0%" stopColor="#3182ce"/>
            <stop offset="100%" stopColor="#2b6cb0"/>
          </linearGradient>
        </defs>
        
        {/* Background circle */}
        <circle cx="32" cy="32" r="30" fill="url(#lg)" stroke="#e2e8f0" strokeWidth="2"/>
        
        {/* Scissors */}
        <g transform="translate(20, 18)">
          <circle cx="6" cy="8" r="3" fill="url(#accent)" stroke="#ffffff" strokeWidth="1"/>
          <circle cx="18" cy="8" r="3" fill="url(#accent)" stroke="#ffffff" strokeWidth="1"/>
          <path d="M9 8 L15 8 M12 5 L12 11" stroke="#ffffff" strokeWidth="2" strokeLinecap="round"/>
        </g>
        
        {/* Needle */}
        <path d="M35 45 L48 18" stroke="url(#accent)" strokeWidth="3" strokeLinecap="round"/>
        <circle cx="47" cy="20" r="2" fill="#ffffff" stroke="url(#accent)" strokeWidth="1.5"/>
        
        {/* Thread curve */}
        <path d="M20 48 Q32 35 44 42" fill="none" stroke="url(#accent)" strokeWidth="2.5" strokeLinecap="round" strokeDasharray="2,3" opacity="0.8"/>
        
        {/* Small decorative elements */}
        <circle cx="15" cy="45" r="1.5" fill="url(#accent)" opacity="0.6"/>
        <circle cx="48" cy="48" r="1.5" fill="url(#accent)" opacity="0.6"/>
      </svg>
      {text && (
        <div style={{display:'flex',flexDirection:'column'}}>
          <span className="brand" style={{fontSize:'1.9rem',fontWeight:700}}>TailoringPro</span>
          <span className="tag" style={{fontSize:'.7rem',color:'var(--color-text-dim)'}}>Professional Management System</span>
        </div>
      )}
    </div>
  );
};

export default Logo;
