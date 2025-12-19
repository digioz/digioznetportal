using System;
using System.Threading.Tasks;
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.PaymentProviders.Providers;
using Microsoft.AspNetCore.Http;

namespace digioz.Portal.Web.Services
{
    public interface IPayPalRedirectService
    {
        Task<(string orderId, string approveUrl)> CreateOrderAsync(PaymentRequest request, string returnBaseUrl);
        Task<PaymentResponse> CaptureOrderAsync(string paypalOrderId, PaymentRequest originalRequest);
    }

    public class PayPalRedirectService : IPayPalRedirectService
    {
        private readonly IPaymentProviderFactory _factory;

        public PayPalRedirectService(IPaymentProviderFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<(string orderId, string approveUrl)> CreateOrderAsync(PaymentRequest request, string returnBaseUrl)
        {
            var provider = ResolveProvider();
            return await provider.CreateOrderAndGetApprovalUrlAsync(request, returnBaseUrl).ConfigureAwait(false);
        }

        public async Task<PaymentResponse> CaptureOrderAsync(string paypalOrderId, PaymentRequest originalRequest)
        {
            var provider = ResolveProvider();
            return await provider.CaptureApprovedOrderAsync(paypalOrderId, originalRequest).ConfigureAwait(false);
        }

        private PayPalProvider ResolveProvider()
        {
            if (!_factory.IsProviderAvailable("PayPal"))
                throw new InvalidOperationException("PayPal provider is not available.");

            var provider = _factory.CreateProvider("PayPal") as PayPalProvider;
            if (provider == null)
                throw new InvalidOperationException("Failed to resolve PayPal provider instance.");

            return provider;
        }
    }
}
