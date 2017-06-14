package protonanalytics;

import datetime.DateTime;
import haxe.Http;
import haxe.io.Bytes;
import haxe.Timer;
import thx.http.RequestInfo;
import thx.http.RequestType;
using thx.http.Request;
using thx.Arrays;
using thx.Strings;
using protonanalytics.Guid;
using protonanalytics.storage.IStorage;
using protonanalytics.storage.SharedObjectStorage;

#if thx_stream
using thx.stream.Stream;
#end

@:expose
@:keep
// An analytics client that talks to the server. Please keep the instance of this
// alive; doing so will auto-retry sending events in the event of a failure.
// If you dispose of the client quickly, failed events won't be resent.
// Please call .dispose when you're done with this instance.
//
// Uses Lime for persistence. You can swap this out for something else that offers
// cross-platform persistence (if you want to support other frameworks).
// For example, Kha might use Storage or StorageFile. Just implement IStorage.
class AnalyticsClient
{
    private static inline var API_BASE_URL:String = "http://aalibhai-d03/ProtonAnalytics/api";

    private var timer:Timer = new Timer(60 * 1000); // every minute
    private var eventsToResend = new Array<ClientRequest>();
    private var apiKey:String;
    private var playerId:String;

    // Initializes (creates or loads) the player ID.
    public function new(apiKey:String)
    {
        this.apiKey = apiKey;

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
        }

        timer.run = this.retryFailedRequests;
    }

    public function startSession()
    {
        var now = this.getUtcDateString();
        var platform = this.getPlatform();
        var operatingSystem = this.getOperatingSystem();

        var body:String = '{
            "apiKey": "${this.apiKey}",
            "playerId": "${this.playerId}",
            "platform": "${platform}",
            "operatingSystem": "${operatingSystem}",
            "sessionStartUtc": "${now}"
        }';

        var request = new ClientRequest("POST", '${API_BASE_URL}/Session', body);
        this.httpRequest(request);
    }

    // If called multiple times, updates the end to the current time (always).
    // If multiple open sessions exist, updates the latest only.
    public function endSession()
    {
        var now = this.getUtcDateString();
        var platform = this.getPlatform();
        var operatingSystem = this.getOperatingSystem();

        var body:String = '{
            "apiKey": "${this.apiKey}",
            "playerId": "${this.playerId}",
            "sessionEndUtc": "${now}"
        }';

        var request = new ClientRequest("PUT", '${API_BASE_URL}/Session', body);
        this.httpRequest(request);
    }

    public function dispose():Void
    {
        this.timer.stop();
    }

    // Core function. Abstracts away customRequest (neko) vs thx (all other platforms).
    // Eventually, calls will queue up the "request" for retry if the call fails.    
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
            trace('Failed to send request ${request.httpVerb} -- queueing for retry.');
            this.eventsToResend.push(request);
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
            trace('Failed to send request ${request.httpVerb} -- queueing for retry.');
            this.eventsToResend.push(request);
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

    // Retries queued events. Also calls endSession every minute, so for clients
    // that don't tell us they end (eg. Flash, JS), we have at least some level
    // of reporting on how long the user was active.
    private function retryFailedRequests():Void
    {
        // Calling httpRequest queues more events, so you get a queue where you're
        // iterating and records are being added to the end simultaneously. EPIC infinite loop.
        // Instead, just go over however-many events are there in the queue right now.
        var events = eventsToResend.length;
        if (events > 0) {
            while (events-- > 0)
            {
                // Take the first item
                var request = eventsToResend.shift();
                trace('Found ${eventsToResend.length + 1} events to retry. Starting with: ${request.httpVerb}');                
                this.httpRequest(request);
            }
        }

        // Notify the server that the player was active for another minute.
        this.endSession();         
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