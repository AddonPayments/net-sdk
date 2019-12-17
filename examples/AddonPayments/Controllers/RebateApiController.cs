using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class RebateApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult RebateApi(Datos detalles) {

            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                RebatePassword = "rebate",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Identificador de la transacción que queremos capturar
            var orderId = detalles.Op.OrderID;

            // Referencia de la transacción original
            var paymentsReference = detalles.Op.pasref;

            // Código de autorización
            var authCode = detalles.Op.authCode;

            // create the refund transaction object
            var transaction = Transaction.FromId(paymentsReference, orderId);
            transaction.AuthorizationCode = authCode;

            try {
                // process an auto-capture authorization
                Transaction response = transaction.Refund(detalles.Tarjeta.importe)
                        .WithCurrency("EUR")
                        .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED
                var order = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var auth = response.AuthorizationCode; // 12345
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                // get the response details to save to the DB for future requests


                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = orderId, authCode = auth};

                return Ok(respuesta);
            }

            catch (ApiException exce)
            {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}
