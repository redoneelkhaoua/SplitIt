import React from 'react';
import { useForm } from 'react-hook-form';
import { useAuth } from '../state/AuthContext';

interface LoginForm {
  username: string;
  password: string;
}

export const LoginPage: React.FC = () => {
  const { register, handleSubmit, formState: { isSubmitting } } = useForm<LoginForm>();
  const { login, loading, error } = useAuth();
  const [localError, setLocalError] = React.useState<string | null>(null);

  const onSubmit = async (data: LoginForm) => {
    setLocalError(null);
    try {
      await login(data.username.trim(), data.password);
    } catch {
      // error handled in context, just trigger re-render
      setLocalError('');
    }
  };

  return (
    <div className="auth-screen">
      <div className="brand-block">
        <h1 className="brand">El Khaoua</h1>
        <p className="tag">Tailoring & Work Orders</p>
      </div>
      <div className="card auth-card">
        <h2 className="card-title">Sign In</h2>
        <form onSubmit={handleSubmit(onSubmit)} className="form-vertical" noValidate>
          <div className="form-field">
            <label>Username</label>
            <input autoComplete="username" required {...register('username', { required: true })} />
          </div>
          <div className="form-field">
            <label>Password</label>
            <input type="password" autoComplete="current-password" required {...register('password', { required: true })} />
          </div>
          {(error || localError) && <div className="alert error">{error}</div>}
          <button type="submit" className="btn primary" disabled={loading || isSubmitting}>
            {(loading || isSubmitting) ? <span className="spinner" /> : 'Login'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default LoginPage;
