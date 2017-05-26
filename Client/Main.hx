import haxe.io.Bytes;
import thx.http.RequestInfo;
import thx.http.RequestType;
using thx.http.Request;
using thx.Arrays;
using thx.Strings;
#if thx_stream
using thx.stream.Stream;
#end

import AnalyticsClient;
import Guid;

@:expose
@:keep
class Main
{
    private static inline var API_KEY:String = "PTIY6d4512lvA//5Sdh0EJmLRbOf2h2L124e9fqlNaE=";

	public static function main()
	{
        var client = new AnalyticsClient();
        var playerId = Guid.newGuid();

        client.startSession(API_KEY, playerId);
	}
}