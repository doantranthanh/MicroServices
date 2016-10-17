namespace Daishi.Microservices.Interfaces
{
    internal interface IMicroservices
    {
        void Init();
        void OneMessagesReceived(object sender, MessageReceivedEventArgs e);
        void Shutdown();
    }
}
