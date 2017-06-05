import haxe.io.Bytes;
import thx.http.RequestInfo;
import thx.http.RequestType;
using thx.http.Request;
using thx.Arrays;
using thx.Strings;
using Guid;

#if thx_stream
using thx.stream.Stream;
#end

import openfl.net.SharedObject;

@:expose
@:keep
// TODO: Don't require Flixel if you can avoid it. Opens this up to non-gaming clients.
class AnalyticsClient
{
    private static inline var API_BASE_URL:String = "http://localhost/ProtonAnalytics/api";
    private static inline var SHARED_OBJECT_STORAGE_NAME = "Proton Analytics client data";

    private var playerId:String;

    // Initializes (creates or loads) the player ID.
    public function new()
    {
        var storage = SharedObject.getLocal(SHARED_OBJECT_STORAGE_NAME);

        if (storage.data.playerId == null)
        {
            this.playerId = Guid.newGuid();
            storage.data.playerId = this.playerId;
            storage.flush();
            trace('Generated a new player ID: ${this.playerId}');
        }
        else
        {
            this.playerId = storage.data.playerId;
            trace('Reusing existing player ID: ${this.playerId}');
        }
    }

    public function startSession(apiKey:String)
    {
        var platform = this.getPlatform();

        var operatingSystem = this.getOperatingSystem();

        var body:String = '{
            "apiKey": "${apiKey}",
            "playerId": "${playerId}",
            "platform": "${platform}",
            "operatingSystem": "${operatingSystem}"
        }';

        this.httpRequest("POST", '${API_BASE_URL}/Session', body);
    }

    private function httpRequest(method:String, url:String, body:String):Void
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
            trace("Request successful: " + r);
        })
        .failure(function(e) 
        {
            trace("Request FAILED: " + e);
        });
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
}