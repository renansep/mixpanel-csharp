﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mixpanel.MessageBuilders;
using Mixpanel.MessageBuilders.People;
using Mixpanel.MessageBuilders.Track;
using Mixpanel.MessageProperties;

namespace Mixpanel
{
    /// <inheritdoc/>
    public sealed partial class MixpanelClient : IMixpanelClient
    {
        private readonly string token;
        private readonly MixpanelConfig config;

        /// <summary>
        /// Func for getting/setting current utc date. Simplifies testing.
        /// </summary>
        internal Func<DateTime> UtcNow { get; set; }

        /// <summary>
        /// Parsed super properties which are added to every message.
        /// Collection is only initialized in constructor and never changed, so it's safe
        /// to be iterated by multiple threads.
        /// </summary>
        private readonly IList<ObjectProperty> superProperties;

        /// <summary>
        /// Creates an instance of <see cref="MixpanelClient"/>.
        /// </summary>
        /// <param name="token">
        /// The Mixpanel token associated with your project. You can find your Mixpanel token in the 
        /// project settings dialog in the Mixpanel app. Events without a valid token will be ignored.</param>
        /// <param name="config">
        /// Configuration for this particular client. Set properties from this class will override global properties.
        /// </param>
        /// <param name="superProperties">
        /// Object with properties that will be attached to every message for the current mixpanel client.
        /// If some of the properties are not valid mixpanel properties they will be ignored. Check documentation
        /// on project page https://github.com/eealeivan/mixpanel-csharp for valid property types.
        /// </param>
        public MixpanelClient(string token, MixpanelConfig config = null, object superProperties = null)
            : this(config, superProperties)
        {
            this.token = token;
        }

        /// <summary>
        /// Creates an instance of <see cref="MixpanelClient"/>. This constructor is usually used
        /// when you want to call only 'Send' and 'SendAsync' methods, because in this case
        /// token is already specified in each <see cref="MixpanelMessage"/>.
        /// </summary>
        /// <param name="config">
        /// Configuration for this particular client. Set properties from this class will override global properties.
        /// </param>
        /// <param name="superProperties">
        /// Object with properties that will be attached to every message for the current mixpanel client.
        /// If some of the properties are not valid mixpanel properties they will be ignored. Check documentation
        /// on project page https://github.com/eealeivan/mixpanel-csharp for valid property types.
        /// </param>
        public MixpanelClient(MixpanelConfig config = null, object superProperties = null)
        {
            this.config = config;
            UtcNow = () => DateTime.UtcNow;

            // Parse super properties only one time
            this.superProperties = PropertiesDigger.Get(superProperties, PropertyOrigin.SuperProperty).ToList();
        }

        #region Track

