using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using GlobalPayments.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprobarVersionController : ControllerBase
    {
        [HttpPost]
        public IActionResult CheckVersion(Tarjeta detalles)
        {

            // Configuramos los datos de nuestro comercio. Los datos son proporcionados por el equipo de Addon Payments, 
            // en caso de duda debe llamar al 914 353 028 e indicar su número de comercio o Merchant ID
            ServicesContainer.ConfigureService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "3ds2",
                SharedSecret = "secret",
                MethodNotificationUrl = "https://dominio.com/notificacion3ds",
                ChallengeNotificationUrl = "https://dominio.com/notificacionAcs",
                MerchantContactUrl = "https://www.dominio.com/about",
                Secure3dVersion = Secure3dVersion.Two
            });

            // Tarjetas de prueba
            // Frictionless: 4263970000005262
            // Challenge: 4012001038488884
            var card = new CreditCardData
            {
                Number = detalles.cardNumber
            };

            ThreeDSecure threeDSecureData = null;

            try
            {
                threeDSecureData = Secure3dService.CheckEnrollment(card)
                   .Execute();

                // Informamos al cliente
                RespuestaVersion3DS2 respuesta = new RespuestaVersion3DS2
                {
                    acsEndVersion = threeDSecureData.AcsEndVersion,
                    acsStartVersion = threeDSecureData.AcsStartVersion,
                    acsTransactionId = threeDSecureData.AcsTransactionId,
                    algorithm = threeDSecureData.Algorithm,
                    authenticationSource = threeDSecureData.AuthenticationSource,
                    authenticationType = threeDSecureData.AuthenticationType,
                    authenticationValue = threeDSecureData.AuthenticationValue,
                    cardHolderResponseInfo = threeDSecureData.CardHolderResponseInfo,
                    cavv = threeDSecureData.Cavv,
                    challengeMandated = threeDSecureData.ChallengeMandated,
                    criticalityIndicator = threeDSecureData.CriticalityIndicator,
                    directoryServerEndVersion = threeDSecureData.DirectoryServerEndVersion,
                    directoryServerStartVersion = threeDSecureData.DirectoryServerStartVersion,
                    directoryServerTransactionId = threeDSecureData.DirectoryServerTransactionId,
                    eci = threeDSecureData.Eci,
                    enrolled = threeDSecureData.Enrolled,
                    issuerAcsUrl = threeDSecureData.IssuerAcsUrl,
                    messageCategory = threeDSecureData.MessageCategory,
                    messageExtensionId = threeDSecureData.MessageExtensionId,
                    messageExtensionName = threeDSecureData.MessageExtensionName,
                    messageVersion = threeDSecureData.MessageVersion,
                    payerAuthenticationRequest = threeDSecureData.PayerAuthenticationRequest,
                    paymentDataSource = threeDSecureData.PaymentDataSource,
                    paymentDataType = threeDSecureData.PaymentDataType,
                    sdkInterface = threeDSecureData.SdkInterface,
                    sdkUiType = threeDSecureData.SdkUiType,
                    serverTransactionId = threeDSecureData.ServerTransactionId,
                    status = threeDSecureData.Status,
                    statusReason = threeDSecureData.StatusReason,
                    xid = threeDSecureData.Xid
                };

                return Ok(respuesta);
            }

            catch (ApiException exce)
            {
                return Ok(exce);
                // TODO: agregue su control de excepciones aquí
            }
        }
    }
}