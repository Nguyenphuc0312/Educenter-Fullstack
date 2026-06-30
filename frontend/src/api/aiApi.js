import http, { callApi } from './httpClient';

export const aiApi = {
  complete: (data) => callApi(http.post('/gateway/ai/knowledge/complete', data)),
};

