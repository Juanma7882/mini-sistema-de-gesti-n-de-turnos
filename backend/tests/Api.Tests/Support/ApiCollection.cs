namespace Turnos.Api.Tests.Support;

/// <summary>Agrupa TODAS las clases de test de integración en una sola
/// colección: comparten un único <see cref="IntegrationTestFactory"/> (una sola
/// base sembrada) y xUnit las corre en serie entre sí — evita que dos factories
/// se pisen las variables de entorno de proceso si corrieran en paralelo (ver
/// <see cref="IntegrationTestFactory"/>).</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<IntegrationTestFactory>
{
    public const string Name = "Api";
}
