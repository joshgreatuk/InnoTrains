# Game Architecture

Communication via server-server, client-server and vice versa are through Events and Interactions
- Interactions are callbacks to the server from either a client or the server
- Events are callbacks to clients from the server

There are different types of Interaction, those being:
- Server-sent, for notifying internal services of events or failures
- Client-sent, usually for when the client is requesting data or doing an action

There are also different types of event, being:
- Continuous, for constantly changing data, these may want to have a timestamp attached to ensure data is processed in the correct order (TCP should handle this)
- Occasional, events that don't happen often but are universal, such as a client connecting
- Callback, for responding to an interaction to the server, for example requested data, confirmation of an action, or a server error

An example of a client grabbing a list of vehicles would be:
- Client -> game/vehicles/get
- Either:
  - Server -> game/vehicle/get using GetVehicleEvent
  - Server -> game/vehicle/get using InteractionFailedEvent

Server event callbacks must be written as:
```c#
[NetworkEvent("game/vehicle/get")]
public void GetVehicleInteraction(string sender, string rawData)
{
	
}
```

Client event callbacks would be written as:
```gdscript
func event_game_vehicle_get(raw_data:String, is_success:bool):
	pass
```

However the isSuccess bool should be able to be ommitted

Note: On the client we can tag some events as continuous and remove old events of that type from the queue, for example
if the client is synchronising the game and it takes 10 seconds, there shouldnt be 200 movement updates to process when
they finish sync.

Some key game services we need include:
- [ ] ClientSyncService (for synchronising map and game data with a new client using 'client_connected')
- [ ] ChatService (for a chat system, uses routes 'chat/*')

In future I want to have the ability to switch where the lobby info / game files are stored, the options are:
- Lobby JSON file -> JSON Provider (lobby config) -> LobbyService
- Lobby Supabase table -> Supabase table provider (lobby config) -> LobbyService
- Game JSON file(s) -> JSON Provider (single/multi file config) -> LobbyService
- Game Supabase bucket -> Supabase bucket provider (single/multi file config) -> LobbyService

To achieve this we can use a middleware like IDataProvider, this will handle:
- Save/Load of files from Supabase or the local drive
- Operating on a config, for example lobby or game configs
- Checking if lobby / game files exist
- Indexing of public lobbies

IDataProvider must handle methods:
- DataExists
- SaveData
- LoadData
- ListData

Because of the split between lobby data and game data classes we will split this into:
- ILobbyDataProvider
- IGameDataProvider

These can always inherit base classes, for example an abstract JSONDataProvider, 
however a SupabaseDataProvider would not be required
