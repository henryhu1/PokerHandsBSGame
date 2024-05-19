# :spades: :hearts: PokerHandsBSGame :diamonds: :clubs:
My first 3D Unity game :beginner:

Multiplayer and networking made possible with :globe_with_meridians:
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
- [Relay](https://unity.com/products/relay)
- [Lobby](https://unity.com/products/lobby)

The UI was built entirely using [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) :gear:

## Manager Classes :bookmark_tabs:
Simplified diagram (UML-like) of the server/host side of the main classes, their *networked members* and server/host logic

![main](Repo/networked_components.png)

## Gameplay Events :arrow_right_hook:
RPCs and events that are used during the game in a pseudo sequence diagram

![events](Repo/game_events.png)

### Shortcomings :triangular_flag_on_post:

(1) and (2) are very similar, they could probably be looped.

\* `OnNextPlayerTurn` is actually invoked twice; the observer on the current turn client ID `NetworkVariable` and `NextRoundClientRPC` both fire the event.

## Nice to Have's :ideograph_advantage:
- Interactivity in player's own cards
- Tooltip containing information about player's own cards for clarity
- Animations for players' played hands, based on their own cards and the played hands log
- Localization
- CPU players

## Things to do Better :white_flag:
- Take more advantage of prefabs
- Having to go through lobby creation everytime to test gameplay (better scene management and scene independence during development?)
- Screen size and UI management
- Database or a relational database design-like class structure
  - Primary keys for hashing in dictionaries and foreign keys to connect to other objects
- Order of event invoking
- Graceful disconnect behaviour
  - An idea I had was only on client disconnect `IsClient && !IsServer`, a server RPC is sent to signal that this client is disconnecting (can determine if the host actually disconnected)
