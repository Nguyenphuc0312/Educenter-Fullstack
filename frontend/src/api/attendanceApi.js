import http, { callApi } from './httpClient';

export const attendanceApi = {
  getSessionsByClass: (classId) => callApi(http.get(`/gateway/attendance/attendance-sessions/by-class/${classId}`)),
  getSession: (id) => callApi(http.get(`/gateway/attendance/attendance-sessions/${id}`)),
  createSession: (data) => callApi(http.post('/gateway/attendance/attendance-sessions', data)),
  lockSession: (id) => callApi(http.put(`/gateway/attendance/attendance-sessions/${id}/lock`)),
  getRecordsBySession: (sessionId) => callApi(http.get(`/gateway/attendance/attendance-records/by-session/${sessionId}`)),
  getRecordsByStudent: (studentId) => callApi(http.get(`/gateway/attendance/attendance-records/by-student/${studentId}`)),
  bulkUpdateRecords: (data) => callApi(http.post('/gateway/attendance/attendance-records/bulk', data)),
  updateRecord: (id, data) => callApi(http.put(`/gateway/attendance/attendance-records/${id}`, data)),
  deleteRecord: (id) => callApi(http.delete(`/gateway/attendance/attendance-records/${id}`)),
};
