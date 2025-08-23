import React from 'react';
import { AuthProvider } from '../state/AuthContext';
import { ToastProvider } from '../state/ToastContext';
import LoginPage from './LoginPage';
import { useAuth } from '../state/AuthContext';
import { CustomerProvider } from '../state/CustomerContext';
import { BrowserRouter, Routes, Route, NavLink, Navigate } from 'react-router-dom';
import CustomersPage from './CustomersPage';
import CustomerDetailPage from './CustomerDetailPage';
import AppointmentsPage from './AppointmentsPage';
import WorkOrdersListPage from './WorkOrdersListPage';
import WorkOrderDetailPage from './WorkOrderDetailPage';
import { Logo } from '../components/Logo';

export const App: React.FC = () => {
  return (
    <ToastProvider>
      <AuthProvider>
        <Inner />
      </AuthProvider>
    </ToastProvider>
  );
};

const Inner: React.FC = () => {
  const { token } = useAuth();
  if (!token) return <LoginPage />;
  return (
    <CustomerProvider>
      <BrowserRouter>
        <AppLayout />
      </BrowserRouter>
    </CustomerProvider>
  );
};

const AppLayout: React.FC = () => {
  const { logout } = useAuth();
  return (
    <div className="app-shell">
      <aside className="sidebar" style={{gap:'1.5rem'}}>
        <div style={{display:'flex',flexDirection:'column',gap:'.35rem'}}>
          <Logo />
          <p className="tag" style={{margin:0}}>Tailoring Console</p>
        </div>
        <nav className="stack gap-sm" style={{fontSize:'.8rem'}}>
          <NavLink to="/customers" className={({isActive})=> isActive? 'nav-link active':'nav-link'}>Customers</NavLink>
          <NavLink to="/appointments" className={({isActive})=> isActive? 'nav-link active':'nav-link'}>Appointments</NavLink>
          <NavLink to="/workorders" className={({isActive})=> isActive? 'nav-link active':'nav-link'}>Work Orders</NavLink>
          <button className="btn danger" style={{marginTop:'1rem'}} onClick={logout}>Logout</button>
        </nav>
      </aside>
      <div className="main">
        <div className="topbar" style={{justifyContent:'space-between'}}>
          <div className="muted" style={{fontSize:'.75rem'}}>El Khaoua Admin</div>
        </div>
        <div className="content">
          <Routes>
            <Route path="/" element={<Navigate to="/customers" replace />} />
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/customers/:id" element={<CustomerDetailPage />} />
            <Route path="/appointments" element={<AppointmentsPage />} />
            <Route path="/workorders" element={<WorkOrdersListPage />} />
            <Route path="/workorders/:workOrderId" element={<WorkOrderDetailPage />} />
            <Route path="*" element={<div className="card"><h2 className="card-title" style={{marginTop:0}}>Not Found</h2></div>} />
          </Routes>
        </div>
      </div>
    </div>
  );
};
