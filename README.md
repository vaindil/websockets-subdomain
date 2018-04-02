I use this code to handle a few different systems that integrate with several other bots of mine ([vbot-d](https://github.com/vaindil/vbot-d) and [FitzTwitch](https://github.com/vaindil/FitzTwitch)).

## W/L Tracking for Twitch Stream

The primary function of the site right now is an API and websocket for [Fitzyhere](https://www.twitch.tv/fitzyhere). Fitzy is primarily an Overwatch streamer, and he keeps track of his win/loss record for the night on the stream itself. To allow mods to update the text on the screen, a few steps are used.

### REST API

[FitzTwitch](https://github.com/vaindil/FitzTwitch) takes commands from mods and sends the relevant request to the really basic REST API in this repo. This API tracks the current record and stores it in the database, as well as in the in-memory cache.

### Websocket

Websocket connections are accepted to provide the current record and an update whenever the record changes. The page using this is currently [vaindil.xyz/fitzy](https://vaindil.xyz/fitzy). That page is displayed on the stream with an [OBS](https://obsproject.com) [browser source](https://obsproject.com/wiki/Sources-Guide#browsersource).

## Acknowledgements

Big thanks to [tpeczek](https://github.com/tpeczek) for his [demo repo](https://github.com/tpeczek/Demo.AspNetCore.WebSockets), I wouldn't have figured out ASP.NET Core websockets without it. Check that repo's readme for some very helpful blog posts.

Thanks also to [Evk](https://stackoverflow.com/users/5311735/evk) on Stack Overflow for [answering my question](https://stackoverflow.com/a/49605801/1672458) about how to keep websocket connections open.