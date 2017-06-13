import datetime.DateTime;
import haxe.Http;
import haxe.io.Bytes;
import thx.http.RequestInfo;
import thx.http.RequestType;
using thx.http.Request;
using thx.Arrays;
using thx.Strings;
using Guid;
using storage.IStorage;
using storage.SharedObjectStorage;

#if thx_stream
using thx.stream.Stream;
#end

@:expose
@:keep
// TODO: Don't require Flixel if you can avoid it. Opens this up to non-gaming clients.
class AnalyticsClient
{
    private static inline var API_BASE_URL:String = "http://localhost/ProtonAnalytics/api";

    private var playerId:String;

    // Initializes (creates or loads) the player ID.
    public function new()
    {
        var storage:IStorage = new SharedObjectStorage();

        if (!storage.has("playerId"))
        {
            this.playerId = Guid.newGuid();
            storage.set("playerId", this.playerId);
            trace('Generated a new player ID: ${this.playerId}');
        }
        else
        {
            this.playerId = storage.get("playerId");
            trace('Reusing existing player ID: ${this.playerId}');
        }
    }

    public function startSession(apiKey:String)
    {
        var now = this.getUtcDateString();
        var platform = this.getPlatform();
        var operatingSystem = this.getOperatingSystem();

        var body:String = '{
            "apiKey": "${apiKey}",
            "playerId": "${playerId}",
            "platform": "${platform}",
            "operatingSystem": "${operatingSystem}",
            "sessionStartUtc": "${now}"
        }';

        var request = new ClientRequest("POST", '${API_BASE_URL}/Session', body);
        this.httpRequest(request);
    }

    // If called multiple times, updates the end to the current time (always).
    // If multiple open sessions exist, updates the latest only.
    public function endSession(apiKey:String)
    {
        var now = this.getUtcDateString();
        var platform = this.getPlatform();
        var operatingSystem = this.getOperatingSystem();

        var body:String = '{
            "apiKey": "${apiKey}",
            "playerId": "${playerId}",
            "sessionEndUtc": "${now}"
        }';

        var request = new ClientRequest("PUT", '${API_BASE_URL}/Session', body);
        this.httpRequest(request);
    }

    private function httpRequest(request:ClientRequest):Void
    {
        #if neko // useful for debugging/development
        this.makeCustomHttpRequest(request);        
        #else
        this.makeThxHttpRequest(request);
        #end
    }

    private function makeThxHttpRequest(request:ClientRequest):Void
    {
        var info:RequestInfo;
        var headers:Map<String, String> = [
            "Agent" => "thx.http.Request",            
        ];

        if (request.httpVerb == "GET")
        {
            info = new RequestInfo(request.httpVerb, request.url, headers);
        }
        else
        {
            headers.set("Content-Type", "application/json");
            info = new RequestInfo(request.httpVerb, request.url, headers, Text(request.body));
        }

        Request.make(info, Json).response.flatMap(function(r)
        {
            trace('${request.httpVerb} DONE (r=${r.statusCode}): ${r.body}');
            return r.body;
        })
        .success(function(r)
        {
            trace('Request successful: ${r}');
        })
        .failure(function(e) 
        {
            trace('Request FAILED: ${e}');
        });
    }

    private function makeCustomHttpRequest(request:ClientRequest):Void
    {
        #if neko
        var httpRequest:Http = new Http(request.url);
        var bytesOutput = new haxe.io.BytesOutput(); // Useless but necessary to call customRequest
        if (request.httpVerb != "GET")
        {
            httpRequest.setHeader("Content-Type", "application/json");
            httpRequest.setPostData(request.body);
        }

        httpRequest.onStatus = function(status)
        {
            trace('Custom Request status update: ${Std.int(status)}');
        }
        
        httpRequest.onError = function (e)
        {
            trace('Custom request FAILED: ${e}');
        }

        if (request.httpVerb == "GET" || request.httpVerb == "POST")
        {
            httpRequest.request(true);
        }
        else
        {
            httpRequest.customRequest(true, bytesOutput, request.httpVerb);
        }
        #end
    }

    private function getPlatform():String
    {
        #if (flash || flash8 || js)
            return "Web";
        #elseif (cpp || neko || cs)
            // is Android CPP?
            return "Desktop";
        #elseif java
            return "Android";
        #end

        throw "Unknown platform!";
    }

    private function getOperatingSystem():String
    {
        #if !js
            return Sys.systemName(); // eg. Windows, Linux, Mac, BSD
        #else
            // Very crude OS detection based on navigator.userAgent (appName is deprecated)
            var rawVersion = js.Browser.window.navigator.userAgent;
            if (rawVersion.indexOf("Win") > -1) {
                return "Windows";
            } else if (rawVersion.indexOf("Mac") > -1) {
                return "MacOS";
            } else if (rawVersion.indexOf("X11") > -1 || rawVersion.indexOf("Linux") > -1) {
                return "Linux";
            } else {
                return "Unknown OS";
            }
        #end
    }

    private function getUtcDateString():String
    {
        var utc = DateTime.now();
        return utc.toString();
    }
}