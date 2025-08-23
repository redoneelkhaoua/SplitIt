import axios from 'axios';
import { getToken } from '../state/tokenStore';

export const api = axios.create({ baseURL: '/api' });

api.interceptors.request.use(cfg => {
  const token = getToken();
  if (token) {
    cfg.headers = cfg.headers ?? {};
    (cfg.headers as any).Authorization = `Bearer ${token}`;
  }
  return cfg;
});

api.interceptors.response.use(r => r, err => {
  if (err.response?.status === 401) {
    localStorage.removeItem('authToken');
  }
  return Promise.reject(err);
});

export interface PagingEnvelope<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
