import http, { callApi } from './httpClient'

export const settingsApi = {
  getNotificationEmail: () =>
    callApi(http.get('/gateway/settings/notification-email')),
  updateNotificationEmail: data =>
    callApi(http.put('/gateway/settings/notification-email', data)),
  sendTestEmail: data =>
    callApi(http.post('/gateway/settings/notification-email/test', data)),
}
