using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class nACSController : ControllerBase
    {
        [HttpGet]
        public IActionResult NotificationAcs(ChallengeUrlResponse detalles)
        {
            /*
              * Este código de muestra está pensado como un simple ejemplo y no debe tratarse como un código listo para producción 
              * Necesitarás añadir tu propio análisis de mensajes y seguridad en línea con tu aplicación o sitio web
              */
            var cres = Request.Form["cres"];

            // Ejemplo CRes (Challenge Result) sent by the ACS
            // var cRes = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6ImFmNjVjMzY5LTU5YjktNGY4ZC1iMmY2LTdkN2Q1ZjVjNjlkNSIsImF"
            // + "jc1RyYW5zSUQiOiIxM2M3MDFhMy01YTg4LTRjNDUtODllOS1lZjY1ZTUwYThiZjkiLCJjaGFsbGVuZ2VDb21wbGV0a"
            // + "W9uSW5kIjoiWSIsIm1lc3NhZ2VUeXBlIjoiQ3JlcyIsIm1lc3NhZ2VWZXJzaW9uIjoiMi4xLjAiLCJ0cmFuc"
            // + "1N0YXR1cyI6IlkifQ==";

            try
            {
                byte[] data = Convert.FromBase64String(cres);
                string challengeUrlResponseString = Encoding.UTF8.GetString(data);
                // Mapear la clase personalizada ChallengeUrlResponse con los Strings recibidos
                ChallengeUrlResponse challengeUrlResponse = JsonConvert.DeserializeObject<ChallengeUrlResponse>(challengeUrlResponseString);

                var threeDSServerTransID = challengeUrlResponse.ThreeDSServerTransID; // af65c369-59b9-4f8d-b2f6-7d7d5f5c69d5
                var acsTransId = challengeUrlResponse.AcsTransID; // 13c701a3-5a88-4c45-89e9-ef65e50a8bf9
                var challengeCompletionInd = challengeUrlResponse.ChallengeCompletionInd; // Y
                var messageType = challengeUrlResponse.MessageType; // Cres
                var messageVersion = challengeUrlResponse.MessageVersion; // 2.1.0
                var transStatus = challengeUrlResponse.TransStatus; // Y

                // TODO: notify client-side that the Challenge step is complete and pass any required data
                return Ok(challengeUrlResponse);
            }

            catch (Exception exce)
            {
                return Ok(exce);
                // TODO: agregue su manejo de excepciones aquí
            }

        }
    }
}