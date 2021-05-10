using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CardHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult CardHpp() {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "api",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2",
                    DisplaySavedCards = true
                }
            });

            var hostedPaymentData = new HostedPaymentData
            {
                OfferToSaveCard = true,
                CustomerExists = true,
                CustomerKey = "3e3b6f0b-3fde-441d-b1e6-32ce927b0ad9"
                // supply your own reference for any new card saved
                // PaymentKey = "48fa69fe-d785-4c27-876d-6ccba660fa2b"
            };

            try {
                // process an auto-capture authorization
                var hppJson = service.Charge(19.99m)
                      .WithHostedPaymentData(hostedPaymentData)
                      .WithCurrency("EUR")
                      .Serialize();


                return Content(hppJson);
            }

            catch (ApiException exce) {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return BadRequest(respuesta);
            }
        }
    }
}
