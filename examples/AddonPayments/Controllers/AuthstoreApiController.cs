using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class AuthstoreApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult AuthstoreApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Identificador del cliente
            var customerId = detalles.Cliente.payerref;

            // Identificador de la tarjeta
            var paymentId = detalles.Cliente.paymentmethod;

            // Asociamos la tarjeta al cliente
            var paymentMethod = new RecurringPaymentMethod(customerId, paymentId);


            try {
                // process an auto-capture authorization
                Transaction response = paymentMethod.Charge(detalles.Tarjeta.importe)
                  .WithCurrency("EUR")
                  .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED

                // get the response details to save to the DB for future requests
                var orderId = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsReference = response.TransactionId; // pasref 14622680939731425
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = orderId, authCode = authCode, paymentsReference = paymentsReference, schemeReferenceData = schemeReferenceData };

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}
