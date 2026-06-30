import http, { callApi } from './httpClient';

export const teachingSubstitutionApi = {
  getAll: () => callApi(http.get('/gateway/teaching-substitutions/')),
  getPending: () => callApi(http.get('/gateway/teaching-substitutions/pending')),
  getByTeacher: (teacherId) => callApi(http.get(`/gateway/teaching-substitutions/by-teacher/${teacherId}`)),
  create: (data) => callApi(http.post('/gateway/teaching-substitutions/', data)),
  approve: (id, data = {}) => callApi(http.put(`/gateway/teaching-substitutions/${id}/approve`, data)),
  reject: (id, data = {}) => callApi(http.put(`/gateway/teaching-substitutions/${id}/reject`, data)),
  delete: (id) => callApi(http.delete(`/gateway/teaching-substitutions/${id}`)),
};
