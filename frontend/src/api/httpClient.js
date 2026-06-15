import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://127.0.0.1:5000';

// =====================================================
// Axios instance
// =====================================================
export const http = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

// =====================================================
// Request interceptor — attach Bearer token
// =====================================================
http.interceptors.request.use(
  config => {
    const token = localStorage.getItem('edu_token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error),
);

// =====================================================
// Response interceptor — unwrap data, handle 401/errors
// =====================================================
http.interceptors.response.use(
  response => response,
  async error => {
    const status = error?.response?.status;
    const message = error?.response?.data?.message || '';
    const accountLocked = status === 403 && /account is locked/i.test(message);

    // 401 — token expired / invalid
    if (status === 401 || accountLocked) {
      localStorage.removeItem('edu_token');
      localStorage.removeItem('edu_user');
      // Avoid redirect loop on login page
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
      return Promise.reject(error);
    }

    return Promise.reject(error);
  },
);

// =====================================================
// Helper: unwrap standard ApiResponse { success, data, message, errors }
// Returns response.data.data or throws with message
// =====================================================
export async function callApi(requestPromise) {
  try {
    const response = await requestPromise;
    const body = response.data;

    if (body?.success === false) {
      const errMsg = body.message || 'Có lỗi xảy ra';
      const err = new Error(errMsg);
      err.errors = body.errors;
      throw err;
    }

    return body?.data ?? body;
  } catch (err) {
    if (err?.response) {
      const body = err.response.data;
      const msg = body?.message || err.message || 'Có lỗi từ server';
      const apiErr = new Error(msg);
      apiErr.status = err.response.status;
      apiErr.errors = body?.errors;
      throw apiErr;
    }
    throw err;
  }
}

/** Unwrap a paged response — returns { items, pageIndex, pageSize, totalItems, totalPages } */
export async function callPagedApi(requestPromise) {
  const data = await callApi(requestPromise);
  if (data && 'items' in data) return data;
  // If backend returns array directly (non-paged)
  return { items: Array.isArray(data) ? data : [], totalItems: Array.isArray(data) ? data.length : 0, pageIndex: 1, pageSize: 100, totalPages: 1 };
}

export function uploadJsonImport(url, file) {
  const formData = new FormData();
  formData.append('file', file);
  return callApi(http.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } }));
}

export async function downloadFile(url, filename) {
  const response = await http.get(url, { responseType: 'blob' });
  const objectUrl = window.URL.createObjectURL(response.data);
  const link = document.createElement('a');
  link.href = objectUrl;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(objectUrl);
}

export default http;
