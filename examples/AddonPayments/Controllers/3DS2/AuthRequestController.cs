using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using Microsoft.AspNetCore.Mvc;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthRequestController : ControllerBase
    {
        [HttpPost]
        public IActionResult AuthFinal([FromQuery] Tarjeta detalles, [FromQuery] RespuestaVersion3DS2 responseACSVersion)
        {
            // Configuramos los datos de nuestro comercio. Los datos son proporcionados por el equipo de Addon Payments, 
            // en caso de duda debe llamar al 914 353 028 e indicar su número de comercio o Merchant ID
            ServicesContainer.ConfigureService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "3ds2",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // Creamos el objeto de la tarjeta
            var card = new CreditCardData
            {
                Number = detalles.cardNumber,
                ExpMonth = detalles.month,
                ExpYear = detalles.year,
                Cvn = detalles.cvn,
                CardHolderName = detalles.cardholderName
            };

            // Añadimos la información obtenida de autenticación 3DS2
            var threeDSecureData = new ThreeDSecure()
            {
                AuthenticationValue = responseACSVersion.authenticationValue,
                DirectoryServerTransactionId = responseACSVersion.directoryServerTransactionId,
                Eci = responseACSVersion.eci,
                MessageVersion = responseACSVersion.messageVersion
            };

            // add the 3D Secure 2 data to the card object
            card.ThreeDSecure = threeDSecureData;

            Transaction response = null;

            try
            {
                response = card.Charge(10.01m)
                   .WithCurrency("EUR")
                   .Execute();


                // Obtenemos la respuesta y la mostramos
                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED
                var orderId = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsReference = response.TransactionId; // pasref 14622680939731425
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                // Devolvemos el resultado de la operación al cliente
                Respuesta respuesta = new Respuesta
                {
                    result = result,
                    message = message,
                    orderId = orderId,
                    authCode = authCode,
                    paymentsReference = paymentsReference,
                    schemeReferenceData = schemeReferenceData
                };

                return Ok(respuesta);
            }

            catch (ApiException exce)
            {
                return BadRequest(exce);
            }
        }
    }
}