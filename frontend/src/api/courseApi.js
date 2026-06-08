import http, { callApi, callPagedApi, downloadFile, uploadJsonImport } from './httpClient';

export const courseApi = {
  getAll: () => callApi(http.get('/gateway/courses')),
  getById: (id) => callApi(http.get(`/gateway/courses/${id}`)),
  getBySlug: (slug) => callApi(http.get(`/gateway/courses/slug/${slug}`)),
  search: (params) => callPagedApi(http.get('/gateway/courses/search', { params })),
  getOpening: () => callApi(http.get('/gateway/courses/opening')),
  getBestSelling: () => callApi(http.get('/gateway/courses/best-selling')),
  getPopularThisWeek: () => callApi(http.get('/gateway/courses/popular-this-week')),
  create: (data) => callApi(http.post('/gateway/courses', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/courses/bulk-create', items)),
  update: (id, data) => callApi(http.put(`/gateway/courses/${id}`, data)),
  bulkUpdate: (items) => callApi(http.put('/gateway/courses/bulk-update', items)),
  delete: (id) => callApi(http.delete(`/gateway/courses/${id}`)),
  bulkDelete: (ids) => callApi(http.post('/gateway/courses/bulk-delete', { ids })),
  import: (file) => uploadJsonImport('/gateway/courses/import', file),
  export: () => downloadFile('/gateway/courses/export', 'courses.csv'),
};
