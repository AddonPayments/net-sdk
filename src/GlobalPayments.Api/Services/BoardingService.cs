﻿using GlobalPayments.Api.Entities.OnlineBoarding;

namespace GlobalPayments.Api.Services {
    public class BoardingService {
        private BoardingConfig _config;

        public BoardingService(BoardingConfig config, string configName = "default") {
            _config = config;
            ServicesContainer.ConfigureService(config, configName);
        }

        public BoardingApplication NewApplication() {
            return new BoardingApplication {
                MerchantInfo = new MerchantInfo {
                    AffiliatePartnerId = _config.Portal
                }
            };
        }

        public BoardingResponse SubmitApplication(string invitation, BoardingApplication application, string configName = "default") {
            var conn = ServicesContainer.Instance.GetBoardingConnector(configName);
            return conn.SendApplication(invitation, application);
        }
    }
}
