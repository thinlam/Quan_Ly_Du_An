using Bogus;

namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Abstract base for Bogus fakers. Provides deterministic seeding.
/// </summary>
/// <typeparam name="TEntity">Entity type to generate</typeparam>
public abstract class EntityFakerBase<TEntity> : Faker<TEntity> where TEntity : class
{
    protected EntityFakerBase()
    {
        UseSeed(FakerSeedManager.CurrentSeed);
    }
}