using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class VoidApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult VoidApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Identificador de la transacción que queremos capturar
            var orderId = detalles.Op.OrderID;

            // Referencia de la transacción original
            var paymentsReference = detalles.Op.pasref;

            // create the capture transaction object
            var transaction = Transaction.FromId(paymentsReference, orderId);

            try {
                // process an auto-capture authorization
                Transaction response = transaction.Void()
                        .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED
                var order = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw

                // get the response details to save to the DB for future requests
                var authCode = response.AuthorizationCode; // 12345
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = order, authCode = authCode, paymentsReference = paymentsReference, schemeReferenceData = schemeReferenceData };

                return Ok(respuesta);
            }

            catch (ApiException exce)
            {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
