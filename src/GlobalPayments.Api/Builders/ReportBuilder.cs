﻿using GlobalPayments.Api.Entities;

namespace GlobalPayments.Api.Builders {
    public abstract class ReportBuilder<TResult> : BaseBuilder<TResult> where TResult : class {
        internal ReportType ReportType { get; set; }
        internal TimeZoneConversion TimeZoneConversion { get; set; }

        public ReportBuilder(ReportType type) : base() {
            ReportType = type;
        }

        /// <summary>
        /// Executes the builder against the gateway.
        /// </summary>
        /// <returns>TResult</returns>
        public override TResult Execute(string configName = "default") {
            base.Execute(configName);

            var client = ServicesContainer.Instance.GetReportingClient(configName);
            return client.ProcessReport(this);
        }
    }
}
