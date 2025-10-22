import React from 'react';
import { BatchKeyForm } from '@/components/validation/BatchKeyForm';

export const BatchValidation: React.FC = () => {
  return (
    <div>
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Batch Validator
        </h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Validate multiple API keys at once
        </p>
      </div>

      <BatchKeyForm />
    </div>
  );
};
