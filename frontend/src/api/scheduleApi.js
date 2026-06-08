import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const scheduleApi = {
  getAll: () => callApi(http.get('/gateway/schedules')),
  getById: (id) => callApi(http.get(`/gateway/schedules/${id}`)),
  getByClass: (classId) => callApi(http.get(`/gateway/schedules/by-class/${classId}`)),
  getByTeacher: (teacherId) => callApi(http.get(`/gateway/schedules/by-teacher/${teacherId}`)),
  create: (data) => callApi(http.post('/gateway/schedules', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/schedules/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/schedules/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/schedules/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/schedules/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/schedules/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/schedules/import', file),
  export: () => downloadFile('/gateway/schedules/export', 'schedules.csv'),
};
