using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class FraudholdApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult FraudholdApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // a hold request requires the original order id
            var orderId = detalles.Fraud.orderID;

            // and the payments reference (pasref) from the authorization response
            var paymentsReference = detalles.Fraud.Pasref;

            // create the hold transaction object
            var transaction = Transaction.FromId(paymentsReference, orderId);

            try {
                // Procesamos una autorización
                Transaction response = transaction.Hold()
                       .WithReasonCode(ReasonCode.FRAUD)
                       .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED

                // get the response details to save to the DB for future requests
                var order = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsRef = response.TransactionId; // pasref 14622680939731425
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = order, authCode = authCode, schemeReferenceData = schemeReferenceData};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
    
}
