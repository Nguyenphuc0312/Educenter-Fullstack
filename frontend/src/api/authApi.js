import http, { callApi } from './httpClient';

export const authApi = {
  login: (credentials) => callApi(http.post('/gateway/auth/login', credentials)),
  register: (data) => callApi(http.post('/gateway/auth/register', data)),
  me: () => callApi(http.get('/gateway/auth/me')),
  logout: () => callApi(http.post('/gateway/auth/logout')).catch(() => {}),
};
