namespace BlazingPizza.Client.Configure
{
    public interface IUserAuthService
    {
       Task<bool> IsAdminAsync();
       bool IsAdmin();
    }
}
