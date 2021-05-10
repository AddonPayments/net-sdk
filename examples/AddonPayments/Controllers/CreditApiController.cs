using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CreditApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreditApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                RefundPassword = "refund",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // create the card object
            var card = new CreditCardData {
                Number = detalles.Tarjeta.cardNumber,
                ExpMonth = detalles.Tarjeta.month,
                ExpYear = detalles.Tarjeta.year,
                CardHolderName = detalles.Tarjeta.cardholderName
            };

            try {
                // process an auto-capture authorization
                Transaction response = card.Refund(detalles.Tarjeta.importe)
                    .WithCurrency("EUR")
                   .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED

                // get the response details to save to the DB for future requests
                var orderId = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsReference = response.TransactionId; // pasref 14622680939731425

                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = orderId, authCode = authCode, paymentsReference = paymentsReference};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
