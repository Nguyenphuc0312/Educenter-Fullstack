import http, { callApi, downloadFile, uploadJsonImport } from './httpClient';

export const paymentApi = {
  getAll: () => callApi(http.get('/gateway/payments')),
  getById: (id) => callApi(http.get(`/gateway/payments/${id}`)),
  getByStudent: (studentId) => callApi(http.get(`/gateway/payments/by-student/${studentId}`)),
  getByInvoice: (invoiceId) => callApi(http.get(`/gateway/payments/by-invoice/${invoiceId}`)),
  create: (data) => callApi(http.post('/gateway/payments', data)),
  bulkCreate: (items) => callApi(http.post('/gateway/payments/bulk-create', items)),
  cancel: (id) => callApi(http.put(`/gateway/payments/${id}/cancel`)),
  bulkCancel: (ids) => callApi(http.put('/gateway/payments/bulk-cancel', { ids })),
  import: (file) => uploadJsonImport('/gateway/payments/import', file),
  export: () => downloadFile('/gateway/payments/export', 'payments.csv'),
};
