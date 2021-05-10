using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers._3DS2
{
    [ApiController]
    [Route("api/[Controller]")]
    public class OtbHpp3DS2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult OtbHpp3DS2()
        {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "3ds2",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2"
                }
            });

            // Add 3D Secure 2 Mandatory and Recommended Fields
            var hostedPaymentData = new HostedPaymentData
            {
                CustomerEmail = "soporte@addonpayments.com",
                CustomerPhoneMobile = "34|914353028",
                AddressesMatch = false
            };

            var billingAddress = new Address
            {
                StreetAddress1 = "Dirección de facturación 1",
                StreetAddress2 = "Dirección de facturación 2",
                StreetAddress3 = "Dirección de facturación 3",
                City = "Elche",
                PostalCode = "03201",
                Country = "826"
            };

            var shippingAddress = new Address
            {
                StreetAddress1 = "Dirección de envío 1",
                StreetAddress2 = "Dirección de envío 2",
                StreetAddress3 = "Dirección de envío 3",
                City = "Elche",
                State = "ES",
                PostalCode = "03201",
                Country = "724",
            };

            try
            {
                // process an auto-capture authorization
                var hppJson = service.Verify(0m)
                     .WithCurrency("EUR")
                     .WithHostedPaymentData(hostedPaymentData)
                     .WithAddress(billingAddress, AddressType.Billing)
                     .WithAddress(shippingAddress, AddressType.Shipping)
                     .Serialize();

                return Content(hppJson);
            }

            catch (ApiException exce)
            {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
