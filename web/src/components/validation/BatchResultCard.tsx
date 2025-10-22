import React, { useState, useMemo } from 'react';
import { Card } from '@/components/common/Card';
import { Button } from '@/components/common/Button';
import { ProviderBadge } from '@/components/common/ProviderBadge';
import { maskKey, formatDuration } from '@/utils/providerDetector';
import {
  CheckCircle,
  XCircle,
  Clock,
  Download,
  Filter,
  ChevronDown,
  ChevronUp,
  Copy,
  Eye,
  EyeOff,
} from 'lucide-react';
import toast from 'react-hot-toast';
import type { BatchValidationResponse } from '@/types/api.types';

interface BatchResultCardProps {
  result: BatchValidationResponse;
  originalKeys: string[];
}

type FilterType = 'all' | 'valid' | 'invalid';

export const BatchResultCard: React.FC<BatchResultCardProps> = ({
  result,
  originalKeys,
}) => {
  const [filter, setFilter] = useState<FilterType>('all');
  const [expandedIndices, setExpandedIndices] = useState<Set<number>>(new Set());
  const [showKeys, setShowKeys] = useState<Set<number>>(new Set());

  // Filter results
  const filteredResults = useMemo(() => {
    return result.results
      .map((r, i) => ({ result: r, index: i, key: originalKeys[i] }))
      .filter(({ result }) => {
        if (filter === 'valid') return result.isValid;
        if (filter === 'invalid') return !result.isValid;
        return true;
      });
  }, [result.results, originalKeys, filter]);

  // Calculate statistics
  const stats = useMemo(() => {
    const byProvider = new Map<string, { valid: number; invalid: number }>();
    
    result.results.forEach((r) => {
      const provider = r.provider || 'Unknown';
      const current = byProvider.get(provider) || { valid: 0, invalid: 0 };
      if (r.isValid) {
        current.valid++;
      } else {
        current.invalid++;
      }
      byProvider.set(provider, current);
    });

    return { byProvider };
  }, [result.results]);

  const toggleExpanded = (index: number) => {
    const newExpanded = new Set(expandedIndices);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedIndices(newExpanded);
  };

  const toggleShowKey = (index: number) => {
    const newShowKeys = new Set(showKeys);
    if (newShowKeys.has(index)) {
      newShowKeys.delete(index);
    } else {
      newShowKeys.add(index);
    }
    setShowKeys(newShowKeys);
  };

  const copyToClipboard = (text: string, label: string) => {
    navigator.clipboard.writeText(text);
    toast.success(`${label} copied to clipboard!`);
  };

  const downloadResults = (format: 'json' | 'csv') => {
    let content: string;
    let filename: string;
    let mimeType: string;

    if (format === 'json') {
      content = JSON.stringify(result, null, 2);
      filename = `batch-validation-${Date.now()}.json`;
      mimeType = 'application/json';
    } else {
      // CSV format
      const headers = ['Key', 'Valid', 'Provider', 'Error', 'Duration (ms)'];
      const rows = result.results.map((r, i) => [
        maskKey(originalKeys[i]),
        r.isValid ? 'Yes' : 'No',
        r.provider || 'Unknown',
        r.errorMessage || '',
        r.validationDurationMs.toString(),
      ]);

      content = [
        headers.join(','),
        ...rows.map(row => row.map(cell => `"${cell}"`).join(',')),
      ].join('\n');
      
      filename = `batch-validation-${Date.now()}.csv`;
      mimeType = 'text/csv';
    }

    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
    toast.success(`Results exported as ${format.toUpperCase()}`);
  };

  const validityRate = ((result.validKeys / result.totalKeys) * 100).toFixed(1);

  return (
    <div className="space-y-4 animate-slide-in">
      {/* Summary Card */}
      <Card>
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
            Batch Validation Results
          </h3>
          <div className="flex space-x-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => downloadResults('json')}
            >
              <Download className="mr-2 h-4 w-4" />
              JSON
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => downloadResults('csv')}
            >
              <Download className="mr-2 h-4 w-4" />
              CSV
            </Button>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
          <div>
            <div className="text-sm text-gray-600 dark:text-gray-400">
              Total Keys
            </div>
            <p className="mt-1 text-2xl font-bold text-gray-900 dark:text-white">
              {result.totalKeys}
            </p>
          </div>

          <div>
            <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
              <CheckCircle className="h-4 w-4 text-green-600" />
              <span>Valid</span>
            </div>
            <p className="mt-1 text-2xl font-bold text-green-600 dark:text-green-400">
              {result.validKeys}
            </p>
          </div>

          <div>
            <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
              <XCircle className="h-4 w-4 text-red-600" />
              <span>Invalid</span>
            </div>
            <p className="mt-1 text-2xl font-bold text-red-600 dark:text-red-400">
              {result.invalidKeys}
            </p>
          </div>

          <div>
            <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
              <Clock className="h-4 w-4" />
              <span>Total Time</span>
            </div>
            <p className="mt-1 text-2xl font-bold text-gray-900 dark:text-white">
              {formatDuration(result.totalDurationMs)}
            </p>
          </div>
        </div>

        {/* Validity Rate */}
        <div className="mt-4">
          <div className="flex items-center justify-between text-sm mb-2">
            <span className="text-gray-600 dark:text-gray-400">
              Validity Rate
            </span>
            <span className="font-semibold text-gray-900 dark:text-white">
              {validityRate}%
            </span>
          </div>
          <div className="h-2 bg-gray-200 rounded-full overflow-hidden dark:bg-gray-700">
            <div
              className="h-full bg-green-600 dark:bg-green-500"
              style={{ width: `${validityRate}%` }}
            />
          </div>
        </div>

        {/* Provider Breakdown */}
        {stats.byProvider.size > 0 && (
          <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <h4 className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
              By Provider
            </h4>
            <div className="grid grid-cols-1 gap-2 md:grid-cols-2 lg:grid-cols-3">
              {Array.from(stats.byProvider.entries()).map(([provider, counts]) => (
                <div
                  key={provider}
                  className="flex items-center justify-between rounded-lg bg-gray-50 p-3 dark:bg-gray-800"
                >
                  <ProviderBadge provider={provider} />
                  <div className="flex items-center space-x-3 text-sm">
                    <span className="text-green-600 dark:text-green-400">
                      ✓ {counts.valid}
                    </span>
                    <span className="text-red-600 dark:text-red-400">
                      ✗ {counts.invalid}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </Card>

      {/* Filter Tabs */}
      <div className="flex items-center space-x-2">
        <Filter className="h-4 w-4 text-gray-600 dark:text-gray-400" />
        <div className="flex rounded-lg border border-gray-200 dark:border-gray-700">
          {(['all', 'valid', 'invalid'] as FilterType[]).map((f) => (
            <button
              key={f}
              onClick={() => setFilter(f)}
              className={`px-4 py-2 text-sm font-medium capitalize first:rounded-l-lg last:rounded-r-lg ${
                filter === f
                  ? 'bg-primary-600 text-white'
                  : 'bg-white text-gray-700 hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700'
              }`}
            >
              {f}
              {f === 'all' && ` (${result.totalKeys})`}
              {f === 'valid' && ` (${result.validKeys})`}
              {f === 'invalid' && ` (${result.invalidKeys})`}
            </button>
          ))}
        </div>
      </div>

      {/* Individual Results */}
      <div className="space-y-3">
        {filteredResults.map(({ result: r, index, key }) => {
          const isExpanded = expandedIndices.has(index);
          const isKeyVisible = showKeys.has(index);
          const displayKey = isKeyVisible ? key : maskKey(key);

          return (
            <Card key={index} className="p-0">
              {/* Header */}
              <div className="p-4 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-start justify-between">
                  <div className="flex items-center space-x-3">
                    {r.isValid ? (
                      <div className="rounded-full bg-green-100 p-2 dark:bg-green-900/30">
                        <CheckCircle className="h-6 w-6 text-green-600 dark:text-green-400" />
                      </div>
                    ) : (
                      <div className="rounded-full bg-red-100 p-2 dark:bg-red-900/30">
                        <XCircle className="h-6 w-6 text-red-600 dark:text-red-400" />
                      </div>
                    )}
                    <div>
                      <h3 className="text-base font-semibold text-gray-900 dark:text-white">
                        {r.isValid ? 'Valid Key' : 'Invalid Key'} #{index + 1}
                      </h3>
                      {r.provider && (
                        <div className="mt-1">
                          <ProviderBadge provider={r.provider} />
                        </div>
                      )}
                    </div>
                  </div>

                  <button
                    onClick={() => toggleExpanded(index)}
                    className="rounded-lg p-2 hover:bg-gray-100 dark:hover:bg-gray-700"
                  >
                    {isExpanded ? (
                      <ChevronUp className="h-5 w-5" />
                    ) : (
                      <ChevronDown className="h-5 w-5" />
                    )}
                  </button>
                </div>
              </div>

              {/* Quick Info */}
              <div className="p-4">
                <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
                  <div>
                    <div className="flex items-center space-x-1 text-sm text-gray-600 dark:text-gray-400">
                      <Clock className="h-4 w-4" />
                      <span>Response Time</span>
                    </div>
                    <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
                      {formatDuration(r.validationDurationMs)}
                    </p>
                  </div>

                  {r.keyInfo?.tier && (
                    <div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">
                        Tier
                      </div>
                      <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
                        {r.keyInfo.tier}
                      </p>
                    </div>
                  )}

                  {r.keyInfo?.rpm !== undefined && (
                    <div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">
                        Rate Limit
                      </div>
                      <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
                        {r.keyInfo.rpm.toLocaleString()} RPM
                      </p>
                    </div>
                  )}

                  {r.keyInfo?.hasQuota !== undefined && (
                    <div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">Quota</div>
                      <p className="mt-1 text-lg font-semibold text-gray-900 dark:text-white">
                        {r.keyInfo.hasQuota ? '✅ Available' : '❌ Limited'}
                      </p>
                    </div>
                  )}
                </div>

                {/* Error Message */}
                {!r.isValid && r.errorMessage && (
                  <div className="mt-4 rounded-lg bg-red-50 p-4 dark:bg-red-900/20">
                    <p className="text-sm text-red-800 dark:text-red-200">
                      {r.errorMessage}
                    </p>
                  </div>
                )}

                {/* Expanded Details */}
                {isExpanded && (
                  <div className="mt-4 space-y-4 border-t border-gray-200 pt-4 dark:border-gray-700">
                    {/* Key Display */}
                    <div>
                      <div className="mb-2 flex items-center justify-between">
                        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                          API Key
                        </span>
                        <div className="flex space-x-2">
                          <button
                            onClick={() => toggleShowKey(index)}
                            className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                          >
                            {isKeyVisible ? (
                              <EyeOff className="h-4 w-4" />
                            ) : (
                              <Eye className="h-4 w-4" />
                            )}
                          </button>
                          <button
                            onClick={() => copyToClipboard(key, 'Key')}
                            className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                          >
                            <Copy className="h-4 w-4" />
                          </button>
                        </div>
                      </div>
                      <div className="rounded-lg bg-gray-100 p-3 font-mono text-sm dark:bg-gray-700 break-all">
                        {displayKey}
                      </div>
                    </div>

                    {/* Additional Info - Only for Valid Keys */}
                    {r.isValid && r.keyInfo && (
                      <>
                        {r.keyInfo.model && (
                          <div>
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Model Access
                            </span>
                            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                              {r.keyInfo.model}
                            </p>
                          </div>
                        )}

                        {r.keyInfo.hasSpecialModels !== undefined && (
                          <div>
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Special Models
                            </span>
                            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                              {r.keyInfo.hasSpecialModels ? '✅ Available' : '❌ Not Available'}
                            </p>
                          </div>
                        )}

                        {r.keyInfo.organizations && r.keyInfo.organizations.length > 0 && (
                          <div>
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Organizations
                            </span>
                            <div className="mt-2 space-y-1">
                              {r.keyInfo.organizations.map((org, orgIndex) => (
                                <div
                                  key={orgIndex}
                                  className="rounded-lg bg-gray-50 px-3 py-2 text-sm font-mono dark:bg-gray-800"
                                >
                                  {org}
                                </div>
                              ))}
                            </div>
                          </div>
                        )}

                        {r.keyInfo.additionalInfo && Object.keys(r.keyInfo.additionalInfo).length > 0 && (
                          <div>
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Additional Information
                            </span>
                            <div className="mt-2 space-y-1">
                              {Object.entries(r.keyInfo.additionalInfo).map(([key, value]) => (
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

                        {/* Validation Timestamp */}
                        <div>
                          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                            Validated At
                          </span>
                          <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                            {new Date(r.validatedAt).toLocaleString()}
                          </p>
                        </div>

                        {/* Actions */}
                        <div className="flex space-x-3 border-t border-gray-200 pt-4 dark:border-gray-700">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              const json = JSON.stringify(r, null, 2);
                              copyToClipboard(json, 'Result');
                            }}
                          >
                            <Copy className="mr-2 h-4 w-4" />
                            Copy JSON
                          </Button>
                        </div>
                      </>
                    )}
                  </div>
                )}
              </div>
            </Card>
          );
        })}

        {filteredResults.length === 0 && (
          <Card>
            <div className="text-center py-8">
              <p className="text-gray-600 dark:text-gray-400">
                No {filter !== 'all' && filter} keys to display
              </p>
            </div>
          </Card>
        )}
      </div>
    </div>
  );
};
