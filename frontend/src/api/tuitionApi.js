import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const tuitionApi = {
  getAll: () => callApi(http.get('/gateway/tuition')),
  getById: (id) => callApi(http.get(`/gateway/tuition/${id}`)),
  getByStudent: (studentId) => callApi(http.get(`/gateway/tuition/by-student/${studentId}`)),
  getByClass: (classId) => callApi(http.get(`/gateway/tuition/by-class/${classId}`)),
  getDebts: () => callApi(http.get('/gateway/tuition/debts')),
  create: (data) => callApi(http.post('/gateway/tuition', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/tuition/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/tuition/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/tuition/bulk-update', items)),
  markOverdue: (id) => callApi(http.put(`/gateway/tuition/${id}/mark-overdue`)),
  scanOverdue: () => callApi(http.put('/gateway/tuition/scan-overdue')),
  getLearningHolds: (params = {}) => callApi(http.get('/gateway/tuition/learning-holds', { params })),
  getLearningHoldsByStudent: (studentId) => callApi(http.get(`/gateway/tuition/learning-holds/by-student/${studentId}`)),
  bulkMarkOverdue: (ids) => callApi(http.put('/gateway/tuition/bulk-mark-overdue', { ids })),
  delete: (id) => callApi(http.delete(`/gateway/tuition/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/tuition/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/tuition/import', file),
  export: () => downloadFile('/gateway/tuition/export', 'tuition-invoices.csv'),
};
