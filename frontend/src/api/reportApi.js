import http, { callApi, downloadFile } from './httpClient';

export const reportApi = {
  getRevenueOverview: () => callApi(http.get('/gateway/reports/revenue/overview')),
  getRevenueByCourse: () => callApi(http.get('/gateway/reports/revenue/by-course')),
  getRevenueByClass: () => callApi(http.get('/gateway/reports/revenue/by-class')),
  getDebtByStudent: () => callApi(http.get('/gateway/reports/debt/by-student')),
  getDebtByClass: () => callApi(http.get('/gateway/reports/debt/by-class')),
  getDashboard: () => callApi(http.get('/gateway/reports/dashboard')),
  exportRevenueByCourse: () => downloadFile('/gateway/reports/revenue/by-course/export', 'revenue-by-course.csv'),
  exportRevenueByClass: () => downloadFile('/gateway/reports/revenue/by-class/export', 'revenue-by-class.csv'),
  exportDebtByStudent: () => downloadFile('/gateway/reports/debt/by-student/export', 'debt-by-student.csv'),
  exportDebtByClass: () => downloadFile('/gateway/reports/debt/by-class/export', 'debt-by-class.csv'),
};
