using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SharedUtils.DTO;
using SharedUtils.OutputWriters;

namespace SharedUtils.EventStore
{
    public class EventStoreHandler : IDisposable
    {
        private readonly UserCredentials _userCredentials;
        readonly IEventStoreConnection _connection;

        public EventStoreHandler(IPEndPoint eventstoreEndpoint)
        {
            _userCredentials = new UserCredentials("admin", "changeit");

            var settings =
                ConnectionSettings.Create()
                    .KeepReconnecting()
                    .KeepRetrying()
                    .LimitReconnectionsTo(10)
                    .SetReconnectionDelayTo(TimeSpan.FromSeconds(3))
                    .SetDefaultUserCredentials(_userCredentials);

            _connection = EventStoreConnection.Create(settings, eventstoreEndpoint);
        }

        public void Connect()
        {
            _connection.ConnectAsync().Wait();
        }


        public List<EventstoreEvent> ReadLastEvents(string streamName, int eventsToRead)
        {
            var result = _connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, eventsToRead, false).Result;
            var mappedEvents = result.Events.Select(MapEventstoreEvent);

            return mappedEvents.Reverse().ToList();
        }


        private EventstoreEvent MapEventstoreEvent(ResolvedEvent resolvedEvent)
        {

            var resultingEvent = new EventstoreEvent()
            {
                Data = resolvedEvent.Event.Data,
                Metadata = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata),
                TimestampUtc = resolvedEvent.Event.Created,
                Datatype = resolvedEvent.Event.EventType,
            };

            return resultingEvent;
        }
        

        public async Task WriteJson(string streamId, string data, string datatype, string metadata)
        {
            var encodedData = Encoding.UTF8.GetBytes(data);
            var encodedMetadata = Encoding.UTF8.GetBytes(metadata ?? "");

            await WriteBytes(streamId, encodedData, datatype, encodedMetadata, isJson: true);
        }

        public async Task WriteBytes(string streamId, byte[] data, string datatype, byte[] metadata, bool isJson)
        {
            var myEvent = new EventData(Guid.NewGuid(), datatype, isJson, data,metadata);

            await _connection.AppendToStreamAsync(streamId, ExpectedVersion.Any, myEvent);

        }


        public void SubscribeToCatchupStream(string streamName, Action<EventstoreEvent> eventHandler)
        {
            _connection.SubscribeToStreamFrom(streamName, null,
                new CatchUpSubscriptionSettings(1, 1, false, false),
                (subscription, receivedEvent) => eventHandler(MapEventstoreEvent(receivedEvent)),
               subscription => {},
                (subscription, reason, arg3) => CatchupSubscriptionDropped(subscription, reason, arg3, streamName, eventHandler),
                _userCredentials);
        }

        private void CatchupSubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception arg3, string ControlChannelName, Action<EventstoreEvent> eventHandler)
        {
            OutputWriter.WriteLine("!! Subscription dropped {0}, {1}, {2}", subscription, reason, arg3);
            OutputWriter.WriteLine(" - Trying to reconnect");
            SubscribeToCatchupStream(ControlChannelName, eventHandler);
        }


        public void SubscribeToStream(string streamName, Action<EventstoreEvent> eventHandler)
        {
            _connection.SubscribeToStreamAsync(streamName, false,
                (subscription, resolvedEvent) => eventHandler(MapEventstoreEvent(resolvedEvent)),
                (subscription, reason, arg3) => SubscriptionDropped(reason, streamName, eventHandler));

        }

        private void SubscriptionDropped(SubscriptionDropReason reason, string streamName, Action<EventstoreEvent> eventHandler)
        {
            OutputWriter.WriteLine(ConsoleColor.Red, "Ticker Subscription dropped: {0}, trying to resubscribe..", reason.ToString());
            SubscribeToStream(streamName, eventHandler);

        }


        public void Dispose()
        {
            try
            {
                _connection.Close();
            }
            catch
            {
                //Äsch
            }
        }
    }
}