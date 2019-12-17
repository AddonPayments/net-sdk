using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class DeletecardApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult DeletecardApi(Datos detalles) {
            
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
                paymentMethod.Delete();

                Respuesta respuesta = new Respuesta { result = "Tarjeta eliminada correctamente."};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}
