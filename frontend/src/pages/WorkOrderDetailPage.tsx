import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { WorkOrderDetail } from '../components/WorkOrderDetail';
import { CustomerSelector } from '../components/CustomerSelector';
import { useCustomers } from '../state/CustomerContext';

export const WorkOrderDetailPage: React.FC = () => {
  const { workOrderId } = useParams<{ workOrderId: string }>();
  const { currentCustomerId } = useCustomers();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!currentCustomerId) {
      navigate('/customers');
    }
  }, [currentCustomerId, navigate]);

  if (!workOrderId) {
    return (
      <div className="card">
        <div className="alert error">Invalid work order ID</div>
      </div>
    );
  }

  return (
    <div className="stack gap-md">
      <div className="card">
        <h2 className="card-title" style={{ marginTop: 0 }}>Customer</h2>
        <CustomerSelector />
      </div>
      
      {currentCustomerId && (
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-start' }}>
          <button 
            className="btn outline" 
            onClick={() => navigate('/workorders')}
            style={{ flexShrink: 0 }}
          >
            ← Back to Work Orders
          </button>
        </div>
      )}

      {currentCustomerId && <WorkOrderDetail workOrderId={workOrderId} />}
    </div>
  );
};

export default WorkOrderDetailPage;
