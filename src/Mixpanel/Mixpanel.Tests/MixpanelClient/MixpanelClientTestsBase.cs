﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Mixpanel.Tests.MixpanelClient
{
    [TestFixture]
    public abstract class MixpanelClientTestsBase : MixpanelTestsBase
    {
        protected Mixpanel.MixpanelClient Client;

        protected List<(string Endpoint, string Data)> HttpPostEntries;

        protected string TrackUrl =>
            string.Format(Mixpanel.MixpanelClient.UrlFormat, Mixpanel.MixpanelClient.EndpointTrack);

        protected string EngageUrl =>
            string.Format(Mixpanel.MixpanelClient.UrlFormat, Mixpanel.MixpanelClient.EndpointEngage);

        [SetUp]
        public void MixpanelClientSetUp()
        {
            MixpanelConfig.Global.Reset();

            Client = new Mixpanel.MixpanelClient(Token, GetConfig());

            HttpPostEntries = new List<(string Endpoint, string Data)>();
        }

        protected static void IncludeDistinctIdIfNeeded(bool includeDistinctId, Dictionary<string, object> dic)
        {
            if (includeDistinctId)
            {
                dic.Add(MixpanelProperty.DistinctId, DistinctId);
            }
        }

        protected MixpanelConfig GetConfig()
        {
            return new MixpanelConfig
            {
                HttpPostFn = (endpoint, data) =>
                {
                    HttpPostEntries.Add((endpoint, data));
                    return true;
                },

                AsyncHttpPostFn = (endpoint, data) =>
                {
                    HttpPostEntries.Add((endpoint, data));
                    return Task.Run(() => true);
                },

#if !JSON
                SerializeJsonFn = obj => JsonConvert.SerializeObject(obj)
#endif
            };
        }

        protected JObject ParseMessageData(string data)
        {
            // Can't use JObject.Parse because it's not possible to disable DateTime parsing
            using (var stringReader = new StringReader(GetJsonFromData(data)))
            {
                using (JsonReader jsonReader = new JsonTextReader(stringReader))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    JObject msg = JObject.Load(jsonReader);
                    return msg;
                }
            }
        }

        protected string GetJsonFromData(string data)
        {
            // Remove "data=" to get raw BASE64
            string base64 = data.Remove(0, 5);
            byte[] bytesString = Convert.FromBase64String(base64);

            var json = Encoding.UTF8.GetString(bytesString, 0, bytesString.Length);
            return json;
        }
    }
}