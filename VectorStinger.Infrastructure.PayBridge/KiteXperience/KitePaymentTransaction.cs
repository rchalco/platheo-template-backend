using VectorStinger.Core.Configurations;
using VectorStinger.Core.Interfaces.Infrastructure.PayBridge;
using VectorStinger.Core.Interfaces.Infrastructure.PayBridge.PayTransaction;
using FluentResults;
using VectorStinger.Infrastructure.PayBridge.KiteXperience.Payment;
using Microsoft.Extensions.Options;
using VectorStinger.Foundation.Abstractions.Infrastructure;

namespace VectorStinger.Infrastructure.PayBridge.KiteXperience
{
    public class KitePaymentTransaction : BaseAPIExternalService, IPaymentTransaction
    {
        private readonly PaymentBridgeSettings _paymentBridgeSettings;

        public KitePaymentTransaction(HttpClient httpClient, IOptions<PaymentBridgeSettings> paymentBridgeSettings) : base(httpClient, paymentBridgeSettings.Value.Url)
        {
            _paymentBridgeSettings = paymentBridgeSettings.Value;
        }

        public Task<Result<PaymentTransactionResponse>> PayTransaction(PaymentTransactionRequest paymentTransactionRequest)
        {
            KitePayTransactionRequest paymentTransactionRequestInner = new KitePayTransactionRequest();
            var resulAPI = PostAsync<KitePayTransactionRequest, KitePayTransactionResponse>(paymentTransactionRequestInner);
            return Task.FromResult(Result.Ok(new PaymentTransactionResponse()));
        }
    }
}
