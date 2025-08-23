let _token: string | null = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null;

export function setToken(t: string | null) {
  _token = t;
  if (typeof window !== 'undefined') {
    if (t) localStorage.setItem('authToken', t); else localStorage.removeItem('authToken');
  }
}

export function getToken() { return _token; }
