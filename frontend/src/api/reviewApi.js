import http, { callApi } from './httpClient';

export const reviewApi = {
  getAll: () => callApi(http.get('/gateway/reviews')),
  getByStudent: (studentId) => callApi(http.get(`/gateway/reviews/by-student/${studentId}`)),
  getByEnrollment: (enrollmentId) => callApi(http.get(`/gateway/reviews/by-enrollment/${enrollmentId}`)),
  create: (data) => callApi(http.post('/gateway/reviews', data)),
  update: (id, data) => callApi(http.put(`/gateway/reviews/${id}`, data)),
  delete: (id) => callApi(http.delete(`/gateway/reviews/${id}`)),
};