        /// <inheritdoc/>
        public async Task<bool> TrackAsync(string @event, object properties)
        {
            return await TrackAsync(@event, null, properties).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> TrackAsync(string @event, object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                    MessageKind.Track,
                    MixpanelMessageEndpoint.Track,
                    () => BuildTrackMessage(@event, distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetTrackMessage(string @event, object properties)
        {
            return GetTrackMessage(@event, null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetTrackMessage(string @event, object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.Track,
                () => BuildTrackMessage(@event, distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest TrackTest(string @event, object properties)
        {
            return TrackTest(@event, null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest TrackTest(string @event, object distinctId, object properties)
        {
            return TestMessage(() => BuildTrackMessage(@event, distinctId, properties));
        }

        private MessageBuildResult BuildTrackMessage(
            string @event, object distinctId, object properties)
        {
            return TrackMessageBuilder.Build(
                token, @event, superProperties, properties, distinctId, config);
        }

        #endregion

        #region Alias

        /// <inheritdoc/>
        public bool Alias(object alias)
        {
            return Alias(null, alias);
        }

        /// <inheritdoc/>
        public bool Alias(object distinctId, object alias)
        {
            return SendMessageInternal(
                MessageKind.Alias,
                MixpanelMessageEndpoint.Track,
                () => BuildAliasMessage(distinctId, alias));
        }

        /// <inheritdoc/>
        public async Task<bool> AliasAsync(object alias)
        {
            return await AliasAsync(null, alias)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> AliasAsync(object distinctId, object alias)
        {
            return await SendMessageInternalAsync(
                MessageKind.Alias,
                MixpanelMessageEndpoint.Track,
                () => BuildAliasMessage(distinctId, alias))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetAliasMessage(object alias)
        {
            return GetAliasMessage(null, alias);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetAliasMessage(object distinctId, object alias)
        {
            return GetMessage(
                MessageKind.Alias,
                () => BuildAliasMessage(distinctId, alias));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest AliasTest(object alias)
        {
            return AliasTest(null, alias);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest AliasTest(object distinctId, object alias)
        {
            return TestMessage(() => BuildAliasMessage(distinctId, alias));
        }

        private MessageBuildResult BuildAliasMessage(object distinctId, object alias)
        {
            return AliasMessageBuilder.Build(token, superProperties, distinctId, alias);
        }

        #endregion Alias

        #region PeopleSet

        /// <inheritdoc/>
        public bool PeopleSet(object properties)
        {
            return PeopleSet(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleSet(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleSet,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleSetMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleSetAsync(object properties)
        {
            return await PeopleSetAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleSetAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleSet,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleSetMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleSetMessage(object properties)
        {
            return GetPeopleSetMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleSetMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleSet,
                () => BuildPeopleSetMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleSetTest(object properties)
        {
            return PeopleSetTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleSetTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleSetMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleSetMessage(object distinctId, object properties)
        {
            return PeopleSetMessageBuilder.BuildSet(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleSet

        #region PeopleSetOnce

        /// <inheritdoc/>
        public bool PeopleSetOnce(object properties)
        {
            return PeopleSetOnce(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleSetOnce(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleSetOnce,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleSetOnceMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleSetOnceAsync(object properties)
        {
            return await PeopleSetOnceAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleSetOnceAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleSetOnce,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleSetOnceMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleSetOnceMessage(object properties)
        {
            return GetPeopleSetOnceMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleSetOnceMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleSetOnce,
                () => BuildPeopleSetOnceMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleSetOnceTest(object properties)
        {
            return PeopleSetOnceTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleSetOnceTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleSetOnceMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleSetOnceMessage(object distinctId, object properties)
        {
            return PeopleSetMessageBuilder.BuildSetOnce(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleSetOnce

        #region PeopleAdd

        /// <inheritdoc/>
        public bool PeopleAdd(object properties)
        {
            return PeopleAdd(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleAdd(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleAdd,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleAddMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleAddAsync(object properties)
        {
            return await PeopleAddAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleAddAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleAdd,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleAddMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleAddMessage(object properties)
        {
            return GetPeopleAddMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleAddMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleAdd,
                () => BuildPeopleAddMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleAddTest(object properties)
        {
            return PeopleAddTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleAddTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleAddMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleAddMessage(object distinctId, object properties)
        {
            return PeopleAddMessageBuilder.Build(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleAdd

        #region PeopleAppend

        /// <inheritdoc/>
        public bool PeopleAppend(object properties)
        {
            return PeopleAppend(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleAppend(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleAppend,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleAppendMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleAppendAsync(object properties)
        {
            return await PeopleAppendAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleAppendAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleAppend,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleAppendMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleAppendMessage(object properties)
        {
            return GetPeopleAppendMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleAppendMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleAppend,
                () => BuildPeopleAppendMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleAppendTest(object properties)
        {
            return PeopleAppendTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleAppendTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleAppendMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleAppendMessage(object distinctId, object properties)
        {
            return PeopleAppendMessageBuilder.Build(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleAppend

        #region PeopleUnion

        /// <inheritdoc/>
        public bool PeopleUnion(object properties)
        {
            return PeopleUnion(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleUnion(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleUnion,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleUnionMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleUnionAsync(object properties)
        {
            return await PeopleUnionAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleUnionAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleUnion,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleUnionMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleUnionMessage(object properties)
        {
            return GetPeopleUnionMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleUnionMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleUnion,
                () => BuildPeopleUnionMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleUnionTest(object properties)
        {
            return PeopleUnionTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleUnionTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleUnionMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleUnionMessage(object distinctId, object properties)
        {
            return PeopleUnionMessageBuilder.Build(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleUnion

        #region PeopleRemove

        /// <inheritdoc/>
        public bool PeopleRemove(object properties)
        {
            return PeopleRemove(null, properties);
        }

        /// <inheritdoc/>
        public bool PeopleRemove(object distinctId, object properties)
        {
            return SendMessageInternal(
                MessageKind.PeopleRemove,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleRemoveMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleRemoveAsync(object properties)
        {
            return await PeopleRemoveAsync(null, properties)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleRemoveAsync(object distinctId, object properties)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleRemove,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleRemoveMessage(distinctId, properties))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleRemoveMessage(object properties)
        {
            return GetPeopleRemoveMessage(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleRemoveMessage(object distinctId, object properties)
        {
            return GetMessage(
                MessageKind.PeopleRemove,
                () => BuildPeopleRemoveMessage(distinctId, properties));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleRemoveTest(object properties)
        {
            return PeopleRemoveTest(null, properties);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleRemoveTest(object distinctId, object properties)
        {
            return TestMessage(() => BuildPeopleRemoveMessage(distinctId, properties));
        }

        private MessageBuildResult BuildPeopleRemoveMessage(object distinctId, object properties)
        {
            return PeopleRemoveMessageBuilder.Build(token, superProperties, properties, distinctId, config);
        }

        #endregion PeopleRemove

        #region PeopleUnset

        /// <inheritdoc/>
        public bool PeopleUnset(IEnumerable<string> propertyNames)
        {
            return PeopleUnset(null, propertyNames);
        }

        /// <inheritdoc/>
        public bool PeopleUnset(object distinctId, IEnumerable<string> propertyNames)
        {
            return SendMessageInternal(
                MessageKind.PeopleUnset,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleUnsetMessage(distinctId, propertyNames));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleUnsetAsync(IEnumerable<string> propertyNames)
        {
            return await PeopleUnsetAsync(null, propertyNames)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleUnsetAsync(object distinctId, IEnumerable<string> propertyNames)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleUnset,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleUnsetMessage(distinctId, propertyNames))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleUnsetMessage(IEnumerable<string> propertyNames)
        {
            return GetPeopleUnsetMessage(null, propertyNames);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleUnsetMessage(object distinctId, IEnumerable<string> propertyNames)
        {
            return GetMessage(
                MessageKind.PeopleUnset,
                () => BuildPeopleUnsetMessage(distinctId, propertyNames));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleUnsetTest(IEnumerable<string> propertyNames)
        {
            return PeopleUnsetTest(null, propertyNames);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleUnsetTest(object distinctId, IEnumerable<string> propertyNames)
        {
            return TestMessage(() => BuildPeopleUnsetMessage(distinctId, propertyNames));
        }

        private MessageBuildResult BuildPeopleUnsetMessage(
            object distinctId,
            IEnumerable<string> propertyNames)
        {
            return PeopleUnsetMessageBuilder.Build(token, superProperties, propertyNames, distinctId, config);
        }

        #endregion PeopleUnset

        #region PeopleDelete

        /// <inheritdoc/>
        public bool PeopleDelete()
        {
            return PeopleDelete(null);
        }

        /// <inheritdoc/>
        public bool PeopleDelete(object distinctId)
        {
            return SendMessageInternal(
                MessageKind.PeopleDelete,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleDeleteMessage(distinctId));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleDeleteAsync()
        {
            return await PeopleDeleteAsync(null)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleDeleteAsync(object distinctId)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleDelete,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleDeleteMessage(distinctId))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleDeleteMessage()
        {
            return GetPeopleDeleteMessage(null);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleDeleteMessage(object distinctId)
        {
            return GetMessage(
                MessageKind.PeopleDelete,
                () => BuildPeopleDeleteMessage(distinctId));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleDeleteTest()
        {
            return PeopleDeleteTest(null);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleDeleteTest(object distinctId)
        {
            return TestMessage(() => BuildPeopleDeleteMessage(distinctId));
        }

        private MessageBuildResult BuildPeopleDeleteMessage(object distinctId)
        {
            return PeopleDeleteMessageBuilder.Build(token, superProperties, distinctId, config);
        }

        #endregion PeopleDelete

        #region PeopleTrackCharge

        /// <inheritdoc/>
        public bool PeopleTrackCharge(decimal amount)
        {
            return PeopleTrackCharge(null, amount);
        }

        /// <inheritdoc/>
        public bool PeopleTrackCharge(object distinctId, decimal amount)
        {
            return PeopleTrackCharge(distinctId, amount, UtcNow());
        }

        /// <inheritdoc/>
        public bool PeopleTrackCharge(decimal amount, DateTime time)
        {
            return PeopleTrackCharge(null, amount, time);
        }

        /// <inheritdoc/>
        public bool PeopleTrackCharge(object distinctId, decimal amount, DateTime time)
        {
            return SendMessageInternal(
                MessageKind.PeopleTrackCharge,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleTrackChargeMessage(distinctId, amount, time));
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleTrackChargeAsync(decimal amount)
        {
            return await PeopleTrackChargeAsync(null, amount)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleTrackChargeAsync(object distinctId, decimal amount)
        {
            return await PeopleTrackChargeAsync(distinctId, amount, UtcNow())
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleTrackChargeAsync(decimal amount, DateTime time)
        {
            return await PeopleTrackChargeAsync(null, amount, time)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> PeopleTrackChargeAsync(object distinctId, decimal amount, DateTime time)
        {
            return await SendMessageInternalAsync(
                MessageKind.PeopleTrackCharge,
                MixpanelMessageEndpoint.Engage,
                () => BuildPeopleTrackChargeMessage(distinctId, amount, time))
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleTrackChargeMessage(decimal amount)
        {
            return GetPeopleTrackChargeMessage(null, amount);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleTrackChargeMessage(object distinctId, decimal amount)
        {
            return GetPeopleTrackChargeMessage(distinctId, amount, UtcNow());
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleTrackChargeMessage(decimal amount, DateTime time)
        {
            return GetPeopleTrackChargeMessage(null, amount, time);
        }

        /// <inheritdoc/>
        public MixpanelMessage GetPeopleTrackChargeMessage(object distinctId, decimal amount, DateTime time)
        {
            return GetMessage(
                MessageKind.PeopleTrackCharge,
                () => BuildPeopleTrackChargeMessage(distinctId, amount, time));
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleTrackChargeTest(decimal amount)
        {
            return PeopleTrackChargeTest(null, amount);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleTrackChargeTest(object distinctId, decimal amount)
        {
            return PeopleTrackChargeTest(distinctId, amount, UtcNow());
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleTrackChargeTest(decimal amount, DateTime time)
        {
            return PeopleTrackChargeTest(null, amount, time);
        }

        /// <inheritdoc/>
        public MixpanelMessageTest PeopleTrackChargeTest(object distinctId, decimal amount, DateTime time)
        {
            return TestMessage(() => BuildPeopleTrackChargeMessage(distinctId, amount, time));
        }

        private MessageBuildResult BuildPeopleTrackChargeMessage(
            object distinctId, decimal amount, DateTime time)
        {
            return PeopleTrackChargeMessageBuilder.Build(
                token,
                superProperties,
                amount,
                time,
                distinctId,
                config);
        }

        #endregion

        #region Send

        /// <inheritdoc/>
        public SendResult Send(params MixpanelMessage[] messages)
        {
            return Send(messages as IEnumerable<MixpanelMessage>);
        }

        /// <inheritdoc/>
        public SendResult Send(IEnumerable<MixpanelMessage> messages)
        {
            var resultInternal = new SendResultInternal();
            var batchMessage = new BatchMessageWrapper(messages);

            SendBatches(MixpanelMessageEndpoint.Track, batchMessage.TrackMessages);
            SendBatches(MixpanelMessageEndpoint.Engage, batchMessage.EngageMessages);

            return resultInternal.ToRealSendResult();

            void SendBatches(MixpanelMessageEndpoint endpoint, List<List<MixpanelMessage>> batches)
            {
                if (batches == null)
                {
                    return;
                }

                foreach (List<MixpanelMessage> batch in batches)
                {
                    bool success = SendMessageInternal(
                        endpoint,
                        () => new BatchMessageBuildResult(batch));
                    resultInternal.Update(success, batch);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<SendResult> SendAsync(params MixpanelMessage[] messages)
        {
            return await SendAsync(messages as IEnumerable<MixpanelMessage>).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<SendResult> SendAsync(IEnumerable<MixpanelMessage> messages)
        {
            var resultInternal = new SendResultInternal();
            var batchMessage = new BatchMessageWrapper(messages);

            await SendBatches(MixpanelMessageEndpoint.Track, batchMessage.TrackMessages);
            await SendBatches(MixpanelMessageEndpoint.Engage, batchMessage.EngageMessages);

            return resultInternal.ToRealSendResult();

            async Task SendBatches(MixpanelMessageEndpoint endpoint, List<List<MixpanelMessage>> batches)
            {
                if (batches == null)
                {
                    return;
                }

                foreach (List<MixpanelMessage> batch in batches)
                {
                    bool success = await SendMessageInternalAsync(
                        endpoint,
                        () => new BatchMessageBuildResult(batch));
                    resultInternal.Update(success, batch);
                }
            }
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<MixpanelBatchMessageTest> SendTest(IEnumerable<MixpanelMessage> messages)
        {
            var batchMessageWrapper = new BatchMessageWrapper(messages);

            // Concatenate both 'TrackMessages' and 'EngageMessages' in one list
            List<List<MixpanelMessage>> batchMessages =
                (batchMessageWrapper.TrackMessages ?? new List<List<MixpanelMessage>>(0))
                .Concat(batchMessageWrapper.EngageMessages ?? new List<List<MixpanelMessage>>(0))
                .ToList();

            var testMessages = new List<MixpanelBatchMessageTest>(batchMessages.Count);

            foreach (List<MixpanelMessage> batchMessage in batchMessages)
            {
                var batchMessageBuildResult = new BatchMessageBuildResult(batchMessage);
                var testMessage = new MixpanelBatchMessageTest { Data = batchMessageBuildResult.Message };

                try
                {
                    testMessage.Json = ToJson(testMessage.Data);
                    testMessage.Base64 = ToBase64(testMessage.Json);
                }
                catch (Exception e)
                {
                    testMessage.Exception = e;
                }

                testMessages.Add(testMessage);
            }

            return testMessages.AsReadOnly();
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<MixpanelBatchMessageTest> SendTest(params MixpanelMessage[] messages)
        {
            return SendTest(messages as IEnumerable<MixpanelMessage>);
        }

        #endregion Send

        #region SendJson

        /// <inheritdoc/>
        public bool SendJson(MixpanelMessageEndpoint endpoint, string messageJson)
        {
            return SendMessageInternal(endpoint, messageJson);
        }

        /// <inheritdoc />
        public async Task<bool> SendJsonAsync(MixpanelMessageEndpoint endpoint, string messageJson)
        {
            return await SendMessageInternalAsync(endpoint, messageJson).ConfigureAwait(false);
        }

        #endregion SendJson

        private MixpanelMessage GetMessage(
            MessageKind messageKind,
            Func<MessageBuildResult> messageBuildResultFn)
        {
            try
            {
                MessageBuildResult messageBuildResult = messageBuildResultFn();
                if (!messageBuildResult.Success)
                {
                    LogError(
                        $"Cannot build message for {messageKind}.",
                        new Exception(messageBuildResult.Error));
                }

                return new MixpanelMessage
                {
                    Kind = messageKind,
                    Data = messageBuildResult.Message
                };
            }
            catch (Exception e)
            {
                LogError($"Error building message for {messageKind}.", e);
                return null;
            }
        }

        private MixpanelMessageTest TestMessage(Func<MessageBuildResult> messageBuildResultFn)
        {
            var testMessage = new MixpanelMessageTest();

            try
            {
                MessageBuildResult messageBuildResult = messageBuildResultFn();
                if (!messageBuildResult.Success)
                {
                    throw new Exception(messageBuildResult.Error);
                }

                testMessage.Data = messageBuildResult.Message;
                testMessage.Json = ToJson(testMessage.Data);
                testMessage.Base64 = ToBase64(testMessage.Json);
            }
            catch (Exception e)
            {
                testMessage.Exception = e;
                return testMessage;
            }

            return testMessage;
        }

        private void LogError(string msg, Exception exception)
        {
            ConfigHelper.GetErrorLogFn(config)?.Invoke(msg, exception);
        }
    }
}
