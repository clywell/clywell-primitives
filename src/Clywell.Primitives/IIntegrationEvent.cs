namespace Clywell.Primitives;

/// <summary>
/// Marker interface for cross-service integration events published via the outbox pattern.
/// Integration events must be serialisable to JSON.
/// </summary>
public interface IIntegrationEvent;