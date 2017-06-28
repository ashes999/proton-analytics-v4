# Proton Analytics v4

MVP of Proton Analytics for my own cross-platform games. Fourth iteration of this product. Goal is bare-minimum; I am the only customer, and I can query data from SQL tools directly.

# Analytics Client

**Important note:** the client library requires OpenFL (which we use to persist the client ID). If you need this for other frameworks, or for things other than games, consider using a different approach, like using OpenFL to persist the client GUID.

To setup the client library and consume it, run `haxelib dev proton-analytics .` from the client directory.

Key notes:

- Create a persistent `AnalyticsClient` instance so that if the server is unreachable, the client will keep retrying. (eg. create a static instance in a `PaClient.hx` class).
- Call `startSession` in `Main.hx` or in your first state.
- `endSession` is automatically called every minute.

If you care about knowing the exact end-session time:
  - For mobile: create a base/common `FlxState` that overrides `onFocusLost` and calls `endSession`. This tracks the client switching away from your app on mobile.
  - For desktop: add this code block to `Main.hx`: `stage.addEventListener(openfl.events.Event.DEACTIVATE, PaClient.endSession);`.
  - HTML5 isn't possible at this time. When HaxeFlixel updates to OpenFL 5.x or newer, we should be able to use `stage.application.onExit(function(exitCode) { PaClient.endSession(); }` instead. For reference, see [this issue on GitHub](https://github.com/HaxeFlixel/flixel/issues/2081).