using System;
using System.Threading.Tasks;
using digioz.Portal.PaymentProviders.Abstractions;
using digioz.Portal.PaymentProviders.Models;
using digioz.Portal.PaymentProviders.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<PayPalRedirectService> _logger;

        public PayPalRedirectService(IPaymentProviderFactory factory, ILogger<PayPalRedirectService> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(string orderId, string approveUrl)> CreateOrderAsync(PaymentRequest request, string returnBaseUrl)
        {
            _logger.LogInformation("PayPalRedirectService.CreateOrderAsync: Starting. TransactionId={TransactionId}, Amount={Amount}, ReturnBaseUrl={ReturnBaseUrl}",
                request.TransactionId, request.Amount, returnBaseUrl);
            
            try
            {
                var provider = ResolveProvider();
                _logger.LogInformation("PayPalRedirectService.CreateOrderAsync: Provider resolved successfully");
                
                var result = await provider.CreateOrderAndGetApprovalUrlAsync(request, returnBaseUrl).ConfigureAwait(false);
                
                _logger.LogInformation("PayPalRedirectService.CreateOrderAsync: Order created. OrderId={OrderId}, ApproveUrl={ApproveUrl}",
                    result.orderId, result.approveUrl);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPalRedirectService.CreateOrderAsync: Failed to create PayPal order. Message={Message}",
                    ex.Message);
                throw;
            }
        }

        public async Task<PaymentResponse> CaptureOrderAsync(string paypalOrderId, PaymentRequest originalRequest)
        {
            _logger.LogInformation("PayPalRedirectService.CaptureOrderAsync: Starting. PayPalOrderId={PayPalOrderId}",
                paypalOrderId);
            
            try
            {
                var provider = ResolveProvider();
                _logger.LogInformation("PayPalRedirectService.CaptureOrderAsync: Provider resolved successfully");
                
                var result = await provider.CaptureApprovedOrderAsync(paypalOrderId, originalRequest).ConfigureAwait(false);
                
                _logger.LogInformation("PayPalRedirectService.CaptureOrderAsync: Capture completed. IsApproved={IsApproved}, TransactionId={TransactionId}",
                    result.IsApproved, result.TransactionId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPalRedirectService.CaptureOrderAsync: Failed to capture PayPal order. Message={Message}",
                    ex.Message);
                throw;
            }
        }

        private PayPalProvider ResolveProvider()
        {
            _logger.LogInformation("PayPalRedirectService.ResolveProvider: Checking if PayPal provider is available");
            
            if (!_factory.IsProviderAvailable("PayPal"))
            {
                _logger.LogError("PayPalRedirectService.ResolveProvider: PayPal provider is not available");
                throw new InvalidOperationException("PayPal provider is not available.");
            }

            var provider = _factory.CreateProvider("PayPal") as PayPalProvider;
            if (provider == null)
            {
                _logger.LogError("PayPalRedirectService.ResolveProvider: Failed to cast provider to PayPalProvider");
                throw new InvalidOperationException("Failed to resolve PayPal provider instance.");
            }

            _logger.LogInformation("PayPalRedirectService.ResolveProvider: PayPal provider resolved successfully");
            return provider;
        }
    }
}
