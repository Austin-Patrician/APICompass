import React from 'react';
import { Card } from '@/components/common/Card';
import { Button } from '@/components/common/Button';
import { ProviderBadge } from '@/components/common/ProviderBadge';
import { getHistory, clearHistory, deleteHistoryItem } from '@/services/storage';
import { Trash2, Download, Clock } from 'lucide-react';
import toast from 'react-hot-toast';

export const History: React.FC = () => {
  const [history, setHistory] = React.useState(getHistory());

  const handleClearAll = () => {
    if (confirm('Are you sure you want to clear all history?')) {
      clearHistory();
      setHistory([]);
      toast.success('History cleared');
    }
  };

  const handleDelete = (id: string) => {
    deleteHistoryItem(id);
    setHistory(getHistory());
    toast.success('Item deleted');
  };

  const handleExport = () => {
    const json = JSON.stringify(history, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `keychecker-history-${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);
    toast.success('History exported');
  };

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Validation History
          </h1>
          <p className="mt-2 text-gray-600 dark:text-gray-400">
            View all your past key validations
          </p>
        </div>
        <div className="flex space-x-3">
          <Button variant="outline" size="sm" onClick={handleExport}>
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Button variant="danger" size="sm" onClick={handleClearAll}>
            <Trash2 className="mr-2 h-4 w-4" />
            Clear All
          </Button>
        </div>
      </div>

      {history.length === 0 ? (
        <Card>
          <div className="text-center py-12">
            <Clock className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-4 text-lg font-semibold text-gray-900 dark:text-white">
              No history yet
            </h3>
            <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Your validation history will appear here
            </p>
          </div>
        </Card>
      ) : (
        <div className="space-y-3">
          {history.map((item) => (
            <Card key={item.id} className="hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-3">
                    {item.isValid ? (
                      <div className="h-3 w-3 rounded-full bg-green-500" />
                    ) : (
                      <div className="h-3 w-3 rounded-full bg-red-500" />
                    )}
                    <span className="font-mono text-sm text-gray-900 dark:text-white">
                      {item.key}
                    </span>
                    {item.provider && (
                      <ProviderBadge provider={item.provider} size="sm" />
                    )}
                  </div>
                  <div className="mt-2 flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
                    <span>
                      {new Date(item.validatedAt).toLocaleString()}
                    </span>
                    <span>{item.validationDurationMs}ms</span>
                    {item.keyInfo?.tier && <span>{item.keyInfo.tier}</span>}
                  </div>
                </div>
                <button
                  onClick={() => handleDelete(item.id)}
                  className="ml-4 rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-red-600 dark:hover:bg-gray-700"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
};
