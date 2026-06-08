import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const enrollmentApi = {
  getAll: () => callApi(http.get('/gateway/enrollments')),
  getById: (id) => callApi(http.get(`/gateway/enrollments/${id}`)),
  getByStudent: (studentId) => callApi(http.get(`/gateway/enrollments/by-student/${studentId}`)),
  getByClass: (classId) => callApi(http.get(`/gateway/enrollments/by-class/${classId}`)),
  create: (data) => callApi(http.post('/gateway/enrollments', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/enrollments/bulk-create', items)),
  confirm: (id) => callApi(http.put(`/gateway/enrollments/${id}/confirm`)),
  bulkConfirm: (ids) => callApi(http.put('/gateway/enrollments/bulk-confirm', { ids })),
  cancel: (id) => callApi(http.put(`/gateway/enrollments/${id}/cancel`)),
  bulkCancel: (ids) => callApi(http.put('/gateway/enrollments/bulk-cancel', { ids })),
  complete: (id) => callApi(http.put(`/gateway/enrollments/${id}/complete`)),
  delete: (id) => callApi(http.delete(`/gateway/enrollments/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/enrollments/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/enrollments/import', file),
  export: () => downloadFile('/gateway/enrollments/export', 'enrollments.csv'),
};
