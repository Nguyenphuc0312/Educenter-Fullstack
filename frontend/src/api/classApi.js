import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const classApi = {
  getAll: () => callApi(http.get('/gateway/classes')),
  getById: (id) => callApi(http.get(`/gateway/classes/${id}`)),
  getByCourse: (courseId) => callApi(http.get(`/gateway/classes/by-course/${courseId}`)),
  getByTeacher: (teacherId) => callApi(http.get(`/gateway/classes/by-teacher/${teacherId}`)),
  getOpening: () => callApi(http.get('/gateway/classes/opening')),
  getStudents: (classId) => callApi(http.get(`/gateway/attendance/classes/${classId}/students`)),
  getAttendanceSummary: (classId) => callApi(http.get(`/gateway/attendance/classes/${classId}/attendance-summary`)),
  create: (data) => callApi(http.post('/gateway/classes', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/classes/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/classes/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/classes/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/classes/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/classes/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/classes/import', file),
  export: () => downloadFile('/gateway/classes/export', 'classes.csv'),
};
