namespace MessageTools.Interceptor;

public interface IInterceptorSettings
{
    string HostName { get; set; }
    string User { get; set; }
    string Password { get; set; }
    string DbConnectionString { get; set; }
    bool NoConsoleLogging { get; set; }
    
    string[] IgnoredMessageTypes { get; set; }
}