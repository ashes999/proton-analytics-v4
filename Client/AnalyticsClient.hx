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

@:expose
@:keep
// TODO: Don't require Flixel if you can avoid it. Opens this up to non-gaming clients.
class AnalyticsClient
{
    private static inline var API_BASE_URL:String = "http://localhost/ProtonAnalytics/api";

    public function new() { }

    public function startSession(apiKey:String, playerId:String)
    {
        var platform = this.getPlatform();

        // TODO: send the OS down too        
        var os = Sys.systemName(); // eg. Windows, Linux, Mac, BSD

        var body:String = '{
            "apiKey": "${apiKey}",
            "playerId": "${playerId}",
            "platform": "${platform}"
        }';

        trace('POSTing ${body}');
        this.httpRequest("POST", '${API_BASE_URL}/Session', body);
    }

    private function httpRequest(method:String, url:String, body:String):Void
    {
        var info:RequestInfo;
        var headers:Map<String, String> = [
            "Agent" => "thx.http.Request",            
        ];

        if (method == "GET") {
            info = new RequestInfo(Get, url, headers);
        } else {
            headers.set("Content-Type", "application/json");
            info = new RequestInfo(method, url, headers, Text(body));
        }

        Request.make(info, Json)
        .response
        .flatMap(function(r) {
            trace('${method} DONE (r=${r.statusCode}): ${r.body}');
            return r.body;
        })
        .success(function(r) trace("Request successful: " + r))
        .failure(function(e) trace("Request FAILED: " + e));
    }

    private function getPlatform():String
    {
        #if flash
            return "Web/Flash";
        #elseif flash8
           return "Web/Flash";
        #elseif cpp
            return "Desktop/C++";
        #elseif neko
            return "Desktop/Neko";
        #elseif js
            return "Web/Javascript";
        #elseif java
            return "Android/Java";
        #elseif cs
            return "Desktop/C#";
        #end

        throw "Unknown platform!";
    }
}