using APICompass.KeyChecker.Core.Models;

namespace APICompass.KeyChecker.Core.Interfaces;

public interface IKeyIdentifierService
{
    (Provider? provider, bool isValid) IdentifyProvider(string key);
}
