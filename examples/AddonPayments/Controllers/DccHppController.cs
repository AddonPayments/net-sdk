using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class DccHppController : ControllerBase
    {
        [HttpGet]
        public IActionResult DccHpp() {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "dcc",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2",
                    DynamicCurrencyConversionEnabled = true
                }
            });

            try {
                // process an auto-capture authorization
                var hppJson = service.Charge(10.01m)
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
