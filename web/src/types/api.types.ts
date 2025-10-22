export enum Provider {
  OpenAI = 'OpenAI',
  Anthropic = 'Anthropic',
  AI21 = 'AI21',
  MakerSuite = 'MakerSuite',
  AWS = 'AWS',
  Azure = 'Azure',
  VertexAI = 'VertexAI',
  Mistral = 'Mistral',
  OpenRouter = 'OpenRouter',
  ElevenLabs = 'ElevenLabs',
  DeepSeek = 'DeepSeek',
  XAI = 'XAI',
}

export interface ValidationOptions {
  verifyOrg?: boolean;
  checkModels?: boolean;
  useCache?: boolean;
  timeoutSeconds?: number;
}

export interface KeyInfo {
  model?: string;
  hasQuota?: boolean;
  tier?: string;
  rpm?: number;
  organizations?: string[];
  hasSpecialModels?: boolean;
  additionalInfo?: Record<string, any>;
}

export interface ValidationResult {
  isValid: boolean;
  provider?: string;
  keyInfo?: KeyInfo;
  errorMessage?: string;
  validatedAt: string;
  validationDurationMs: number;
}

export interface ValidateKeyRequest {
  key: string;
  provider?: string;
  options?: ValidationOptions;
}

export interface BatchValidationResponse {
  totalKeys: number;
  validKeys: number;
  invalidKeys: number;
  results: ValidationResult[];
  totalDurationMs: number;
}

export interface ProviderInfo {
  name: Provider;
  color: string;
  icon: string;
  keyFormat: string;
}
