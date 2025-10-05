using Xunit;

// Aktiver xUnit.DependencyInjection
[assembly: TestFramework("Xunit.DependencyInjection.TestFramework", "Xunit.DependencyInjection")]

// Valgfrit: slå parallel kørsel fra (nemmere med én Postgres-container)
[assembly: CollectionBehavior(DisableTestParallelization = true)]