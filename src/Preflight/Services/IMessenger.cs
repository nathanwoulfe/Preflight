using Preflight.Models;

namespace Preflight.Services
{
    public interface IMessenger
    {
        void SendTestResult(PreflightPropertyResponseModel model);
        void PreflightComplete();
    }
}
