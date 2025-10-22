import React, { useState, useMemo } from 'react';
import { Button } from '@/components/common/Button';
import { Card } from '@/components/common/Card';
import { TextArea } from '@/components/common/Input';
import { useBatchValidation } from '@/hooks/useBatchValidation';
import { detectProvider, maskKey } from '@/utils/providerDetector';
import { saveToHistory } from '@/services/storage';
import { PackageCheck, Upload, Trash2, FileText } from 'lucide-react';
import toast from 'react-hot-toast';
import { BatchResultCard } from './BatchResultCard';
import type { BatchValidationResponse } from '@/types/api.types';

export const BatchKeyForm: React.FC = () => {
  const [keysText, setKeysText] = useState('');
  const [result, setResult] = useState<BatchValidationResponse | null>(null);

  const { mutate: batchValidate, isPending } = useBatchValidation();

  // Parse and dedupe keys
  const parsedKeys = useMemo(() => {
    const lines = keysText
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0);
    
    // Remove duplicates
    const uniqueKeys = Array.from(new Set(lines));
    return uniqueKeys;
  }, [keysText]);

  const keyCount = parsedKeys.length;

  // Detect providers for preview
  const detectedProviders = useMemo(() => {
    const providers = new Map<string, number>();
    parsedKeys.forEach(key => {
      const provider = detectProvider(key) || 'Unknown';
      providers.set(provider, (providers.get(provider) || 0) + 1);
    });
    return Array.from(providers.entries()).sort((a, b) => b[1] - a[1]);
  }, [parsedKeys]);

  const handleValidate = () => {
    if (parsedKeys.length === 0) {
      toast.error('Please enter at least one API key');
      return;
    }

    if (parsedKeys.length > 100) {
      toast.error('Maximum 100 keys allowed per batch');
      return;
    }

    batchValidate(parsedKeys, {
      onSuccess: (data) => {
        setResult(data);
        
        // Save valid keys to history
        data.results.forEach((result, index) => {
          if (result.isValid) {
            saveToHistory(result, maskKey(parsedKeys[index]));
          }
        });

        toast.success(
          `Validation complete! ${data.validKeys} valid, ${data.invalidKeys} invalid`
        );
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.message || 'Batch validation failed');
      },
    });
  };

  const handleClear = () => {
    setKeysText('');
    setResult(null);
  };

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Check file size (max 1MB)
    if (file.size > 1024 * 1024) {
      toast.error('File too large. Maximum size is 1MB');
      return;
    }

    const reader = new FileReader();
    reader.onload = (event) => {
      const text = event.target?.result as string;
      setKeysText(text);
      toast.success('File loaded successfully');
    };
    reader.onerror = () => {
      toast.error('Failed to read file');
    };
    reader.readAsText(file);
  };

  return (
    <div className="space-y-6">
      <Card
        title="Batch Validate API Keys"
        subtitle="Enter multiple API keys (one per line) to validate them all at once"
      >
        <div className="space-y-4">
          {/* Keys Input */}
          <div>
            <div className="mb-2 flex items-center justify-between">
              <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                API Keys
              </label>
              <div className="flex items-center space-x-2">
                <label className="cursor-pointer">
                  <input
                    type="file"
                    accept=".txt,.csv"
                    onChange={handleFileUpload}
                    className="hidden"
                  />
                  <span className="inline-flex items-center rounded-lg px-3 py-1 text-sm font-medium text-primary-600 hover:bg-primary-50 dark:text-primary-400 dark:hover:bg-primary-900/20">
                    <Upload className="mr-1.5 h-4 w-4" />
                    Upload File
                  </span>
                </label>
              </div>
            </div>
            <TextArea
              value={keysText}
              onChange={(e) => setKeysText(e.target.value)}
              placeholder="Paste your API keys here, one per line...&#10;sk-proj-...&#10;sk-ant-...&#10;..."
              rows={12}
              className="font-mono text-sm"
            />
            <div className="mt-2 flex items-center justify-between text-sm text-gray-600 dark:text-gray-400">
              <span>
                {keyCount} {keyCount === 1 ? 'key' : 'keys'} entered
                {keyCount > 100 && (
                  <span className="ml-2 text-red-600 dark:text-red-400">
                    (Maximum 100 allowed)
                  </span>
                )}
              </span>
            </div>
          </div>

          {/* Provider Preview */}
          {detectedProviders.length > 0 && (
            <div className="rounded-lg border border-gray-200 p-4 dark:border-gray-700">
              <div className="mb-2 flex items-center space-x-2">
                <FileText className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  Detected Providers
                </span>
              </div>
              <div className="flex flex-wrap gap-2">
                {detectedProviders.map(([provider, count]) => (
                  <div
                    key={provider}
                    className="inline-flex items-center rounded-full bg-gray-100 px-3 py-1 text-sm dark:bg-gray-700"
                  >
                    <span className="font-medium text-gray-900 dark:text-white">
                      {provider}
                    </span>
                    <span className="ml-1.5 text-gray-600 dark:text-gray-400">
                      ({count})
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex space-x-3">
            <Button
              variant="primary"
              onClick={handleValidate}
              isLoading={isPending}
              disabled={keyCount === 0 || keyCount > 100}
              className="flex-1"
            >
              <PackageCheck className="mr-2 h-4 w-4" />
              Validate {keyCount > 0 && `${keyCount} Keys`}
            </Button>
            <Button variant="secondary" onClick={handleClear}>
              <Trash2 className="mr-2 h-4 w-4" />
              Clear
            </Button>
          </div>
        </div>
      </Card>

      {/* Results */}
      {result && <BatchResultCard result={result} originalKeys={parsedKeys} />}
    </div>
  );
};
