import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const resultApi = {
  getAll: () => callApi(http.get('/gateway/results')),
  getById: (id) => callApi(http.get(`/gateway/results/${id}`)),
  getByStudent: (studentId) => callApi(http.get(`/gateway/results/by-student/${studentId}`)),
  getByClass: (classId) => callApi(http.get(`/gateway/results/by-class/${classId}`)),
  create: (data) => callApi(http.post('/gateway/results', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/results/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/results/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/results/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/results/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/results/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/results/import', file),
  export: () => downloadFile('/gateway/results/export', 'student-results.csv'),
};
