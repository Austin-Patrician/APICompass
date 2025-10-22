import React from 'react';
import { SingleKeyForm } from '@/components/validation/SingleKeyForm';

export const SingleValidation: React.FC = () => {
  return (
    <div>
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Single Key Validator
        </h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Validate a single API key and get detailed information
        </p>
      </div>

      <SingleKeyForm />
    </div>
  );
};
