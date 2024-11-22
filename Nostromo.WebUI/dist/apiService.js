import axios from 'axios';


const apiClient = axios.create({
  baseURL: 'https://localhost:5001', 
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getData(endpoint) {
    return apiClient.get(endpoint);
  },
  postData(endpoint, data) {
    return apiClient.post(endpoint, data);
  },
  putData(endpoint, data) {
    return apiClient.put(endpoint, data);
  },
  deleteData(endpoint) {
    return apiClient.delete(endpoint);
  },
};
