public interface IServiceProviders
{
    ITheGraph theGraph { get; }
    ICatalyst catalyst { get; }
}

public class ServiceProviders : IServiceProviders
{
    public ITheGraph theGraph { get ; } = new TheGraph();
    public ICatalyst catalyst { get ; } = new Catalyst();
}