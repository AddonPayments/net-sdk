using System;
using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using GlobalPayments.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class IniciarAuthController : ControllerBase
    {
        [HttpPost]
        public IActionResult IniciarAuth([FromQuery] Tarjeta detalles, [FromQuery] NavegadorCliente navClient, [FromQuery] RespuestaVersion3DS2 responseACSVersion)
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
                Number = detalles.cardNumber,
                ExpMonth = detalles.month,
                ExpYear = detalles.year,
                Cvn = detalles.cvn,
                CardHolderName = detalles.cardholderName
            };

            // Indicamos la dirección de facturación
            Address billingAddress = new Address
            {
                StreetAddress1 = "Apartment 852",
                StreetAddress2 = "Complex 741",
                StreetAddress3 = "Unit 4",
                City = "Chicago",
                PostalCode = "50001",
                State = "IL",
                CountryCode = "840"
            };

            // Indicamos la dirección de envío
            Address shippingAddress = new Address
            {
                StreetAddress1 = "Flat 456",
                StreetAddress2 = "House 789",
                StreetAddress3 = "Basement Flat",
                City = "Halifax",
                PostalCode = "W5 9HR",
                CountryCode = "826"
            };

            // Indicamos los datos del navegaddor del cliente
            BrowserData browserData = new BrowserData
            {
                AcceptHeader = "text/html,application/xhtml+xml,application/xml;q=9,image/webp,img/apng,/;q=0.8",
                ColorDepth = navClient.colorDepth,
                IpAddress = "123.123.123.123",
                JavaEnabled = navClient.javaEnabled,
                Language = navClient.browserLanguage,
                ScreenHeight = navClient.screenHeight,
                ScreenWidth = navClient.screenWidth,
                ChallengeWindowSize = ChallengeWindowSize.FULL_SCREEN,
                Timezone = navClient.browserTimezoneZoneOffset,
                UserAgent = navClient.userAgent
            };


            ThreeDSecure threeDSecureData = new ThreeDSecure
            {
                ServerTransactionId = responseACSVersion.serverTransactionId
            };

            try
            {
                threeDSecureData = Secure3dService.InitiateAuthentication(card, threeDSecureData)
                   .WithAmount(10.01m)
                   .WithCurrency("EUR")
                   .WithOrderCreateDate(DateTime.Parse("2019-09-09T11:19:12"))
                   .WithCustomerEmail("james.mason@example.com")
                   .WithAddress(billingAddress, AddressType.Billing)
                   .WithAddress(shippingAddress, AddressType.Shipping)
                   .WithBrowserData(browserData)
                   .WithMethodUrlCompletion(MethodUrlCompletion.YES)
                   .WithMobileNumber("44", "7123456789")
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
                return BadRequest(exce);
                // TODO: agregue su control de excepciones aquí
            }
        }
    }
}