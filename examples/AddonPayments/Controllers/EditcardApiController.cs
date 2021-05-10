using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using System;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class EditcardApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult EditcardApi(Datos detalles) {
            
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


            // Creamos el objeto de la tarjeta
            var newCardDetails = new CreditCardData {
                Number = detalles.Tarjeta.cardNumber,
                ExpMonth = detalles.Tarjeta.month,
                ExpYear = detalles.Tarjeta.year,
                CardHolderName = detalles.Tarjeta.cardholderName
            };

            // Añadimos los nuevos datos de tarjeta al objeto paymentMethod
            paymentMethod.PaymentMethod = newCardDetails;

            try {
                // process an auto-capture authorization
                paymentMethod.SaveChanges();

                Respuesta respuesta = new Respuesta { result = "Tarjeta modificada correctamente."};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
