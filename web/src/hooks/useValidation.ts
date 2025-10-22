import { useMutation } from '@tanstack/react-query';
import { validateKey } from '@/services/api';
import type { ValidateKeyRequest, ValidationResult } from '@/types/api.types';

export const useValidation = () => {
  return useMutation({
    mutationFn: (request: ValidateKeyRequest) => validateKey(request),
    onError: (error: any) => {
      console.error('Validation error:', error);
    },
  });
};
