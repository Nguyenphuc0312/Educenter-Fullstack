import http, { callApi, callPagedApi, downloadFile, uploadJsonImport } from './httpClient';

export const studentApi = {
  getAll: () => callApi(http.get('/gateway/students')),
  getById: (id) => callApi(http.get(`/gateway/students/${id}`)),
  search: (params) => callPagedApi(http.get('/gateway/students/search', { params })),
  getLearningProfile: (studentId) => callApi(http.get(`/gateway/students/${studentId}/learning-profile`)),
  getMyCourses: (studentId) => callApi(http.get(`/gateway/students/${studentId}/my-courses`)),
  getMyAttendance: (studentId) => callApi(http.get(`/gateway/students/${studentId}/my-attendance`)),
  getMyResults: (studentId) => callApi(http.get(`/gateway/students/${studentId}/my-results`)),
  getAttendanceSummary: (studentId) => callApi(http.get(`/gateway/students/${studentId}/attendance-summary`)),
  create: (data) => callApi(http.post('/gateway/students', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/students/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/students/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/students/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/students/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/students/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/students/import', file),
  export: () => downloadFile('/gateway/students/export', 'students.csv'),
};
