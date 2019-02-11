﻿using System.Collections.Generic;
using Mixpanel.MessageBuilders;
using Mixpanel.MessageBuilders.People;
using Mixpanel.MessageProperties;
using NUnit.Framework;

namespace Mixpanel.Tests.MessageBuilders.People
{
    // Message example:
    // {
    //     "$token": "36ada5b10da39a1347559321baf13063",
    //     "$distinct_id": "13793",
    //     "$set_once": {
    //         "First login date": "2013-04-01T13:20:00"
    //     }
    // }

    [TestFixture]
    public class PeopleSetOnceMessageBuilderTests : PeopleTestsBase
    {
        protected override string OperationName => "$set_once";

        [Test]
        public void When_DistinctIdParameter_Then_DistinctIdSetInMessage()
        {
            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, null, null, DistinctId, null);

            AssertMessageSuccess(
                messageBuildResult, 
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, DistinctId)
                },
                null);
        }

        [Test]
        public void When_DistinctIdFromSuperProperties_Then_DistinctIdSetInMessage()
        {
            var superProperties = CreateSuperProperties(
                ObjectProperty.Default(DistinctIdPropertyName, PropertyOrigin.SuperProperty, SuperDistinctId));

            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, superProperties, null, null, null);

            AssertMessageSuccess(
                messageBuildResult,
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, SuperDistinctId)
                },
                null);
        }

        [Test]
        public void When_NonSpecialSuperProperties_Then_Ignored()
        {
            var superProperties = CreateSuperProperties(
                ObjectProperty.Default(DistinctIdPropertyName, PropertyOrigin.SuperProperty, SuperDistinctId),
                // Should be ignored
                ObjectProperty.Default(StringPropertyName, PropertyOrigin.SuperProperty, StringPropertyValue));

            var rawProperties = new Dictionary<string, object>
            {
                {DecimalPropertyName, DecimalPropertyValue}
            };

            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, superProperties, rawProperties, null, null);

            AssertMessageSuccess(
                messageBuildResult,
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, SuperDistinctId)
                },
                new (string name, object value)[]
                {
                    (DecimalPropertyName, DecimalPropertyValue)
                });
        }


        [Test]
        public void When_RawProperties_Then_AllPropertiesInMessage()
        {
            var rawProperties = new Dictionary<string, object>
            {
                {DistinctIdPropertyName, DistinctId },
                {StringPropertyName, StringPropertyValue},
                {DecimalPropertyName, DecimalPropertyValue}
            };

            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, null, rawProperties, null, null);

            AssertMessageSuccess(
                messageBuildResult,
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, DistinctId)
                },
                new (string name, object value)[]
                {
                    (StringPropertyName, StringPropertyValue),
                    (DecimalPropertyName, DecimalPropertyValue)
                });
        }

        [Test]
        public void When_AllSpecialProperties_Then_AllPropertiesInMessage()
        {
            var rawProperties = new
            {
                DistinctId,
                Ip,
                Time,
                IgnoreTime,
                IgnoreAlias,
                FirstName,
                LastName,
                Name,
                Email,
                Phone,
                Created
            };

            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, null, rawProperties, null, null);

            AssertMessageSuccess(
                messageBuildResult,
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, DistinctId),
                    (PeopleSpecialProperty.Ip, Ip),
                    (PeopleSpecialProperty.Time, TimeUnix),
                    (PeopleSpecialProperty.IgnoreTime, IgnoreTime),
                    (PeopleSpecialProperty.IgnoreAlias, IgnoreAlias)
                },
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.FirstName, FirstName),
                    (PeopleSpecialProperty.LastName, LastName),
                    (PeopleSpecialProperty.Name, Name),
                    (PeopleSpecialProperty.Email, Email),
                    (PeopleSpecialProperty.Phone, Phone),
                    (PeopleSpecialProperty.Created, CreatedFormat),
                });
        }

        [Test]
        public void When_NameFormattingConfigured_Then_FormattingAppliedToPropertyNames()
        {
            var config = new MixpanelConfig { MixpanelPropertyNameFormat = MixpanelPropertyNameFormat.TitleCase };

            var rawProperties = new
            {
                PropertyA = 1,
                PropertyB = 2
            };

            MessageBuildResult messageBuildResult =
                PeopleSetMessageBuilder.BuildSetOnce(Token, null, rawProperties, DistinctId, config);

            AssertMessageSuccess(
                messageBuildResult,
                new (string name, object value)[]
                {
                    (PeopleSpecialProperty.Token, Token),
                    (PeopleSpecialProperty.DistinctId, DistinctId)
                },
                new (string name, object value)[]
                {
                    ("Property A", 1),
                    ("Property B", 2)
                });
        }

        [Test]
        public void When_NoToken_Then_MessageBuildFails()
        {
            MessageBuildResult messageBuildResult = PeopleSetMessageBuilder.BuildSetOnce(null, null, null, null, null);
            AssertMessageFail(messageBuildResult);
        }

        [Test]
        public void When_NoDistinctId_Then_MessageBuildFails()
        {
            MessageBuildResult messageBuildResult = PeopleSetMessageBuilder.BuildSetOnce(Token, null, null, null, null);
            AssertMessageFail(messageBuildResult);
        }
    }
}