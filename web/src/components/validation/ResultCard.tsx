import React, { useState } from 'react';
import { Card } from '@/components/common/Card';
import { Button } from '@/components/common/Button';
import { ProviderBadge } from '@/components/common/ProviderBadge';
import { maskKey, formatDuration } from '@/utils/providerDetector';
import {
  CheckCircle,
  XCircle,
  Clock,
  Zap,
  Copy,
  ChevronDown,
  ChevronUp,
  Eye,
  EyeOff,
} from 'lucide-react';
import toast from 'react-hot-toast';
import type { ValidationResult } from '@/types/api.types';

interface ResultCardProps {
  result: ValidationResult;
  originalKey: string;
}

export const ResultCard: React.FC<ResultCardProps> = ({ result, originalKey }) => {
  const [expanded, setExpanded] = useState(false);
  const [showKey, setShowKey] = useState(false);

  const displayKey = showKey ? originalKey : maskKey(originalKey);

  const copyToClipboard = (text: string, label: string) => {
    navigator.clipboard.writeText(text);
    toast.success(`${label} copied to clipboard!`);
  };

  const copyJSON = () => {
    const json = JSON.stringify(result, null, 2);
    navigator.clipboard.writeText(json);
    toast.success('Result copied as JSON!');
  };

  return (
    <Card className="animate-slide-in">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center space-x-3">
          {result.isValid ? (
            <div className="rounded-full bg-green-100 p-2 dark:bg-green-900/30">
              <CheckCircle className="h-6 w-6 text-green-600 dark:text-green-400" />
            </div>
          ) : (
            <div className="rounded-full bg-red-100 p-2 dark:bg-red-900/30">
              <XCircle className="h-6 w-6 text-red-600 dark:text-red-400" />
            </div>
          )}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
              {result.isValid ? 'Valid Key' : 'Invalid Key'}
            </h3>
            {result.provider && (
              <div className="mt-1">
                <ProviderBadge provider={result.provider} />
              </div>
            )}
          </div>
        </div>

        <button
          onClick={() => setExpanded(!expanded)}
          className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
        >
          {expanded ? (
            <ChevronUp className="h-5 w-5" />
          ) : (
            <ChevronDown className="h-5 w-5" />
          )}
        </button>
      </div>

      {/* Quick Info */}
      <div className="mt-4 grid grid-cols-2 gap-4 md:grid-cols-4">
        <div>
          <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
            <Clock className="h-4 w-4" />
            <span>Response Time</span>
          </div>
          <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
            {formatDuration(result.validationDurationMs)}
          </p>
        </div>

        {result.keyInfo?.tier && (
          <div>
            <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
              <Zap className="h-4 w-4" />
              <span>Tier</span>
            </div>
            <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
              {result.keyInfo.tier}
            </p>
          </div>
        )}

        {result.keyInfo?.rpm !== undefined && (
          <div>
            <div className="text-sm text-gray-600 dark:text-gray-400">
              Rate Limit
            </div>
            <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
              {result.keyInfo.rpm.toLocaleString()} RPM
            </p>
          </div>
        )}

        {result.keyInfo?.hasQuota !== undefined && (
          <div>
            <div className="text-sm text-gray-600 dark:text-gray-400">Quota</div>
            <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
              {result.keyInfo.hasQuota ? '✅ Available' : '❌ Limited'}
            </p>
          </div>
        )}
      </div>

      {/* Error Message */}
      {!result.isValid && result.errorMessage && (
        <div className="mt-4 rounded-lg bg-red-50 p-4 dark:bg-red-900/20">
          <p className="text-sm text-red-800 dark:text-red-200">
            {result.errorMessage}
          </p>
        </div>
      )}

      {/* Expanded Details */}
      {expanded && result.isValid && result.keyInfo && (
        <div className="mt-6 space-y-4 border-t border-gray-200 pt-4 dark:border-gray-700">
          {/* Key Display */}
          <div>
            <div className="mb-2 flex items-center justify-between">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                API Key
              </span>
              <div className="flex space-x-2">
                <button
                  onClick={() => setShowKey(!showKey)}
                  className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                >
                  {showKey ? (
                    <EyeOff className="h-4 w-4" />
                  ) : (
                    <Eye className="h-4 w-4" />
                  )}
                </button>
                <button
                  onClick={() => copyToClipboard(originalKey, 'Key')}
                  className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                >
                  <Copy className="h-4 w-4" />
                </button>
              </div>
            </div>
            <div className="rounded-lg bg-gray-100 p-3 font-mono text-sm dark:bg-gray-700">
              {displayKey}
            </div>
          </div>

          {/* Additional Info */}
          {result.keyInfo.model && (
            <div>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Model Access
              </span>
              <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                {result.keyInfo.model}
              </p>
            </div>
          )}

          {result.keyInfo.organizations && result.keyInfo.organizations.length > 0 && (
            <div>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Organizations
              </span>
              <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                {result.keyInfo.organizations.length} organization(s)
              </p>
            </div>
          )}

          {result.keyInfo.additionalInfo && (
            <div>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Additional Info
              </span>
              <div className="mt-2 space-y-1">
                {Object.entries(result.keyInfo.additionalInfo).map(([key, value]) => (
                  <div key={key} className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">
                      {key}:
                    </span>
                    <span className="font-medium text-gray-900 dark:text-white">
                      {typeof value === 'boolean' ? (value ? '✓' : '✗') : String(value)}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex space-x-3 border-t border-gray-200 pt-4 dark:border-gray-700">
            <Button variant="outline" size="sm" onClick={copyJSON}>
              <Copy className="mr-2 h-4 w-4" />
              Copy JSON
            </Button>
          </div>
        </div>
      )}
    </Card>
  );
};
