import { useMutation } from '@tanstack/react-query';
import { batchValidate } from '@/services/api';

export const useBatchValidation = () => {
  return useMutation({
    mutationFn: (keys: string[]) => batchValidate(keys),
    onError: (error: any) => {
      console.error('Batch validation error:', error);
    },
  });
};
