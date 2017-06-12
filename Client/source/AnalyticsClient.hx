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

        this.httpRequest("POST", '${API_BASE_URL}/Session', body);
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

        this.httpRequest("PUT", '${API_BASE_URL}/Session', body);
    }

    // Uses thx. Allows you to PUT/DELETE, and works in JS, but not in Neko.
    // The alternative is customRequest, which doesn't work in JS, but works in Neko.
    // Since we don't ship production games in Neko, we're going with thx.
    private function httpRequest(method:String, url:String, body:String):Void
    {
        method = method.toUpperCase();
        
        #if neko
        this.makeCustomHttpRequest(method, url, body);        
        #else
        this.makeThxHttpRequest(method, url, body);
        #end
    }

    private function makeThxHttpRequest(method:String, url:String, body:String):Void
    {
        var info:RequestInfo;
        var headers:Map<String, String> = [
            "Agent" => "thx.http.Request",            
        ];

        if (method == "GET")
        {
            info = new RequestInfo(method, url, headers);
        }
        else
        {
            headers.set("Content-Type", "application/json");
            info = new RequestInfo(method, url, headers, Text(body));
        }

        Request.make(info, Json).response.flatMap(function(r)
        {
            trace('${method} DONE (r=${r.statusCode}): ${r.body}');
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

    private function makeCustomHttpRequest(method:String, url:String, body:String):Void
    {
        #if neko
        var request:Http = new Http(url);
        var bytesOutput = new haxe.io.BytesOutput(); // Useless but necessary to call customRequest
        if (method != "GET")
        {
            request.setHeader("Content-Type", "application/json");
            request.setPostData(body);
        }

        request.onStatus = function(status)
        {
            trace('Custom Request status update: ${Std.int(status)}');
        }
        
        request.onError = function (e)
        {
            trace('Custom request FAILED: ${e}');
        }

        if (method == "GET" || method == "POST")
        {
            request.request(true);
        }
        else
        {
            request.customRequest(true, bytesOutput, method);
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