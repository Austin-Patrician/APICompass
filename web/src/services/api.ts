import axios from 'axios';
import type {
  ValidateKeyRequest,
  ValidationResult,
  BatchValidationResponse,
} from '@/types/api.types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:50000';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Server responded with error
      console.error('API Error:', error.response.data);
    } else if (error.request) {
      // Request made but no response
      console.error('Network Error:', error.message);
    }
    return Promise.reject(error);
  }
);

export const validateKey = async (
  request: ValidateKeyRequest
): Promise<ValidationResult> => {
  const response = await api.post<ValidationResult>(
    '/api/v1/keys/validate',
    request
  );
  return response.data;
};

export const batchValidate = async (
  keys: string[]
): Promise<BatchValidationResponse> => {
  const response = await api.post<BatchValidationResponse>(
    '/api/v1/keys/validate/batch',
    { keys }
  );
  return response.data;
};

export const checkHealth = async (): Promise<{ status: string }> => {
  const response = await api.get('/health');
  return response.data;
};

export default api;
