import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const roomApi = {
  getAll: () => callApi(http.get('/gateway/rooms')),
  getById: (id) => callApi(http.get(`/gateway/rooms/${id}`)),
  getUsage: (id) => callApi(http.get(`/gateway/rooms/${id}/usage`)),
  create: (data) => callApi(http.post('/gateway/rooms', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/rooms/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/rooms/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/rooms/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/rooms/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/rooms/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/rooms/import', file),
  export: () => downloadFile('/gateway/rooms/export', 'rooms.csv'),
};
