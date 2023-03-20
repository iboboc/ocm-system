﻿using OCM.API.Common.Model;
using OCM.API.Common.Model.OCPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCM.Import.Providers
{
    public class ImportProvider_OCPI : BaseImportProvider, IImportProvider
    {
        private string _authHeaderKey = "";
        private string _authHeaderValue = "";

        private int _dataProviderId = 1;

        public Dictionary<string,int> OperatorMappings = new Dictionary<string,int>();
        internal OCPIDataAdapter _adapter;
        public ImportProvider_OCPI()
        {
            this.ProviderName = "ocpi";
            OutputNamePrefix = "ocpi";
            SourceEncoding = Encoding.GetEncoding("UTF-8");
            IsAutoRefreshed = true;
            AllowDuplicatePOIWithDifferentOperator = true;

        }

        public string AuthHeaderValue { set { _authHeaderValue = value; } }

        public void Init(int dataProviderId, string locationsEndpoint, string authHeaderKey = null, string authHeaderValue = null)
        {
            AutoRefreshURL = locationsEndpoint;

            _authHeaderKey = authHeaderKey;

            _authHeaderValue = authHeaderValue;

            _dataProviderId = dataProviderId;
        }

        public List<ChargePoint> Process(CoreReferenceData coreRefData)
        {

            _adapter= new OCPIDataAdapter(coreRefData, useLiveStatus: false);

            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OCM.Model.OCPI.Location>>(InputData);

            var poiResults = _adapter.FromOCPI(response, _dataProviderId, OperatorMappings);

            var unmappedOperators = _adapter.GetUnmappedOperators();

            foreach (var o in unmappedOperators.OrderByDescending(i => i.Value))
            {
                System.Diagnostics.Debug.WriteLine($"Unmapped Operator: {o.Key} {o.Value}");
            }

            return poiResults.ToList();
        }

        public new bool LoadInputFromURL(string url)
        {
            try
            {
                if (_authHeaderKey != null)
                {
                    webClient.Headers.Add(_authHeaderKey, _authHeaderValue);
                }
                
                webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");

                InputData = webClient.DownloadString(url);

                return true;
            }
            catch (Exception)
            {
                Log(": Failed to fetch input from url :" + url);
                return false;
            }
        }
    }
}
