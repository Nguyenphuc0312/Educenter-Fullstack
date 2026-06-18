import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const classroomApi = {
  getAll: () => callApi(http.get('/gateway/classrooms')),
  getById: (id) => callApi(http.get(`/gateway/classrooms/${id}`)),
  create: (data) => callApi(http.post('/gateway/classrooms', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/classrooms/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/classrooms/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/classrooms/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/classrooms/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/classrooms/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/classrooms/import', file),
  export: () => downloadFile('/gateway/classrooms/export', 'classrooms.csv'),
};
