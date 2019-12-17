using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CustomerHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult CustomerHpp() {

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
                    CardStorageEnabled = true
                }
            });

            // Datos que deben transferirse a la HPP junto con las opciones de nivel de transacción
            var hostedPaymentData = new HostedPaymentData
            {
                OfferToSaveCard = true, // display the save card tick box
                CustomerExists = false // new customer
                                       // supply your own references
                                       // CustomerKey = "a7960ada-3da9-4a5b-bca5-7942085b03c6",
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
                return Ok(respuesta);
            }
        }
    }
}
