using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Reputaly.API.Infrastructure.Services
{
    public interface IOAuthStateService
    {
        string GenerateState(Guid tenantId, Guid locationId);
        (Guid tenantId, Guid locationId)? ValidateAndConsume(string state);
    }
    public class OAuthStateService: IOAuthStateService
    {
        // ConcurrentDictionary es thread-safe: varios rquest pueden leerlo/escribirlo
        private readonly ConcurrentDictionary<string, (Guid tenantId, Guid locationId, DateTime expiresAt)> _states = new();

        public string GenerateState(Guid tenantId,Guid locationId)
        {
            var state = Guid.NewGuid().ToString("N"); // token aleatorio incluido
            _states[state] = (tenantId,locationId,DateTime.UtcNow.AddMinutes(10));
            return state;
        }

        public (Guid tenantId, Guid locationId)? ValidateAndConsume(string state)
        {
            // TryRemove: elminia y devuelve el valor en una operacion atomica
            if (!_states.TryRemove(state, out var entry))
                return null;

            if (DateTime.UtcNow > entry.expiresAt)
                return null; // Expirado

            return(entry.tenantId, entry.locationId);
        }
    }
}
