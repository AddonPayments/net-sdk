using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class FraudApiController : ControllerBase
    {
        [HttpPost]
        public IActionResult FraudApi(Datos detalles) {
            
            // configure client & request settings
            ServicesContainer.ConfigureService(new GatewayConfig {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://remote.sandbox.addonpayments.com/remote"
            });

            // create the card object
            var card = new CreditCardData {
                Number = detalles.Tarjeta.cardNumber,
                ExpMonth = detalles.Tarjeta.month,
                ExpYear = detalles.Tarjeta.year,
                Cvn = detalles.Tarjeta.cvn,
                CardHolderName = detalles.Tarjeta.cardholderName
            };

            // Facturación
            var billingAddress = new Address
            {
                Country = detalles.Fraud.billCo,
                PostalCode = detalles.Fraud.billCp
            };

            // supply the customer's shipping country and post code
            var shippingAddress = new Address
            {
                Country = detalles.Fraud.shipCo,
                PostalCode = detalles.Fraud.shipCp
            };

            try {
                // Procesamos una autorización
                Transaction response = card.Charge(detalles.Tarjeta.importe)
                       .WithCurrency("EUR")
                       .WithAddress(billingAddress, AddressType.Billing)
                       .WithAddress(shippingAddress, AddressType.Shipping)
                       .WithProductId(detalles.Fraud.prodid) // prodid
                       .WithClientTransactionId(detalles.Fraud.varref) // varref
                       .WithCustomerId(detalles.Fraud.custnum) // custnum
                       .WithCustomerIpAddress("123.123.123.123")
                       .Execute();

                var result = response.ResponseCode; // 00 == Success
                var message = response.ResponseMessage; // [ test system ] AUTHORISED

                // get the response details to save to the DB for future requests
                var orderId = response.OrderId; // ezJDQjhENTZBLTdCNzNDQw
                var authCode = response.AuthorizationCode; // 12345
                var paymentsReference = response.TransactionId; // pasref 14622680939731425
                var schemeReferenceData = response.SchemeId; // MMC0F00YE4000000715

                Respuesta respuesta = new Respuesta { result = result, message = message, orderId = orderId, authCode = authCode, paymentsReference = paymentsReference, schemeReferenceData = schemeReferenceData};

                return Ok(respuesta);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
    
}
