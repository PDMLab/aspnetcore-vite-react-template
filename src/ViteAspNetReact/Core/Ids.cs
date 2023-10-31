using Marten.Schema.Identity;

namespace ViteAspNetReact.Core;

public interface IIdGenerator
{
  Guid New();
}

public class MartenIdGenerator : IIdGenerator
{
  public Guid New() => CombGuidIdGeneration.NewGuid();
}
