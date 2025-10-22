namespace APICompass.KeyChecker.Core.Models;

public class APIKey
{
    public Provider Provider { get; set; }
    public string Key { get; set; } = string.Empty;
    
    // OpenAI specific properties
    public string? Model { get; set; }
    public bool HasQuota { get; set; }
    public string? DefaultOrg { get; set; }
    public List<string> Organizations { get; set; } = new();
    public int Rpm { get; set; }
    public string? Tier { get; set; }
    public bool HasSpecialModels { get; set; }
    public bool Real32K { get; set; }
    public bool TheOne { get; set; }
    public bool ExtraModels { get; set; }
    public HashSet<string> ExtraModelList { get; set; } = new();
    public bool IdVerified { get; set; }
    public bool Slop { get; set; } = true;
    public HashSet<string> MissingModels { get; set; } = new();
    public bool AccessToModelListing { get; set; } = true;
    
    // Anthropic specific properties
    public bool Pozzed { get; set; }
    public bool RateLimited { get; set; }
    public long RemainingTokens { get; set; }
    
    // AI21 specific properties
    public bool TrialElapsed { get; set; }
    
    // AWS specific properties
    public string? Username { get; set; }
    public bool Useless { get; set; } = true;
    public bool AdminPriv { get; set; }
    public bool BedrockEnabled { get; set; }
    public string? Region { get; set; }
    public List<string> AltRegions { get; set; } = new();
    public List<string> UselessReasons { get; set; } = new();
    public bool Logged { get; set; }
    public Dictionary<string, object> Models { get; set; } = new();
    
    // Azure specific properties
    public string? Endpoint { get; set; }
    public string? BestDeployment { get; set; }
    public List<string> Deployments { get; set; } = new();
    public bool Unfiltered { get; set; }
    public string? DalleDeployments { get; set; }
    public List<string> HasGpt4Turbo { get; set; } = new();
    
    // VertexAI specific properties
    public string? ProjectId { get; set; }
    public bool HasOpus { get; set; }
    
    // Mistral specific properties
    public bool Subbed { get; set; }
    
    // MakerSuite specific properties
    public List<string> MakerSuiteModels { get; set; } = new();
    public bool EnabledBilling { get; set; }
    
    // OpenRouter specific properties
    public decimal Usage { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public bool LimitReached { get; set; }
    public bool BoughtCredits { get; set; }
    
    // ElevenLabs specific properties
    public long CharactersLeft { get; set; }
    public string? ElevenLabsUsage { get; set; }
    public bool Unlimited { get; set; }
    public int ProVoiceLimit { get; set; }
    
    // DeepSeek specific properties
    public string? DeepSeekBalance { get; set; }
    public bool Available { get; set; }
    
    // XAI specific properties
    public bool Blocked { get; set; } = true;

    public APIKey Clone()
    {
        return new APIKey
        {
            Provider = this.Provider,
            Key = this.Key,
            Model = this.Model,
            HasQuota = this.HasQuota,
            DefaultOrg = this.DefaultOrg,
            Organizations = new List<string>(this.Organizations),
            Rpm = this.Rpm,
            Tier = this.Tier,
            HasSpecialModels = this.HasSpecialModels,
            Real32K = this.Real32K,
            TheOne = this.TheOne,
            ExtraModels = this.ExtraModels,
            ExtraModelList = new HashSet<string>(this.ExtraModelList),
            IdVerified = this.IdVerified,
            Slop = this.Slop,
            MissingModels = new HashSet<string>(this.MissingModels),
            AccessToModelListing = this.AccessToModelListing,
            Pozzed = this.Pozzed,
            RateLimited = this.RateLimited,
            RemainingTokens = this.RemainingTokens,
            TrialElapsed = this.TrialElapsed,
            Username = this.Username,
            Useless = this.Useless,
            AdminPriv = this.AdminPriv,
            BedrockEnabled = this.BedrockEnabled,
            Region = this.Region,
            AltRegions = new List<string>(this.AltRegions),
            UselessReasons = new List<string>(this.UselessReasons),
            Logged = this.Logged,
            Models = new Dictionary<string, object>(this.Models),
            Endpoint = this.Endpoint,
            BestDeployment = this.BestDeployment,
            Deployments = new List<string>(this.Deployments),
            Unfiltered = this.Unfiltered,
            DalleDeployments = this.DalleDeployments,
            HasGpt4Turbo = new List<string>(this.HasGpt4Turbo),
            ProjectId = this.ProjectId,
            HasOpus = this.HasOpus,
            Subbed = this.Subbed,
            MakerSuiteModels = new List<string>(this.MakerSuiteModels),
            EnabledBilling = this.EnabledBilling,
            Usage = this.Usage,
            CreditLimit = this.CreditLimit,
            Balance = this.Balance,
            LimitReached = this.LimitReached,
            BoughtCredits = this.BoughtCredits,
            CharactersLeft = this.CharactersLeft,
            ElevenLabsUsage = this.ElevenLabsUsage,
            Unlimited = this.Unlimited,
            ProVoiceLimit = this.ProVoiceLimit,
            DeepSeekBalance = this.DeepSeekBalance,
            Available = this.Available,
            Blocked = this.Blocked
        };
    }
}
