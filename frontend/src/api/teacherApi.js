import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const teacherApi = {
  getAll: () => callApi(http.get('/gateway/teachers')),
  getById: (id) => callApi(http.get(`/gateway/teachers/${id}`)),
  create: (data) => callApi(http.post('/gateway/teachers', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/teachers/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/teachers/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/teachers/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/teachers/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/teachers/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/teachers/import', file),
  export: () => downloadFile('/gateway/teachers/export', 'teachers.csv'),
};
