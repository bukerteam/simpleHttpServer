namespace SimpleServer
{
    public interface IHttpServer
    {
        void Start(HttpServerConfiguration Configuration);
        void Stop();
    }
}