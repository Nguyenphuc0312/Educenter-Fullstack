import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const accountApi = {
  getAll: () => callApi(http.get('/gateway/accounts')),
  getById: (id) => callApi(http.get(`/gateway/accounts/${id}`)),
  create: (data) => callApi(http.post('/gateway/accounts', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/accounts/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/accounts/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/accounts/bulk-update', items)),
  lock: (id) => callApi(http.put(`/gateway/accounts/${id}/lock`)),
  bulkLock: (ids) => callApi(http.put('/gateway/accounts/bulk-lock', { ids })),
  unlock: (id) => callApi(http.put(`/gateway/accounts/${id}/unlock`)),
  bulkUnlock: (ids) => callApi(http.put('/gateway/accounts/bulk-unlock', { ids })),
  delete: (id) => callApi(http.delete(`/gateway/accounts/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/accounts/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/accounts/import', file),
  export: () => downloadFile('/gateway/accounts/export', 'accounts.csv'),
};
