import React from 'react';
import { Provider } from '@/types/api.types';
import { getProviderColor } from '@/utils/providerDetector';

interface ProviderBadgeProps {
  provider: Provider | string;
  size?: 'sm' | 'md' | 'lg';
}

export const ProviderBadge: React.FC<ProviderBadgeProps> = ({
  provider,
  size = 'md',
}) => {
  const color = getProviderColor(provider);

  const sizes = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-3 py-1 text-sm',
    lg: 'px-4 py-1.5 text-base',
  };

  return (
    <span
      className={`inline-flex items-center rounded-full font-medium ${sizes[size]}`}
      style={{
        backgroundColor: `${color}15`,
        color: color,
        border: `1px solid ${color}30`,
      }}
    >
      {provider}
    </span>
  );
};
