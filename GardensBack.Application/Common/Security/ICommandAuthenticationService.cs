namespace GardensBack.Application.Common.Security
{
    public interface ICommandAuthenticationService<in TCommand>
    {
        string Sign(TCommand command, byte[] key);
    }
}
