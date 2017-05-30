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
class Main extends openfl.display.Sprite
{
    private static inline var API_KEY:String = "PTIY6d4512lvA//5Sdh0EJmLRbOf2h2L124e9fqlNaE=";

	public function new()
	{
        super();
        var client = new AnalyticsClient();
        client.startSession(API_KEY);
	}
}