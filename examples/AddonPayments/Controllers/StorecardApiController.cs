using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using System;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class StorecardApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult StorecardApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // ID del cliente
            var customer = new Customer
            {
                Key = detalles.Cliente.payerref
            };

            // Creamos un nuevo ID de tarjeta
            var paymentMethodRef = Guid.NewGuid().ToString();

            // Creamos el objeto de la tarjeta
            var card = new CreditCardData {
                Number = detalles.Tarjeta.cardNumber,
                ExpMonth = detalles.Tarjeta.month,
                ExpYear = detalles.Tarjeta.year,
                CardHolderName = detalles.Tarjeta.cardholderName
            };

            // Asociamos el nuevo ID de tarjeta al cliente
            var paymentMethod = customer.AddPaymentMethod(paymentMethodRef, card);

            try {
                // process an auto-capture authorization
                var response = paymentMethod.Create();

                Respuesta respuesta = new Respuesta { result = paymentMethodRef};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
