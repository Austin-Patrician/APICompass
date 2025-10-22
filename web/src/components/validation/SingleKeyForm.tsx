import React, { useState, useEffect } from 'react';
import { Button } from '@/components/common/Button';
import { Card } from '@/components/common/Card';
import { TextArea } from '@/components/common/Input';
import { ProviderBadge } from '@/components/common/ProviderBadge';
import { useValidation } from '@/hooks/useValidation';
import { detectProvider, maskKey } from '@/utils/providerDetector';
import { saveToHistory } from '@/services/storage';
import { Check, Key, Settings2, Sparkles } from 'lucide-react';
import toast from 'react-hot-toast';
import { ResultCard } from './ResultCard';
import type { ValidationResult } from '@/types/api.types';

export const SingleKeyForm: React.FC = () => {
  const [key, setKey] = useState('');
  const [detectedProvider, setDetectedProvider] = useState<string | null>(null);
  const [checkModels, setCheckModels] = useState(true);
  const [useCache, setUseCache] = useState(true);
  const [verifyOrg, setVerifyOrg] = useState(false);
  const [result, setResult] = useState<ValidationResult | null>(null);

  const { mutate: validate, isPending } = useValidation();

  useEffect(() => {
    if (key) {
      const provider = detectProvider(key);
      setDetectedProvider(provider);
    } else {
      setDetectedProvider(null);
    }
  }, [key]);

  const handleValidate = () => {
    if (!key.trim()) {
      toast.error('Please enter an API key');
      return;
    }

    validate(
      {
        key: key.trim(),
        options: {
          checkModels,
          useCache,
          verifyOrg,
        },
      },
      {
        onSuccess: (data) => {
          setResult(data);
          if (data.isValid) {
            toast.success('Key validated successfully!');
            saveToHistory(data, maskKey(key.trim()));
          } else {
            toast.error(data.errorMessage || 'Invalid key');
          }
        },
        onError: (error: any) => {
          toast.error(error.response?.data?.message || 'Validation failed');
        },
      }
    );
  };

  const handleClear = () => {
    setKey('');
    setResult(null);
    setDetectedProvider(null);
  };

  return (
    <div className="space-y-6">
      <Card title="Validate API Key" subtitle="Enter your API key to check its validity and get detailed information">
        <div className="space-y-4">
          {/* Key Input */}
          <div>
            <TextArea
              label="API Key"
              value={key}
              onChange={(e) => setKey(e.target.value)}
              placeholder="Paste your API key here... (e.g., sk-proj-...)"
              rows={4}
              className="font-mono text-sm"
            />
          </div>

          {/* Provider Detection */}
          {detectedProvider && (
            <div className="flex items-center space-x-2 animate-slide-in">
              <Sparkles className="h-5 w-5 text-primary-600" />
              <span className="text-sm text-gray-600 dark:text-gray-400">
                Auto-detected:
              </span>
              <ProviderBadge provider={detectedProvider} />
            </div>
          )}

          {/* Options */}
          <div className="rounded-lg border border-gray-200 p-4 dark:border-gray-700">
            <div className="mb-3 flex items-center space-x-2">
              <Settings2 className="h-4 w-4 text-gray-600 dark:text-gray-400" />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Validation Options
              </span>
            </div>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={checkModels}
                  onChange={(e) => setCheckModels(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700 dark:text-gray-300">
                  Check Models
                </span>
              </label>
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={useCache}
                  onChange={(e) => setUseCache(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700 dark:text-gray-300">
                  Use Cache
                </span>
              </label>
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={verifyOrg}
                  onChange={(e) => setVerifyOrg(e.target.checked)}
                  className="h-4 w-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700 dark:text-gray-300">
                  Verify Org ID
                </span>
              </label>
            </div>
          </div>

          {/* Actions */}
          <div className="flex space-x-3">
            <Button
              variant="primary"
              onClick={handleValidate}
              isLoading={isPending}
              className="flex-1"
            >
              <Key className="mr-2 h-4 w-4" />
              Validate Key
            </Button>
            <Button variant="secondary" onClick={handleClear}>
              Clear
            </Button>
          </div>
        </div>
      </Card>

      {/* Result */}
      {result && <ResultCard result={result} originalKey={key} />}
    </div>
  );
};
