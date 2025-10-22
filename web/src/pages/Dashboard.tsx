import React from 'react';
import { Link } from 'react-router-dom';
import { Card } from '@/components/common/Card';
import { Button } from '@/components/common/Button';
import { Key, PackageCheck, Clock, TrendingUp } from 'lucide-react';
import { getHistory } from '@/services/storage';

export const Dashboard: React.FC = () => {
  const history = getHistory();
  const validCount = history.filter((item) => item.isValid).length;
  const totalCount = history.length;
  const successRate = totalCount > 0 ? ((validCount / totalCount) * 100).toFixed(1) : 0;
  
  const avgResponseTime =
    history.length > 0
      ? (history.reduce((sum, item) => sum + item.validationDurationMs, 0) / history.length).toFixed(0)
      : 0;

  const recentValidations = history.slice(0, 5);

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Dashboard
        </h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Welcome to KeyChecker - Validate your AI API keys instantly
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Total Validations
              </p>
              <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">
                {totalCount}
              </p>
            </div>
            <div className="rounded-full bg-primary-100 p-3 dark:bg-primary-900/30">
              <PackageCheck className="h-6 w-6 text-primary-600 dark:text-primary-400" />
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Valid Keys
              </p>
              <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">
                {validCount}
              </p>
              <p className="mt-1 text-sm text-green-600 dark:text-green-400">
                {successRate}% success rate
              </p>
            </div>
            <div className="rounded-full bg-green-100 p-3 dark:bg-green-900/30">
              <TrendingUp className="h-6 w-6 text-green-600 dark:text-green-400" />
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Avg Response
              </p>
              <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">
                {avgResponseTime}ms
              </p>
            </div>
            <div className="rounded-full bg-blue-100 p-3 dark:bg-blue-900/30">
              <Clock className="h-6 w-6 text-blue-600 dark:text-blue-400" />
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Quick Action
              </p>
              <Link to="/validate">
                <Button variant="primary" className="mt-2" size="sm">
                  <Key className="mr-2 h-4 w-4" />
                  Validate Key
                </Button>
              </Link>
            </div>
          </div>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <Link to="/validate">
          <Card className="cursor-pointer transition-shadow hover:shadow-lg">
            <div className="flex items-center space-x-4">
              <div className="rounded-full bg-primary-100 p-4 dark:bg-primary-900/30">
                <Key className="h-8 w-8 text-primary-600 dark:text-primary-400" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  Single Key Validator
                </h3>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                  Validate and get detailed information about a single API key
                </p>
              </div>
            </div>
          </Card>
        </Link>

        <Link to="/batch">
          <Card className="cursor-pointer transition-shadow hover:shadow-lg">
            <div className="flex items-center space-x-4">
              <div className="rounded-full bg-green-100 p-4 dark:bg-green-900/30">
                <PackageCheck className="h-8 w-8 text-green-600 dark:text-green-400" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  Batch Validator
                </h3>
                <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">
                  Validate multiple API keys at once with progress tracking
                </p>
              </div>
            </div>
          </Card>
        </Link>
      </div>

      {/* Recent Validations */}
      {recentValidations.length > 0 && (
        <Card title="Recent Validations" subtitle="Your latest key validation results">
          <div className="space-y-3">
            {recentValidations.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between rounded-lg border border-gray-200 p-3 dark:border-gray-700"
              >
                <div className="flex items-center space-x-3">
                  {item.isValid ? (
                    <div className="h-2 w-2 rounded-full bg-green-500" />
                  ) : (
                    <div className="h-2 w-2 rounded-full bg-red-500" />
                  )}
                  <span className="font-mono text-sm text-gray-600 dark:text-gray-400">
                    {item.key}
                  </span>
                  {item.provider && (
                    <span className="rounded-full bg-primary-100 px-2 py-0.5 text-xs text-primary-600 dark:bg-primary-900/30 dark:text-primary-400">
                      {item.provider}
                    </span>
                  )}
                </div>
                <span className="text-sm text-gray-500 dark:text-gray-400">
                  {new Date(item.validatedAt).toLocaleDateString()}
                </span>
              </div>
            ))}
          </div>
          <Link to="/history">
            <Button variant="outline" className="mt-4 w-full" size="sm">
              View All History
            </Button>
          </Link>
        </Card>
      )}

      {/* Empty State */}
      {totalCount === 0 && (
        <Card>
          <div className="text-center py-12">
            <Key className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-4 text-lg font-semibold text-gray-900 dark:text-white">
              No validations yet
            </h3>
            <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Get started by validating your first API key
            </p>
            <Link to="/validate">
              <Button variant="primary" className="mt-4">
                <Key className="mr-2 h-4 w-4" />
                Validate Your First Key
              </Button>
            </Link>
          </div>
        </Card>
      )}
    </div>
  );
};
