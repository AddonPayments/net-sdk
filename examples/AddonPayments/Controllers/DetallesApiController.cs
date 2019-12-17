using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;

namespace AddonPayments.Controllers {
    [ApiController]
    [Route("[Controller]")]
    public class DetallesApiController: ControllerBase
    {
        [HttpPost]
        public IActionResult DetallesAuth(Datos detalles) {
            
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

            // Cliente
            var customer = new Customer
            {
                FirstName = detalles.Cliente.nombre,
                LastName = detalles.Cliente.apellidos,
                DateOfBirth = detalles.Cliente.cumple,
                CustomerPassword = detalles.Cliente.pass,
                Email = detalles.Cliente.email,
                HomePhone = detalles.Cliente.homephone,
                DeviceFingerPrint = detalles.Cliente.device
            };

            // Dirección de facturación
            var facturacion = new Address
            {
                StreetAddress1 = detalles.Billing.billing1,
                StreetAddress2 = detalles.Billing.billing2,
                StreetAddress3 = detalles.Billing.billing3,
                City = detalles.Billing.billingcity,
                Province = detalles.Billing.billingprovince,
                State = detalles.Billing.billingstate,
                PostalCode = detalles.Billing.billingcode,
                Country = detalles.Billing.billingcountry
            };

            // Dirección de envío
            var envio = new Address
            {
                StreetAddress1 = detalles.Shipping.street1,
                StreetAddress2 = detalles.Shipping.street2,
                StreetAddress3 = detalles.Shipping.street3,
                City = detalles.Shipping.enviocity,
                Province = detalles.Shipping.envioprovince,
                State = detalles.Shipping.enviostate,
                PostalCode = detalles.Shipping.enviocode,
                Country = detalles.Shipping.enviocountry
            };

            try {
                // process an auto-capture authorization
                Transaction response = card.Charge(detalles.Tarjeta.importe)
                   .WithCurrency("EUR")
                   .WithAddress(facturacion, AddressType.Billing)
                   .WithAddress(envio, AddressType.Shipping)
                   .WithCustomerData(customer)
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
                return Ok(respuesta);
            }
        }
    }
    
}
