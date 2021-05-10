using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class FraudHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult FraudHpp() {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2",
                    FraudFilterMode = FraudFilterMode.PASSIVE
                }
            });

            // Datos que deben transferirseal servidor de Addon Payments junto con las opciones de nivel de transacción
            var hostedPaymentData = new HostedPaymentData
            {
                CustomerNumber = "E8953893489",
                ProductId = "SID9838383"
            };

            var billingAddress = new Address
            {
                Country = "726",
                PostalCode = "50001|Flat 123"
            };

            var shippingAddress = new Address
            {
                Country = "726",
                PostalCode = "654|123"
            };

            var variableReference = "Car Part HV";

            try {
                // process an auto-capture authorization
                var hppJson = service.Charge(19.99m)
                      .WithCurrency("EUR")
                      .WithHostedPaymentData(hostedPaymentData)
                      .WithAddress(billingAddress, AddressType.Billing)
                      .WithAddress(shippingAddress, AddressType.Shipping)
                      .WithClientTransactionId(variableReference)
                      .Serialize();


                return Content(hppJson);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
