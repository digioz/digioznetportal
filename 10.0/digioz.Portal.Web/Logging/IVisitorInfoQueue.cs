using digioz.Portal.Bo;

namespace digioz.Portal.Web.Logging
{
    public interface IVisitorInfoQueue
    {
        bool TryEnqueue(VisitorInfo item);
    }
}
