import { Provider } from '@/types/api.types';

export const providerColors: Record<Provider, string> = {
  [Provider.OpenAI]: '#10A37F',
  [Provider.Anthropic]: '#D97706',
  [Provider.AWS]: '#FF9900',
  [Provider.Azure]: '#0078D4',
  [Provider.MakerSuite]: '#4285F4',
  [Provider.DeepSeek]: '#6366F1',
  [Provider.XAI]: '#000000',
  [Provider.Mistral]: '#F97316',
  [Provider.AI21]: '#6B7280',
  [Provider.OpenRouter]: '#8B5CF6',
  [Provider.ElevenLabs]: '#EC4899',
  [Provider.VertexAI]: '#34A853',
};

export const providerPatterns: Record<Provider, RegExp> = {
  [Provider.OpenAI]: /sk-[a-zA-Z0-9_-]+T3BlbkFJ[a-zA-Z0-9_-]+/,
  [Provider.Anthropic]: /sk-ant-/,
  [Provider.MakerSuite]: /AIzaSy[A-Za-z0-9\-_]{33}/,
  [Provider.XAI]: /xai-[A-Za-z0-9]{80}/,
  [Provider.OpenRouter]: /sk-or-v1-[a-z0-9]{64}/,
  [Provider.DeepSeek]: /sk-[a-f0-9]{32}/,
  [Provider.AI21]: /^[A-Za-z0-9]{32}$/,
  [Provider.Mistral]: /^[A-Za-z0-9]{32}$/,
  [Provider.ElevenLabs]: /(sk_[a-z0-9]{48}|[a-z0-9]{32})/,
  [Provider.AWS]: /^AKIA[0-9A-Z]{16}:/,
  [Provider.Azure]: /^.+:[a-z0-9]{32}$/,
  [Provider.VertexAI]: /\.json$/i,
};

export function detectProvider(key: string): Provider | null {
  if (!key) return null;

  const trimmedKey = key.trim();

  // Check each provider pattern
  if (providerPatterns[Provider.OpenAI].test(trimmedKey)) {
    return Provider.OpenAI;
  }
  if (trimmedKey.startsWith('sk-ant-')) {
    return Provider.Anthropic;
  }
  if (providerPatterns[Provider.MakerSuite].test(trimmedKey)) {
    return Provider.MakerSuite;
  }
  if (providerPatterns[Provider.XAI].test(trimmedKey)) {
    return Provider.XAI;
  }
  if (providerPatterns[Provider.OpenRouter].test(trimmedKey)) {
    return Provider.OpenRouter;
  }
  if (trimmedKey.startsWith('sk-') && trimmedKey.length < 36) {
    return Provider.DeepSeek;
  }
  if (trimmedKey.startsWith('AKIA') && trimmedKey.includes(':')) {
    return Provider.AWS;
  }
  if (trimmedKey.includes(':') && !trimmedKey.startsWith('AKIA')) {
    return Provider.Azure;
  }
  if (trimmedKey.endsWith('.json')) {
    return Provider.VertexAI;
  }
  if (trimmedKey.startsWith('sk_')) {
    return Provider.ElevenLabs;
  }
  if (providerPatterns[Provider.AI21].test(trimmedKey) && trimmedKey.length === 32) {
    return Provider.AI21;
  }

  return null;
}

export function maskKey(key: string): string {
  if (!key || key.length < 12) return key;
  
  const start = key.slice(0, 8);
  const end = key.slice(-4);
  const masked = '•'.repeat(Math.min(key.length - 12, 20));
  
  return `${start}${masked}${end}`;
}

export function formatDuration(ms: number): string {
  if (ms < 1000) return `${ms.toFixed(0)}ms`;
  return `${(ms / 1000).toFixed(2)}s`;
}

export function getProviderColor(provider: Provider | string): string {
  return providerColors[provider as Provider] || '#6B7280';
}
