namespace Ocelot.Provider.Consul
{
    using Errors;

    public class UnableToSetConfigInConsulError : Error
    {
        public UnableToSetConfigInConsulError(string s, int statusCode) 
            : base(s, OcelotErrorCode.UnknownError, statusCode)
        {
        }
    }
}
